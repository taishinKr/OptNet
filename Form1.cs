namespace OptNet
{
    public partial class Form1 : Form
    {
        OptMD<double> mOptMD;
        OptMD<double> mOptMD1D;
        OptMD<double> mOptMD2D;
        OptMD<double> mOptMD3D;
        Opt1D<double> mOpt1D;

        public Form1()
        {
            InitializeComponent();

            mOpt1D = new Opt1D<double>(Opt1D<double>.JTest1D, 11, 0, 1, new OfflineGet<double>(11), "Both", null);  // "Min", "Max", or "Both"

            mOptMD1D = new OptMD<double>(OptMD<double>.JTest1D, 1, [11], [0], [1], "Max") { DisplayMsgFunc = DisplayMessage };
            mOptMD2D = new OptMD<double>(OptMD<double>.JTest2D, 2, [11, 11], [0, 0], [1, 1], "Max") { DisplayMsgFunc = DisplayMessage };
            mOptMD3D = new OptMD<double>(OptMD<double>.JTest3D, 3, [11, 11, 11], [0, 0, 0], [1, 1, 1], "Max") { DisplayMsgFunc = DisplayMessage };

            mOptMD = mOptMD2D;
            buttonMD2D.FlatStyle = FlatStyle.Popup;
            buttonShrink.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form load event handler
        }

        public void DisplayMessage(string message)
        {
            if (textBoxMsg.Text.Length + message.Length > textBoxMsg.MaxLength)
            {
                textBoxMsg.ResetText();
            }
            textBoxMsg.AppendText(message + Environment.NewLine);
        }

        private void buttonOptimzeMD_Click(object sender, EventArgs e)
        {
            mOptMD.optimizeMD();
            buttonShrink.Enabled = true;
        }

        private void buttonShrink_Click(object sender, EventArgs e)
        {
            buttonShrink.Enabled = false;
            mOptMD.shrinkBy(0.5);
        }

        private void buttonMD1D_Click(object sender, EventArgs e)
        {
            mOptMD = mOptMD1D;
            buttonMD1D.FlatStyle = FlatStyle.Popup;
            buttonMD2D.FlatStyle = FlatStyle.Standard;
            buttonMD3D.FlatStyle = FlatStyle.Standard;
            buttonShrink.Enabled = false;
        }

        private void buttonMD2D_Click(object sender, EventArgs e)
        {
            mOptMD = mOptMD2D;
            buttonMD2D.FlatStyle = FlatStyle.Popup;
            buttonMD1D.FlatStyle = FlatStyle.Standard;
            buttonMD3D.FlatStyle = FlatStyle.Standard;
            buttonShrink.Enabled = false;
        }

        private void buttonMD3D_Click(object sender, EventArgs e)
        {
            mOptMD = mOptMD3D;
            buttonMD3D.FlatStyle = FlatStyle.Popup;
            buttonMD1D.FlatStyle = FlatStyle.Standard;
            buttonMD2D.FlatStyle = FlatStyle.Standard;
            buttonShrink.Enabled = false;
        }

        private void buttonResetAll_Click(object sender, EventArgs e)
        {
            mOptMD1D = new OptMD<double>(OptMD<double>.JTest1D, 1, [11], [0], [1], "Max") { DisplayMsgFunc = DisplayMessage };
            mOptMD2D = new OptMD<double>(OptMD<double>.JTest2D, 2, [11, 11], [0, 0], [1, 1], "Max") { DisplayMsgFunc = DisplayMessage };
            mOptMD3D = new OptMD<double>(OptMD<double>.JTest3D, 3, [11, 11, 11], [0, 0, 0], [1, 1, 1], "Max") { DisplayMsgFunc = DisplayMessage };
            mOptMD = mOptMD2D;
            buttonMD2D.FlatStyle = FlatStyle.Popup;
            buttonMD1D.FlatStyle = FlatStyle.Standard;
            buttonMD3D.FlatStyle = FlatStyle.Standard;
            buttonShrink.Enabled = false;
        }

        private void button1DOnly_Click(object sender, EventArgs e)
        {
            mOpt1D.DisplayResult(DisplayMessage);
            buttonShrink.Enabled = false;
        }
    }
}
