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
            textBoxMsg = new TextBox();
            buttonOptimzeMD = new Button();
            buttonShrink = new Button();
            SuspendLayout();
            // 
            // textBoxMsg
            // 
            textBoxMsg.BackColor = SystemColors.Desktop;
            textBoxMsg.ForeColor = Color.Khaki;
            textBoxMsg.Location = new Point(12, 167);
            textBoxMsg.Multiline = true;
            textBoxMsg.Name = "textBoxMsg";
            textBoxMsg.Size = new Size(776, 426);
            textBoxMsg.TabIndex = 0;
            // 
            // buttonOptimzeMD
            // 
            buttonOptimzeMD.Location = new Point(31, 12);
            buttonOptimzeMD.Name = "buttonOptimzeMD";
            buttonOptimzeMD.Size = new Size(94, 23);
            buttonOptimzeMD.TabIndex = 1;
            buttonOptimzeMD.Text = "OptimzeMD";
            buttonOptimzeMD.UseVisualStyleBackColor = true;
            buttonOptimzeMD.Click += buttonOptimzeMD_Click;
            // 
            // buttonShrink
            // 
            buttonShrink.Location = new Point(31, 41);
            buttonShrink.Name = "buttonShrink";
            buttonShrink.Size = new Size(94, 23);
            buttonShrink.TabIndex = 2;
            buttonShrink.Text = "Shrink";
            buttonShrink.UseVisualStyleBackColor = true;
            buttonShrink.Click += buttonShrink_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 632);
            Controls.Add(buttonShrink);
            Controls.Add(buttonOptimzeMD);
            Controls.Add(textBoxMsg);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxMsg;
        private Button buttonOptimzeMD;
        private Button buttonShrink;
    }
}
