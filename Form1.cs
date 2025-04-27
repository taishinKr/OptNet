namespace OptNet
{
    public partial class Form1 : Form
    {
        OptMD<double> mOptMD;
        public Form1()
        {
            InitializeComponent();


            /*var mConstVars = new OfflineGet<double>(11);
            var mOpt1D = new Opt1D<double>(Opt1D<double>.JTest1D, 11, 0, 1, mConstVars, "Both", null);  // "Min", "Max", or "Both"
            mOpt1D.DisplayResult(DisplayMessage);*/

            /*mOptMD = new OptMD<double>(OptMD<double>.JTest1D, 1, [11], [0], [1], "Max") { DisplayMsgFunc = DisplayMessage };
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);*/

            //mOptMD = new OptMD<double>(OptMD<double>.JTest2D, 2, [11, 11], [0, 0], [1, 1], "Max") { DisplayMsgFunc = DisplayMessage };
            //mOptMD.optimizeMD();
            /*//mOptMD.DisplayMsg(DisplayMessage);
            // mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);*/

            mOptMD = new OptMD<double>(OptMD<double>.JTest3D, 3, [11, 11, 11], [0, 0, 0], [1, 1, 1], "Max") { DisplayMsgFunc = DisplayMessage };

            /*//mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);
            mOptMD.shrinkBy(0.5);
            mOptMD.optimizeMD();
            //mOptMD.DisplayMsg(DisplayMessage);*/
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
    }
}
