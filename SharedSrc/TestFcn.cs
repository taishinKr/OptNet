using System.Numerics;

namespace OptNet
{
    public class TestFcn<T> where T : struct, INumber<T>
    {
        // Test functions for optimization problems, OptMD ////////////////////
        public T JTest1D(T[] xT)
        {
            double[] x = new double[1]; ;
            x[0] = double.CreateChecked(xT[0]);
            return T.CreateChecked(Math.Exp(x[0]) * Math.Sin(5.5 * Math.PI * x[0]));
        }
        public T JTest2D(T[] xT)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1]))
            );
        }
        public T JTest3D(T[] xT)
        {
            double[] x = new double[3];

            for (uint i = 0; i < 3; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                (1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1])))
                * Math.Sin(Math.PI * x[2])
            );
        }

        public T Bird2D(T[] xT)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                Math.Pow(x[0] - x[1], 2) + Math.Exp(Math.Pow(1 - Math.Sin(x[0]), 2)) * Math.Cos(x[1]) + Math.Exp(Math.Pow(1 - Math.Cos(x[1]), 2)) * Math.Sin(x[0])
            );
        }



        // Test functions for optimization problems, Opt1D ////////////////////
        public T JTest1D(T xT)
        {
            double x = double.CreateChecked(xT);
            return T.CreateChecked(Math.Exp(x) * Math.Sin(5.5 * Math.PI * x));
        }
    }
}