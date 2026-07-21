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

        private bool _initializing;
        private bool verboseEnabled;
        private bool threadEnabled;

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
                Mode = OptMD<double>.StaMaskEnum.Max
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
                Mode = OptMD<double>.StaMaskEnum.Max
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
                Mode = OptMD<double>.StaMaskEnum.Max
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
                Mode = OptMD<double>.StaMaskEnum.Min
            };
            mOptMD = new OptMD<double>(mParamTst2D);

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

            if (checkBoxVerbose.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.Verbose);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.Verbose);
            }
            if (checkBoxThread.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.MThread);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.MThread);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form load event handler          

            comboBoxBenchFcn.Items.AddRange([
                "JTest1D", "JTest2D", "JTest3D", "Bird2D", "1DOnly"
            ]);
            comboBoxBenchFcn.SelectedIndex = 1; // Default to the first item
            numericUpDownShrink.Value = 0.5M; // Default value for numericUpDown
            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ
            numericUpDownTolX.Value = (decimal)(mOptMD.TolX * 100); // Set the default value for numericUpDownTolX

            comboBoxMinX.Items.Clear();
            comboBoxMaxX.Items.Clear();
            comboBoxMinXdX.Items.Clear();
            comboBoxMaxXdX.Items.Clear();
            for (int i = 0; i < mOptMD.DimN; i++)
            {
                comboBoxMinX.Items.Add(mOptMD.MinX[i].ToString("F4"));
                comboBoxMaxX.Items.Add(mOptMD.MaxX[i].ToString("F4"));
                comboBoxMinXdX.Items.Add((mOptMD.MinX[i] - mOptMD.DXHalf[i]).ToString("F4"));
                comboBoxMaxXdX.Items.Add((mOptMD.MaxX[i] + mOptMD.DXHalf[i]).ToString("F4"));
            }
            comboBoxMinX.SelectedIndex = 0; // Set default selected index for comboBoxMinX
            comboBoxMaxX.SelectedIndex = 0; // Set default selected index for comboBoxMaxX
            comboBoxMinXdX.SelectedIndex = 0; // Set default selected index for comboBoxMinXdX
            comboBoxMaxXdX.SelectedIndex = 0; // Set default selected index for comboBoxMaxXdX

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
            numericUpDownShrink.Value = 0.5M; // Default value for numericUpDown
            numericUpDownTolX.Value = (decimal)mOptMD.TolX * 100; // Reset the tolerance value

            comboBoxMinX.Items.Clear();
            comboBoxMaxX.Items.Clear();
            comboBoxMinXdX.Items.Clear();
            comboBoxMaxXdX.Items.Clear();
            for (int i = 0; i < mOptMD.DimN; i++)
            {
                comboBoxMinX.Items.Add(mOptMD.MinX[i].ToString("F4"));
                comboBoxMaxX.Items.Add(mOptMD.MaxX[i].ToString("F4"));
                comboBoxMinXdX.Items.Add((mOptMD.MinX[i] - mOptMD.DXHalf[i]).ToString("F4"));
                comboBoxMaxXdX.Items.Add((mOptMD.MaxX[i] + mOptMD.DXHalf[i]).ToString("F4"));
            }
            comboBoxMinX.SelectedIndex = 0; // Set default selected index for comboBoxMinX
            comboBoxMaxX.SelectedIndex = 0; // Set default selected index for comboBoxMaxX
            comboBoxMinXdX.SelectedIndex = 0; // Set default selected index for comboBoxMinXdX
            comboBoxMaxXdX.SelectedIndex = 0; // Set default selected index for comboBoxMaxXdX
        }
        private void ButtonResetAll_Click(object sender, EventArgs e)
        {
            mOptMD = new OptMD<double>(mParamTst2D);
            comboBoxBenchFcn.SelectedIndex = 1;
            buttonShrink.Enabled = false;

            SetModeOfNewOptMD();
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {

            if (mOptMD.IsSetSysFlag(OptMD<double>.StaMaskEnum.Done))
            {
                comboBoxIdxXlist.Enabled = true;
                numericUpDownShrink.Enabled = true;
                buttonShrink.Enabled = true;
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
            numericUpDownShrink.Value = 0.5M; // Default value for numericUpDown
            numericUpDownTolJ.Value = (decimal)(mOptMD.TolJ * 100); // Set the default value for numericUpDownTolJ
            numericUpDownTolX.Value = (decimal)(mOptMD.TolX * 100); // Set the default value for numericUpDownTolX

            comboBoxMinX.Items.Clear();
            comboBoxMaxX.Items.Clear();
            comboBoxMinXdX.Items.Clear();
            comboBoxMaxXdX.Items.Clear();
            for (int i = 0; i < mOptMD.DimN; i++)
            {
                comboBoxMinX.Items.Add(mOptMD.MinX[i].ToString("F4"));
                comboBoxMaxX.Items.Add(mOptMD.MaxX[i].ToString("F4"));
                comboBoxMinXdX.Items.Add((mOptMD.MinX[i] - mOptMD.DXHalf[i]).ToString("F4"));
                comboBoxMaxXdX.Items.Add((mOptMD.MaxX[i] + mOptMD.DXHalf[i]).ToString("F4"));
            }
            comboBoxMinX.SelectedIndex = 0; // Set default selected index for comboBoxMinX
            comboBoxMaxX.SelectedIndex = 0; // Set default selected index for comboBoxMaxX
            comboBoxMinXdX.SelectedIndex = 0; // Set default selected index for comboBoxMinXdX
            comboBoxMaxXdX.SelectedIndex = 0; // Set default selected index for comboBoxMaxXdX

            SetModeOfNewOptMD();
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

        private void checkBoxVerbose_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (checkBoxThread.Checked)
            {
                mOptMD.SetSysFlag(OptMD<double>.StaMaskEnum.Verbose);
            }
            else
            {
                mOptMD.ClrSysFlag(OptMD<double>.StaMaskEnum.Verbose);
            }
        }
    }
}
