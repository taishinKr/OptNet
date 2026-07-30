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
                IsScalable = false
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
                IsScalable = false
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
                IsScalable = false
            };
            mParamBird = new OptParam<double>
            {
                J = mTestFcn.Bird2D,
                TolJ = 0.1,
                TolX = 0.03,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-6, -6],
                MaxX = [6, 6],
                Mode = OptMD<double>.StaMaskEnum.Min,
                IsScalable = false
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
                IsScalable = true
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
                IsScalable = true
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
                IsScalable = true
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
                IsScalable = true
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
                IsScalable = true
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
                IsScalable = true
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
                IsScalable = true
            };
            mOptMD = new OptMD<double>(mParamAckleyN1); // Default to AckleyN1 function

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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form load event handler          

            comboBoxBenchFcn.Items.AddRange([
                "JTest1D", "JTest2D", "JTest3D", "Bird2D", "1DOnly", "AckleyN1", "Griewank", "Rastrigin", "Schwefel", "Alpine", "ExpandedSchafferF6", "Salomons"
            ]);
            comboBoxBenchFcn.SelectedIndex = 5; // Default to the last item

            comboBoxLogLevel.Items.AddRange([
                "None", "Error", "Warning", "Info0", "Info1","Info2","Info3","Debug", "Verbose"
            ]);
            comboBoxLogLevel.SelectedIndex = 3;


            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ


            ResetUICtr();

            labelTolJReal.Text = $"dJ: {mOptMD.TolJ:F4}";
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
            numericUpDownShrink.Enabled = false;
            comboBoxIdxXlist.Enabled = false;

            mOptMD.OptimizeMD();
        }
        private void ButtonShrink_Click(object sender, EventArgs e)
        {
            buttonShrink.Enabled = false;
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
            mOptMD = new OptMD<double>(mParamAckleyN1); // Reset to AckleyN1 function
			comboBoxBenchFcn.SelectedIndex = 5;
            comboBoxLogLevel.SelectedIndex = 3;
            buttonShrink.Enabled = false;

            ResetUICtr();

            SetModeOfNewOptMD();
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {

            if (mOptMD.IsSetSysFlag(OptMD<double>.StaMaskEnum.Done))
            {
                comboBoxIdxXlist.Enabled = true;
                numericUpDownShrink.Enabled = true;
                buttonShrink.Enabled = true;
                buttonOptimzeMD.Enabled = true;
            }
            else
            {
                buttonShrink.Enabled = false;
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

            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ

            ResetUICtr();

            SetModeOfNewOptMD();
        }

        private void ResetUICtr()
        {
            numericUpDownShrink.Value = 0.1M; // Default value for numericUpDown                        
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

            numericUpDownDimension.Value = mOptMD.DimN; // Reset the dimension value
            numericUpDownDimension.Enabled = mOptMD.OptParam.IsScalable;

            mOptMD.CurrentLevel = (OptMD<double>.LogLevel)comboBoxLogLevel.SelectedIndex;
        }

        private void NumericUpDownTolJ_ValueChanged(object sender, EventArgs e)
        {
            mOptMD.TolJ = (double)(numericUpDownTolJ.Value / 100);
            labelTolJReal.Text = $"dJ: {mOptMD.TolJReal:F4}";
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

        private void numericUpDownDimension_ValueChanged(object sender, EventArgs e)
        {
            buttonOptimzeMD.Enabled = false;
            OptParam<double> optParam = new(mOptMD.OptParam, (uint)numericUpDownDimension.Value);
            mOptMD = new OptMD<double>(optParam);
            buttonOptimzeMD.Enabled = true;

            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ

            ResetUICtr();

            SetModeOfNewOptMD();
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

        
    }
}
