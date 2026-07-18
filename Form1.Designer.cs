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
            labelTolJReal = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDownShrink).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTolX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownTolJ).BeginInit();
            SuspendLayout();
            // 
            // textBoxMsg
            // 
            textBoxMsg.BackColor = SystemColors.Desktop;
            textBoxMsg.ForeColor = Color.Khaki;
            textBoxMsg.Location = new Point(12, 135);
            textBoxMsg.Multiline = true;
            textBoxMsg.Name = "textBoxMsg";
            textBoxMsg.ScrollBars = ScrollBars.Vertical;
            textBoxMsg.Size = new Size(776, 486);
            textBoxMsg.TabIndex = 2;
            // 
            // buttonOptimzeMD
            // 
            buttonOptimzeMD.Location = new Point(148, 22);
            buttonOptimzeMD.Name = "buttonOptimzeMD";
            buttonOptimzeMD.Size = new Size(100, 28);
            buttonOptimzeMD.TabIndex = 0;
            buttonOptimzeMD.Text = "OptimzeMD";
            buttonOptimzeMD.UseVisualStyleBackColor = true;
            buttonOptimzeMD.Click += ButtonOptimzeMD_Click;
            // 
            // buttonShrink
            // 
            buttonShrink.Enabled = false;
            buttonShrink.Location = new Point(148, 92);
            buttonShrink.Name = "buttonShrink";
            buttonShrink.Size = new Size(100, 28);
            buttonShrink.TabIndex = 1;
            buttonShrink.Text = "Shrink";
            buttonShrink.UseVisualStyleBackColor = true;
            buttonShrink.Click += ButtonShrink_Click;
            // 
            // buttonResetAll
            // 
            buttonResetAll.Location = new Point(31, 56);
            buttonResetAll.Name = "buttonResetAll";
            buttonResetAll.Size = new Size(100, 28);
            buttonResetAll.TabIndex = 4;
            buttonResetAll.Text = "ResetAll";
            buttonResetAll.UseVisualStyleBackColor = true;
            buttonResetAll.Click += ButtonResetAll_Click;
            // 
            // buttonClearMsgs
            // 
            buttonClearMsgs.Location = new Point(31, 92);
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
            comboBoxIdxXlist.Location = new Point(258, 95);
            comboBoxIdxXlist.Name = "comboBoxIdxXlist";
            comboBoxIdxXlist.Size = new Size(65, 23);
            comboBoxIdxXlist.TabIndex = 3;
            comboBoxIdxXlist.EnabledChanged += ComboBoxIdxXlist_EnabledChanged;
            // 
            // comboBoxBenchFcn
            // 
            comboBoxBenchFcn.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBenchFcn.Font = new Font("Malgun Gothic", 9F);
            comboBoxBenchFcn.FormattingEnabled = true;
            comboBoxBenchFcn.Location = new Point(31, 25);
            comboBoxBenchFcn.Name = "comboBoxBenchFcn";
            comboBoxBenchFcn.Size = new Size(100, 23);
            comboBoxBenchFcn.TabIndex = 11;
            comboBoxBenchFcn.SelectedIndexChanged += ComboBoxBenchFcn_SelectedIndexChanged;
            // 
            // numericUpDownShrink
            // 
            numericUpDownShrink.DecimalPlaces = 3;
            numericUpDownShrink.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDownShrink.Location = new Point(332, 95);
            numericUpDownShrink.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownShrink.Name = "numericUpDownShrink";
            numericUpDownShrink.Size = new Size(65, 23);
            numericUpDownShrink.TabIndex = 12;
            numericUpDownShrink.TextAlign = HorizontalAlignment.Center;
            numericUpDownShrink.Value = new decimal(new int[] { 5, 0, 0, 65536 });
            // 
            // numericUpDownTolX
            // 
            numericUpDownTolX.DecimalPlaces = 2;
            numericUpDownTolX.Location = new Point(332, 25);
            numericUpDownTolX.Name = "numericUpDownTolX";
            numericUpDownTolX.Size = new Size(65, 23);
            numericUpDownTolX.TabIndex = 13;
            numericUpDownTolX.TextAlign = HorizontalAlignment.Center;
            numericUpDownTolX.Value = new decimal(new int[] { 1, 0, 0, 196608 });
            numericUpDownTolX.ValueChanged += NumericUpDownTolX_ValueChanged;
            // 
            // numericUpDownTolJ
            // 
            numericUpDownTolJ.DecimalPlaces = 2;
            numericUpDownTolJ.Location = new Point(258, 25);
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
            labelIdx.Location = new Point(258, 78);
            labelIdx.Name = "labelIdx";
            labelIdx.Size = new Size(36, 15);
            labelIdx.TabIndex = 15;
            labelIdx.Text = "Index";
            // 
            // labelFactor
            // 
            labelFactor.AutoSize = true;
            labelFactor.Location = new Point(332, 78);
            labelFactor.Name = "labelFactor";
            labelFactor.Size = new Size(40, 15);
            labelFactor.TabIndex = 16;
            labelFactor.Text = "Factor";
            // 
            // labelTolJ
            // 
            labelTolJ.AutoSize = true;
            labelTolJ.Location = new Point(258, 8);
            labelTolJ.Name = "labelTolJ";
            labelTolJ.Size = new Size(45, 15);
            labelTolJ.TabIndex = 17;
            labelTolJ.Text = "TolJ(%)";
            // 
            // labelTolX
            // 
            labelTolX.AutoSize = true;
            labelTolX.Location = new Point(332, 8);
            labelTolX.Name = "labelTolX";
            labelTolX.Size = new Size(48, 15);
            labelTolX.TabIndex = 18;
            labelTolX.Text = "TolX(%)";
            // 
            // labelMaxX
            // 
            labelMaxX.AutoSize = true;
            labelMaxX.Location = new Point(588, 78);
            labelMaxX.Name = "labelMaxX";
            labelMaxX.Size = new Size(37, 15);
            labelMaxX.TabIndex = 19;
            labelMaxX.Text = "MaxX";
            // 
            // labelMinX
            // 
            labelMinX.AutoSize = true;
            labelMinX.Location = new Point(497, 78);
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
            comboBoxMinX.Location = new Point(497, 95);
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
            comboBoxMaxX.Location = new Point(588, 95);
            comboBoxMaxX.Name = "comboBoxMaxX";
            comboBoxMaxX.Size = new Size(82, 23);
            comboBoxMaxX.TabIndex = 22;
            // 
            // labelFcn
            // 
            labelFcn.AutoSize = true;
            labelFcn.Location = new Point(31, 8);
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
            comboBoxMaxXdX.Location = new Point(679, 95);
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
            comboBoxMinXdX.Location = new Point(406, 95);
            comboBoxMinXdX.Name = "comboBoxMinXdX";
            comboBoxMinXdX.Size = new Size(82, 23);
            comboBoxMinXdX.TabIndex = 26;
            // 
            // labelMinXdX
            // 
            labelMinXdX.AutoSize = true;
            labelMinXdX.Location = new Point(406, 78);
            labelMinXdX.Name = "labelMinXdX";
            labelMinXdX.Size = new Size(66, 15);
            labelMinXdX.TabIndex = 25;
            labelMinXdX.Text = "MinX-dX/2";
            // 
            // labelMaxXdX
            // 
            labelMaxXdX.AutoSize = true;
            labelMaxXdX.Location = new Point(679, 78);
            labelMaxXdX.Name = "labelMaxXdX";
            labelMaxXdX.Size = new Size(71, 15);
            labelMaxXdX.TabIndex = 24;
            labelMaxXdX.Text = "MaxX+dX/2";
            // 
            // labelTolJReal
            // 
            labelTolJReal.AutoSize = true;
            labelTolJReal.Location = new Point(258, 51);
            labelTolJReal.Name = "labelTolJReal";
            labelTolJReal.Size = new Size(21, 15);
            labelTolJReal.TabIndex = 28;
            labelTolJReal.Text = "dJ:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(802, 632);
            Controls.Add(labelTolJReal);
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
        private Label labelTolJReal;
    }
}
