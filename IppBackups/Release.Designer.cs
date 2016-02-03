namespace IppBackups
{
    partial class Release
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblSelectedDb = new System.Windows.Forms.Label();
            this.lblPath = new System.Windows.Forms.Label();
            this.tBox_Path = new System.Windows.Forms.TextBox();
            this.btn_Path = new System.Windows.Forms.Button();
            this.btn_Run = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.rTxtBox_Output = new System.Windows.Forms.RichTextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // lblSelectedDb
            // 
            this.lblSelectedDb.AutoSize = true;
            this.lblSelectedDb.Location = new System.Drawing.Point(2, 9);
            this.lblSelectedDb.Name = "lblSelectedDb";
            this.lblSelectedDb.Size = new System.Drawing.Size(101, 13);
            this.lblSelectedDb.TabIndex = 0;
            this.lblSelectedDb.Text = "Selected Database:";
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(2, 36);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(29, 13);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "Path";
            // 
            // tBox_Path
            // 
            this.tBox_Path.Location = new System.Drawing.Point(37, 33);
            this.tBox_Path.Name = "tBox_Path";
            this.tBox_Path.Size = new System.Drawing.Size(203, 20);
            this.tBox_Path.TabIndex = 2;
            // 
            // btn_Path
            // 
            this.btn_Path.Location = new System.Drawing.Point(246, 33);
            this.btn_Path.Name = "btn_Path";
            this.btn_Path.Size = new System.Drawing.Size(30, 23);
            this.btn_Path.TabIndex = 3;
            this.btn_Path.Text = "...";
            this.btn_Path.UseVisualStyleBackColor = true;
            this.btn_Path.Click += new System.EventHandler(this.btn_Path_Click);
            // 
            // btn_Run
            // 
            this.btn_Run.Location = new System.Drawing.Point(201, 482);
            this.btn_Run.Name = "btn_Run";
            this.btn_Run.Size = new System.Drawing.Size(75, 23);
            this.btn_Run.TabIndex = 4;
            this.btn_Run.Text = "Run";
            this.btn_Run.UseVisualStyleBackColor = true;
            this.btn_Run.Click += new System.EventHandler(this.btn_Run_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(12, 482);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(75, 23);
            this.btn_Close.TabIndex = 5;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // rTxtBox_Output
            // 
            this.rTxtBox_Output.Location = new System.Drawing.Point(12, 354);
            this.rTxtBox_Output.Name = "rTxtBox_Output";
            this.rTxtBox_Output.Size = new System.Drawing.Size(264, 122);
            this.rTxtBox_Output.TabIndex = 6;
            this.rTxtBox_Output.Text = "";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Release
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 517);
            this.Controls.Add(this.rTxtBox_Output);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Run);
            this.Controls.Add(this.btn_Path);
            this.Controls.Add(this.tBox_Path);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.lblSelectedDb);
            this.Name = "Release";
            this.Text = "Release";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSelectedDb;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox tBox_Path;
        private System.Windows.Forms.Button btn_Path;
        private System.Windows.Forms.Button btn_Run;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.RichTextBox rTxtBox_Output;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}