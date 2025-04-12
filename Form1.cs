namespace OptNet
{
    public partial class Form1 : Form
    {
        ObjectJClass<double> mObjectJ;
        Opt1D<double> mOpt1D;
        public Form1()
        {
            InitializeComponent();

            mObjectJ = new ObjectJClass<double>(ObjectJClass<double>.JTest, 11, 0, 1);
            mOpt1D = new Opt1D<double>(mObjectJ);
            // Display the Result
            string s = string.Format("Key = {0,8:G5},   Value = {1,8:G5}", mOpt1D.Max.Key, mOpt1D.Max.Value);
            DisplayMessage("Max"+s);
            s = string.Format("Key = {0,8:G5}, Value = {1,8:G5}", mOpt1D.Min.Key, mOpt1D.Min.Value);
            DisplayMessage("Min" + s);
            //DisplayMessage($"Max: Key = {mOpt1D.Max.Key,10:G5}, Value = {mOpt1D.Max.Value,10:G5}");
            //DisplayMessage($"Min: Key = {mOpt1D.Min.Key}, Value = {mOpt1D.Min.Value}");
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
