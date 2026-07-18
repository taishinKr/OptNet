using System.Numerics;

namespace OptNet
{
    public class Opt1D<T> where T : struct, INumber<T>
    {
        private readonly T[] W;
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

        [Flags]
        public enum StaMaskEnum : UInt32 { Done = 1, Min = 1 << 1, Max = 1 << 2, Bottom = 1 << 3, Verbose = 1 << 4 }
        private UInt32 mState = 0;

        /// <summary>
        /// Calculates the weight vector W based on the provided Js vector and the constant variables.
        /// </summary>
        /// <param name="Js"></param>
        /// <returns></returns>
        private T[] CalcW(in T[] Js) => GeneralMath<T>.LinearTransform(constVars.P.Data, Js, 0, constVars.P.GetLength(1) - 1);

        public T[] Js => sampleJs;
        /// <summary>
        /// Gets the list of optimal (X, J) pairs based on the specified optimization criteria (Max or Min) and the provided tolerance for J values.
        /// </summary>
        public (T X, T J)[] Optlist { get; } = [];
        public List<(T X, T J)> Lo { get; }

        /// <summary>
        /// Initializes a new instance of the Opt1D class with the specified parameters, including sampleJs, tolerance for J values, number of samples, minimum and maximum X values, constant variables, optimization mode, and optional optimization interval.
        /// </summary>
        /// <param name="sampleJs"></param>
        /// <param name="TolJ"></param>
        /// <param name="Ns"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="constvars"></param>
        /// <param name="mode"></param>
        /// <param name="optIntv"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public Opt1D(in T[] sampleJs, double TolJ, in uint Ns, in T minX, in T maxX, in OfflineGet<T> constvars, in StaMaskEnum mode, double[]? optIntv)
        {
            ClrSysFlag(StaMaskEnum.Max);
            ClrSysFlag(StaMaskEnum.Min);
            var modetmp = mode;
            if (mode == (StaMaskEnum.Max | StaMaskEnum.Min))
                modetmp = StaMaskEnum.Max;
            SetSysFlag(modetmp);

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

            W = CalcW(Js);
            //omegaW = calcOmegaW(Js);

            CalOptCIntv(out minDpK, out maxDpK);
            A = GeneralMath<T>.LinearTransform(constVars.Ra.Data, Js, (int)minDpK, (int)maxDpK);
            B = GeneralMath<T>.LinearTransform(constVars.Rb.Data, Js, (int)minDpK, (int)maxDpK);
            C = GeneralMath<T>.LinearTransform(constVars.Rc.Data, Js, (int)minDpK, (int)maxDpK);

            Lo = GetLo();
            Lo.Add((T.Zero, Jw(T.Zero)));
            Lo.Add((T.One, Jw(T.One)));

            var maxJ = Lo.Max(item => item.J);
            var minJ = Lo.Min(item => item.J);
            var spanJ = maxJ - minJ;

            //if ( double.CreateChecked(spanJ) < 0.00001)
            if (spanJ == T.Zero)
                throw new InvalidOperationException("The span of J values is zero, countinuous solution!");

            if (IsSetSysFlag(StaMaskEnum.Max))
            {
                //Optlist = [.. Lo.Where(item => Math.Abs(double.CreateChecked((item.J - maxJ) / spanJ)) <= TolJ)]; //.ToArray();
                Optlist = [.. Lo.Where(item => Math.Abs(double.CreateChecked(item.J - maxJ)) <= TolJ)]; //.ToArray();
            }
            if (IsSetSysFlag(StaMaskEnum.Min))
            {
                //Optlist = [.. Lo.Where(item => Math.Abs(double.CreateChecked((item.J - minJ) / spanJ)) <= TolJ)]; //.ToArray();
                Optlist = [.. Lo.Where(item => Math.Abs(double.CreateChecked(item.J - minJ)) <= TolJ)]; //.ToArray();
            }
        }
        /// <summary>
        /// Initializes a new instance of the Opt1D class with the specified parameters, including a function j, tolerance for J values, number of samples, minimum and maximum X values, constant variables, optimization mode, and optional optimization interval.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="TolJ"></param>
        /// <param name="Ns"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="constvars"></param>
        /// <param name="mode"></param>
        /// <param name="optIntv"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public Opt1D(in Func<T, T> j, double TolJ, in uint Ns, in T minX, in T maxX, in OfflineGet<T> constvars, in StaMaskEnum mode, double[]? optIntv)
        {
            ClrSysFlag(StaMaskEnum.Max);
            ClrSysFlag(StaMaskEnum.Min);
            SetSysFlag(mode);

            this.Ns = Ns;
            this.minX = minX;
            this.maxX = maxX;
            dX = (maxX - minX) / T.CreateChecked((Ns - 1));
            dXHalf = dX / T.CreateChecked(2);

            this.optIntv = optIntv;

            sampleJs = GetSamples(j);

            if (constvars == null)
                constVars = new OfflineGet<T>(Ns);
            else
                constVars = constvars;

            W = CalcW(Js);
            //omegaW = calcOmegaW(Js);

            CalOptCIntv(out minDpK, out maxDpK);
            A = GeneralMath<T>.LinearTransform(constVars.Ra.Data, Js, minDpK, maxDpK);
            B = GeneralMath<T>.LinearTransform(constVars.Rb.Data, Js, minDpK, maxDpK);
            C = GeneralMath<T>.LinearTransform(constVars.Rc.Data, Js, minDpK, maxDpK);

            Lo = GetLo();
            Lo.Add((T.Zero, Jw(T.Zero)));
            Lo.Add((T.One, Jw(T.One)));

            var maxJ = Lo.Max(item => item.J);
            var minJ = Lo.Min(item => item.J);
            var spanJ = maxJ - minJ;

            //if ( double.CreateChecked(spanJ) < 0.00001)
            if (spanJ == T.Zero)
                throw new InvalidOperationException("The span of J values is zero, countinuous solution!");

            if (IsSetSysFlag(StaMaskEnum.Max))
            {
                Optlist = [.. Lo.Where(item => Math.Abs(double.CreateChecked((item.J - maxJ) / spanJ)) <= TolJ)]; //.ToArray();
            }
            if (IsSetSysFlag(StaMaskEnum.Min))
            {
                Optlist = [.. Lo.Where(item => Math.Abs(double.CreateChecked((item.J - minJ) / spanJ)) <= TolJ)]; //.ToArray();
            }
        }
        /// <summary>
        /// Generates an array of samples by applying the provided function j to a range of values between minX and maxX, divided into Ns intervals.
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        private T[] GetSamples(in Func<T, T> j)
        {
            var tmpJs = new T[Ns];
            for (uint n = 0; n < Ns; n++)
                tmpJs[n] = j(minX + T.CreateChecked(n) * dX);
            return tmpJs;
        }
        /// <summary>
        /// Calculates the list of local optima (Lo) by solving quadratic equations derived from the coefficients A, B, and C for each interval defined by the constant variables. The method checks if the solutions are within the valid range and adds them to the list along with their corresponding J values.
        /// </summary>
        /// <returns></returns>
        private List<(T X, T J)> GetLo()
        {
            var lo = new List<(T X, T J)>();
            for (int k = 0; k < A.Length; k++)
            {
                T tmpD2 = B[k] * B[k] - A[k] * C[k] * (T.One + T.One + T.One + T.One);

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
                    if (tmpD2 < T.Zero)
                        continue;
                    T sqrtTmpD2 = T.CreateChecked(Math.Sqrt(double.CreateChecked(tmpD2)));

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
        /// <summary>
        /// Calculates the minimum and maximum indices (minK and maxK) for the optimization interval based on the provided optIntv array. The method checks if the optIntv is not null and determines the corresponding indices in the constant variables' Dp array. If optIntv is null, it sets minK to 0 and maxK to the last index of the Dp array minus one.
        /// </summary>
        /// <param name="minK"></param>
        /// <param name="maxK"></param>
        private void CalOptCIntv(out int minK, out int maxK)
        {
            minK = 0;
            maxK = (int)constVars.Dp.Length - 1 - 1;
            if (optIntv != null) // Ensure optIdx is not null
            {
                var xmin = T.CreateChecked(optIntv[0]);
                var xmax = T.CreateChecked(optIntv[1]);
                var dpsize = constVars.Dp.Length;
                for (int i = 0; (i + 1) < dpsize; i++)
                {
                    if ((constVars.Dp[i] <= xmin) && (xmin < constVars.Dp[i + 1]))
                    {
                        minK = i;
                    }
                    if ((constVars.Dp[i] <= xmax) && (xmax < constVars.Dp[i + 1]))
                    {
                        maxK = i + 1;
                        if (maxK > ((int)constVars.Dp.Length - 1 - 1))
                            maxK = (int)constVars.Dp.Length - 1 - 1;
                        break;
                    }
                }
            }
            /*else
            {
                minK = 0;
                maxK = (int)constVars.Dp.Length - 1 - 1;
            }*/
        }
        /// <summary>
        /// Checks if the given value x is within the interval defined by the k-th and (k+1)-th elements of the constant variables' Dp array. Returns true if x is within the interval, otherwise returns false.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        private bool IsInCi(in T x, in int k)
        {

            if ((x >= constVars.Dp[k]) && (x < constVars.Dp[k + 1]))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Generates a formatted string representation of the optimization results, including the local optima (Lo) and the optimal values (Optlist) based on the specified optimization criteria (Max or Min). The method constructs a message template and iterates through the Lo and Optlist to create a string that displays the key-value pairs of X and J values. The resulting string is returned for display or logging purposes.
        /// </summary>
        /// <returns></returns>
        public string ResultMsg()
        {
            string msgTemplate = "Key = {0,8:G5},   Value = {1,8:G5}";
            string s = string.Empty;

            foreach ((T X, T J) in Lo)
            {
                s += "Lo  " + string.Format(msgTemplate, CalcRealX(X), J) + Environment.NewLine;
            }
            if (IsSetSysFlag(StaMaskEnum.Max))
            {
                foreach (var (X, J) in Optlist)
                {
                    s += "Max:  " + string.Format(msgTemplate, CalcRealX(X), J) + Environment.NewLine;
                }
            }
            if (IsSetSysFlag(StaMaskEnum.Min))
            {
                foreach (var (X, J) in Optlist)
                {
                    s += "Min:  " + string.Format(msgTemplate, CalcRealX(X), J) + Environment.NewLine;
                }
            }
            return s;
        }
        /// <summary>
        /// Calculates the value of the function Jw at a given point x using the weight vector W and the cosine function. The method computes the sum of the products of the weights and the cosine of n times pi times x for each index n from 0 to Ns-1. The result is returned as a value of type T.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public T Jw(in T x)
        {
            T sum = T.Zero;
            for (uint n = 0; n <= Ns - 1; n++)
                sum += W[n] * T.CreateChecked(Math.Cos(n * Math.PI * double.CreateChecked(x))); // Convert T to double    
            return sum;
        }
        /// <summary>
        /// Calculates the real-world x-coordinate from the projected x-coordinate.
        /// </summary>
        /// <param name="projX"></param>
        /// <returns></returns>
        public T CalcRealX(in T projX) => (maxX - minX + dX) * projX + minX - dXHalf;
        /// <summary>
        /// Calculates the projected x-coordinate from the real-world x-coordinate.
        /// </summary>
        /// <param name="realX"></param>
        /// <returns></returns>
        public T CalcProjX(in T realX) => (realX - minX + dXHalf) / (maxX - minX + dX);

        /// <summary>
        /// Sets the system flag for the optimization process based on the provided StaMaskEnum value. The method updates the internal state variable mState by setting the corresponding bit for the specified mask. This allows for tracking and controlling various optimization modes, such as Max, Min, Done, Bottom, and Verbose.
        /// </summary>
        /// <param name="mask"></param>
        public void SetSysFlag(in StaMaskEnum mask)
        {
            mState = GeneralMath<T>.SetFlag(ref mState, (UInt32)mask);

        }
        /// <summary>
        /// Clears the system flag for the optimization process based on the provided StaMaskEnum value. The method updates the internal state variable mState by clearing the corresponding bit for the specified mask. This allows for resetting or disabling specific optimization modes, such as Max, Min, Done, Bottom, and Verbose.
        /// </summary>
        /// <param name="mask"></param>
        public void ClrSysFlag(in StaMaskEnum mask)
        {
            mState = GeneralMath<T>.ClrFlag(ref mState, (UInt32)mask);
        }
        /// <summary>
        /// Checks if the specified system flag is set in the optimization process. The method evaluates the internal state variable mState to determine if the corresponding bit for the provided StaMaskEnum value is set. It returns true if the flag is set, indicating that the associated optimization mode is active; otherwise, it returns false.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public bool IsSetSysFlag(in StaMaskEnum mask)
        {
            return GeneralMath<T>.IsSetFlag(mState, (UInt32)mask);
        }

    }
}
