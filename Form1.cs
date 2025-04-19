namespace OptNet
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var mObjectJ = new ObjectJClass1D<double>(Opt1D<double>.JTest1D, 11, 0, 1);
            var mConstVars = new OfflineGet<double>((uint)mObjectJ.Js.Length);
            var mOpt1D = new Opt1D<double>(mObjectJ,mConstVars,"Both");  // "Min", "Max", or "Both"
            mOpt1D.DisplayResult(DisplayMessage);

            var mOptMD = new OptMD<double>(OptMD<double>.JTest1D, 1, [11], [0], [1], "Max");
            mOptMD.DisplayMsg(DisplayMessage);

            mOptMD = new OptMD<double>(OptMD<double>.JTest2D, 2, [11, 11], [0, 0], [1, 1], "Max");
            mOptMD.DisplayMsg(DisplayMessage);

            mOptMD = new OptMD<double>(OptMD<double>.JTest3D, 3, [11, 11, 11], [0, 0, 0], [1, 1, 1], "Max");
            mOptMD.DisplayMsg(DisplayMessage);
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
    }
}
