namespace IppBackups
{
    partial class DatabaseUpdates
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
            this.rTxtBox_Script = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tlp_ScriptBuilder = new System.Windows.Forms.TableLayoutPanel();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lbl_DatabaseName = new System.Windows.Forms.Label();
            this.grpBox_DML = new System.Windows.Forms.GroupBox();
            this.rBtn_Delete = new System.Windows.Forms.RadioButton();
            this.rBtn_Insert = new System.Windows.Forms.RadioButton();
            this.rBtn_Replace = new System.Windows.Forms.RadioButton();
            this.rBtn_Update = new System.Windows.Forms.RadioButton();
            this.btn_Commit = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.grpBox_DML.SuspendLayout();
            this.SuspendLayout();
            // 
            // rTxtBox_Script
            // 
            this.rTxtBox_Script.Location = new System.Drawing.Point(2, 175);
            this.rTxtBox_Script.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rTxtBox_Script.Name = "rTxtBox_Script";
            this.rTxtBox_Script.Size = new System.Drawing.Size(858, 166);
            this.rTxtBox_Script.TabIndex = 0;
            this.rTxtBox_Script.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tlp_ScriptBuilder);
            this.panel1.Controls.Add(this.rTxtBox_Script);
            this.panel1.Location = new System.Drawing.Point(9, 66);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(861, 340);
            this.panel1.TabIndex = 1;
            // 
            // tlp_ScriptBuilder
            // 
            this.tlp_ScriptBuilder.ColumnCount = 4;
            this.tlp_ScriptBuilder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp_ScriptBuilder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp_ScriptBuilder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp_ScriptBuilder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp_ScriptBuilder.Location = new System.Drawing.Point(0, 0);
            this.tlp_ScriptBuilder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tlp_ScriptBuilder.Name = "tlp_ScriptBuilder";
            this.tlp_ScriptBuilder.RowCount = 2;
            this.tlp_ScriptBuilder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_ScriptBuilder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_ScriptBuilder.Size = new System.Drawing.Size(150, 81);
            this.tlp_ScriptBuilder.TabIndex = 1;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(13, 9);
            this.lblDatabase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(59, 13);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Database :";
            // 
            // lbl_DatabaseName
            // 
            this.lbl_DatabaseName.AutoSize = true;
            this.lbl_DatabaseName.Location = new System.Drawing.Point(76, 9);
            this.lbl_DatabaseName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_DatabaseName.Name = "lbl_DatabaseName";
            this.lbl_DatabaseName.Size = new System.Drawing.Size(0, 13);
            this.lbl_DatabaseName.TabIndex = 3;
            // 
            // grpBox_DML
            // 
            this.grpBox_DML.Controls.Add(this.rBtn_Delete);
            this.grpBox_DML.Controls.Add(this.rBtn_Insert);
            this.grpBox_DML.Controls.Add(this.rBtn_Replace);
            this.grpBox_DML.Controls.Add(this.rBtn_Update);
            this.grpBox_DML.Location = new System.Drawing.Point(338, 17);
            this.grpBox_DML.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpBox_DML.Name = "grpBox_DML";
            this.grpBox_DML.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpBox_DML.Size = new System.Drawing.Size(530, 44);
            this.grpBox_DML.TabIndex = 4;
            this.grpBox_DML.TabStop = false;
            this.grpBox_DML.Text = "DML Type";
            // 
            // rBtn_Delete
            // 
            this.rBtn_Delete.AutoSize = true;
            this.rBtn_Delete.Location = new System.Drawing.Point(436, 15);
            this.rBtn_Delete.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rBtn_Delete.Name = "rBtn_Delete";
            this.rBtn_Delete.Size = new System.Drawing.Size(67, 17);
            this.rBtn_Delete.TabIndex = 3;
            this.rBtn_Delete.TabStop = true;
            this.rBtn_Delete.Text = "DELETE";
            this.rBtn_Delete.UseVisualStyleBackColor = true;
            // 
            // rBtn_Insert
            // 
            this.rBtn_Insert.AutoSize = true;
            this.rBtn_Insert.Location = new System.Drawing.Point(313, 15);
            this.rBtn_Insert.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rBtn_Insert.Name = "rBtn_Insert";
            this.rBtn_Insert.Size = new System.Drawing.Size(65, 17);
            this.rBtn_Insert.TabIndex = 2;
            this.rBtn_Insert.TabStop = true;
            this.rBtn_Insert.Text = "INSERT";
            this.rBtn_Insert.UseVisualStyleBackColor = true;
            // 
            // rBtn_Replace
            // 
            this.rBtn_Replace.AutoSize = true;
            this.rBtn_Replace.Location = new System.Drawing.Point(159, 15);
            this.rBtn_Replace.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rBtn_Replace.Name = "rBtn_Replace";
            this.rBtn_Replace.Size = new System.Drawing.Size(74, 17);
            this.rBtn_Replace.TabIndex = 1;
            this.rBtn_Replace.TabStop = true;
            this.rBtn_Replace.Text = "REPLACE";
            this.rBtn_Replace.UseVisualStyleBackColor = true;
            // 
            // rBtn_Update
            // 
            this.rBtn_Update.AutoSize = true;
            this.rBtn_Update.Location = new System.Drawing.Point(4, 17);
            this.rBtn_Update.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rBtn_Update.Name = "rBtn_Update";
            this.rBtn_Update.Size = new System.Drawing.Size(69, 17);
            this.rBtn_Update.TabIndex = 0;
            this.rBtn_Update.TabStop = true;
            this.rBtn_Update.Text = "UPDATE";
            this.rBtn_Update.UseVisualStyleBackColor = true;
            // 
            // btn_Commit
            // 
            this.btn_Commit.Location = new System.Drawing.Point(792, 412);
            this.btn_Commit.Name = "btn_Commit";
            this.btn_Commit.Size = new System.Drawing.Size(75, 23);
            this.btn_Commit.TabIndex = 5;
            this.btn_Commit.Text = "Commit";
            this.btn_Commit.UseVisualStyleBackColor = true;
            this.btn_Commit.Click += new System.EventHandler(this.btn_Commit_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(676, 412);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(75, 23);
            this.btn_Close.TabIndex = 6;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // DatabaseUpdates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 445);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Commit);
            this.Controls.Add(this.grpBox_DML);
            this.Controls.Add(this.lbl_DatabaseName);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DatabaseUpdates";
            this.Text = "DatabaseUpdates";
            this.panel1.ResumeLayout(false);
            this.grpBox_DML.ResumeLayout(false);
            this.grpBox_DML.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rTxtBox_Script;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tlp_ScriptBuilder;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.Label lbl_DatabaseName;
        private System.Windows.Forms.GroupBox grpBox_DML;
        private System.Windows.Forms.RadioButton rBtn_Delete;
        private System.Windows.Forms.RadioButton rBtn_Insert;
        private System.Windows.Forms.RadioButton rBtn_Replace;
        private System.Windows.Forms.RadioButton rBtn_Update;
        private System.Windows.Forms.Button btn_Commit;
        private System.Windows.Forms.Button btn_Close;
    }
}