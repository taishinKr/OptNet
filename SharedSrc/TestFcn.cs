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
        /// Computes the Ackley benchmark function for generic numeric types.
        /// </summary>
        /// <typeparam name="T">A numeric type implementing trigonometric, exponential, and root functions.</typeparam>
        /// <param name="xT">The decision variable array (input vector).</param>
        /// <param name="dim">The dimension of the decision variable space.</param>
        /// <returns>The evaluated Ackley function value.</returns>
        public T Ackley(T[] xT, uint dim)
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



        // Test functions for optimization problems, Opt1D ////////////////////
        public T JTest1D(T xT)
        {
            double x = double.CreateChecked(xT);
            return T.CreateChecked(Math.Exp(x) * Math.Sin(5.5 * Math.PI * x));
        }
    }
}