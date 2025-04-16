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
            getSample(0);
            KeyValuePair < T, T > optx = optimizeMD(0,0);
            optXY = new KeyValuePair<T[], T>([optx.Key,T.Zero], optx.Value);
            optx = optimizeMD(1, 1);
            optXY.Key[1] = optx.Key;
        }
        private void getSample(uint idxDim)
        {
            T dX = (maxX[idxDim] - minX[idxDim]) / T.CreateChecked((Ns[idxDim] - 1));

            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX;
                if (idxDim == dimN-1)
                    sampleJtot.Add(J(inputX));
                else
                    getSample(idxDim + 1);
            }
        }
        private uint getIdTot()
        {
            uint idTot = inputIndex[0];            
            for (uint i = 1; i < dimN; i++)
            {
                uint accumNs = 1;
                for (uint j = 0; j < i; j++)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }
            
            return idTot;
        }
        private KeyValuePair<T, T> optimizeMD(uint axisIdx, uint idxDim)
        {
            ObjectJClass1D<T> objectJ;
            Opt1D<T> opt1D;

            var prevIdx = (int)(axisIdx - 1) < 0 ? dimN - 1 : (axisIdx - 1);
            var nextIdx = (axisIdx + 1) >= dimN ? 0 : (axisIdx + 1);

            var sampleJs = new T[Ns[idxDim]];
            //var sampleX = new T[Ns[idxDim]];
            KeyValuePair<T, T> sampleOpt;
            
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputIndex[idxDim] = n;
                
                //if (idxDim == dimN-1)
                if (idxDim == prevIdx)
                {
                    sampleJs[n] = sampleJtot[(int)getIdTot()];
                }
                else
                {
                    sampleOpt = optimizeMD(axisIdx, nextIdx);
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

        public T J1D(T x)
        {
            inputX[dimN - 1] = x;
            return J(inputX);
        }
        public static T JTestMD(T[] xT)
        {
            double[] x = new double[2];

            for (uint i = 0; i < 2; i++)
                x[i] = double.CreateChecked(xT[i]);

            return T.CreateChecked(
                1.0E-14 * Math.Exp(-34 * x[0] * x[0] + x[0] * (64 - 60 * x[1]) + (64 - 34 * x[1]) * x[1]) * Math.Sin(6 * Math.Sqrt(2) * (x[0] - x[1]))
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
