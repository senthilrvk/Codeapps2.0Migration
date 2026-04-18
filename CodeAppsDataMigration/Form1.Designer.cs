namespace CodeAppsDataMigration
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
            btnDataTransfer = new Button();
            SuspendLayout();
            //
            // btnDataTransfer
            //
            btnDataTransfer.Location = new Point(143, 72);
            btnDataTransfer.Name = "btnDataTransfer";
            btnDataTransfer.Size = new Size(179, 30);
            btnDataTransfer.TabIndex = 0;
            btnDataTransfer.Text = "Start Migration";
            btnDataTransfer.UseVisualStyleBackColor = true;
            btnDataTransfer.Click += btnDataTransfer_Click;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 200);
            Controls.Add(btnDataTransfer);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Data Migration";
            ResumeLayout(false);
        }

        #endregion

        private Button btnDataTransfer;
    }
}
