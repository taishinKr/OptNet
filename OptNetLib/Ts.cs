using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptNet
{
    public class Ts
    {
        public OptMD<double>? dlOptMD = null;
        public Opt1D<double>? dlOpt1D = null;
        public OfflineGet<double>? dlConstVars = null;
        public TestFcn<double>? dlTestFcn = null;

        public Ts(in Func<double, double> J, double TolJ, uint Ns, double minX, double maxX, uint mode)
        {
            dlConstVars = new OfflineGet<double>(Ns);
            dlOpt1D = new Opt1D<double>(J, TolJ, Ns, minX, maxX, dlConstVars, (Opt1D<double>.staMaskEnum)mode, null);
        }
        /*public Ts(uint dimN, uint[] Ns, double[] minX, double[] maxX, uint mode)
        {
            dlOptMD = new OptMD<double>(OptMD<double>.JTest3D, dimN, Ns, minX, maxX, (OptMD<double>.staMaskEnum)mode);
        }*/
        public Ts(in Func<double[], double> J, double TolJ, uint dimN, uint[] Ns, double[] minX, double[] maxX, uint mode)
        {
            if (Ns.Length != dimN || minX.Length != dimN || maxX.Length != dimN)
                throw new ArgumentException("The lengths of Ns, minX, and maxX must match the dimension dimN.");

            //dlOptMD = new OptMD<double>(J, TolJ, dimN, Ns, minX, maxX, (OptMD<double>.staMaskEnum)mode);
            dlOptMD = new OptMD<double>(new OptParam<double>
            {
                J = J,
                TolJ = TolJ,
                DimN = dimN,
                Ns = Ns,
                MinX = minX,
                MaxX = maxX,
                Mode = (OptMD<double>.staMaskEnum)mode
            });
        }
        public Ts()
        {
            dlTestFcn = new TestFcn<double>();
        }
    }        
}
