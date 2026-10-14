namespace OptNet
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            textBoxMsg = new TextBox();
            buttonOptimzeMD = new Button();
            buttonShrink = new Button();
            buttonResetAll = new Button();
            buttonClearMsgs = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            comboBoxIdxXlist = new ComboBox();
            comboBoxBenchFcn = new ComboBox();
            numericUpDownShrink = new NumericUpDown();
            numericUpDownTolX = new NumericUpDown();
            numericUpDownTolJ = new NumericUpDown();
            labelIdx = new Label();
            labelFactor = new Label();
            labelTolJ = new Label();
            labelTolX = new Label();
            labelMaxX = new Label();
            labelMinX = new Label();
            comboBoxMinX = new ComboBox();
            comboBoxMaxX = new ComboBox();
            labelFcn = new Label();
            comboBoxMaxXdX = new ComboBox();
            comboBoxMinXdX = new ComboBox();
            labelMinXdX = new Label();
            labelMaxXdX = new Label();
            radioButtonMin = new RadioButton();
            checkBoxThread = new CheckBox();
            radioButtonMax = new RadioButton();
            labelTolJReal = new Label();
            labelDimension = new Label();
            numericUpDownDimension = new NumericUpDown();
            comboBoxLogLevel = new ComboBox();
            labelLogLevel = new Label();
            comboBoxNs = new ComboBox();
            labelNs = new Label();
            checkBoxSampleReuse = new CheckBox();
            buttonUndo = new Button();
            label1 = new Label();
            checkBoxStrictLimit = new CheckBox();
            groupBox1 = new GroupBox();
            labelTolXReal = new Label();
            labelSpanX = new Label();
            labelSpanJ = new Label();
            checkBoxAutoShrink = new CheckBox();
            checkBoxSampledOpt = new CheckBox();
            radioButtonReSample = new RadioButton();
            radioButtonAxisChange = new RadioButton();
            groupBoxLowerAxis = new GroupBox();
            radioButtonBoth = new RadioButton();
            checkBoxPartialConstJ = new CheckBox();
            label2 = new Label();
            numericUpDownNs = new NumericUpDown();
            labelNFE = new Label();
            labelShrinkCnt = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDownShrink).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTolX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTolJ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDimension).BeginInit();
            groupBox1.SuspendLayout();
            groupBoxLowerAxis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownNs).BeginInit();
            SuspendLayout();
            // 
            // textBoxMsg
            // 
            textBoxMsg.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxMsg.BackColor = SystemColors.Desktop;
            textBoxMsg.ForeColor = Color.Khaki;
            textBoxMsg.Location = new Point(12, 210);
            textBoxMsg.Multiline = true;
            textBoxMsg.Name = "textBoxMsg";
            textBoxMsg.ScrollBars = ScrollBars.Vertical;
            textBoxMsg.Size = new Size(1098, 497);
            textBoxMsg.TabIndex = 2;
            // 
            // buttonOptimzeMD
            // 
            buttonOptimzeMD.Location = new Point(139, 115);
            buttonOptimzeMD.Name = "buttonOptimzeMD";
            buttonOptimzeMD.Size = new Size(92, 28);
            buttonOptimzeMD.TabIndex = 0;
            buttonOptimzeMD.Text = "OptimzeMD";
            buttonOptimzeMD.UseVisualStyleBackColor = true;
            buttonOptimzeMD.Click += ButtonOptimzeMD_Click;
            // 
            // buttonShrink
            // 
            buttonShrink.Enabled = false;
            buttonShrink.Location = new Point(17, 179);
            buttonShrink.Name = "buttonShrink";
            buttonShrink.Size = new Size(49, 28);
            buttonShrink.TabIndex = 1;
            buttonShrink.Text = "Shrink";
            buttonShrink.UseVisualStyleBackColor = true;
            buttonShrink.Click += ButtonShrink_Click;
            // 
            // buttonResetAll
            // 
            buttonResetAll.Location = new Point(237, 115);
            buttonResetAll.Name = "buttonResetAll";
            buttonResetAll.Size = new Size(92, 28);
            buttonResetAll.TabIndex = 4;
            buttonResetAll.Text = "ResetAll";
            buttonResetAll.UseVisualStyleBackColor = true;
            buttonResetAll.Click += ButtonResetAll_Click;
            // 
            // buttonClearMsgs
            // 
            buttonClearMsgs.Location = new Point(552, 50);
            buttonClearMsgs.Name = "buttonClearMsgs";
            buttonClearMsgs.Size = new Size(100, 28);
            buttonClearMsgs.TabIndex = 5;
            buttonClearMsgs.Text = "ClearMsgs";
            buttonClearMsgs.UseVisualStyleBackColor = true;
            buttonClearMsgs.Click += ButtonClearMsgs_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 300;
            timer1.Tick += Timer1_Tick;
            // 
            // comboBoxIdxXlist
            // 
            comboBoxIdxXlist.AllowDrop = true;
            comboBoxIdxXlist.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxIdxXlist.Font = new Font("Malgun Gothic", 9F);
            comboBoxIdxXlist.FormattingEnabled = true;
            comboBoxIdxXlist.ItemHeight = 15;
            comboBoxIdxXlist.Location = new Point(137, 182);
            comboBoxIdxXlist.MaxDropDownItems = 100;
            comboBoxIdxXlist.Name = "comboBoxIdxXlist";
            comboBoxIdxXlist.Size = new Size(65, 23);
            comboBoxIdxXlist.TabIndex = 3;
            comboBoxIdxXlist.SelectedIndexChanged += comboBoxIdxXlist_SelectedIndexChanged;
            comboBoxIdxXlist.EnabledChanged += ComboBoxIdxXlist_EnabledChanged;
            // 
            // comboBoxBenchFcn
            // 
            comboBoxBenchFcn.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBenchFcn.Font = new Font("Malgun Gothic", 9F);
            comboBoxBenchFcn.FormattingEnabled = true;
            comboBoxBenchFcn.Location = new Point(17, 118);
            comboBoxBenchFcn.Name = "comboBoxBenchFcn";
            comboBoxBenchFcn.Size = new Size(114, 23);
            comboBoxBenchFcn.TabIndex = 11;
            comboBoxBenchFcn.SelectedIndexChanged += ComboBoxBenchFcn_SelectedIndexChanged;
            // 
            // numericUpDownShrink
            // 
            numericUpDownShrink.DecimalPlaces = 3;
            numericUpDownShrink.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDownShrink.Location = new Point(211, 182);
            numericUpDownShrink.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownShrink.Name = "numericUpDownShrink";
            numericUpDownShrink.Size = new Size(65, 23);
            numericUpDownShrink.TabIndex = 12;
            numericUpDownShrink.TextAlign = HorizontalAlignment.Center;
            numericUpDownShrink.Value = new decimal(new int[] { 5, 0, 0, 65536 });
            // 
            // numericUpDownTolX
            // 
            numericUpDownTolX.DecimalPlaces = 3;
            numericUpDownTolX.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDownTolX.Location = new Point(516, 118);
            numericUpDownTolX.Name = "numericUpDownTolX";
            numericUpDownTolX.Size = new Size(65, 23);
            numericUpDownTolX.TabIndex = 13;
            numericUpDownTolX.TextAlign = HorizontalAlignment.Center;
            numericUpDownTolX.Value = new decimal(new int[] { 1, 0, 0, 196608 });
            numericUpDownTolX.ValueChanged += NumericUpDownTolX_ValueChanged;
            // 
            // numericUpDownTolJ
            // 
            numericUpDownTolJ.DecimalPlaces = 3;
            numericUpDownTolJ.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericUpDownTolJ.Location = new Point(700, 118);
            numericUpDownTolJ.Name = "numericUpDownTolJ";
            numericUpDownTolJ.Size = new Size(65, 23);
            numericUpDownTolJ.TabIndex = 14;
            numericUpDownTolJ.TextAlign = HorizontalAlignment.Center;
            numericUpDownTolJ.Value = new decimal(new int[] { 1, 0, 0, 196608 });
            numericUpDownTolJ.ValueChanged += NumericUpDownTolJ_ValueChanged;
            // 
            // labelIdx
            // 
            labelIdx.AutoSize = true;
            labelIdx.Location = new Point(137, 165);
            labelIdx.Name = "labelIdx";
            labelIdx.Size = new Size(36, 15);
            labelIdx.TabIndex = 15;
            labelIdx.Text = "Index";
            // 
            // labelFactor
            // 
            labelFactor.AutoSize = true;
            labelFactor.Location = new Point(211, 165);
            labelFactor.Name = "labelFactor";
            labelFactor.Size = new Size(40, 15);
            labelFactor.TabIndex = 16;
            labelFactor.Text = "\u03B1_zoom";
            // 
            // labelTolJ
            // 
            labelTolJ.AutoSize = true;
            labelTolJ.Location = new Point(700, 100);
            labelTolJ.Name = "labelTolJ";
            labelTolJ.Size = new Size(78, 15);
            labelTolJ.TabIndex = 17;
            labelTolJ.Text = "\u03B3J(%)";
            // 
            // labelTolX
            // 
            labelTolX.AutoSize = true;
            labelTolX.Location = new Point(516, 100);
            labelTolX.Name = "labelTolX";
            labelTolX.Size = new Size(48, 15);
            labelTolX.TabIndex = 18;
            labelTolX.Text = "\u03C1x(%)";
            // 
            // labelMaxX
            // 
            labelMaxX.AutoSize = true;
            labelMaxX.Location = new Point(467, 165);
            labelMaxX.Name = "labelMaxX";
            labelMaxX.Size = new Size(37, 15);
            labelMaxX.TabIndex = 19;
            labelMaxX.Text = "MaxX";
            // 
            // labelMinX
            // 
            labelMinX.AutoSize = true;
            labelMinX.Location = new Point(376, 165);
            labelMinX.Name = "labelMinX";
            labelMinX.Size = new Size(35, 15);
            labelMinX.TabIndex = 20;
            labelMinX.Text = "MinX";
            // 
            // comboBoxMinX
            // 
            comboBoxMinX.AllowDrop = true;
            comboBoxMinX.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMinX.Font = new Font("Malgun Gothic", 9F);
            comboBoxMinX.FormattingEnabled = true;
            comboBoxMinX.ItemHeight = 15;
            comboBoxMinX.Location = new Point(376, 182);
            comboBoxMinX.MaxDropDownItems = 100;
            comboBoxMinX.Name = "comboBoxMinX";
            comboBoxMinX.Size = new Size(82, 23);
            comboBoxMinX.TabIndex = 21;
            // 
            // comboBoxMaxX
            // 
            comboBoxMaxX.AllowDrop = true;
            comboBoxMaxX.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMaxX.Font = new Font("Malgun Gothic", 9F);
            comboBoxMaxX.FormattingEnabled = true;
            comboBoxMaxX.ItemHeight = 15;
            comboBoxMaxX.Location = new Point(467, 182);
            comboBoxMaxX.MaxDropDownItems = 100;
            comboBoxMaxX.Name = "comboBoxMaxX";
            comboBoxMaxX.Size = new Size(82, 23);
            comboBoxMaxX.TabIndex = 22;
            // 
            // labelFcn
            // 
            labelFcn.AutoSize = true;
            labelFcn.Location = new Point(17, 100);
            labelFcn.Name = "labelFcn";
            labelFcn.Size = new Size(59, 15);
            labelFcn.TabIndex = 23;
            labelFcn.Text = "Functions";
            // 
            // comboBoxMaxXdX
            // 
            comboBoxMaxXdX.AllowDrop = true;
            comboBoxMaxXdX.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMaxXdX.Font = new Font("Malgun Gothic", 9F);
            comboBoxMaxXdX.FormattingEnabled = true;
            comboBoxMaxXdX.ItemHeight = 15;
            comboBoxMaxXdX.Location = new Point(558, 182);
            comboBoxMaxXdX.MaxDropDownItems = 100;
            comboBoxMaxXdX.Name = "comboBoxMaxXdX";
            comboBoxMaxXdX.Size = new Size(82, 23);
            comboBoxMaxXdX.TabIndex = 27;
            // 
            // comboBoxMinXdX
            // 
            comboBoxMinXdX.AllowDrop = true;
            comboBoxMinXdX.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMinXdX.Font = new Font("Malgun Gothic", 9F);
            comboBoxMinXdX.FormattingEnabled = true;
            comboBoxMinXdX.ItemHeight = 15;
            comboBoxMinXdX.Location = new Point(285, 182);
            comboBoxMinXdX.MaxDropDownItems = 100;
            comboBoxMinXdX.Name = "comboBoxMinXdX";
            comboBoxMinXdX.Size = new Size(82, 23);
            comboBoxMinXdX.TabIndex = 26;
            // 
            // labelMinXdX
            // 
            labelMinXdX.AutoSize = true;
            labelMinXdX.Location = new Point(285, 165);
            labelMinXdX.Name = "labelMinXdX";
            labelMinXdX.Size = new Size(66, 15);
            labelMinXdX.TabIndex = 25;
            labelMinXdX.Text = "MinX-dX/2";
            // 
            // labelMaxXdX
            // 
            labelMaxXdX.AutoSize = true;
            labelMaxXdX.Location = new Point(558, 165);
            labelMaxXdX.Name = "labelMaxXdX";
            labelMaxXdX.Size = new Size(71, 15);
            labelMaxXdX.TabIndex = 24;
            labelMaxXdX.Text = "MaxX+dX/2";
            // 
            // radioButtonMin
            // 
            radioButtonMin.AutoSize = true;
            radioButtonMin.Location = new Point(14, 22);
            radioButtonMin.Name = "radioButtonMin";
            radioButtonMin.Size = new Size(86, 19);
            radioButtonMin.TabIndex = 29;
            radioButtonMin.TabStop = true;
            radioButtonMin.Text = "Minimizing";
            radioButtonMin.UseVisualStyleBackColor = true;
            radioButtonMin.CheckedChanged += radioButtonMin_CheckedChanged;
            // 
            // checkBoxThread
            // 
            checkBoxThread.AutoSize = true;
            checkBoxThread.Location = new Point(17, 14);
            checkBoxThread.Name = "checkBoxThread";
            checkBoxThread.Size = new Size(90, 19);
            checkBoxThread.TabIndex = 30;
            checkBoxThread.Text = "MultiThread";
            checkBoxThread.UseVisualStyleBackColor = true;
            checkBoxThread.CheckedChanged += checkBoxThread_CheckedChanged;
            // 
            // radioButtonMax
            // 
            radioButtonMax.AutoSize = true;
            radioButtonMax.Location = new Point(14, 43);
            radioButtonMax.Name = "radioButtonMax";
            radioButtonMax.Size = new Size(88, 19);
            radioButtonMax.TabIndex = 31;
            radioButtonMax.TabStop = true;
            radioButtonMax.Text = "Maximizing";
            radioButtonMax.UseVisualStyleBackColor = true;
            radioButtonMax.CheckedChanged += radioButtonMax_CheckedChanged;
            // 
            // labelTolJReal
            // 
            labelTolJReal.AutoSize = true;
            labelTolJReal.Location = new Point(774, 115);
            labelTolJReal.Name = "labelTolJReal";
            labelTolJReal.Size = new Size(49, 15);
            labelTolJReal.TabIndex = 33;
            labelTolJReal.Text = "TolJReal";
            // 
            // labelDimension
            // 
            labelDimension.AutoSize = true;
            labelDimension.Location = new Point(337, 101);
            labelDimension.Name = "labelDimension";
            labelDimension.Size = new Size(65, 15);
            labelDimension.TabIndex = 35;
            labelDimension.Text = "Dimension";
            // 
            // numericUpDownDimension
            // 
            numericUpDownDimension.Location = new Point(337, 118);
            numericUpDownDimension.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDownDimension.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownDimension.Name = "numericUpDownDimension";
            numericUpDownDimension.Size = new Size(65, 23);
            numericUpDownDimension.TabIndex = 34;
            numericUpDownDimension.TextAlign = HorizontalAlignment.Center;
            numericUpDownDimension.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownDimension.ValueChanged += numericUpDownDimension_ValueChanged;
            // 
            // comboBoxLogLevel
            // 
            comboBoxLogLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxLogLevel.Font = new Font("Malgun Gothic", 9F);
            comboBoxLogLevel.FormattingEnabled = true;
            comboBoxLogLevel.Location = new Point(552, 24);
            comboBoxLogLevel.Name = "comboBoxLogLevel";
            comboBoxLogLevel.Size = new Size(100, 23);
            comboBoxLogLevel.TabIndex = 36;
            comboBoxLogLevel.SelectedIndexChanged += comboBoxLogLevel_SelectedIndexChanged;
            // 
            // labelLogLevel
            // 
            labelLogLevel.AutoSize = true;
            labelLogLevel.Location = new Point(557, 6);
            labelLogLevel.Name = "labelLogLevel";
            labelLogLevel.Size = new Size(58, 15);
            labelLogLevel.TabIndex = 37;
            labelLogLevel.Text = "Log Level";
            // 
            // comboBoxNs
            // 
            comboBoxNs.AllowDrop = true;
            comboBoxNs.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxNs.Font = new Font("Malgun Gothic", 9F);
            comboBoxNs.FormattingEnabled = true;
            comboBoxNs.ItemHeight = 15;
            comboBoxNs.Location = new Point(649, 181);
            comboBoxNs.MaxDropDownItems = 100;
            comboBoxNs.Name = "comboBoxNs";
            comboBoxNs.Size = new Size(65, 23);
            comboBoxNs.TabIndex = 39;
            // 
            // labelNs
            // 
            labelNs.AutoSize = true;
            labelNs.Location = new Point(649, 164);
            labelNs.Name = "labelNs";
            labelNs.Size = new Size(21, 15);
            labelNs.TabIndex = 38;
            labelNs.Text = "Ns";
            // 
            // checkBoxSampleReuse
            // 
            checkBoxSampleReuse.AutoSize = true;
            checkBoxSampleReuse.Location = new Point(118, 14);
            checkBoxSampleReuse.Name = "checkBoxSampleReuse";
            checkBoxSampleReuse.Size = new Size(101, 19);
            checkBoxSampleReuse.TabIndex = 40;
            checkBoxSampleReuse.Text = "Sample Reuse";
            checkBoxSampleReuse.UseVisualStyleBackColor = true;
            checkBoxSampleReuse.CheckedChanged += checkBoxSampleReuse_CheckedChanged;
            // 
            // buttonUndo
            // 
            buttonUndo.Location = new Point(82, 179);
            buttonUndo.Name = "buttonUndo";
            buttonUndo.Size = new Size(49, 28);
            buttonUndo.TabIndex = 41;
            buttonUndo.Text = "Undo";
            buttonUndo.UseVisualStyleBackColor = true;
            buttonUndo.Click += buttonUndo_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(68, 186);
            label1.Name = "label1";
            label1.Size = new Size(12, 15);
            label1.TabIndex = 42;
            label1.Text = "/";
            // 
            // checkBoxStrictLimit
            // 
            checkBoxStrictLimit.AutoSize = true;
            checkBoxStrictLimit.Location = new Point(118, 31);
            checkBoxStrictLimit.Name = "checkBoxStrictLimit";
            checkBoxStrictLimit.Size = new Size(81, 19);
            checkBoxStrictLimit.TabIndex = 43;
            checkBoxStrictLimit.Text = "StrictLimit";
            checkBoxStrictLimit.UseVisualStyleBackColor = true;
            checkBoxStrictLimit.CheckedChanged += checkBoxStrictLimit_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButtonMin);
            groupBox1.Controls.Add(radioButtonMax);
            groupBox1.Location = new Point(237, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(113, 72);
            groupBox1.TabIndex = 44;
            groupBox1.TabStop = false;
            groupBox1.Text = "MODE";
            // 
            // labelTolXReal
            // 
            labelTolXReal.AutoSize = true;
            labelTolXReal.Location = new Point(587, 118);
            labelTolXReal.Name = "labelTolXReal";
            labelTolXReal.Size = new Size(52, 15);
            labelTolXReal.TabIndex = 45;
            labelTolXReal.Text = "TolXReal";
            // 
            // labelSpanX
            // 
            labelSpanX.AutoSize = true;
            labelSpanX.Location = new Point(587, 133);
            labelSpanX.Name = "labelSpanX";
            labelSpanX.Size = new Size(41, 15);
            labelSpanX.TabIndex = 46;
            labelSpanX.Text = "SpanX";
            // 
            // labelSpanJ
            // 
            labelSpanJ.AutoSize = true;
            labelSpanJ.Location = new Point(774, 129);
            labelSpanJ.Name = "labelSpanJ";
            labelSpanJ.Size = new Size(38, 15);
            labelSpanJ.TabIndex = 47;
            labelSpanJ.Text = "SpanJ";
            // 
            // checkBoxAutoShrink
            // 
            checkBoxAutoShrink.AutoSize = true;
            checkBoxAutoShrink.Checked = true;
            checkBoxAutoShrink.CheckState = CheckState.Checked;
            checkBoxAutoShrink.Location = new Point(118, 48);
            checkBoxAutoShrink.Name = "checkBoxAutoShrink";
            checkBoxAutoShrink.Size = new Size(86, 19);
            checkBoxAutoShrink.TabIndex = 48;
            checkBoxAutoShrink.Text = "AutoShrink";
            checkBoxAutoShrink.UseVisualStyleBackColor = true;
            checkBoxAutoShrink.CheckedChanged += checkBoxAutoShrink_CheckedChanged;
            // 
            // checkBoxSampledOpt
            // 
            checkBoxSampledOpt.AutoSize = true;
            checkBoxSampledOpt.Checked = true;
            checkBoxSampledOpt.CheckState = CheckState.Checked;
            checkBoxSampledOpt.Location = new Point(17, 50);
            checkBoxSampledOpt.Name = "checkBoxSampledOpt";
            checkBoxSampledOpt.Size = new Size(93, 19);
            checkBoxSampledOpt.TabIndex = 49;
            checkBoxSampledOpt.Text = "SampledOpt";
            checkBoxSampledOpt.UseVisualStyleBackColor = true;
            checkBoxSampledOpt.CheckedChanged += checkBoxSampledOpt_CheckedChanged;
            // 
            // radioButtonReSample
            // 
            radioButtonReSample.AutoSize = true;
            radioButtonReSample.Checked = true;
            radioButtonReSample.Location = new Point(14, 22);
            radioButtonReSample.Name = "radioButtonReSample";
            radioButtonReSample.Size = new Size(89, 19);
            radioButtonReSample.TabIndex = 29;
            radioButtonReSample.TabStop = true;
            radioButtonReSample.Text = "ReSampling";
            radioButtonReSample.UseVisualStyleBackColor = true;
            radioButtonReSample.CheckedChanged += radioButtonReSample_CheckedChanged;
            // 
            // radioButtonAxisChange
            // 
            radioButtonAxisChange.AutoSize = true;
            radioButtonAxisChange.Location = new Point(14, 42);
            radioButtonAxisChange.Name = "radioButtonAxisChange";
            radioButtonAxisChange.Size = new Size(99, 19);
            radioButtonAxisChange.TabIndex = 31;
            radioButtonAxisChange.Text = "AxisChanging";
            radioButtonAxisChange.UseVisualStyleBackColor = true;
            radioButtonAxisChange.CheckedChanged += radioButtonAxisChange_CheckedChanged;
            // 
            // groupBoxLowerAxis
            // 
            groupBoxLowerAxis.Controls.Add(radioButtonBoth);
            groupBoxLowerAxis.Controls.Add(radioButtonReSample);
            groupBoxLowerAxis.Controls.Add(radioButtonAxisChange);
            groupBoxLowerAxis.Location = new Point(369, 6);
            groupBoxLowerAxis.Name = "groupBoxLowerAxis";
            groupBoxLowerAxis.Size = new Size(148, 87);
            groupBoxLowerAxis.TabIndex = 50;
            groupBoxLowerAxis.TabStop = false;
            groupBoxLowerAxis.Text = "Lower Axis Handling";
            // 
            // radioButtonBoth
            // 
            radioButtonBoth.AutoSize = true;
            radioButtonBoth.Location = new Point(14, 62);
            radioButtonBoth.Name = "radioButtonBoth";
            radioButtonBoth.Size = new Size(50, 19);
            radioButtonBoth.TabIndex = 32;
            radioButtonBoth.Text = "Both";
            radioButtonBoth.UseVisualStyleBackColor = true;
            radioButtonBoth.CheckedChanged += radioButtonBoth_CheckedChanged;
            // 
            // checkBoxPartialConstJ
            // 
            checkBoxPartialConstJ.AutoSize = true;
            checkBoxPartialConstJ.Checked = true;
            checkBoxPartialConstJ.CheckState = CheckState.Checked;
            checkBoxPartialConstJ.Location = new Point(17, 31);
            checkBoxPartialConstJ.Name = "checkBoxPartialConstJ";
            checkBoxPartialConstJ.Size = new Size(94, 19);
            checkBoxPartialConstJ.TabIndex = 51;
            checkBoxPartialConstJ.Text = "PartialConstJ";
            checkBoxPartialConstJ.UseVisualStyleBackColor = true;
            checkBoxPartialConstJ.CheckedChanged += checkBoxPartialConstJ_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(412, 101);
            label2.Name = "label2";
            label2.Size = new Size(78, 15);
            label2.TabIndex = 53;
            label2.Text = "# of Samples";
            // 
            // numericUpDownNs
            // 
            numericUpDownNs.Location = new Point(412, 118);
            numericUpDownNs.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericUpDownNs.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numericUpDownNs.Name = "numericUpDownNs";
            numericUpDownNs.Size = new Size(92, 23);
            numericUpDownNs.TabIndex = 52;
            numericUpDownNs.TextAlign = HorizontalAlignment.Center;
            numericUpDownNs.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numericUpDownNs.ValueChanged += numericUpDownNs_ValueChanged;
            // 
            // labelNFE
            // 
            labelNFE.AutoSize = true;
            labelNFE.Location = new Point(17, 164);
            labelNFE.Name = "labelNFE";
            labelNFE.Size = new Size(28, 15);
            labelNFE.TabIndex = 54;
            labelNFE.Text = "NFE";
            // 
            // labelShrinkCnt
            // 
            labelShrinkCnt.AutoSize = true;
            labelShrinkCnt.Location = new Point(17, 150);
            labelShrinkCnt.Name = "labelShrinkCnt";
            labelShrinkCnt.Size = new Size(26, 15);
            labelShrinkCnt.TabIndex = 55;
            labelShrinkCnt.Text = "Cnt";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1122, 723);
            Controls.Add(labelShrinkCnt);
            Controls.Add(labelNFE);
            Controls.Add(label2);
            Controls.Add(numericUpDownNs);
            Controls.Add(checkBoxPartialConstJ);
            Controls.Add(groupBoxLowerAxis);
            Controls.Add(checkBoxSampledOpt);
            Controls.Add(checkBoxAutoShrink);
            Controls.Add(labelSpanJ);
            Controls.Add(labelSpanX);
            Controls.Add(labelTolXReal);
            Controls.Add(groupBox1);
            Controls.Add(checkBoxStrictLimit);
            Controls.Add(label1);
            Controls.Add(buttonUndo);
            Controls.Add(checkBoxSampleReuse);
            Controls.Add(comboBoxNs);
            Controls.Add(labelNs);
            Controls.Add(labelLogLevel);
            Controls.Add(comboBoxLogLevel);
            Controls.Add(labelDimension);
            Controls.Add(numericUpDownDimension);
            Controls.Add(labelTolJReal);
            Controls.Add(checkBoxThread);
            Controls.Add(comboBoxMaxXdX);
            Controls.Add(comboBoxMinXdX);
            Controls.Add(labelMinXdX);
            Controls.Add(labelMaxXdX);
            Controls.Add(labelFcn);
            Controls.Add(comboBoxMaxX);
            Controls.Add(comboBoxMinX);
            Controls.Add(labelMinX);
            Controls.Add(labelMaxX);
            Controls.Add(labelTolX);
            Controls.Add(labelTolJ);
            Controls.Add(labelFactor);
            Controls.Add(labelIdx);
            Controls.Add(numericUpDownTolJ);
            Controls.Add(numericUpDownTolX);
            Controls.Add(numericUpDownShrink);
            Controls.Add(comboBoxBenchFcn);
            Controls.Add(comboBoxIdxXlist);
            Controls.Add(buttonClearMsgs);
            Controls.Add(buttonResetAll);
            Controls.Add(buttonShrink);
            Controls.Add(buttonOptimzeMD);
            Controls.Add(textBoxMsg);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDownShrink).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTolX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTolJ).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDimension).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBoxLowerAxis.ResumeLayout(false);
            groupBoxLowerAxis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownNs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxMsg;
        private Button buttonOptimzeMD;
        private Button buttonShrink;
        private Button buttonResetAll;
        private Button buttonClearMsgs;
        private System.Windows.Forms.Timer timer1;
        private ComboBox comboBoxIdxXlist;
        private ComboBox comboBoxBenchFcn;
        private NumericUpDown numericUpDownShrink;
        private NumericUpDown numericUpDownTolX;
        private NumericUpDown numericUpDownTolJ;
        private Label labelIdx;
        private Label labelFactor;
        private Label labelTolJ;
        private Label labelTolX;
        private Label labelMaxX;
        private Label labelMinX;
        private ComboBox comboBoxMinX;
        private ComboBox comboBoxMaxX;
        private Label labelFcn;
        private ComboBox comboBoxMaxXdX;
        private ComboBox comboBoxMinXdX;
        private Label labelMinXdX;
        private Label labelMaxXdX;
        private RadioButton radioButtonMin;
        private CheckBox checkBoxThread;
        private RadioButton radioButtonMax;
        private Label labelTolJReal;
        private Label labelDimension;
        private NumericUpDown numericUpDownDimension;
        private ComboBox comboBoxLogLevel;
        private Label labelLogLevel;
        private ComboBox comboBoxNs;
        private Label labelNs;
        private CheckBox checkBoxSampleReuse;
        private Button buttonUndo;
        private Label label1;
        private CheckBox checkBoxStrictLimit;
        private GroupBox groupBox1;
        private Label labelTolXReal;
        private Label labelSpanX;
        private Label labelSpanJ;
        private CheckBox checkBoxAutoShrink;
        private CheckBox checkBoxSampledOpt;
        private RadioButton radioButtonReSample;
        private RadioButton radioButtonAxisChange;
        private GroupBox groupBoxLowerAxis;
        private CheckBox checkBoxPartialConstJ;
        private Label label2;
        private NumericUpDown numericUpDownNs;
        private Label labelNFE;
        private RadioButton radioButtonBoth;
        private Label labelShrinkCnt;
    }
}
