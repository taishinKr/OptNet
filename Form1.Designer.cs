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
            buttonMD1D = new Button();
            buttonMD2D = new Button();
            buttonMD3D = new Button();
            buttonResetAll = new Button();
            button1DOnly = new Button();
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
            buttonShrink.Location = new Point(31, 35);
            buttonShrink.Name = "buttonShrink";
            buttonShrink.Size = new Size(94, 23);
            buttonShrink.TabIndex = 2;
            buttonShrink.Text = "Shrink";
            buttonShrink.UseVisualStyleBackColor = true;
            buttonShrink.Click += buttonShrink_Click;
            // 
            // buttonMD1D
            // 
            buttonMD1D.Location = new Point(149, 12);
            buttonMD1D.Name = "buttonMD1D";
            buttonMD1D.Size = new Size(94, 23);
            buttonMD1D.TabIndex = 3;
            buttonMD1D.Text = "MD1D";
            buttonMD1D.UseVisualStyleBackColor = true;
            buttonMD1D.Click += buttonMD1D_Click;
            // 
            // buttonMD2D
            // 
            buttonMD2D.Location = new Point(149, 35);
            buttonMD2D.Name = "buttonMD2D";
            buttonMD2D.Size = new Size(94, 23);
            buttonMD2D.TabIndex = 4;
            buttonMD2D.Text = "MD2D";
            buttonMD2D.UseVisualStyleBackColor = true;
            buttonMD2D.Click += buttonMD2D_Click;
            // 
            // buttonMD3D
            // 
            buttonMD3D.Location = new Point(149, 58);
            buttonMD3D.Name = "buttonMD3D";
            buttonMD3D.Size = new Size(94, 23);
            buttonMD3D.TabIndex = 5;
            buttonMD3D.Text = "MD3D";
            buttonMD3D.UseVisualStyleBackColor = true;
            buttonMD3D.Click += buttonMD3D_Click;
            // 
            // buttonResetAll
            // 
            buttonResetAll.Location = new Point(31, 58);
            buttonResetAll.Name = "buttonResetAll";
            buttonResetAll.Size = new Size(94, 23);
            buttonResetAll.TabIndex = 6;
            buttonResetAll.Text = "ResetAll";
            buttonResetAll.UseVisualStyleBackColor = true;
            buttonResetAll.Click += buttonResetAll_Click;
            // 
            // button1DOnly
            // 
            button1DOnly.Location = new Point(149, 81);
            button1DOnly.Name = "button1DOnly";
            button1DOnly.Size = new Size(94, 23);
            button1DOnly.TabIndex = 7;
            button1DOnly.Text = "1DOnly";
            button1DOnly.UseVisualStyleBackColor = true;
            button1DOnly.Click += button1DOnly_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 632);
            Controls.Add(button1DOnly);
            Controls.Add(buttonResetAll);
            Controls.Add(buttonMD3D);
            Controls.Add(buttonMD2D);
            Controls.Add(buttonMD1D);
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
        private Button buttonMD1D;
        private Button buttonMD2D;
        private Button buttonMD3D;
        private Button buttonResetAll;
        private Button button1DOnly;
    }
}
