namespace OptNet
{
    public partial class Form1 : Form
    {
        private OptMD<double> mOptMD;
        private OptMD<double> mOptMD1D;
        private OptMD<double> mOptMD2D;
        private OptMD<double> mOptMD3D;
        private Opt1D<double> mOpt1D;

        public Form1()
        {
            InitializeComponent();

            mOpt1D = new Opt1D<double>(Opt1D<double>.JTest1D, 0.001, 11, 0, 1, new OfflineGet<double>(11), Opt1D<double>.staMaskEnum.Max, null);  // "Min", "Max", or "Both"

            mOptMD1D = new OptMD<double>(OptMD<double>.JTest1D, 1, [11], [0], [1], OptMD<double>.staMaskEnum.Max);// { DisplayMsgFunc = DisplayMessage };
            mOptMD2D = new OptMD<double>(OptMD<double>.JTest2D, 2, [11, 11], [0, 0], [1, 1], OptMD<double>.staMaskEnum.Max);// { DisplayMsgFunc = DisplayMessage };
            mOptMD3D = new OptMD<double>(OptMD<double>.JTest3D, 3, [11, 11, 11], [0, 0, 0], [1, 1, 1], OptMD<double>.staMaskEnum.Max);

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
            textBoxMsg.AppendText(message);
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
            mOptMD1D = new OptMD<double>(OptMD<double>.JTest1D, 1, [11], [0], [1], OptMD<double>.staMaskEnum.Max);
            mOptMD2D = new OptMD<double>(OptMD<double>.JTest2D, 2, [11, 11], [0, 0], [1, 1], OptMD<double>.staMaskEnum.Max);
            mOptMD3D = new OptMD<double>(OptMD<double>.JTest3D, 3, [11, 11, 11], [0, 0, 0], [1, 1, 1], OptMD<double>.staMaskEnum.Max);
            mOptMD = mOptMD2D;
            buttonMD2D.FlatStyle = FlatStyle.Popup;
            buttonMD1D.FlatStyle = FlatStyle.Standard;
            buttonMD3D.FlatStyle = FlatStyle.Standard;
            buttonShrink.Enabled = false;
        }

        private void button1DOnly_Click(object sender, EventArgs e)
        {
            mOpt1D = new Opt1D<double>(Opt1D<double>.JTest1D, 0.001, 11, 0, 1, new OfflineGet<double>(11), Opt1D<double>.staMaskEnum.Max, null);  // "Min", "Max", or "Both"

            buttonShrink.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string msg;
            while ((msg = mOptMD.getDispMsg()) != string.Empty)
                DisplayMessage(msg);
            if (mOpt1D != null)
                while ((msg = mOpt1D.getDispMsg()) != string.Empty)
                    DisplayMessage(msg);
        }

        private void buttonClearMsgs_Click(object sender, EventArgs e)
        {
            textBoxMsg.Text = "";
        }
    }
}
