using System.Windows.Forms;

namespace OptNet
{
    public partial class Form1 : Form
    {

        private TestFcn<decimal> mTestFcn;
        private OptMD<decimal> mOptMD;
        private Opt1D<decimal> mOpt1D;

        public OptParam<decimal> mParamTst1D;
        public OptParam<decimal> mParamTst2D;
        public OptParam<decimal> mParamTst3D;
        public OptParam<decimal> mParamBird;

        public Form1()
        {
            InitializeComponent();

            mTestFcn = new TestFcn<decimal>();
            mOpt1D = new Opt1D<decimal>(mTestFcn.JTest1D, 0.01, 11, 0, 1, new OfflineGet<decimal>(11), Opt1D<decimal>.staMaskEnum.Max, null);  // "Min", "Max", or "Both"

            mParamTst1D = new OptParam<decimal>
            {
                J = mTestFcn.JTest1D,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 1,
                Ns = [11],
                MinX = [0],
                MaxX = [1],
                Mode = OptMD<decimal>.staMaskEnum.Max
            };
            mParamTst2D = new OptParam<decimal>
            {
                J = mTestFcn.JTest2D,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 2,
                Ns = [11, 11],
                MinX = [0, 0],
                MaxX = [1, 1],
                Mode = OptMD<decimal>.staMaskEnum.Max
            };
            mParamTst3D = new OptParam<decimal>
            {
                J = mTestFcn.JTest3D,
                TolJ = 0.001,
                TolX = 0.1,
                DimN = 3,
                Ns = [11, 11, 11],
                MinX = [0, 0, 0],
                MaxX = [1, 1, 1],
                Mode = OptMD<decimal>.staMaskEnum.Max
            };
            mParamBird = new OptParam<decimal>
            {
                J = mTestFcn.Bird2D,
                TolJ = 0.1,
                TolX = 0.03,
                DimN = 2,
                Ns = [11, 11],
                MinX = [-6, -6],
                MaxX = [6, 6],
                Mode = OptMD<decimal>.staMaskEnum.Min
            };
            mOptMD = new OptMD<decimal>(mParamTst2D);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form load event handler          

            comboBoxBenchFcn.Items.AddRange(new object[] {
                "JTest1D", "JTest2D", "JTest3D", "Bird2D", "1DOnly"
            });
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
        private void buttonOptimzeMD_Click(object sender, EventArgs e)
        {
            mOptMD.optimizeMD();
        }
        private void buttonShrink_Click(object sender, EventArgs e)
        {
            buttonShrink.Enabled = false;
            try
            {
                mOptMD.shrinkBy((double)(numericUpDownShrink.Value), comboBoxIdxXlist.SelectedIndex);
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
        private void buttonResetAll_Click(object sender, EventArgs e)
        {
            mOptMD = new OptMD<decimal>(mParamTst2D);
            comboBoxBenchFcn.SelectedIndex = 1;
            buttonShrink.Enabled = false;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (mOptMD.isSetSysFlag(OptMD<decimal>.staMaskEnum.Done))
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
            while ((msg = mOptMD.getDispMsg()) != string.Empty)
                DisplayMessage(msg);
            /*if (mOpt1D != null)
                while ((msg = mOpt1D.getDispMsg()) != string.Empty)
                    DisplayMessage(msg);*/

        }
        private void buttonClearMsgs_Click(object sender, EventArgs e)
        {
            textBoxMsg.Text = "";
        }
        private void comboBoxIdxXlist_EnabledChanged(object sender, EventArgs e)
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
        private void comboBoxBenchFcn_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = comboBoxBenchFcn.SelectedItem?.ToString();
            if (selectedItem == null)
            {
                DisplayMessage("No item selected in comboBoxBenchFcn.");
                return;
            }
            else
                mOptMD.clrSysFlag(OptMD<decimal>.staMaskEnum.Done);

            switch (selectedItem)
            {
                case "JTest1D":
                    mOptMD = new OptMD<decimal>(mParamTst1D);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "JTest2D":
                    mOptMD = new OptMD<decimal>(mParamTst2D);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "JTest3D":
                    mOptMD = new OptMD<decimal>(mParamTst3D);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "Bird2D":
                    mOptMD = new OptMD<decimal>(mParamBird);
                    buttonOptimzeMD.Enabled = true;
                    break;
                case "1DOnly":
                    mOpt1D = new Opt1D<decimal>(mTestFcn.JTest1D, 0.001, 11, 0, 1, new OfflineGet<decimal>(11), Opt1D<decimal>.staMaskEnum.Max, null);  // "Min", "Max"
                    DisplayMessage(mOpt1D.resultMsg());
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
        }

        private void numericUpDownTolJ_ValueChanged(object sender, EventArgs e)
        {
            mOptMD.TolJ = (double)(numericUpDownTolJ.Value / 100);
            labelTolJReal.Text = $"dJ: {mOptMD.TolJReal:F4}";
        }

        private void numericUpDownTolX_ValueChanged(object sender, EventArgs e)
        {
            mOptMD.TolX = (double)(numericUpDownTolX.Value / 100); // Convert percentage to decimal
        }
    }
}
