using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OptNet
{
    internal class OfflineCalc<T> where T : struct, INumber<T>
    {
        private readonly uint M;
        private readonly uint tildM;
        private readonly uint Ns;
        //private readonly T[] Dp;
        private readonly T[,] Q;
        private readonly T[,] Rw;
        private readonly T[,] A;
        private readonly T[,] B;
        private readonly T[,] C;
        
        public T[,] P { get; }
        public T[,] Ra { get; }
        public T[,] Rb { get; }
        public T[,] Rc { get; }
        public T[] Dp { get; }
        public OfflineCalc(uint M, uint tildM)
        {
            this.M = M;
            this.tildM = tildM;
            this.Ns = M + 1;
            Dp = calcDp();
            P = calcP();
            Q = calcQ();
            Rw = calcRw();
            A = calcABC(anx);
            B = calcABC(bnx);
            C = calcABC(cnx);
            Ra = GeneralMath<T>.MultiplyMatrices(A, Rw);
            Rb = GeneralMath<T>.MultiplyMatrices(B, Rw);
            Rc = GeneralMath<T>.MultiplyMatrices(C, Rw);
        }

        private T[] calcDp()
        {
            T[] dp = new T[M + 1];
            T tmpoffset = T.CreateChecked(1.0 / 2.0 / Ns);
            var dpSet = new SortedSet<T> { tmpoffset, T.One - tmpoffset };
            for (uint n = 1; n <= M; n++)
                for (uint k = 1; k <= n - 1; k++)
                    dpSet.Add(T.CreateChecked(k / (double)n));
            dp = dpSet.ToArray();
            return dp;
        }
        private T[,] calcP()
        {
            var p = new T[M + 1, M + 1];
            for (uint i = 1; i <= M + 1; i++)
                for (uint j = 1; j <= M + 1; j++)
                {
                    if (i == 1)
                        p[i-1, j-1] = T.One / (T.CreateChecked(M) + T.One); // Replace T.One with T.CreateChecked(1)
                    else
                        p[i-1, j-1] = T.CreateChecked(2.0 / (double)(M + 1) * Math.Cos((i-1) * Math.PI * ( (j-0.5) / (M + 1.0))));
                }
            return p;
        }
        private T Qmn(uint m, uint n)
        {
            if (n % m != 0)
                return T.CreateChecked(0);
            else
            {
                uint div = n / m;

                if (div % 2 == 0)
                    return T.CreateChecked(0);
                else
                    return T.CreateChecked(32 / Math.Pow(Math.PI * div, 3));
            }
        }
        private T[,] calcQ()
        {
            var q = new T[M, tildM];
            for (uint i = 0; i < M; i++)
                for (uint j = 0; j < tildM; j++)
                {
                    q[i, j] = Qmn(i + 1, j + 1);
                }
            return q;
        }
        private T[,] calcRw()
        {
            var Qa = GeneralMath<T>.ExtractSubarray(Q, 0, 0, M, M);
            var Ka = new T[M, M];
            for (int i = 1; i <= M; i++)
                for (int j = 1; j <= M; j++)
                {
                    if (i==j)
                        Ka[i - 1, j - 1] = T.CreateChecked(-i * Math.PI);
                    else
                        Ka[i - 1, j - 1] = T.Zero;
                }
            var Pbar = GeneralMath<T>.ExtractSubarray(P, 1, 0, M, M+1);

            var tmpQt = GeneralMath<T>.TransposeMatrix(Q);
            var tmpQQt = GeneralMath<T>.MultiplyMatrices(Q, tmpQt);
            var tmpInvQQt = GeneralMath<T>.InvertMatrix(tmpQQt);
            var tmpInvQQtQa = GeneralMath<T>.MultiplyMatrices(tmpInvQQt, Qa);
            var tmpInvQQtQaKa = GeneralMath<T>.MultiplyMatrices(tmpInvQQtQa, Ka);
            var tmpInvQQtQaKaPbar = GeneralMath<T>.MultiplyMatrices(tmpInvQQtQaKa, Pbar);
            return tmpInvQQtQaKaPbar;
        }
        private uint kK(uint n, T x) => (uint)Math.Floor(n * double.CreateChecked(x));
        private T anx(uint n, T x)
        {
            uint k = kK(n, x);
            if (k % 2 == 0)
                return T.CreateChecked(-4 * n * n);
            else
                return T.CreateChecked(4 * n * n);
        }
        private T bnx(uint n, T x)
        {
            uint k = kK(n, x);
            if (k % 2 == 0)
                return T.CreateChecked(4 * n * (2 * k + 1));
            else
                return T.CreateChecked(-4 * n * (2 * k + 1));
        }
        private T cnx(uint n, T x)
        {
            uint k = kK(n, x);
            if (k % 2 == 0)
                return T.CreateChecked(-4 * k * (k + 1));
            else
                return T.CreateChecked(4 * k * (k + 1));
        }
        private T[,] calcABC(Func<uint, T, T> abc)
        {
            var ABC = new T[Dp.Length - 1,M];
            for (uint n = 1; n <= M ; n++)
                for (uint k = 1; k <= Dp.Length - 1; k++)
                {
                    ABC[k-1, n - 1] = abc(n, Dp[k - 1]);
                }
            return ABC;            
        }
        //var tmpAaRw=GeneralMath<T>.MultiplyMatrices(Aa,Rw);
        
    }
}
