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
            mOpt1D = new Opt1D<double>(mObjectJ,"Both");  // "Min", "Max", or "Both"
            // Display the Result
            mOpt1D.DisplayResult(DisplayMessage);
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
