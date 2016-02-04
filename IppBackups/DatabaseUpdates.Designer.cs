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
            this.rBtn_Update = new System.Windows.Forms.RadioButton();
            this.rBtn_Replace = new System.Windows.Forms.RadioButton();
            this.rBtn_Insert = new System.Windows.Forms.RadioButton();
            this.rBtn_Delete = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.grpBox_DML.SuspendLayout();
            this.SuspendLayout();
            // 
            // rTxtBox_Script
            // 
            this.rTxtBox_Script.Location = new System.Drawing.Point(3, 215);
            this.rTxtBox_Script.Name = "rTxtBox_Script";
            this.rTxtBox_Script.Size = new System.Drawing.Size(1142, 204);
            this.rTxtBox_Script.TabIndex = 0;
            this.rTxtBox_Script.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tlp_ScriptBuilder);
            this.panel1.Controls.Add(this.rTxtBox_Script);
            this.panel1.Location = new System.Drawing.Point(12, 81);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1148, 418);
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
            this.tlp_ScriptBuilder.Name = "tlp_ScriptBuilder";
            this.tlp_ScriptBuilder.RowCount = 2;
            this.tlp_ScriptBuilder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_ScriptBuilder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_ScriptBuilder.Size = new System.Drawing.Size(200, 100);
            this.tlp_ScriptBuilder.TabIndex = 1;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(12, 9);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(77, 17);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Database :";
            // 
            // lbl_DatabaseName
            // 
            this.lbl_DatabaseName.AutoSize = true;
            this.lbl_DatabaseName.Location = new System.Drawing.Point(0, 0);
            this.lbl_DatabaseName.Name = "lbl_DatabaseName";
            this.lbl_DatabaseName.Size = new System.Drawing.Size(0, 17);
            this.lbl_DatabaseName.TabIndex = 3;
            // 
            // grpBox_DML
            // 
            this.grpBox_DML.Controls.Add(this.rBtn_Delete);
            this.grpBox_DML.Controls.Add(this.rBtn_Insert);
            this.grpBox_DML.Controls.Add(this.rBtn_Replace);
            this.grpBox_DML.Controls.Add(this.rBtn_Update);
            this.grpBox_DML.Location = new System.Drawing.Point(451, 21);
            this.grpBox_DML.Name = "grpBox_DML";
            this.grpBox_DML.Size = new System.Drawing.Size(706, 54);
            this.grpBox_DML.TabIndex = 4;
            this.grpBox_DML.TabStop = false;
            this.grpBox_DML.Text = "DML Type";
            // 
            // rBtn_Update
            // 
            this.rBtn_Update.AutoSize = true;
            this.rBtn_Update.Location = new System.Drawing.Point(6, 21);
            this.rBtn_Update.Name = "rBtn_Update";
            this.rBtn_Update.Size = new System.Drawing.Size(85, 21);
            this.rBtn_Update.TabIndex = 0;
            this.rBtn_Update.TabStop = true;
            this.rBtn_Update.Text = "UPDATE";
            this.rBtn_Update.UseVisualStyleBackColor = true;
            // 
            // rBtn_Replace
            // 
            this.rBtn_Replace.AutoSize = true;
            this.rBtn_Replace.Location = new System.Drawing.Point(212, 18);
            this.rBtn_Replace.Name = "rBtn_Replace";
            this.rBtn_Replace.Size = new System.Drawing.Size(92, 21);
            this.rBtn_Replace.TabIndex = 1;
            this.rBtn_Replace.TabStop = true;
            this.rBtn_Replace.Text = "REPLACE";
            this.rBtn_Replace.UseVisualStyleBackColor = true;
            // 
            // rBtn_Insert
            // 
            this.rBtn_Insert.AutoSize = true;
            this.rBtn_Insert.Location = new System.Drawing.Point(417, 18);
            this.rBtn_Insert.Name = "rBtn_Insert";
            this.rBtn_Insert.Size = new System.Drawing.Size(79, 21);
            this.rBtn_Insert.TabIndex = 2;
            this.rBtn_Insert.TabStop = true;
            this.rBtn_Insert.Text = "INSERT";
            this.rBtn_Insert.UseVisualStyleBackColor = true;
            // 
            // rBtn_Delete
            // 
            this.rBtn_Delete.AutoSize = true;
            this.rBtn_Delete.Location = new System.Drawing.Point(581, 18);
            this.rBtn_Delete.Name = "rBtn_Delete";
            this.rBtn_Delete.Size = new System.Drawing.Size(83, 21);
            this.rBtn_Delete.TabIndex = 3;
            this.rBtn_Delete.TabStop = true;
            this.rBtn_Delete.Text = "DELETE";
            this.rBtn_Delete.UseVisualStyleBackColor = true;
            // 
            // DatabaseUpdates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1172, 548);
            this.Controls.Add(this.grpBox_DML);
            this.Controls.Add(this.lbl_DatabaseName);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.panel1);
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
    }
}