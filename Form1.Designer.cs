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
            SuspendLayout();
            // 
            // textBoxMsg
            // 
            textBoxMsg.BackColor = SystemColors.Desktop;
            textBoxMsg.ForeColor = Color.Khaki;
            textBoxMsg.Location = new Point(12, 12);
            textBoxMsg.Multiline = true;
            textBoxMsg.Name = "textBoxMsg";
            textBoxMsg.Size = new Size(776, 426);
            textBoxMsg.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBoxMsg);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxMsg;
    }
}
