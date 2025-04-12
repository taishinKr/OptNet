using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections.Immutable;
using System.Drawing.Design;

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
        private readonly Dictionary<T, T> Lo;

        private readonly ObjectJClass<T> objectJ;
        //private readonly ImmutableArray<T> sampleJs;
        private readonly T[] sampleJs;
        private readonly OfflineCalc<T> offlineCalc;

        private T[] calcW(T[] Js) => GeneralMath<T>.LinearTransform(offlineCalc.P, Js);
        private T[] calcOmegaW(T[] Js) => GeneralMath<T>.LinearTransform(offlineCalc.P, Js);


        public uint OrderM => M;
        public uint NumOfSample => Ns;
        //public ImmutableArray<T> Js => sampleJs;
        public T[] Js => sampleJs;
        public ObjectJClass<T> ObjectJ => objectJ;
        public KeyValuePair<T, T> Max { get; }
        public KeyValuePair<T, T> Min { get; }

        public Opt1D(ObjectJClass<T> argObjectJ)
        {
            objectJ = argObjectJ;
            Ns = (uint)objectJ.Js.Length;
            M = Ns - 1;
            tildM = M * 3;
            sampleJs = objectJ.Js;

            offlineCalc = new OfflineCalc<T>(M, tildM);

            W = calcW(Js);
            omegaW = calcOmegaW(Js);

            A = GeneralMath<T>.LinearTransform(offlineCalc.Ra, Js);
            B = GeneralMath<T>.LinearTransform(offlineCalc.Rb, Js);
            C = GeneralMath<T>.LinearTransform(offlineCalc.Rc, Js);

            Lo = getLo();
            Max = Lo.MaxBy(x => x.Value);
            Min = Lo.MinBy(x => x.Value);
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
            if ((x >= offlineCalc.Dp[k]) && (x < offlineCalc.Dp[k + 1]))
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
    }
}
