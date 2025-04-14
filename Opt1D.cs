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

namespace OptNet
{
    internal class ObjectJClass<T> where T : struct, INumber<T>
    {
        private readonly T minX;
        private readonly T maxX;
        private readonly T dX;
        private readonly T dXHalf;
        private readonly uint Ns;
        //private ImmutableArray<T> sampleJs;
        private readonly T[] sampleJs;


        public T MinX => minX;
        public T MaxX => maxX;
        //public ImmutableArray<T> Js => sampleJs;
        public T[] Js => sampleJs;
        public Func<T, T> J;
        public ObjectJClass(Func<T, T> j, uint Ns, T minX, T maxX)
        {
            this.Ns = Ns;
            this.minX = minX;
            this.maxX = maxX;
            dX = (maxX - minX) / T.CreateChecked((Ns - 1));
            dXHalf = dX / T.CreateChecked(2);
            sampleJs = getSamples(j);
            J = j;
        }
        private T[] getSamples(Func<T, T> j)
        {
            var tmpJs = new T[Ns];
            for (uint n = 0; n < Ns; n++)
                tmpJs[n] = j(minX + T.CreateChecked(n) * dX);
            return tmpJs;
        }
        public T calcRealX(T projX)
        {
            return (maxX - minX + dX) * projX + minX - dXHalf;
        }
        public T calcProjX(T realX) => (realX - minX + dXHalf) / (maxX - minX + dX);

        static public double JTest(double x)
        {
            return Math.Exp(x) * Math.Sin(5.5 * Math.PI * x);
        }
    }

    internal class Opt1D<T> where T : struct, INumber<T>
    {
        private readonly uint M, tildM;
        private readonly uint Ns;
        private readonly T[] W;
        private readonly T[] omegaW;
        private readonly T[] A;
        private readonly T[] B;
        private readonly T[] C;        

        private readonly ObjectJClass<T> objectJ;
        //private readonly ImmutableArray<T> sampleJs;
        private readonly T[] sampleJs;
        private readonly OfflineGet<T> constVars;

        [Flags]
        public enum staMaskEnum : UInt32 { Done = 1, Min = 1 << 1, Max = 1 << 2, Bottom = 1 << 3 }
        private UInt32 mState = 0;

        private T[] calcW(T[] Js) => GeneralMath<T>.LinearTransform(constVars.P, Js);
        private T[] calcOmegaW(T[] Js) => GeneralMath<T>.LinearTransform(constVars.P, Js);


        public uint OrderM => M;
        public uint NumOfSample => Ns;
        //public ImmutableArray<T> Js => sampleJs;
        public T[] Js => sampleJs;
        public ObjectJClass<T> ObjectJ => objectJ;
        public KeyValuePair<T, T> Max { get; }
        public KeyValuePair<T, T> Min { get; }
        public Dictionary<T, T> Lo { get; }

        public Opt1D(ObjectJClass<T> argObjectJ, string mode)
        {
            if (mode == "Max") setSysFlag(staMaskEnum.Max);
            else clrSysFlag(staMaskEnum.Max);
            if (mode == "Min") setSysFlag(staMaskEnum.Min);
            else clrSysFlag(staMaskEnum.Min);
            if (mode == "Both") setSysFlag(staMaskEnum.Max | staMaskEnum.Min);

            objectJ = argObjectJ;
            Ns = (uint)objectJ.Js.Length;
            M = Ns - 1;
            tildM = M * 3;
            sampleJs = objectJ.Js;

            constVars = new OfflineGet<T>(M, tildM);

            W = calcW(Js);
            omegaW = calcOmegaW(Js);

            A = GeneralMath<T>.LinearTransform(constVars.Ra, Js);
            B = GeneralMath<T>.LinearTransform(constVars.Rb, Js);
            C = GeneralMath<T>.LinearTransform(constVars.Rc, Js);

            Lo = getLo();
            if (isSetSysFlag(staMaskEnum.Max)) Max = Lo.MaxBy(x => x.Value);
            if (isSetSysFlag(staMaskEnum.Min)) Min = Lo.MinBy(x => x.Value);
        }

        public void DisplayResult(Action<string> DisplayMessage)
        {
            string msgTemplate = "Key = {0,8:G5},   Value = {1,8:G5}";
            string s;
            foreach (KeyValuePair<T, T> kv in Lo)
            {
                s = string.Format(msgTemplate, kv.Key, kv.Value);
                DisplayMessage("Lo  " + s);
            }
            if (isSetSysFlag(staMaskEnum.Max))
            {
                s = string.Format(msgTemplate, Max.Key, Max.Value);
                DisplayMessage("Max:  " + s);
            }
            if (isSetSysFlag(staMaskEnum.Min))
            {
                s = string.Format(msgTemplate, Min.Key, Min.Value);
                DisplayMessage("Min:  " + s);
            }             
        }
        private Dictionary<T, T> getLo()
        {
            var lo = new Dictionary<T, T>();
            for (uint k = 0; k < A.Length; k++)
            {
                T tmpD2 = B[k] * B[k] - A[k] * C[k] * (T.One + T.One + T.One + T.One);
                T sqrtTmpD2 = T.CreateChecked(Math.Sqrt(double.CreateChecked(tmpD2)));
                T x;
                if(sqrtTmpD2 == T.Zero)
                {
                    x = -B[k] / A[k] / (T.One + T.One);
                    if (IsInCi(x, k))
                        lo.Add(x, objectJ.J(objectJ.calcRealX(x)));
                        //lo.Add(x, Jw(x));
                    else
                        continue;
                }
                else
                {
                    x = (-B[k] - sqrtTmpD2) / A[k] / (T.One + T.One);
                    if (IsInCi(x, k))
                        lo.Add(x, objectJ.J(objectJ.calcRealX(x)));
                        //lo.Add(x, Jw(x));
                    x = (-B[k] + sqrtTmpD2) / A[k] / (T.One + T.One);
                    if (IsInCi(x, k))
                        lo.Add(x, objectJ.J(objectJ.calcRealX(x)));
                        //lo.Add(x, Jw(x));
                }
            }
            return lo;
        }

        private bool IsInCi(T x, uint k)
        {
            if ((x >= constVars.Dp[k]) && (x < constVars.Dp[k + 1]))
                return true;
            else
                return false;
        }

        public T Jw(T x)
        {
            T sum = T.Zero;
            for (uint n = 0; n <= M; n++)
                sum += W[n] * T.CreateChecked( Math.Cos(n * Math.PI * double.CreateChecked(x))); // Convert T to double    
            return sum;
        }

        public void setSysFlag(staMaskEnum mask)
        {
            mState = setFlag(mState, (UInt32)mask);

        }
        public void clrSysFlag(staMaskEnum mask)
        {
            mState = clrFlag(mState, (UInt32)mask);
        }
        public bool isSetSysFlag(staMaskEnum mask)
        {
            return isSetFlag(mState, (UInt32)mask);
        }
        private UInt32 setFlag(UInt32 Flag, UInt32 mask)
        {
            Flag |= mask;
            return Flag;
        }
        private UInt32 clrFlag(UInt32 Flag, UInt32 mask)
        {
            Flag &= (UInt32)(~mask);
            return Flag;
        }
        private bool isSetFlag(UInt32 Flag, UInt32 mask)
        {
            if ((Flag & mask) == 0)
                return false;
            else
                return true;
        }
    }
}
