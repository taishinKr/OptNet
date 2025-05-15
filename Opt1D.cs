using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections.Immutable;
using System.Drawing.Design;
using static System.Windows.Forms.AxHost;
using System.Runtime.Intrinsics.Arm;

namespace OptNet
{
    internal class Opt1D<T> where T : struct, INumber<T>
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
        [Flags]
        public enum staMaskEnum : UInt32 { Done = 1, Min = 1 << 1, Max = 1 << 2, Bottom = 1 << 3 }
        private UInt32 mState = 0;

        private T[] calcW(in T[] Js) => GeneralMath<T>.LinearTransform(constVars.P.Data, Js, 0, constVars.P.GetLength(1) - 1);
        //private T[] calcOmegaW(in T[] Js) => GeneralMath<T>.LinearTransform(constVars.P, Js);

        public T[] Js => sampleJs;
        public KeyValuePair<T, T> Max { get; }
        public KeyValuePair<T, T> Min { get; }
        public Dictionary<T, T> Lo { get; }

        public Opt1D(in T[] sampleJs, in uint Ns, in T minX, in T maxX, in OfflineGet<T> constvars, in string mode)
        {
            if (mode == "Max") setSysFlag(staMaskEnum.Max);
            else clrSysFlag(staMaskEnum.Max);
            if (mode == "Min") setSysFlag(staMaskEnum.Min);
            else clrSysFlag(staMaskEnum.Min);
            if (mode == "Both") setSysFlag(staMaskEnum.Max | staMaskEnum.Min);

            this.Ns = Ns;
            this.sampleJs = sampleJs;
            this.minX = minX;
            this.maxX = maxX;
            dX = (maxX - minX) / T.CreateChecked((Ns - 1));
            dXHalf = dX / T.CreateChecked(2);

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
            if (isSetSysFlag(staMaskEnum.Max)) Max = Lo.MaxBy(x => x.Value);
            if (isSetSysFlag(staMaskEnum.Min)) Min = Lo.MinBy(x => x.Value);
        }
        public Opt1D(in Func<T, T> j, in uint Ns, in T minX, in T maxX, in OfflineGet<T> constvars, in string mode)
        {
            if (mode == "Max") setSysFlag(staMaskEnum.Max);
            else clrSysFlag(staMaskEnum.Max);
            if (mode == "Min") setSysFlag(staMaskEnum.Min);
            else clrSysFlag(staMaskEnum.Min);
            if (mode == "Both") setSysFlag(staMaskEnum.Max | staMaskEnum.Min);

            this.Ns = Ns;
            this.minX = minX;
            this.maxX = maxX;
            dX = (maxX - minX) / T.CreateChecked((Ns - 1));
            dXHalf = dX / T.CreateChecked(2);
            sampleJs = getSamples(j);

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
            if (isSetSysFlag(staMaskEnum.Max)) Max = Lo.MaxBy(x => x.Value);
            if (isSetSysFlag(staMaskEnum.Min)) Min = Lo.MinBy(x => x.Value);
        }
        private T[] getSamples(in Func<T, T> j)
        {
            var tmpJs = new T[Ns];
            for (uint n = 0; n < Ns; n++)
                tmpJs[n] = j(minX + T.CreateChecked(n) * dX);
            return tmpJs;
        }
        private Dictionary<T, T> getLo()
        {
            var lo = new Dictionary<T, T>();
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
                            lo.Add(constVars.Dp[k], T.Zero);
                    }
                    else // Linear equation
                    {
                        x = -C[k] / B[k];
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add(x, Jw(x));
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
                            lo.Add(x, Jw(x));
                        else
                            continue;
                    }
                    else
                    {
                        x = (-B[k] - sqrtTmpD2) / A[k] / (T.One + T.One);
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add(x, Jw(x));
                        x = (-B[k] + sqrtTmpD2) / A[k] / (T.One + T.One);
                        if (IsInCi(x, k))
                            //lo.Add(x, objectJ.J(x));
                            lo.Add(x, Jw(x));
                    }
                }
            }
            return lo;
        }
        private void calOptCIntv(out int minK, out int maxK)
        {
                minK = 0;
                maxK = (int)constVars.Dp.Length - 1 - 1;
        }
        private bool IsInCi(in T x, in int k)
        {

            if ((x >= constVars.Dp[k]) && (x < constVars.Dp[k + 1]))
                return true;
            else
                return false;
        }
        public void DisplayResult(in Action<string> DisplayMessage)
        {
            string msgTemplate = "Key = {0,8:G5},   Value = {1,8:G5}";
            string s;
            foreach (KeyValuePair<T, T> kv in Lo)
            {
                s = string.Format(msgTemplate, calcRealX(kv.Key), kv.Value);
                DisplayMessage("Lo  " + s);
            }
            if (isSetSysFlag(staMaskEnum.Max))
            {
                s = string.Format(msgTemplate, calcRealX(Max.Key), Max.Value);
                DisplayMessage("Max:  " + s);
            }
            if (isSetSysFlag(staMaskEnum.Min))
            {
                s = string.Format(msgTemplate, calcRealX(Min.Key), Min.Value);
                DisplayMessage("Min:  " + s);
            }
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
