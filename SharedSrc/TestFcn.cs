using System.Numerics;

namespace OptNet
{
    public class TestFcn<T> where T : struct, INumber<T>, ITrigonometricFunctions<T>, IExponentialFunctions<T>, IRootFunctions<T>
    {
        // Test functions for optimization problems, OptMD ////////////////////
        public T JTest1D(T[] xT, uint dim)
        {
            double[] x = new double[1]; ;
            x[0] = double.CreateChecked(xT[0]);
            return T.CreateChecked(Math.Exp(x[0]) * Math.Sin(5.5 * Math.PI * x[0]));
        }
        public T JTest2D(T[] xT, uint dim)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1]))
            );
        }
        public T JTest3D(T[] xT, uint dim)
        {
            double[] x = new double[3];

            for (uint i = 0; i < 3; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                (1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1])))
                * Math.Sin(Math.PI * x[2])
            );
        }

        public T Bird2D(T[] xT, uint dim)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                Math.Pow(x[0] - x[1], 2) + Math.Exp(Math.Pow(1 - Math.Sin(x[0]), 2)) * Math.Cos(x[1]) + Math.Exp(Math.Pow(1 - Math.Cos(x[1]), 2)) * Math.Sin(x[0])
            );
        }

        /// <summary>
        /// Computes the AckleyN1 benchmark function for generic numeric types.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing trigonometric, exponential, and root functions.</typeparam>
        /// <param name="xT">The decision variable array (input vector).</param>
        /// <param name="dim">The dimension of the decision variable space.</param>
        /// <returns>The evaluated AckleyN1 function value.</returns>
        public T AckleyN1(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim == 0)
                return T.Zero;

            T sum1 = T.Zero;
            T sum2 = T.Zero;
            T twoPi = T.CreateChecked(2.0 * Math.PI);

            for (int i = 0; i < dim; i++)
            {
                sum1 += xT[i] * xT[i];
                sum2 += T.Cos(twoPi * xT[i]);
            }

            T d = T.CreateChecked(dim);

            // Term 1: -20 * exp(-0.2 * sqrt( (1/d) * sum1 ))
            // IRootFunctions<T> 덕분에 T.Sqrt 가 정상 작동합니다.
            T term1 = T.CreateChecked(-20.0) * T.Exp(T.CreateChecked(-0.2) * T.Sqrt(sum1 / d));

            // Term 2: -exp( (1/d) * sum2 )
            T term2 = -T.Exp(sum2 / d);

            // Constant: 20 + e
            T constant = T.CreateChecked(20.0) + T.E;

            return term1 + term2 + constant;
        }

        /// <summary>
        /// 1. Griewank Function
        /// Global Minimum: f(x) = 0 at x = [0, 0, ..., 0]
        /// Standard Domain: x_i in [-600, 600]
        /// </summary>
        public T Griewank(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim == 0)
                return T.Zero;

            T sum = T.Zero;
            T prod = T.One;

            for (int i = 0; i < dim; i++)
            {
                // sum += x_i^2 / 4000
                sum += (xT[i] * xT[i]) / T.CreateChecked(4000.0);

                // prod *= cos(x_i / sqrt(i + 1))
                T index = T.CreateChecked(i + 1);
                T term = xT[i] / T.Sqrt(index);
                prod *= T.Cos(term);
            }

            // f(x) = sum - prod + 1
            return sum - prod + T.One;
        }

        /// <summary>
        /// 2. Rastrigin Function
        /// Global Minimum: f(x) = 0 at x = [0, 0, ..., 0]
        /// Standard Domain: x_i in [-5.12, 5.12]
        /// </summary>
        public T Rastrigin(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim == 0)
                return T.Zero;

            T sum = T.Zero;
            T ten = T.CreateChecked(10.0);
            T twoPi = T.CreateChecked(2.0 * Math.PI);

            for (int i = 0; i < dim; i++)
            {
                // sum += x_i^2 - 10 * cos(2 * pi * x_i)
                sum += (xT[i] * xT[i]) - (ten * T.Cos(twoPi * xT[i]));
            }

            T d = T.CreateChecked(dim);

            // f(x) = 10 * d + sum
            return (ten * d) + sum;
        }

        /// <summary>
        /// 3. Schwefel Function
        /// Global Minimum: f(x) = 0 at x = [420.9687, 420.9687, ..., 420.9687]
        /// Standard Domain: x_i in [-500, 500]
        /// </summary>
        public T Schwefel(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim == 0)
                return T.Zero;

            T sum = T.Zero;
            T c418 = T.CreateChecked(418.9829);

            for (int i = 0; i < dim; i++)
            {
                // sum += x_i * sin(sqrt(|x_i|))
                T absX = T.Abs(xT[i]);
                sum += xT[i] * T.Sin(T.Sqrt(absX));
            }

            T d = T.CreateChecked(dim);

            // f(x) = 418.9829 * d - sum
            return (c418 * d) - sum;
        }

        // Test functions for optimization problems, Opt1D ////////////////////
        public T JTest1D(T xT)
        {
            double x = double.CreateChecked(xT);
            return T.CreateChecked(Math.Exp(x) * Math.Sin(5.5 * Math.PI * x));
        }
    }
}