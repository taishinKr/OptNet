using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace OptNet
{
    public class Opt1D<T> where T : struct, INumber<T>
    {
        private readonly T[] W;
        //private readonly T[] omegaW;
        private readonly T[] A;
        private readonly T[] B;
        private readonly T[] C;

        private readonly T minX;
        private readonly T maxX;
        private readonly T dX;
        private readonly T dXHalf;
        private readonly uint Ns;
        private readonly T[] sampleJs;

        private readonly OfflineGet<T> constVars;
        private readonly int minDpK;
        private readonly int maxDpK;

        private readonly double[]? optIntv;
        public ConcurrentQueue<string> DisplayMsgQueue;
        [Flags]
        public  enum staMaskEnum : UInt32 { Done = 1, Min = 1 << 1, Max = 1 << 2, Bottom = 1 << 3, Verbose = 1 << 4 }
        private UInt32 mState = 0;

        private T[] calcW(in T[] Js) => GeneralMath<T>.LinearTransform(constVars.P.Data, Js, 0, constVars.P.GetLength(1) - 1);
        //private T[] calcOmegaW(in T[] Js) => GeneralMath<T>.LinearTransform(constVars.P, Js);

        public T[] Js => sampleJs;
        public (T[] Xlist, T J) OptVal { get; }
        //public Dictionary<T, T> Lo { get; }
        public List<(T X, T J)> Lo { get; }

        public Opt1D(in T[] sampleJs, double tolJs, in uint Ns, in T minX, in T maxX, in OfflineGet<T> constvars, in staMaskEnum mode, double[]? optIntv)
        {
            clrSysFlag(staMaskEnum.Max);
            clrSysFlag(staMaskEnum.Min);
            var modetmp = mode;
            if (mode == (staMaskEnum.Max | staMaskEnum.Min))
                modetmp = staMaskEnum.Max;
            setSysFlag(modetmp);

            this.Ns = Ns;
            this.sampleJs = sampleJs;
            this.minX = minX;
            this.maxX = maxX;
            dX = (maxX - minX) / T.CreateChecked((Ns - 1));
            dXHalf = dX / T.CreateChecked(2);

            this.optIntv = optIntv;

            if (constvars == null)
                constVars = new OfflineGet<T>(Ns);
            else
                constVars = constvars;

            W = calcW(Js);
            //omegaW = calcOmegaW(Js);

            calOptCIntv(out minDpK, out maxDpK);
            A = GeneralMath<T>.LinearTransform(constVars.Ra.Data, Js, (int)minDpK, (int)maxDpK);
            B = GeneralMath<T>.LinearTransform(constVars.Rb.Data, Js, (int)minDpK, (int)maxDpK);
            C = GeneralMath<T>.LinearTransform(constVars.Rc.Data, Js, (int)minDpK, (int)maxDpK);

            Lo = getLo();
            if (isSetSysFlag(staMaskEnum.Max))
            {
                var maxJ = Lo.Max(item => item.J);
                var xlist = Lo.Where(item => Math.Abs(double.CreateChecked(item.J - maxJ)) <= tolJs)
                            .Select(item => item.X)
                            .ToArray();
                OptVal = (xlist, maxJ);
            }
            if (isSetSysFlag(staMaskEnum.Min))
            {
                var minJ = Lo.Min(item => item.J);
                var xlist = Lo.Where(item => Math.Abs(double.CreateChecked(item.J - minJ)) <= tolJs)
                            .Select(item => item.X)
                            .ToArray();
                OptVal = (xlist, minJ);
            }

            DisplayMsgQueue = new();
            clrSysFlag(staMaskEnum.Verbose);
            addDispMsg(resultMsg());
            setSysFlag(staMaskEnum.Verbose);
        }
        public Opt1D(in Func<T, T> j, double tolJs, in uint Ns, in T minX, in T maxX, in OfflineGet<T> constvars, in staMaskEnum mode, double[]? optIntv)
        {
            clrSysFlag(staMaskEnum.Max);
            clrSysFlag(staMaskEnum.Min);
            setSysFlag(mode);

            this.Ns = Ns;
            this.minX = minX;
            this.maxX = maxX;
            dX = (maxX - minX) / T.CreateChecked((Ns - 1));
            dXHalf = dX / T.CreateChecked(2);

            this.optIntv = optIntv;

            sampleJs = getSamples(j);

            if (constvars == null)
                constVars = new OfflineGet<T>(Ns);
            else
                constVars = constvars;

            W = calcW(Js);
            //omegaW = calcOmegaW(Js);

            calOptCIntv(out minDpK, out maxDpK);
            A = GeneralMath<T>.LinearTransform(constVars.Ra.Data, Js, minDpK, maxDpK);
            B = GeneralMath<T>.LinearTransform(constVars.Rb.Data, Js, minDpK, maxDpK);
            C = GeneralMath<T>.LinearTransform(constVars.Rc.Data, Js, minDpK, maxDpK);

            Lo = getLo();
            if (isSetSysFlag(staMaskEnum.Max))
            {
                var maxJ = Lo.Max(item => item.J);
                var xlist = Lo.Where(item => Math.Abs(double.CreateChecked(item.J - maxJ)) <= tolJs)
                            .Select(item => item.X)
                            .ToArray();
                OptVal = (xlist, maxJ);
            }
            if (isSetSysFlag(staMaskEnum.Min))
            {
                var minJ = Lo.Min(item => item.J);
                var xlist = Lo.Where(item => Math.Abs(double.CreateChecked(item.J - minJ)) <= tolJs)
                            .Select(item => item.X)
                            .ToArray();
                OptVal = (xlist, minJ);
            }

            DisplayMsgQueue = new();
            clrSysFlag(staMaskEnum.Verbose);
            addDispMsg(resultMsg());
            setSysFlag(staMaskEnum.Verbose);
        }
        private T[] getSamples(in Func<T, T> j)
        {
            var tmpJs = new T[Ns];
            for (uint n = 0; n < Ns; n++)
                tmpJs[n] = j(minX + T.CreateChecked(n) * dX);
            return tmpJs;
        }
        private List<(T X, T J)> getLo()
        {
            var lo = new List<(T X, T J)>();
            for (int k = 0; k < A.Length; k++)
            {
                T tmpD2 = B[k] * B[k] - A[k] * C[k] * (T.One + T.One + T.One + T.One);
                T sqrtTmpD2 = T.CreateChecked(Math.Sqrt(double.CreateChecked(tmpD2)));
                T x;

                //uint modK = k + minDpK;
                if (A[k] == T.Zero)
                {
                    if (B[k] == T.Zero) // Constant equation
                    {
                        if (C[k] == T.Zero) // All x are solution
                            lo.Add((constVars.Dp[k], T.Zero));
                    }
                    else // Linear equation
                    {
                        x = -C[k] / B[k];
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add((x, Jw(x)));
                        else
                            continue;
                    }
                }
                else  // Quadratic equation
                {
                    if (sqrtTmpD2 == T.Zero)
                    {
                        x = -B[k] / A[k] / (T.One + T.One);
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add((x, Jw(x)));
                        else
                            continue;
                    }
                    else
                    {
                        x = (-B[k] - sqrtTmpD2) / A[k] / (T.One + T.One);
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add((x, Jw(x)));
                        x = (-B[k] + sqrtTmpD2) / A[k] / (T.One + T.One);
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add((x, Jw(x)));
                    }
                }
            }
            return lo;
        }
        private void calOptCIntv(out int minK, out int maxK)
        {
            minK = 0;
            maxK = 0;
            if (optIntv!=null) // Ensure optIdx is not null
            {
                var xmin = T.CreateChecked(optIntv[0]);
                var xmax = T.CreateChecked(optIntv[1]);
                var dpsize = constVars.Dp.Length;
                for (int i = 0; i < dpsize; i++)
                {
                    if ((constVars.Dp[i] <= xmin) && (xmin < constVars.Dp[i + 1]))
                    {
                        minK = i;
                    }
                    if ((constVars.Dp[i] <= xmax) && (xmax < constVars.Dp[i + 1]))
                    {
                        maxK = i + 1;
                        break;
                    }
                }
            }
            else
            {
                minK = 0;
                maxK = (int)constVars.Dp.Length - 1 - 1;
            }
        }
        private bool IsInCi(in T x, in int k)
        {

            if ((x >= constVars.Dp[k]) && (x < constVars.Dp[k + 1]))
                return true;
            else
                return false;
        }
        public string resultMsg()
        {
            string msgTemplate = "Key = {0,8:G5},   Value = {1,8:G5}";
            string s=string.Empty;
            foreach ( (T X, T J) lo in Lo)
            {
                s += "Lo  " + string.Format(msgTemplate, calcRealX(lo.X), lo.J) + Environment.NewLine;
            }
            if (isSetSysFlag(staMaskEnum.Max))
            {
                foreach ( T optx in OptVal.Xlist)
                {
                    s += "Max:  " + string.Format(msgTemplate, calcRealX(optx), OptVal.J ) + Environment.NewLine;
                }
            }
            if (isSetSysFlag(staMaskEnum.Min))
            {
                foreach ( T optx in OptVal.Xlist)
                {
                    s += "Min:  " + string.Format(msgTemplate, calcRealX(optx), OptVal.J) + Environment.NewLine;
                }
            }
            return s;
        }
        public T Jw(in T x)
        {
            T sum = T.Zero;
            for (uint n = 0; n <= Ns - 1; n++)
                sum += W[n] * T.CreateChecked(Math.Cos(n * Math.PI * double.CreateChecked(x))); // Convert T to double    
            return sum;
        }
        public T calcRealX(in T projX) => (maxX - minX + dX) * projX + minX - dXHalf;
        public T calcProjX(in T realX) => (realX - minX + dXHalf) / (maxX - minX + dX);
        static public T JTest1D(T xT)
        {
            double x = double.CreateChecked(xT);
            return T.CreateChecked(Math.Exp(x) * Math.Sin(5.5 * Math.PI * x));
        }
        public void addDispMsg(string msg)
        {
            if (isSetSysFlag(staMaskEnum.Verbose) && !string.IsNullOrEmpty(msg))
            {
                DisplayMsgQueue.Enqueue(msg);
            }

        }
        public string getDispMsg()
        {
            if (isSetSysFlag(staMaskEnum.Verbose) && DisplayMsgQueue.TryDequeue(out string? msg))
                return msg;
            else
                return string.Empty;
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
