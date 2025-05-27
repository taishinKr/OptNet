using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OptNet
{
    public class OptMD<T> where T : struct, INumber<T>
    {
        private readonly uint dimN;
        private ImmutableArray<uint> Ns;
        private ImmutableArray<T> minX;
        private ImmutableArray<T> maxX;
        private ImmutableArray<T> dX;
        private ImmutableArray<T> dXHalf;
        private ImmutableList<T> sampleJtot;
        private readonly staMaskEnum mode;
        private ImmutableDictionary<uint, OfflineGet<T>> lutConstant;

        private static readonly Lock _locker = new ();//new Lock();
        private static readonly PCQueue pcQ = new(Environment.ProcessorCount); // new PCQueue(Environment.ProcessorCount);

        private T[] inputX;
        private (T[][] Xlist, T J) optResult;

        public double[][] IntvX { get; set; } // minX, maxX
        public Func<T[], T> J;
        //public Action<string> DisplayMsgFunc;
        public ConcurrentQueue<string> DisplayMsgQueue;
        [Flags]
        public enum staMaskEnum : UInt32 { Done = 1, Min = 1 << 1, Max = 1 << 2, Bottom = 1 << 3, Verbose = 1 << 4 }
        private UInt32 mState = 0;

        public OptMD(in Func<T[], T> j, in uint dimN, in uint[] Ns, in T[] minX, in T[] maxX, in staMaskEnum mode)
        {
            J = j;
            this.dimN = dimN;
            this.Ns = ImmutableArray.Create(Ns);
            this.minX = ImmutableArray.Create(minX);
            this.maxX = ImmutableArray.Create(maxX);

            this.mode = mode;
            if (mode == (staMaskEnum.Max | staMaskEnum.Min))
                this.mode = staMaskEnum.Max;
            // throw new ArgumentException("staMaskEnum must be Max, Min, or Both.");

            inputX = new T[dimN];
            lutConstant = ImmutableDictionary<uint, OfflineGet<T>>.Empty;

            var dx = new T[dimN];
            var dxhalf = new T[dimN];
            for (uint i = 0; i < dimN; i++)
            {
                dx[i] = (maxX[i] - minX[i]) / T.CreateChecked((Ns[i] - 1));
                dxhalf[i] = dx[i] / T.CreateChecked(2);
            }
            dX = ImmutableArray.Create(dx);
            dXHalf = ImmutableArray.Create(dxhalf);

            var samplejtot = new List<T>();
            getSample(0, ref samplejtot);
            sampleJtot = samplejtot.ToImmutableList();

            IntvX = new double[2][]; // minX, maxX

            DisplayMsgQueue = new(); // new ConcurrentQueue<string>();
            setSysFlag(staMaskEnum.Verbose);
        }
        public void shrinkBy(double ratio) // ratio other than 0.5 makes the sample number(Ns) changed!!! 
        {
            var minx = new T[dimN];
            var maxx = new T[dimN];
            var dx = new T[dimN];
            var dxhalf = new T[dimN];
            var ns = new uint[dimN];
            var inputIndex = new uint[dimN];

            for (int idxDim = 0; idxDim < dimN; idxDim++)
            {
                int dxIdx = (int)(Ns[idxDim] - 1.0);
                int rdxIdx = (int)(ratio * dxIdx);
                double optidx = (double.CreateChecked(optResult.Xlist[0][idxDim]) * dxIdx);
                int minIdx = (int)(optidx - rdxIdx / 2.0);
                if (minIdx < 0)
                    minIdx = 0;
                int maxIdx = minIdx + rdxIdx;
                if (maxIdx >= Ns[idxDim] - 1)
                {
                    maxIdx = (int)(Ns[idxDim] - 1);
                    minIdx = maxIdx - rdxIdx;
                }
                minx[idxDim] = minX[idxDim] + T.CreateChecked(minIdx) * dX[idxDim];
                maxx[idxDim] = minX[idxDim] + T.CreateChecked(maxIdx) * dX[idxDim];
                ns[idxDim] = (uint)(rdxIdx * 2 + 1);
                dx[idxDim] = dX[idxDim] / (T.One + T.One);
                dxhalf[idxDim] = dx[idxDim] / (T.One + T.One);

                inputIndex[idxDim] = (uint)minIdx;
            }

            minX = ImmutableArray.Create(minx);
            maxX = ImmutableArray.Create(maxx);
            dX = ImmutableArray.Create(dx);
            dXHalf = ImmutableArray.Create(dxhalf);
            Ns = ImmutableArray.Create(ns);

            var samplejtot = new List<T>();
            upSample(0, ref samplejtot, true, inputIndex);
            sampleJtot = samplejtot.ToImmutableList();
        }

        public async void optimizeMD()
        {
            var xOpt = new T[dimN];

            // for Parallel running ***************************************
            await optimaizeMdRecurAsync(0, sampleJtot, xOpt).ContinueWith(
                t =>
                {
                    var (Xlist, J) = t.Result;
                    optResult = (Xlist.ToArray(), J);
                }
            );

            // for Serial running ***************************************
            /*var (Xlist, J) = optimaizeMdRecur(0, sampleJtot, xOpt);
            optResult = (Xlist.ToArray(), J);*/

            addDispMsg(resultMsg());
        }
        private (List<T[]> Xlist, T J) optimaizeMdRecur(int axisIdx, ImmutableList<T> samplejtot, T[] xOpt)
        {
            addDispMsg(string.Format("MdReur({0,0:G3})" + Environment.NewLine, axisIdx));

            var Xlist = new List<T[]>();
            T J = T.Zero;

            var opt1Axis = optimizeMD1Axis(samplejtot, axisIdx);

            if (axisIdx == 0)
                IntvX = opt1Axis.optIntv;

            foreach (var optx in opt1Axis.Xlist1Axis)
            {
                xOpt[axisIdx] = optx;

                if (axisIdx == dimN - 1)
                {
                    Xlist.Add(xOpt);
                    J = opt1Axis.J;
                }
                else
                {
                    List<T> newSample = []; // new List<T>()
                    var inputIdx = new uint[dimN];
                    reSample(optx, axisIdx + 1, axisIdx + 1, ref inputIdx, ref samplejtot, ref newSample);

                    //[.. newSample]; // newSample.ToImmutableList();
                    (var XlistNew, J) = optimaizeMdRecur(axisIdx + 1, [.. newSample], xOpt);
                    Xlist.AddRange(XlistNew);
                }
            }
            return (Xlist, J);
        }
        private async Task<(List<T[]> Xlist, T J)> optimaizeMdRecurAsync(int axisIdx, ImmutableList<T> samplejtot, T[] xOpt)
        {
            addDispMsg(string.Format("MdReur({0,0:G3})" + Environment.NewLine, axisIdx));

            var Xlist = new List<T[]>();
            T J = T.Zero;

            (T[] Xlist1Axis, T J, double[][] optIntv) opt1Axis = ([], T.Zero, []);
            await optimizeMD1AxisAsync(samplejtot, axisIdx).ContinueWith(
                    t =>
                    {
                        opt1Axis = t.Result;
                    }
                );

            if (axisIdx == 0)
                IntvX = opt1Axis.optIntv;

            foreach (var optx in opt1Axis.Xlist1Axis)
            {
                xOpt[axisIdx] = optx;

                if (axisIdx == dimN - 1)
                {
                    Xlist.Add(xOpt);
                    J = opt1Axis.J;
                }
                else
                {
                    List<T> newSample = []; // new List<T>()
                    var inputIdx = new uint[dimN];
                    reSample(optx, axisIdx + 1, axisIdx + 1, ref inputIdx, ref samplejtot, ref newSample);

                    //[.. newSample]; // newSample.ToImmutableList();
                    await optimaizeMdRecurAsync(axisIdx + 1, [.. newSample], xOpt).ContinueWith(
                        t =>
                        {
                            (var XlistNew, J) = t.Result;
                            Xlist.AddRange(XlistNew);
                        }
                    );
                }
            }
            return (Xlist, J);
        }
        private async Task<(T[] Xlist1Axis, T J, double[][] optIntv)> optimizeMD1AxisAsync(ImmutableList<T> samplejtot, int idxDim)
        {
            Opt1D<T> opt1D;
            var sampleJs = new T[Ns[idxDim]];
            var minXs = new double[Ns[idxDim]][];
            var maxXs = new double[Ns[idxDim]][];
            var dx = 1.0 / Ns[idxDim]; // caution!!! not (1.0 / (Ns[idxDim] - 1.0))
            double[][]? optIntv =
                [
                    new double[dimN], // minX
            new double[dimN]  // maxX
                ];

            var axisIdx = idxDim;
            var tasks = new List<Task<(T[] Xlist1Axis, T J, double[][] optIntv)>>();

            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                var inputIndex = new uint[dimN];
                inputIndex[idxDim] = n;

                if (idxDim == dimN - 1)
                {
                    sampleJs[n] = samplejtot[(int)getIdTot(inputIndex)];
                    // optmal value is definitive.
                    if (axisIdx == 0)
                    {
                        minXs[n] = new double[dimN];
                        maxXs[n] = new double[dimN];
                        minXs[n][idxDim] = dx * (n + 0.5);
                        maxXs[n][idxDim] = dx * (n + 0.5);
                    }
                }
                else
                {
                    addDispMsg(string.Format("    task: Md1Axis({0,0:G3},{1,0:G3})" + Environment.NewLine, idxDim, n));

                    var task = pcQ.Enqueue<(T[] Xlist1Axis, T J, double[][] optIntv)>(
                        () => optimizeMD1AxisRecur(samplejtot, axisIdx, idxDim + 1, inputIndex)
                    );
                    tasks.Add(task);

                }
            }
            if (tasks.Count > 0)
            {
                //var _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                await Task.WhenAll(tasks).ContinueWith(
                    t =>
                    {
                        sampleJs = [.. t.Result.Select(x => x.J)]; // t.Result.Select(x => x.optJ).ToArray();
                        minXs = [.. t.Result.Select(x => x.optIntv[0])]; // t.Result.Select(x => x.optIntv[0]).ToArray();
                        maxXs = [.. t.Result.Select(x => x.optIntv[1])]; // t.Result.Select(x => x.optIntv[1]).ToArray();
                    }
                // , _uiScheduler
                );
            }

            OfflineGet<T> constVar;
            lock (_locker)
            {
                if (lutConstant.ContainsKey(Ns[idxDim]))
                    constVar = lutConstant[Ns[idxDim]];
                else
                {
                    constVar = new OfflineGet<T>(Ns[idxDim]);
                    lutConstant = lutConstant.Add(Ns[idxDim], constVar);
                }
            }

            if ((axisIdx != 0) && (idxDim == axisIdx))
            {
                var optIntV1D = new[] { IntvX[0][idxDim], IntvX[1][idxDim] };
                opt1D = new Opt1D<T>(sampleJs, 0.001, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.staMaskEnum)mode, optIntV1D);
                //opt1D = new Opt1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mode, null);
            }
            else
            {
                opt1D = new Opt1D<T>(sampleJs, 0.001, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.staMaskEnum)mode, null);
                if (axisIdx == 0)
                {
                    for (int i = idxDim + 1; i < dimN; i++)
                    {
                        var intvMin = double.MaxValue;
                        var intvMax = double.MinValue;

                        foreach (var optX in opt1D.OptVal.Xlist)
                        {
                            var optidx = (int)(double.CreateChecked(optX) / dx);
                            intvMin = Math.Min(
                                intvMin,
                                Math.Min(
                                    minXs[optidx][i],
                                    minXs[optidx + 1][i]
                                )
                            );
                            intvMax = Math.Max(
                                intvMax,
                                Math.Max(
                                    maxXs[optidx][i],
                                    maxXs[optidx + 1][i]
                                )
                            );
                        }
                        optIntv[0][i] = intvMin;
                        optIntv[1][i] = intvMax;
                    }

                    var marginDx = 0.1 * dx;

                    optIntv[0][idxDim] = double.CreateChecked(opt1D.OptVal.Xlist.Min()) - marginDx;
                    if (optIntv[0][idxDim] < 0.0)
                    {
                        optIntv[0][idxDim] = 0.0;
                        optIntv[1][idxDim] = 2 * marginDx;
                    }

                    optIntv[1][idxDim] = double.CreateChecked(opt1D.OptVal.Xlist.Max()) + marginDx;
                    if (optIntv[1][idxDim] > 1.0)
                    {
                        optIntv[0][idxDim] = 1.0 - 2 * marginDx;
                        optIntv[1][idxDim] = 1.0;
                    }
                }
            }
            return (opt1D.OptVal.Xlist, opt1D.OptVal.J, optIntv);
        }
        private (T[] Xlist1Axis, T J, double[][] optIntv) optimizeMD1Axis(ImmutableList<T> samplejtot, int idx)
        {
            var inputIndex = new uint[dimN];
            (T[] Xlist1Axis, T J, double[][] optIntv) = optimizeMD1AxisRecur(samplejtot, idx, idx, inputIndex);

            return (Xlist1Axis, J, optIntv);
        }
        private (T[] Xlist1Axis, T J, double[][] optIntv) optimizeMD1AxisRecur(ImmutableList<T> samplejtot, int axisIdx, int idxDim, uint[] inputIndex)
        {
            Opt1D<T> opt1D;
            var sampleJs = new T[Ns[idxDim]];
            var minXs = new double[Ns[idxDim]][];
            var maxXs = new double[Ns[idxDim]][];
            var dx = 1.0 / Ns[idxDim]; // caution!!! not (1.0 / (Ns[idxDim] - 1.0))
            double[][]? optIntv =
                [
                    new double[dimN], // minX
                    new double[dimN]  // maxX
                ];

            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputIndex[idxDim] = n;

                if (idxDim == dimN - 1)
                {
                    sampleJs[n] = samplejtot[(int)getIdTot(inputIndex)];
                    // optmal value is definitive.
                    if (axisIdx == 0)
                    {
                        minXs[n] = new double[dimN];
                        maxXs[n] = new double[dimN];
                        minXs[n][idxDim] = dx * (n + 0.5);
                        maxXs[n][idxDim] = dx * (n + 0.5);
                    }
                }
                else
                {
                    addDispMsg(string.Format("        Md1Axis({0,0:G3},{1,0:G3},{2,0:G3})" + Environment.NewLine, axisIdx, idxDim, n));
                    (_, sampleJs[n], optIntv) = optimizeMD1AxisRecur(samplejtot, axisIdx, idxDim + 1, inputIndex);
                    if (axisIdx == 0)
                    {
                        minXs[n] = optIntv[0];
                        maxXs[n] = optIntv[1];
                    }
                }
            }

            OfflineGet<T> constVar;
            lock (_locker)
            {
                if (lutConstant.ContainsKey(Ns[idxDim]))
                    constVar = lutConstant[Ns[idxDim]];
                else
                {
                    constVar = new OfflineGet<T>(Ns[idxDim]);
                    lutConstant = lutConstant.Add(Ns[idxDim], constVar);
                }
            }

            if ((axisIdx != 0) && (idxDim == axisIdx))
            {
                var optIntV1D = new[] { IntvX[0][idxDim], IntvX[1][idxDim] };
                opt1D = new Opt1D<T>(sampleJs, 0.001, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.staMaskEnum)mode, optIntV1D);
                //opt1D = new Opt1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mode, null);
            }
            else
            {
                opt1D = new Opt1D<T>(sampleJs, 0.001, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.staMaskEnum)mode, null);
                if (axisIdx == 0)
                {
                    for (int i = idxDim + 1; i < dimN; i++)
                    {
                        var intvMin = double.MaxValue;
                        var intvMax = double.MinValue;

                        foreach (var optX in opt1D.OptVal.Xlist)
                        {
                            var optidx = (int)(double.CreateChecked(optX) / dx);
                            intvMin = Math.Min(
                                intvMin,
                                Math.Min(
                                    minXs[optidx][i],
                                    minXs[optidx + 1][i]
                                )
                            );
                            intvMax = Math.Max(
                                intvMax,
                                Math.Max(
                                    maxXs[optidx][i],
                                    maxXs[optidx + 1][i]
                                )
                            );
                        }
                        optIntv[0][i] = intvMin;
                        optIntv[1][i] = intvMax;
                    }

                    var marginDx = 0.1 * dx;

                    optIntv[0][idxDim] = double.CreateChecked(opt1D.OptVal.Xlist.Min()) - marginDx;
                    if (optIntv[0][idxDim] < 0.0)
                    {
                        optIntv[0][idxDim] = 0.0;
                        optIntv[1][idxDim] = 2 * marginDx;
                    }

                    optIntv[1][idxDim] = double.CreateChecked(opt1D.OptVal.Xlist.Max()) + marginDx;
                    if (optIntv[1][idxDim] > 1.0)
                    {
                        optIntv[0][idxDim] = 1.0 - 2 * marginDx;
                        optIntv[1][idxDim] = 1.0;
                    }
                }
            }
            return (opt1D.OptVal.Xlist, opt1D.OptVal.J, optIntv);
        }
        private void getSample(int idxDim, ref List<T> samplejtot)
        {
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX[idxDim];
                if (idxDim == dimN - 1)
                    samplejtot.Add(J(inputX));
                else
                    getSample(idxDim + 1, ref samplejtot);
            }
        }
        private void reSample(T x, int axisIdx, int idxDim, ref uint[] inputIdx, ref ImmutableList<T> iSample, ref List<T> oSample)
        {
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputIdx[idxDim] = n;
                if (idxDim == dimN - 1)
                {
                    var samplej = new T[Ns[axisIdx - 1]];
                    for (uint i = 0; i < Ns[axisIdx - 1]; i++)
                    {
                        inputIdx[axisIdx - 1] = i;
                        samplej[i] = iSample[(int)getIdTot(inputIdx)];
                    }
                    OfflineGet<T> constVars = lutConstant[Ns[idxDim]];
                    var w = GeneralMath<T>.LinearTransform(constVars.P.Data, samplej);

                    T sum = T.Zero;
                    for (uint i = 0; i < w.Length; i++)
                        sum += w[i] * T.CreateChecked(Math.Cos(i * Math.PI * double.CreateChecked(x)));
                    oSample.Add(sum);
                }
                else
                    reSample(x, axisIdx, idxDim + 1, ref inputIdx, ref iSample, ref oSample);
            }
        }
        private void upSample(int idxDim, ref List<T> samplejtot, bool isEven, in uint[] inputIndex)
        {
            uint tmpInputIdx = inputIndex[idxDim];
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                bool iseven = isEven && (n % 2 == 0);

                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX[idxDim];
                if (idxDim == dimN - 1)
                {
                    if (iseven)
                    {
                        samplejtot.Add(sampleJtot[(int)getIdTot(inputIndex)]);
                        inputIndex[idxDim]++;
                        //samplejtot.Add(J(inputX));
                    }
                    else
                        samplejtot.Add(J(inputX));
                }
                else
                {
                    upSample(idxDim + 1, ref samplejtot, iseven, inputIndex);
                    if (iseven)
                        inputIndex[idxDim]++;
                }

            }
            inputIndex[idxDim] = tmpInputIdx;
        }
        /// <summary>
        /// get an index for the sampleJtot array from the dimN dimensional inputIndex array.
        /// resample() makes the dimension to decrease by 1 and reduces the size of sampleJtot array,
        /// but, the index of the reduced demension, which is assumed to be zero, does no effect on the calculation.
        /// then, use the same getIdTot() method without any modification even after resample().
        /// </summary>
        /// <param name="inputIndex"></param>
        /// <returns></returns>
        private uint getIdTot(in uint[] inputIndex)
        {
            /*uint idTot = inputIndex[0];
            for (uint i = 1; i < dimN; i++)
            {
                uint accumNs = 1;
                for (uint j = 0; j < i; j++)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }*/
            uint idTot = inputIndex[dimN - 1];
            for (int i = (int)dimN - 2; i >= 0; i--)
            {
                uint accumNs = 1;
                for (int j = (int)dimN - 1; j > i; j--)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }

            return idTot;
        }

        public T calcRealX(in T projX, in int idxDim) => (maxX[idxDim] - minX[idxDim] + dX[idxDim]) * projX + minX[idxDim] - dXHalf[idxDim];
        public T calcProjX(in T realX, in int idxDim) => (realX - minX[idxDim] + dXHalf[idxDim]) / (maxX[idxDim] - minX[idxDim] + dX[idxDim]);
        public static T JTest1D(T[] xT)
        {
            double[] x = new double[1]; ;
            x[0] = double.CreateChecked(xT[0]);
            return T.CreateChecked(Math.Exp(x[0]) * Math.Sin(5.5 * Math.PI * x[0]));
        }
        public static T JTest2D(T[] xT)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1]))
            );
        }
        public static T JTest3D(T[] xT)
        {
            double[] x = new double[3];

            for (uint i = 0; i < 3; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                (1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1])))
                * Math.Sin(Math.PI * x[2])
            );
        }
        
        public void addDispMsg(string msg)
        {
            if(isSetSysFlag(staMaskEnum.Verbose) && !string.IsNullOrEmpty(msg))
            {
                DisplayMsgQueue.Enqueue(msg);
            }
            
        }
        public string getDispMsg()
        {
            if ( isSetSysFlag(staMaskEnum.Verbose) && DisplayMsgQueue.TryDequeue(out string? msg))
                return msg;
            else
                return string.Empty;
        }
        /*public void DisplayMsg(Action<string> DisplayMessage)
        {
            string msgOptX = "Optimum ";
            msgOptX += string.Format("{0,8:G5} at ", optResult.J); ;

            for (int i = 0; i < dimN; i++)
            {
                msgOptX += string.Format("{0,8:G5} ", calcRealX(optResult.Xlist[0][i], i)); ;
            }
            DisplayMessage(msgOptX);
        }*/
        public string resultMsg()
        {
            string msgOptX = "";
            foreach (var x in optResult.Xlist)
            {
                string msgOptX1 = "Optimum ";
                msgOptX1 += string.Format("{0,8:G5} at ", optResult.J);

                for (int i = 0; i < dimN; i++)
                {
                    msgOptX1 += string.Format("{0,8:G5} ", calcRealX( x[i], i)); ;
                }
                msgOptX += msgOptX1 + Environment.NewLine;
            }

            //DisplayMsgFunc(msgOptX);
            return msgOptX;
        }

        public void setSysFlag(in staMaskEnum mask)
        {
            mState = GeneralMath<T>.setFlag(ref mState, (UInt32)mask);

        }
        public void clrSysFlag(in staMaskEnum mask)
        {
            mState = GeneralMath<T>.clrFlag(ref mState, (UInt32)mask);
        }
        public bool isSetSysFlag(in staMaskEnum mask)
        {
            return GeneralMath<T>.isSetFlag(mState, (UInt32)mask);
        }
    }
}
