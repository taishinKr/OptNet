using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OptNet
{
    internal class OptMD<T> where T : struct, INumber<T>
    {
        private readonly uint dimN;

        private readonly uint[] Ns;
        private readonly T[] minX;
        private readonly T[] maxX;
        private readonly string mode;
        private Dictionary<uint, OfflineGet<T>> lutConstant;

        private T[] inputX;
        private uint[] inputIndex;
        private List<T> sampleJtot;
        private KeyValuePair<T[], T> optXY;

        public  Func<T[], T> J;

        public OptMD(Func<T[], T> j, uint dimN, uint[] Ns, T[] minX, T[] maxX, string mode)
        {
            J = j;
            this.dimN = dimN;
            this.Ns = Ns;
            this.minX = minX;
            this.maxX = maxX;
            this.mode = mode;
            inputX = new T[dimN];
            inputIndex = new uint[dimN];
            sampleJtot = new List<T>();
            lutConstant = new Dictionary<uint, OfflineGet<T>>();
            optimizeMD();
        }
        private void optimizeMD()
        {
            getSample(0);
            var optx = new KeyValuePair<T, T> (T.Zero,T.Zero);            
            var tmpkey = new T[dimN];

            for (uint i = 0; i < dimN; i++)
            {
                optx = optimizeMD1Axis(i, i);
                tmpkey[i] = optx.Key;
            }
            optXY = new KeyValuePair<T[], T>(tmpkey, optx.Value);
        }
        private KeyValuePair<T, T> optimizeMD1Axis(uint axisIdx, uint idxDim)
        {
            ObjectJClass1D<T> objectJ;
            Opt1D<T> opt1D;

            var prevAxisIdx = (int)(axisIdx - 1) < 0 ? dimN - 1 : (axisIdx - 1);
            var nextIdx = (idxDim + 1) >= dimN ? 0 : (idxDim + 1);

            var sampleJs = new T[Ns[idxDim]];
            //var sampleX = new T[Ns[idxDim]];
            KeyValuePair<T, T> sampleOpt;
            
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputIndex[idxDim] = n;
                
                //if (idxDim == dimN-1)
                if (idxDim == prevAxisIdx)
                {
                    sampleJs[n] = sampleJtot[(int)getIdTot()];
                }
                else
                {
                    sampleOpt = optimizeMD1Axis(axisIdx, nextIdx);
                    //sampleX[n] = sampleOpt.Key;
                    sampleJs[n] = sampleOpt.Value;
                }
            }

            objectJ = new ObjectJClass1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim]);

            OfflineGet<T> constVar;
            if ( lutConstant.ContainsKey(Ns[idxDim]) )
                constVar = lutConstant[Ns[idxDim]];
            else
            {
                constVar = new OfflineGet<T>(Ns[idxDim]);
                lutConstant.Add(Ns[idxDim], constVar);
            }
            opt1D = new Opt1D<T>(objectJ, constVar, mode);
            if (mode == "Max")
            {
                return opt1D.Max;
            }                
            else
            {
                return opt1D.Min;
            }
                
        }

        private void getSample(uint idxDim)
        {
            T dX = (maxX[idxDim] - minX[idxDim]) / T.CreateChecked((Ns[idxDim] - 1));

            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX;
                if (idxDim == dimN - 1)
                    sampleJtot.Add(J(inputX));
                else
                    getSample(idxDim + 1);
            }
        }
        private uint getIdTot()
        {
            /*uint idTot = inputIndex[0];
            for (uint i = 1; i < dimN; i++)
            {
                uint accumNs = 1;
                for (uint j = 0; j < i; j++)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }*/
            uint idTot = inputIndex[dimN-1];
            for (int i = (int)dimN-2; i >= 0; i--)
            {
                uint accumNs = 1;
                for (int j = (int)dimN-1; j > i; j--)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }

            return idTot;
        }
        public T J1D(T x)
        {
            inputX[dimN - 1] = x;
            return J(inputX);
        }
        public static T JTest1D(T[] xT)
        {
            double[] x = new double[1]; ;
            x[0] = double.CreateChecked(xT[0]);
            return T.CreateChecked(Math.Exp(x[0]) * Math.Sin(5.5 * Math.PI * x[0]));
        }
        public static T JTest2D(T[] xT)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1]))
            );
        }
        public static T JTest3D(T[] xT)
        {
            double[] x = new double[3];

            for (uint i = 0; i < 3; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                (1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1])))
                * Math.Sin(Math.PI * x[2])
            );
        }
        public void DisplayMsg(Action<string> DisplayMessage)
        {
            //string msgTemplate = inputIndex.ToString();
            string msgOptX = "Optimum ";
            msgOptX += string.Format("{0,8:G5} at ", optXY.Value); ;

            foreach ( T optx in optXY.Key)
            {
                msgOptX += string.Format("{0,8:G5} ", optx); ;
            }
            DisplayMessage(msgOptX);
        }

    }
}
