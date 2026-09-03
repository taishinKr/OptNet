using System.Numerics;

namespace OptNet
{
    public class TestFcn<T> where T : struct, INumber<T>, ITrigonometricFunctions<T>, IExponentialFunctions<T>, IRootFunctions<T>, IPowerFunctions<T>
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

        //================================
        // 1.Main benchmark functions for optimization problems, OptMD ////////////////////
        //================================

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
            T c418 = T.CreateChecked(418.9828872724338);

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

        //=============================
        // 2.continuous, differentiable, scalable, multimodal
        //=============================

        /// <summary>
        /// 1. Alpine No. 1 Function
        /// Global Minimum: f(x) = 0 at x = [0, 0, ..., 0]
        /// Standard Domain: x_i in [-10, 10]
        /// </summary>
        public T Alpine(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim == 0)
                return T.Zero;

            T sum = T.Zero;

            for (int i = 0; i < dim; i++)
            {
                // sum += | x_i * sin(x_i) + 0.1 * x_i |
                T term = (xT[i] * T.Sin(xT[i])) + (T.CreateChecked(0.1) * xT[i]);
                sum += T.Abs(term);
            }

            return sum;
        }

        /// <summary>
        /// 2. Expanded Schaffer's F6 Function
        /// Global Minimum: f(x) = 0 at x = [0, 0, ..., 0]
        /// Standard Domain: x_i in [-100, 100]
        /// </summary>
        public T ExpandedSchafferF6(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim <= 1)
                return T.Zero;

            T sum = T.Zero;
            T zeroPointFive = T.CreateChecked(0.5);
            T pointZeroZeroOne = T.CreateChecked(0.001);

            // 2차원 Schaffer F6 구조를 인접 원소 쌍(x_i, x_{i+1}) 및 (x_{dim-1}, x_0)에 대해 확장 연산
            for (int i = 0; i < dim; i++)
            {
                T x1 = xT[i];
                T x2 = xT[(i + 1) % dim]; // Cyclic expansion

                T temp = (x1 * x1) + (x2 * x2);
                T sinVal = T.Sin(T.Sqrt(temp));
                T num = (sinVal * sinVal) - zeroPointFive;
                T denom = T.One + (pointZeroZeroOne * temp);
                T denomSq = denom * denom;

                sum += zeroPointFive + (num / denomSq);
            }

            return sum;
        }

        /// <summary>
        /// 3. Salomon's Function
        /// Global Minimum: f(x) = 0 at x = [0, 0, ..., 0]
        /// Standard Domain: x_i in [-100, 100]
        /// </summary>
        public T Salomons(T[] xT, uint dim)
        {
            if (xT == null || xT.Length < dim)
                throw new ArgumentException("Input array length must be greater than or equal to the specified dimension.");

            if (dim == 0)
                return T.Zero;

            T sumSq = T.Zero;
            for (int i = 0; i < dim; i++)
            {
                sumSq += xT[i] * xT[i];
            }

            T r = T.Sqrt(sumSq); // Euclidean distance from origin
            T twoPi = T.CreateChecked(2.0 * Math.PI);

            // f(x) = 1 - cos(2 * pi * r) + 0.1 * r
            return T.One - T.Cos(twoPi * r) + (T.CreateChecked(0.1) * r);
        }

        // ==========================================
        // 3. Fixed 2D Functions (not scalable)
        // ==========================================

        /// <summary>
        /// 1. Beale's Function (Domain: [-4.5, 4.5], Min: f(3, 0.5) = 0)
        /// </summary>
        public T Beale(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T t1 = T.CreateChecked(1.5) - x + (x * y);
            T t2 = T.CreateChecked(2.25) - x + (x * y * y);
            T t3 = T.CreateChecked(2.625) - x + (x * y * y * y);
            return (t1 * t1) + (t2 * t2) + (t3 * t3);
        }

        /// <summary>
        /// 2. Booth's Function (Domain: [-10, 10], Min: f(1, 3) = 0)
        /// </summary>
        public T Booth(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T t1 = x + (T.CreateChecked(2.0) * y) - T.CreateChecked(7.0);
            T t2 = (T.CreateChecked(2.0) * x) + y - T.CreateChecked(5.0);
            return (t1 * t1) + (t2 * t2);
        }

        /// <summary>
        /// 3. Cross-in-Tray Function (Domain: [-10, 10], Min: f(±1.3494, ±1.3494) = -2.06261)
        /// </summary>
        public T CrossInTray(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T expTerm = T.Exp(T.Abs(T.CreateChecked(100.0) - (T.Sqrt((x * x) + (y * y)) / T.CreateChecked(Math.PI))));
            T sinTerm = T.Sin(x) * T.Sin(y);
            return -T.CreateChecked(0.0001) * T.Pow(T.Abs(sinTerm * expTerm) + T.One, T.CreateChecked(0.1));

        }

        /// <summary>
        /// 4. Drop-Wave Function (Domain: [-5.12, 5.12], Min: f(0, 0) = -1)
        /// </summary>
        public T DropWave(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T r2 = (x * x) + (y * y);
            T num = T.One + T.Cos(T.CreateChecked(12.0) * T.Sqrt(r2));
            T denom = (T.CreateChecked(0.5) * r2) + T.CreateChecked(2.0);
            return -num / denom;
        }

        /// <summary>
        /// 5. Easom Function (Domain: [-100, 100], Min: f(pi, pi) = -1)
        /// </summary>
        public T Easom(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T pi = T.CreateChecked(Math.PI);
            T expTerm = -((x - pi) * (x - pi)) - ((y - pi) * (y - pi));
            return -T.Cos(x) * T.Cos(y) * T.Exp(expTerm);
        }

        /// <summary>
        /// 6. Goldstein-Price Function (Domain: [-2, 2], Min: f(0, -1) = 3)
        /// </summary>
        public T GoldsteinPrice(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T t1 = x + y + T.One;
            T part1 = T.One + ((t1 * t1) * (T.CreateChecked(19.0) - (T.CreateChecked(14.0) * x) + (T.CreateChecked(3.0) * x * x) - (T.CreateChecked(14.0) * y) + (T.CreateChecked(6.0) * x * y) + (T.CreateChecked(3.0) * y * y)));

            T t2 = (T.CreateChecked(2.0) * x) - (T.CreateChecked(3.0) * y);
            T part2 = T.CreateChecked(30.0) + ((t2 * t2) * (T.CreateChecked(18.0) - (T.CreateChecked(32.0) * x) + (T.CreateChecked(12.0) * x * x) + (T.CreateChecked(48.0) * y) - (T.CreateChecked(36.0) * x * y) + (T.CreateChecked(27.0) * y * y)));

            return part1 * part2;
        }

        /// <summary>
        /// 7. Himmelblau's Function (Domain: [-5, 5], Min: f(x*) = 0)
        /// </summary>
        public T Himmelblau(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T t1 = (x * x) + y - T.CreateChecked(11.0);
            T t2 = x + (y * y) - T.CreateChecked(7.0);
            return (t1 * t1) + (t2 * t2);
        }

        /// <summary>
        /// 8. Holder Table Function (Domain: [-10, 10], Min: f(±8.05502, ±9.66459) = -19.2085)
        /// </summary>
        public T HolderTable(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T pi = T.CreateChecked(Math.PI);
            T expTerm = T.Abs(T.One - (T.Sqrt((x * x) + (y * y)) / pi));
            return -T.Abs(T.Sin(x) * T.Cos(y) * T.Exp(expTerm));
        }

        /// <summary>
        /// 9. Levi N.13 Function (Domain: [-10, 10], Min: f(1, 1) = 0)
        /// </summary>
        public T LeviN13(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T pi3 = T.CreateChecked(3.0 * Math.PI);
            T pi2 = T.CreateChecked(2.0 * Math.PI);

            T term1 = T.Sin(pi3 * x) * T.Sin(pi3 * x);
            T term2 = (x - T.One) * (x - T.One) * (T.One + (T.Sin(pi3 * y) * T.Sin(pi3 * y)));
            T term3 = (y - T.One) * (y - T.One) * (T.One + (T.Sin(pi2 * y) * T.Sin(pi2 * y)));

            return term1 + term2 + term3;
        }

        /// <summary>
        /// 10. Matyas Function (Domain: [-10, 10], Min: f(0, 0) = 0)
        /// </summary>
        public T Matyas(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            return (T.CreateChecked(0.26) * ((x * x) + (y * y))) - (T.CreateChecked(0.48) * x * y);
        }

        /// <summary>
        /// 11. Schaffer N. 2 Function (Domain: [-100, 100], Min: f(0, 0) = 0)
        /// </summary>
        public T SchafferN2(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T sinTerm = T.Sin((x * x) - (y * y));
            T num = (sinTerm * sinTerm) - T.CreateChecked(0.5);
            T denom = T.One + (T.CreateChecked(0.001) * ((x * x) + (y * y)));
            return T.CreateChecked(0.5) + (num / (denom * denom));
        }

        /// <summary>
        /// 12. Three-hump Camel Function (Domain: [-5, 5], Min: f(0, 0) = 0)
        /// </summary>
        public T ThreeHumpCamel(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T x2 = x * x, x4 = x2 * x2, x6 = x4 * x2;
            return (T.CreateChecked(2.0) * x2) - (T.CreateChecked(1.05) * x4) + (x6 / T.CreateChecked(6.0)) + (x * y) + (y * y);
        }

        // ==========================================
        // 4. Scalable but not differentiable, or not multimodal
        // ==========================================

        /// <summary>
        /// 13. Expanded Zakharov Function (Domain: [-5, 10], Min: f(0, ..., 0) = 0)
        /// </summary>
        public T ExpandedZakharov(T[] xT, uint dim)
        {
            T sum1 = T.Zero, sum2 = T.Zero;
            for (int i = 0; i < dim; i++)
            {
                sum1 += xT[i] * xT[i];
                sum2 += T.CreateChecked(0.5 * (i + 1)) * xT[i];
            }
            return sum1 + (sum2 * sum2) + (sum2 * sum2 * sum2 * sum2);
        }

        /// <summary>
        /// 14. Michalewicz Function (Domain: [0, pi], m=10, Min depends on dimension)
        /// </summary>
        public T Michalewicz(T[] xT, uint dim)
        {
            T sum = T.Zero;
            T m2 = T.CreateChecked(20.0); // 2 * m where m = 10
            for (int i = 0; i < dim; i++)
            {
                T arg = T.CreateChecked(i + 1) * xT[i] * xT[i] / T.CreateChecked(Math.PI);
                sum += T.Sin(xT[i]) * T.Pow(T.Sin(arg), m2);
            }
            return -sum;
        }

        /// <summary>
        /// 15. Rosenbrock Function (Domain: [-30, 30], Min: f(1, ..., 1) = 0)
        /// </summary>
        public T Rosenbrock(T[] xT, uint dim)
        {
            T sum = T.Zero;
            T hundred = T.CreateChecked(100.0);
            for (int i = 0; i < dim - 1; i++)
            {
                T t1 = xT[i + 1] - (xT[i] * xT[i]);
                T t2 = xT[i] - T.One;
                sum += (hundred * t1 * t1) + (t2 * t2);
            }
            return sum;
        }

        /// <summary>
        /// 16. Sphere Function (Domain: [-5.12, 5.12], Min: f(0, ..., 0) = 0)
        /// </summary>
        public T Sphere(T[] xT, uint dim)
        {
            T sum = T.Zero;
            for (int i = 0; i < dim; i++)
            {
                sum += xT[i] * xT[i];
            }
            return sum;
        }

        /// <summary>
        /// 17. Styblinski-Tang Function (Domain: [-5, 5], Min: f(-2.903534, ...) = -39.16599 * dim)
        /// </summary>
        public T StyblinskiTang(T[] xT, uint dim)
        {
            T sum = T.Zero;
            T c16 = T.CreateChecked(16.0);
            T c5 = T.CreateChecked(5.0);
            for (int i = 0; i < dim; i++)
            {
                T x = xT[i];
                T x2 = x * x;
                T x4 = x2 * x2;
                sum += x4 - (c16 * x2) + (c5 * x);
            }
            return sum / (T.One + T.One);
        }

        /// <summary>
        /// 18. Schaffer F6 (Standard 2D Base)
        /// </summary>
        public T SchafferF6(T[] xT, uint dim = 2)
        {
            T x = xT[0], y = xT[1];
            T temp = (x * x) + (y * y);
            T sinVal = T.Sin(T.Sqrt(temp));
            T num = (sinVal * sinVal) - T.CreateChecked(0.5);
            T denom = T.One + (T.CreateChecked(0.001) * temp);
            return T.CreateChecked(0.5) + (num / (denom * denom));
        }
        // Test functions for optimization problems, Opt1D ////////////////////
        public T JTest1D(T xT)
        {
            double x = double.CreateChecked(xT);
            return T.CreateChecked(Math.Exp(x) * Math.Sin(5.5 * Math.PI * x));
        }
    }
}