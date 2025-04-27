using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OptNet
{
    internal class OptMD<T> where T : struct, INumber<T>
    {
        private readonly uint dimN;
        private ImmutableArray<uint> Ns;
        private ImmutableArray<T> minX;
        private ImmutableArray<T> maxX;
        private ImmutableArray<T> dX;
        private ImmutableArray<T> dXHalf;
        private ImmutableList<T> sampleJtot;
        private ImmutableArray<uint> optIdx;
        private readonly string mode;

        private Dictionary<uint, OfflineGet<T>> lutConstant;

        private T[] inputX;
        private PCQueue pcQ;
        private KeyValuePair<T[], T> optXY;

        //public Task EndTsk { get; set; }
        public Func<T[], T> J;
        public Action<string> DisplayMsgFunc;

        public OptMD(in Func<T[], T> j, in uint dimN, in uint[] Ns, in T[] minX, in T[] maxX, in string mode)
        {
            J = j;
            this.dimN = dimN;
            this.Ns = ImmutableArray.Create(Ns);
            this.minX = ImmutableArray.Create(minX);
            this.maxX = ImmutableArray.Create(maxX);
            this.mode = mode;
            inputX = new T[dimN];
            lutConstant = new Dictionary<uint, OfflineGet<T>>();

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

            int coreCount = Environment.ProcessorCount;
            pcQ = new PCQueue(coreCount);
            //optimizeMD();
            /*shrinkBy(0.5);
            optimizeMD();
            shrinkBy(0.5);
            optimizeMD();
            shrinkBy(0.5);
            optimizeMD();
            shrinkBy(0.5);
            optimizeMD();*/
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
                int minIdx = (int)(optIdx[idxDim] - rdxIdx / 2);
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
            reSample(0, ref samplejtot, true, inputIndex);
            sampleJtot = samplejtot.ToImmutableList();
        }
        public async void optimizeMD()
        {
            //sampleJtot = new List<T>();
            //getSample(0);          // resetting "sampleJtot" is always needed before getSample(0) !!!!
            var optx = new KeyValuePair<T, T>(T.Zero, T.Zero);
            var tmpkey = new T[dimN];
            var inputIndex = new uint[dimN];

            var tasks = new List<Task>();

            for (int i = 0; i < dimN; i++)
            {
                var optidx = new uint[dimN];

                if (i == 0)
                {
                    optx = optimizeMD1AxisRecur(i, i, inputIndex, ref optidx);
                    optIdx = ImmutableArray.Create(optidx);
                    tmpkey[i] = optx.Key;
                }
                else
                {
                    var idx = i;
                    var task = pcQ.Enqueue<KeyValuePair<T, T>>(
                        () => optimizeMD1Axis(idx, optIdx.ToArray())
                    );
                    var task2 = task.ContinueWith(
                        t =>
                        {
                            optx = t.Result;
                            tmpkey[idx] = optx.Key;
                        }
                        //,TaskContinuationOptions.ExecuteSynchronously
                    );
                    tasks.Add(task2);

                    /*  for serial running ***************************************/
                    /* optx = optimizeMD1Axis(i, optIdx.ToArray() );
                    tmpkey[i] = optx.Key;*/
                }
            }      

            if (dimN <= 1)
            {
                optXY = new KeyValuePair<T[], T>(tmpkey, optx.Value);
                DisplayMsg();
            }
            else
            {
                var _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                await Task.WhenAll(tasks).ContinueWith(
                    t =>
                    {
                        optXY = new KeyValuePair<T[], T>(tmpkey, optx.Value);
                        DisplayMsg();
                    }
                    , _uiScheduler
                );

                /*  for serial running ***************************************/
                /* 
                    optXY = new KeyValuePair<T[], T>(tmpkey, optx.Value);
                    DisplayMsg();
                */
            }
    }
    private KeyValuePair<T, T> optimizeMD1Axis(int idx, uint[] optidx)
    {
        //var optx = new KeyValuePair<T, T>(T.Zero, T.Zero);
        var tmpkey = new T[dimN];
        var inputIndex = new uint[dimN];
        //var optxy = new KeyValuePair<T[], T>([T.Zero], T.Zero);

        var optx = optimizeMD1AxisRecur(idx, idx, inputIndex, ref optidx);
        return optx;
    }
    private KeyValuePair<T, T> optimizeMD1AxisRecur(int axisIdx, int idxDim, uint[] inputIndex, ref uint[] optidx)
    {
        Opt1D<T> opt1D;

        var prevAxisIdx = (int)(axisIdx - 1) < 0 ? (int)dimN - 1 : (axisIdx - 1);
        var nextIdx = (idxDim + 1) >= dimN ? 0 : (idxDim + 1);

        var sampleJs = new T[Ns[idxDim]];
        var optIdxCandidate = new uint[Ns[idxDim]][];

        KeyValuePair<T, T> sampleOpt;

        for (uint n = 0; n < Ns[idxDim]; n++)
        {
            inputIndex[idxDim] = n;

            if (idxDim == prevAxisIdx)
            {
                sampleJs[n] = sampleJtot[(int)getIdTot(inputIndex)];
                if (axisIdx == 0)
                    optIdxCandidate[n] = new uint[dimN];
            }
            else
            {
                sampleOpt = optimizeMD1AxisRecur(axisIdx, nextIdx, inputIndex, ref optidx);
                sampleJs[n] = sampleOpt.Value;
                if (axisIdx == 0)
                    optIdxCandidate[n] = optidx;
            }
        }

        OfflineGet<T> constVar;
        if (lutConstant.ContainsKey(Ns[idxDim]))
            constVar = lutConstant[Ns[idxDim]];
        else
        {
            constVar = new OfflineGet<T>(Ns[idxDim]);
            lutConstant.Add(Ns[idxDim], constVar);
        }

        if ((axisIdx != 0) && (idxDim == axisIdx))
            opt1D = new Opt1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mode, optIdx[idxDim]);
        //opt1D = new Opt1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mode, null);
        else
            opt1D = new Opt1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mode, null);

        if (mode == "Max")
        {
            if (axisIdx == 0)
            {
                var curOptIdx = (uint)(double.CreateChecked(opt1D.Max.Key) * (Ns[idxDim] - 1));
                optidx = optIdxCandidate[curOptIdx];
                optidx[idxDim] = curOptIdx;
            }
            return opt1D.Max;
        }
        else
        {
            if (axisIdx == 0)
            {
                //var curOptIdx = uint.CreateChecked((opt1D.Min.Key - minX[idxDim]) / dX[idxDim]);
                var curOptIdx = (uint)(double.CreateChecked(opt1D.Max.Key) * (Ns[idxDim] - 1));
                optidx = optIdxCandidate[curOptIdx];
                optidx[idxDim] = curOptIdx;
            }
            return opt1D.Min;
        }
    }
    private void getSample(in int idxDim, ref List<T> samplejtot)
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
    private void reSample(in int idxDim, ref List<T> samplejtot, bool isEven, in uint[] inputIndex)
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
                reSample(idxDim + 1, ref samplejtot, iseven, inputIndex);
                if (iseven)
                    inputIndex[idxDim]++;
            }

        }
        inputIndex[idxDim] = tmpInputIdx;
    }
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
        public void DisplayMsg(Action<string> DisplayMessage)
        {
            string msgOptX = "Optimum ";
            msgOptX += string.Format("{0,8:G5} at ", optXY.Value); ;

            for (int i = 0; i < dimN; i++)
            {
                msgOptX += string.Format("{0,8:G5} ", calcRealX(optXY.Key[i], i)); ;
            }
            DisplayMessage(msgOptX);

            /*string msgTest = "";
            for (uint i = 0; i < Ns[0]; i++)
            {
                for (uint j = 0; j < Ns[1]; j++)
                {
                    inputIndex[0] = i;
                    inputIndex[1] = j;
                    //msgTest += string.Format("{0,8:G5} ", inputIndex[i]);
                    var tmpIndex = (int)getIdTot();
                    msgTest += string.Format("{0,10:F4} ", sampleJtot[tmpIndex]);

                }
                msgTest += Environment.NewLine;
            }
            DisplayMessage(msgTest);*/
        }
        public string DisplayMsg()
        {
            string msgOptX = "Optimum ";
            msgOptX += string.Format("{0,8:G5} at ", optXY.Value); ;

            for (int i = 0; i < dimN; i++)
            {
                msgOptX += string.Format("{0,8:G5} ", calcRealX(optXY.Key[i], i)); ;
            }

            DisplayMsgFunc(msgOptX);
            return msgOptX;
            /*string msgTest = "";
            for (uint i = 0; i < Ns[0]; i++)
            {
                for (uint j = 0; j < Ns[1]; j++)
                {
                    inputIndex[0] = i;
                    inputIndex[1] = j;
                    //msgTest += string.Format("{0,8:G5} ", inputIndex[i]);
                    var tmpIndex = (int)getIdTot();
                    msgTest += string.Format("{0,10:F4} ", sampleJtot[tmpIndex]);

                }
                msgTest += Environment.NewLine;
            }
            DisplayMessage(msgTest);*/
        }
    }
}
