using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OptNet
{
    public partial class Form1 : Form
    {
        private TestFcn<double> mTestFcn;
        private OptMD<double> mOptMD;
        private Opt1D<double> mOpt1D;

        public OptParam<double> mParamTst1D;
        public OptParam<double> mParamTst2D;
        public OptParam<double> mParamTst3D;
        public OptParam<double> mParamBird;
        public OptParam<double> mParamAckleyN1;
        public OptParam<double> mParamGriewank;
        public OptParam<double> mParamRastrigin;
        public OptParam<double> mParamSchwefel;
        public OptParam<double> mParamAlpine;
        public OptParam<double> mParamExpandedSchafferF6;
        public OptParam<double> mParamSalomons;
        public OptParam<double> mParamBeale;
        public OptParam<double> mParamBooth;
        public OptParam<double> mParamCrossInTray;
        public OptParam<double> mParamDropWave;
        public OptParam<double> mParamEasom;
        public OptParam<double> mParamGoldsteinPrice;
        public OptParam<double> mParamHimmelblau;
        public OptParam<double> mParamHolderTable;
        public OptParam<double> mParamLevyN13;
        public OptParam<double> mParamMatyas;
        public OptParam<double> mParamSchafferN2;
        public OptParam<double> mParamThreeHumpCamel;
        public OptParam<double> mParamExpandedZakharov;
        public OptParam<double> mParamMichalewicz;
        public OptParam<double> mParamRosenbrock;
        public OptParam<double> mParamSphere;
        public OptParam<double> mParamStyblinskiTang;
        public OptParam<double> mParamSchafferF6;


        private bool _initializing;

        public Form1()
        {
            InitializeComponent();

            mTestFcn = new TestFcn<double>();
            mOpt1D = new Opt1D<double>(mTestFcn.JTest1D, 0.01, 11, 0, 1, new OfflineGet<double>(11), Opt1D<double>.StaMaskEnum.Max, null);  // "Min", "Max", or "Both"

            mParamTst1D = new OptParam<double>
            {
                J = mTestFcn.JTest1D,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 1,
                Ns = [11],
                MinX = [0],
                MaxX = [1],
                Mode = OptMD<double>.StaMaskEnum.Max,
                IsScalable = false,
                Optima = [([0.82153], 2.2702)]
            };
            mParamTst2D = new OptParam<double>
            {
                J = mTestFcn.JTest2D,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [0, 0],
                MaxX = [1, 1],
                Mode = OptMD<double>.StaMaskEnum.Max,
                IsScalable = false,
                Optima = [([0.5877, 0.4123], 0.73998)]
            };
            mParamTst3D = new OptParam<double>
            {
                J = mTestFcn.JTest3D,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [0, 0, 0],
                MaxX = [1, 1, 1],
                Mode = OptMD<double>.StaMaskEnum.Max,
                IsScalable = false,
                Optima = [([0.5877, 0.4123, 0.5], 0.73998)]
            };
            mParamBird = new OptParam<double>
            {
                J = mTestFcn.Bird2D,
                TolJ = 0.1,
                TolX = 0.3,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-6, -6],
                MaxX = [6, 6],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([4.70104, 3.15294], -106.7645), ([-1.58214, -3.13024], -106.7645)]
            };
            mParamAckleyN1 = new OptParam<double>
            {
                J = mTestFcn.AckleyN1,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-32, -32, -32],
                MaxX = [32, 32, 32],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamGriewank = new OptParam<double>
            {
                J = mTestFcn.Griewank,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-600, -600, -600],
                MaxX = [600, 600, 600],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamRastrigin = new OptParam<double>
            {
                J = mTestFcn.Rastrigin,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-5.12, -5.12, -5.12],
                MaxX = [5.12, 5.12, 5.12],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamSchwefel = new OptParam<double>
            {
                J = mTestFcn.Schwefel,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-500, -500, -500],
                MaxX = [500, 500, 500],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([420.9687, 420.9687, 420.9687], 0)]
            };
            mParamAlpine = new OptParam<double>
            {
                J = mTestFcn.Alpine,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-10, -10, -10],
                MaxX = [10, 10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamExpandedSchafferF6 = new OptParam<double>
            {
                J = mTestFcn.ExpandedSchafferF6,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-100, -100, -100],
                MaxX = [100, 100, 100],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamSalomons = new OptParam<double>
            {
                J = mTestFcn.Salomons,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-100, -100, -100],
                MaxX = [100, 100, 100],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamBeale = new OptParam<double>
            {
                J = mTestFcn.Beale,
                TolJ = 0.0005,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-4.5, -4.5],
                MaxX = [4.5, 4.5],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([3, 0.5], 0.0)]
            };
            mParamBooth = new OptParam<double>
            {
                J = mTestFcn.Booth,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-10, -10],
                MaxX = [10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([1, 3], 0.0)]
            };
            mParamCrossInTray = new OptParam<double>
            {
                J = mTestFcn.CrossInTray,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-10, -10],
                MaxX = [10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, 0], 0.0)]
            };
            mParamDropWave = new OptParam<double>
            {
                J = mTestFcn.DropWave,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-5.12, -5.12],
                MaxX = [5.12, 5.12],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, 0], -1.0)]
            };
            mParamEasom = new OptParam<double>
            {
                J = mTestFcn.Easom,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-100, -100],
                MaxX = [100, 100],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([Math.PI, Math.PI], -1.0)]
            };
            mParamGoldsteinPrice = new OptParam<double>
            {
                J = mTestFcn.GoldsteinPrice,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-2, -2],
                MaxX = [2, 2],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, -1], 3.0)]
            };
            mParamHimmelblau = new OptParam<double>
            {
                J = mTestFcn.Himmelblau,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-5, -5],
                MaxX = [5, 5],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [
                    ([3, 2], 0.0),
                    ([-2.805118, 3.131313], 0.0),
                    ([-3.779310, -3.283186], 0.0),
                    ([3.584428, -1.848126], 0.0)
                ]
            };
            mParamHolderTable = new OptParam<double>
            {
                J = mTestFcn.HolderTable,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-10, -10],
                MaxX = [10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [
                    ([8.05502, 9.66459], -19.2085),
                    ([-8.05502, 9.66459], -19.2085),
                    ([8.05502, -9.66459], -19.2085),
                    ([-8.05502, -9.66459], -19.2085)
                ]
            };
            mParamLevyN13 = new OptParam<double>
            {
                J = mTestFcn.LeviN13,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-10, -10],
                MaxX = [10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([1, 1], 0.0)]
            };
            mParamMatyas = new OptParam<double>
            {
                J = mTestFcn.Matyas,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-10, -10],
                MaxX = [10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, 0], 0.0)]
            };
            mParamSchafferN2 = new OptParam<double>
            {
                J = mTestFcn.SchafferN2,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-100, -100],
                MaxX = [100, 100],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, 0], 0.0)]
            };
            mParamThreeHumpCamel = new OptParam<double>
            {
                J = mTestFcn.ThreeHumpCamel,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-5, -5],
                MaxX = [5, 5],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, 0], 0.0)]
            };
            mParamExpandedZakharov = new OptParam<double>
            {
                J = mTestFcn.ExpandedZakharov,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-5, -5, -5],
                MaxX = [10, 10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamMichalewicz = new OptParam<double>
            {
                J = mTestFcn.Michalewicz,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [0, 0],
                MaxX = [3.14, 3.14],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false, // scalable but the known optimum is for 2D, so we set IsScalable to false
                Optima = [([2.2029, 1.5708], -1.8013)]
            };
            mParamRosenbrock = new OptParam<double>
            {
                J = mTestFcn.Rosenbrock,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-5, -5, -5],
                MaxX = [10, 10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([1, 1, 1], 0.0)]
            };
            mParamSphere = new OptParam<double>
            {
                J = mTestFcn.Sphere,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-5, -5, -5],
                MaxX = [10, 10, 10],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([0, 0, 0], 0.0)]
            };
            mParamStyblinskiTang = new OptParam<double>
            {
                J = mTestFcn.StyblinskiTang,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [-5, -5, -5],
                MaxX = [5, 5, 5],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = true,
                Optima = [([-2.903534, -2.903534, -2.903534], -39.16599)]
            };
            mParamSchafferF6 = new OptParam<double>
            {
                J = mTestFcn.SchafferF6,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-100, -100],
                MaxX = [100, 100],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false,
                Optima = [([0, 0], 0.0)]
            };
            //mOptMD = new OptMD<double>(mParamAckleyN1); // Default to AckleyN1 function
            mOptMD = new OptMD<double>(mParamBeale); // Default to Beale function


            SetModeOfNewOptMD();
        }

        private void SetModeOfNewOptMD()
        {
            _initializing = true;
            //checkBoxThread.Checked = mOptMD.IsSetSysFlag(OptMD<double>.StaMaskEnum.MThread);
            radioButtonMin.Checked = (mOptMD.Mode == OptMD<double>.StaMaskEnum.Min);
            radioButtonMax.Checked = (mOptMD.Mode == OptMD<double>.StaMaskEnum.Max);
            //checkBoxVerbose.Checked = mOptMD.IsSetSysFlag(OptMD<double>.StaMaskEnum.Verbose);
            _initializing = false;

            if (checkBoxThread.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.MThread);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.MThread);
            }

            if (checkBoxSampleReuse.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.SampleReuse);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.SampleReuse);
            }

            if (checkBoxStrictLimit.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.StrictLimit);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.StrictLimit);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form load event handler          

            comboBoxBenchFcn.Items.AddRange([
                "JTest1D", "JTest2D", "JTest3D", "Bird2D", "1DOnly", "AckleyN1", "Griewank", "Rastrigin", "Schwefel", "Alpine", "ExpandedSchafferF6", "Salomons"
                ,"Beale", "Booth", "CrossInTray", "DropWave", "Easom", "GoldsteinPrice", "Himmelblau", "HolderTable", "LevyN13", "Matyas", "SchafferN2", "ThreeHumpCamel"
                ,"ExpandedZakharov", "Michalewicz", "Rosenbrock", "Sphere", "StyblinskiTang", "SchafferF6"
            ]);
            //comboBoxBenchFcn.SelectedIndex = 5; // Default to the last item
            comboBoxBenchFcn.SelectedIndex = 12; // Default to the last item

            comboBoxLogLevel.Items.AddRange([
                "None", "Error", "Warning", "Info0", "Info1","Info2","Info3","Debug", "Verbose"
            ]);
            comboBoxLogLevel.SelectedIndex = 3;


            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ


            ResetUICtr();

            labelTolJReal.Text = $"TolJReal: {mOptMD.TolJ:F4}";
            labelTolXReal.Text = $"TolXReal: {mOptMD.TolXReal:F4}";
        }

        public void DisplayMessage(string message)
        {
            if (textBoxMsg.Text.Length + message.Length > textBoxMsg.MaxLength)
            {
                textBoxMsg.ResetText();
            }
            textBoxMsg.AppendText(message);
        }
        private void ButtonOptimzeMD_Click(object sender, EventArgs e)
        {
            buttonOptimzeMD.Enabled = false;

            buttonShrink.Enabled = false;
            //buttonUndo.Enabled = false;
            numericUpDownShrink.Enabled = false;
            comboBoxIdxXlist.Enabled = false;

            mOptMD.OptimizeMD();
        }
        private void ButtonShrink_Click(object sender, EventArgs e)
        {
            try
            {
                mOptMD.ShrinkBy((double)(numericUpDownShrink.Value), comboBoxIdxXlist.SelectedIndex);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Shrink operation failed: {ex.Message}");
            }
            var old = numericUpDownShrink.Value;
            ResetUICtr();
            numericUpDownShrink.Value = old;
        }
        private void ButtonResetAll_Click(object sender, EventArgs e)
        {
            DisplayMessage("Initializing...");
            //mOptMD = new OptMD<double>(mParamAckleyN1); // Reset to AckleyN1 function
            mOptMD = new OptMD<double>(mParamBeale); // Reset to Beale function

            DisplayMessage("Done." + Environment.NewLine);
            comboBoxBenchFcn.SelectedIndex = 12; // for Beale  // 5 for AckleyN;
            comboBoxLogLevel.SelectedIndex = 3;
            buttonShrink.Enabled = false;
            //buttonUndo.Enabled = false;

            ResetUICtr();
            SetModeOfNewOptMD();
            MsgForNewFunction();
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            labelTolJReal.Text = $"TolJReal: {mOptMD.TolJReal:F4}";
            labelTolXReal.Text = $"TolXReal: {mOptMD.TolXReal:F4}";

            if (mOptMD.IsSetSysFlag(OptMD<double>.StaMaskEnum.Done))
            {
                comboBoxIdxXlist.Enabled = true;
                numericUpDownShrink.Enabled = true;
                buttonShrink.Enabled = true;
                //buttonUndo.Enabled = true;
                buttonOptimzeMD.Enabled = true;
            }
            else
            {
                buttonShrink.Enabled = false;
                //buttonUndo.Enabled = false;
                numericUpDownShrink.Enabled = false;
                comboBoxIdxXlist.Enabled = false;
            }

            string msg;
            while ((msg = mOptMD.GetDispMsg()) != string.Empty)
                DisplayMessage(msg);
            /*if (mOpt1D != null)
                while ((msg = mOpt1D.getDispMsg()) != string.Empty)
                    DisplayMessage(msg);*/

        }
        private void ButtonClearMsgs_Click(object sender, EventArgs e)
        {
            textBoxMsg.Text = "";
        }
        private void ComboBoxIdxXlist_EnabledChanged(object sender, EventArgs e)
        {
            if (comboBoxIdxXlist.Enabled)
            {
                var listSize = mOptMD.optResult.Length;
                for (int i = 0; i < listSize; i++)
                {
                    comboBoxIdxXlist.Items.Add(i.ToString());
                }
                comboBoxIdxXlist.SelectedIndex = 0;
            }
            else
            {
                comboBoxIdxXlist.Items.Clear();
            }
        }
        private void ComboBoxBenchFcn_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = comboBoxBenchFcn.SelectedItem?.ToString();
            if (selectedItem == null)
            {
                DisplayMessage("No item selected in comboBoxBenchFcn.");
                return;
            }
            else
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.Done);

            DisplayMessage("Initializing...");
            switch (selectedItem)
            {
                case "JTest1D":
                    mOptMD = new OptMD<double>(mParamTst1D);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "JTest2D":
                    mOptMD = new OptMD<double>(mParamTst2D);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "JTest3D":
                    mOptMD = new OptMD<double>(mParamTst3D);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Bird2D":
                    mOptMD = new OptMD<double>(mParamBird);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "AckleyN1":
                    mOptMD = new OptMD<double>(mParamAckleyN1);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Griewank":
                    mOptMD = new OptMD<double>(mParamGriewank);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Rastrigin":
                    mOptMD = new OptMD<double>(mParamRastrigin);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Schwefel":
                    mOptMD = new OptMD<double>(mParamSchwefel);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Alpine":
                    mOptMD = new OptMD<double>(mParamAlpine);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "ExpandedSchafferF6":
                    mOptMD = new OptMD<double>(mParamExpandedSchafferF6);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Salomons":
                    mOptMD = new OptMD<double>(mParamSalomons);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Beale":
                    mOptMD = new OptMD<double>(mParamBeale);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Booth":
                    mOptMD = new OptMD<double>(mParamBooth);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "CrossInTray":
                    mOptMD = new OptMD<double>(mParamCrossInTray);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "DropWave":
                    mOptMD = new OptMD<double>(mParamDropWave);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Easom":
                    mOptMD = new OptMD<double>(mParamEasom);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "GoldsteinPrice":
                    mOptMD = new OptMD<double>(mParamGoldsteinPrice);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Himmelblau":
                    mOptMD = new OptMD<double>(mParamHimmelblau);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "HolderTable":
                    mOptMD = new OptMD<double>(mParamHolderTable);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "LevyN13":
                    mOptMD = new OptMD<double>(mParamLevyN13);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Matyas":
                    mOptMD = new OptMD<double>(mParamMatyas);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "SchafferN2":
                    mOptMD = new OptMD<double>(mParamSchafferN2);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "ThreeHumpCamel":
                    mOptMD = new OptMD<double>(mParamThreeHumpCamel);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "ExpandedZakharov":
                    mOptMD = new OptMD<double>(mParamExpandedZakharov);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Michalewicz":
                    mOptMD = new OptMD<double>(mParamMichalewicz);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Rosenbrock":
                    mOptMD = new OptMD<double>(mParamRosenbrock);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Sphere":
                    mOptMD = new OptMD<double>(mParamSphere);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "StyblinskiTang":
                    mOptMD = new OptMD<double>(mParamStyblinskiTang);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "SchafferF6":
                    mOptMD = new OptMD<double>(mParamSchafferF6);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "1DOnly":
                    mOpt1D = new Opt1D<double>(mTestFcn.JTest1D, 0.001, 11, 0, 1, new OfflineGet<double>(11), Opt1D<double>.StaMaskEnum.Max, null);  // "Min", "Max"
                    DisplayMessage(mOpt1D.ResultMsg());
                    buttonOptimzeMD.Enabled = false;
                    //buttonShrink.Enabled = false;
                    break;
                default:
                    DisplayMessage("Invalid selection in comboBoxBenchFcn.");
                    break;
            }
            DisplayMessage("Done." + Environment.NewLine);

            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ

            ResetUICtr();
            SetModeOfNewOptMD();
            MsgForNewFunction();
        }

        private void MsgForNewFunction()
        {
            DisplayMessage(Environment.NewLine);
            DisplayMessage($"Selected function: {comboBoxBenchFcn.SelectedItem}" + Environment.NewLine);
            DisplayMessage($"DimN: {mOptMD.DimN}, TolJ: {mOptMD.TolJ}, TolX: {mOptMD.TolX}" + Environment.NewLine);
            DisplayMessage($"MinX: {string.Join(", ", mOptMD.MinX)}, MaxX: {string.Join(", ", mOptMD.MaxX)}" + Environment.NewLine);
            DisplayMessage($"Ns: {string.Join(", ", mOptMD.Nsample)}" + Environment.NewLine);
            DisplayMessage($"Mode: {mOptMD.Mode}, IsScalable: {mOptMD.OptParam.IsScalable}" + Environment.NewLine);
            DisplayMessage($"Known Optima: {string.Join(", ", mOptMD.OptParam.Optima.Select(opt => $"J={opt.Item2} at ({string.Join(", ", opt.Item1)})"))}" + Environment.NewLine);
            DisplayMessage(Environment.NewLine);
        }

        private void ResetUICtr()
        {
            numericUpDownShrink.Value = 0.25M; // Default value for numericUpDown                        
            numericUpDownTolX.Value = (decimal)mOptMD.TolX * 100; // Reset the tolerance value

            comboBoxMinX.Items.Clear();
            comboBoxMaxX.Items.Clear();
            comboBoxMinXdX.Items.Clear();
            comboBoxMaxXdX.Items.Clear();
            comboBoxNs.Items.Clear();
            for (int i = 0; i < mOptMD.DimN; i++)
            {
                comboBoxMinX.Items.Add(mOptMD.MinX[i].ToString("F4"));
                comboBoxMaxX.Items.Add(mOptMD.MaxX[i].ToString("F4"));
                comboBoxMinXdX.Items.Add((mOptMD.MinX[i] - mOptMD.DXHalf[i]).ToString("F4"));
                comboBoxMaxXdX.Items.Add((mOptMD.MaxX[i] + mOptMD.DXHalf[i]).ToString("F4"));
                comboBoxNs.Items.Add(mOptMD.Nsample[i].ToString("F0"));
            }
            comboBoxMinX.SelectedIndex = 0; // Set default selected index for comboBoxMinX
            comboBoxMaxX.SelectedIndex = 0; // Set default selected index for comboBoxMaxX
            comboBoxMinXdX.SelectedIndex = 0; // Set default selected index for comboBoxMinXdX
            comboBoxMaxXdX.SelectedIndex = 0; // Set default selected index for comboBoxMaxXdX
            comboBoxNs.SelectedIndex = 0; // Set default selected index for comboBoxNs

            _isChangedFromUI = false; // Prevent triggering ValueChanged event during initialization
            numericUpDownDimension.Value = mOptMD.DimN; // Reset the dimension value
            _isChangedFromUI = true; // Re-enable ValueChanged event after initialization

            numericUpDownDimension.Enabled = mOptMD.OptParam.IsScalable;

            mOptMD.CurrentLevel = (OptMD<double>.LogLevel)comboBoxLogLevel.SelectedIndex;
        }

        private void NumericUpDownTolJ_ValueChanged(object sender, EventArgs e)
        {
            mOptMD.TolJ = (double)(numericUpDownTolJ.Value / 100);
            labelTolJReal.Text = $"TolJReal: {mOptMD.TolJReal:F4}";
            labelTolXReal.Text = $"TolXReal: {mOptMD.TolXReal:F4}";
        }

        private void NumericUpDownTolX_ValueChanged(object sender, EventArgs e)
        {
            mOptMD.TolX = (double)(numericUpDownTolX.Value / 100); // Convert percentage to decimal
        }

        private void checkBoxThread_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (checkBoxThread.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.MThread);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.MThread);
            }
        }

        private void radioButtonMin_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (radioButtonMin.Checked)
                mOptMD.Mode = OptMD<double>.StaMaskEnum.Min;
            else
                mOptMD.Mode = OptMD<double>.StaMaskEnum.Max;
        }

        private void radioButtonMax_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (radioButtonMax.Checked)
                mOptMD.Mode = OptMD<double>.StaMaskEnum.Max;
            else
                mOptMD.Mode = OptMD<double>.StaMaskEnum.Min;
        }

        private bool _isChangedFromUI = false;
        private void numericUpDownDimension_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangedFromUI)
            {
                buttonOptimzeMD.Enabled = false;
                OptParam<double> optParam = new(mOptMD.OptParam, (uint)numericUpDownDimension.Value);

                DisplayMessage("Initializing...");
                mOptMD = new OptMD<double>(optParam);
                DisplayMessage("Done." + Environment.NewLine);

                buttonOptimzeMD.Enabled = true;

                numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ

                ResetUICtr();
                SetModeOfNewOptMD();
                MsgForNewFunction();
            }
        }

        private void comboBoxLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = comboBoxLogLevel.SelectedItem?.ToString();
            if (selectedItem == null)
            {
                DisplayMessage("No item selected in comboBoxLogLevel.");
                return;
            }

            switch (selectedItem)
            {
                case "None":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.None;
                    break;
                case "Error":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Error;
                    break;
                case "Warning":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Warning;
                    break;
                case "Info0":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Info0;
                    break;
                case "Info1":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Info1;
                    break;
                case "Info2":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Info2;
                    break;
                case "Info3":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Info3;
                    break;
                case "Debug":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Debug;
                    break;
                case "Verbose":
                    mOptMD.CurrentLevel = OptMD<double>.LogLevel.Verbose;
                    break;
                default:
                    DisplayMessage("Invalid selection in comboBoxLogLevel.");
                    break;
            }
        }

        private void checkBoxSampleReuse_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (checkBoxSampleReuse.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.SampleReuse);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.SampleReuse);
            }
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            //buttonUndo.Enabled = false;
            try
            {
                mOptMD.RestoreState();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Shrink Undooperation failed: {ex.Message}");
            }
            var old = numericUpDownShrink.Value;
            ResetUICtr();
            numericUpDownShrink.Value = old;
            mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.Done); // Set the Done flag to true after restoring state
        }

        private void checkBoxStrictLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (checkBoxStrictLimit.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.StrictLimit);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.StrictLimit);
            }
        }
    }
}