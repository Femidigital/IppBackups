namespace IppBackups
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.taskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oneOffBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbl_Environment = new System.Windows.Forms.Label();
            this.cBox_Environment = new System.Windows.Forms.ComboBox();
            this.grpBox_Databases = new System.Windows.Forms.GroupBox();
            this.grpBox_Output = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbl_Output = new System.Windows.Forms.Label();
            this.cBox_Server = new System.Windows.Forms.ComboBox();
            this.lbl_Server = new System.Windows.Forms.Label();
            this.rBtn_Backup = new System.Windows.Forms.RadioButton();
            this.rBtn_Restore = new System.Windows.Forms.RadioButton();
            this.rBtn_Refresh = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_Execute = new System.Windows.Forms.Button();
            this.backupbackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.backupbackgroundWorker.DoWork +=backupbackgroundWorker_DoWork;
            this.backupbackgroundWorker.ProgressChanged += backupbackgroundWorker_ProgressChanged;
            this.backupbackgroundWorker.RunWorkerCompleted += backupbackgroundWorker_RunWorkerCompleted;
            this.startAsynButton = new System.Windows.Forms.Button();
            this.cancelAsynButton = new System.Windows.Forms.Button();
            this.lbl_DestServer = new System.Windows.Forms.Label();
            this.cBox_DestServer = new System.Windows.Forms.ComboBox();
            this.lbl_DestEnvironment = new System.Windows.Forms.Label();
            this.cBox_DestEnvironment = new System.Windows.Forms.ComboBox();
            this.restorebackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.restorebackgroundWorker.DoWork += restorebackgroundWorker_DoWork;
            this.restorebackgroundWorker.ProgressChanged += restorebackgroundWorker_ProgressChanged;
            this.restorebackgroundWorker.RunWorkerCompleted += restorebackgroundWorker_RunWorkerCompleted;
            this.menuStrip1.SuspendLayout();
            this.grpBox_Output.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.taskToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(744, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // taskToolStripMenuItem
            // 
            this.taskToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oneOffBackupToolStripMenuItem,
            this.restoreToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.taskToolStripMenuItem.Name = "taskToolStripMenuItem";
            this.taskToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.taskToolStripMenuItem.Text = "Task";
            // 
            // oneOffBackupToolStripMenuItem
            // 
            this.oneOffBackupToolStripMenuItem.Name = "oneOffBackupToolStripMenuItem";
            this.oneOffBackupToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.oneOffBackupToolStripMenuItem.Text = "Backup";
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.restoreToolStripMenuItem.Text = "Restore";
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.serversToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // serversToolStripMenuItem
            // 
            this.serversToolStripMenuItem.Name = "serversToolStripMenuItem";
            this.serversToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.serversToolStripMenuItem.Text = "Servers...";
            this.serversToolStripMenuItem.Click += new System.EventHandler(this.serversToolStripMenuItem_Click);
            // 
            // lbl_Environment
            // 
            this.lbl_Environment.AutoSize = true;
            this.lbl_Environment.Location = new System.Drawing.Point(171, 44);
            this.lbl_Environment.Name = "lbl_Environment";
            this.lbl_Environment.Size = new System.Drawing.Size(66, 13);
            this.lbl_Environment.TabIndex = 1;
            this.lbl_Environment.Text = "Environment";
            // 
            // cBox_Environment
            // 
            this.cBox_Environment.FormattingEnabled = true;
            this.cBox_Environment.Location = new System.Drawing.Point(243, 41);
            this.cBox_Environment.Name = "cBox_Environment";
            this.cBox_Environment.Size = new System.Drawing.Size(96, 21);
            this.cBox_Environment.TabIndex = 2;
            this.cBox_Environment.SelectedIndexChanged += new System.EventHandler(this.cBox_Environment_SelectedIndexChanged);
            // 
            // grpBox_Databases
            // 
            this.grpBox_Databases.Location = new System.Drawing.Point(15, 110);
            this.grpBox_Databases.Name = "grpBox_Databases";
            this.grpBox_Databases.Size = new System.Drawing.Size(717, 344);
            this.grpBox_Databases.TabIndex = 3;
            this.grpBox_Databases.TabStop = false;
            this.grpBox_Databases.Text = "Databases:";
            // 
            // grpBox_Output
            // 
            this.grpBox_Output.Controls.Add(this.panel2);
            this.grpBox_Output.Location = new System.Drawing.Point(15, 460);
            this.grpBox_Output.Name = "grpBox_Output";
            this.grpBox_Output.Size = new System.Drawing.Size(717, 174);
            this.grpBox_Output.TabIndex = 4;
            this.grpBox_Output.TabStop = false;
            this.grpBox_Output.Text = "Output";
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.lbl_Output);
            this.panel2.Location = new System.Drawing.Point(3, 19);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(708, 149);
            this.panel2.TabIndex = 8;
            // 
            // lbl_Output
            // 
            this.lbl_Output.AutoSize = true;
            this.lbl_Output.Location = new System.Drawing.Point(6, 16);
            this.lbl_Output.Name = "lbl_Output";
            this.lbl_Output.Size = new System.Drawing.Size(0, 13);
            this.lbl_Output.TabIndex = 7;
            // 
            // cBox_Server
            // 
            this.cBox_Server.FormattingEnabled = true;
            this.cBox_Server.Location = new System.Drawing.Point(59, 41);
            this.cBox_Server.Name = "cBox_Server";
            this.cBox_Server.Size = new System.Drawing.Size(97, 21);
            this.cBox_Server.TabIndex = 5;
            this.cBox_Server.SelectedIndexChanged += new System.EventHandler(this.cBox_Server_SelectedIndexChanged);
            // 
            // lbl_Server
            // 
            this.lbl_Server.AutoSize = true;
            this.lbl_Server.Location = new System.Drawing.Point(15, 44);
            this.lbl_Server.Name = "lbl_Server";
            this.lbl_Server.Size = new System.Drawing.Size(38, 13);
            this.lbl_Server.TabIndex = 6;
            this.lbl_Server.Text = "Server";
            // 
            // rBtn_Backup
            // 
            this.rBtn_Backup.AutoSize = true;
            this.rBtn_Backup.Location = new System.Drawing.Point(16, 3);
            this.rBtn_Backup.Name = "rBtn_Backup";
            this.rBtn_Backup.Size = new System.Drawing.Size(62, 17);
            this.rBtn_Backup.TabIndex = 7;
            this.rBtn_Backup.TabStop = true;
            this.rBtn_Backup.Text = "Backup";
            this.rBtn_Backup.UseVisualStyleBackColor = true;
            // 
            // rBtn_Restore
            // 
            this.rBtn_Restore.AutoSize = true;
            this.rBtn_Restore.Location = new System.Drawing.Point(160, 3);
            this.rBtn_Restore.Name = "rBtn_Restore";
            this.rBtn_Restore.Size = new System.Drawing.Size(62, 17);
            this.rBtn_Restore.TabIndex = 8;
            this.rBtn_Restore.TabStop = true;
            this.rBtn_Restore.Text = "Restore";
            this.rBtn_Restore.UseVisualStyleBackColor = true;
            // 
            // rBtn_Refresh
            // 
            this.rBtn_Refresh.AutoSize = true;
            this.rBtn_Refresh.Location = new System.Drawing.Point(317, 3);
            this.rBtn_Refresh.Name = "rBtn_Refresh";
            this.rBtn_Refresh.Size = new System.Drawing.Size(62, 17);
            this.rBtn_Refresh.TabIndex = 9;
            this.rBtn_Refresh.TabStop = true;
            this.rBtn_Refresh.Text = "Refresh";
            this.rBtn_Refresh.UseVisualStyleBackColor = true;
            this.rBtn_Refresh.CheckedChanged += new System.EventHandler(this.rBtn_Refresh_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rBtn_Restore);
            this.panel1.Controls.Add(this.rBtn_Refresh);
            this.panel1.Controls.Add(this.rBtn_Backup);
            this.panel1.Location = new System.Drawing.Point(15, 81);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(717, 23);
            this.panel1.TabIndex = 10;
            // 
            // btn_Execute
            // 
            this.btn_Execute.Location = new System.Drawing.Point(651, 640);
            this.btn_Execute.Name = "btn_Execute";
            this.btn_Execute.Size = new System.Drawing.Size(75, 23);
            this.btn_Execute.TabIndex = 8;
            this.btn_Execute.Text = "Execute";
            this.btn_Execute.UseVisualStyleBackColor = true;
            this.btn_Execute.Click += new System.EventHandler(this.btn_Execute_Click);
            // 
            // startAsynButton
            // 
            this.startAsynButton.Location = new System.Drawing.Point(539, 640);
            this.startAsynButton.Name = "startAsynButton";
            this.startAsynButton.Size = new System.Drawing.Size(75, 23);
            this.startAsynButton.TabIndex = 11;
            this.startAsynButton.Text = "Start";
            this.startAsynButton.UseVisualStyleBackColor = true;
            // 
            // cancelAsynButton
            // 
            this.cancelAsynButton.Location = new System.Drawing.Point(408, 640);
            this.cancelAsynButton.Name = "cancelAsynButton";
            this.cancelAsynButton.Size = new System.Drawing.Size(75, 23);
            this.cancelAsynButton.TabIndex = 12;
            this.cancelAsynButton.Text = "Cancel";
            this.cancelAsynButton.UseVisualStyleBackColor = true;
            this.cancelAsynButton.Click += new System.EventHandler(this.cancelAsynButton_Click);
            // 
            // lbl_DestServer
            // 
            this.lbl_DestServer.AutoSize = true;
            this.lbl_DestServer.Location = new System.Drawing.Point(405, 44);
            this.lbl_DestServer.Name = "lbl_DestServer";
            this.lbl_DestServer.Size = new System.Drawing.Size(38, 13);
            this.lbl_DestServer.TabIndex = 13;
            this.lbl_DestServer.Text = "Server";
            // 
            // cBox_DestServer
            // 
            this.cBox_DestServer.Enabled = false;
            this.cBox_DestServer.FormattingEnabled = true;
            this.cBox_DestServer.Location = new System.Drawing.Point(449, 41);
            this.cBox_DestServer.Name = "cBox_DestServer";
            this.cBox_DestServer.Size = new System.Drawing.Size(98, 21);
            this.cBox_DestServer.TabIndex = 14;
            this.cBox_DestServer.SelectedIndexChanged += new System.EventHandler(this.cBox_DestServer_SelectedIndexChanged);
            // 
            // lbl_DestEnvironment
            // 
            this.lbl_DestEnvironment.AutoSize = true;
            this.lbl_DestEnvironment.Location = new System.Drawing.Point(553, 44);
            this.lbl_DestEnvironment.Name = "lbl_DestEnvironment";
            this.lbl_DestEnvironment.Size = new System.Drawing.Size(66, 13);
            this.lbl_DestEnvironment.TabIndex = 15;
            this.lbl_DestEnvironment.Text = "Environment";
            // 
            // cBox_DestEnvironment
            // 
            this.cBox_DestEnvironment.Enabled = false;
            this.cBox_DestEnvironment.FormattingEnabled = true;
            this.cBox_DestEnvironment.Location = new System.Drawing.Point(625, 38);
            this.cBox_DestEnvironment.Name = "cBox_DestEnvironment";
            this.cBox_DestEnvironment.Size = new System.Drawing.Size(109, 21);
            this.cBox_DestEnvironment.TabIndex = 16;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 661);
            this.Controls.Add(this.cBox_DestEnvironment);
            this.Controls.Add(this.lbl_DestEnvironment);
            this.Controls.Add(this.cBox_DestServer);
            this.Controls.Add(this.lbl_DestServer);
            this.Controls.Add(this.cancelAsynButton);
            this.Controls.Add(this.startAsynButton);
            this.Controls.Add(this.btn_Execute);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lbl_Server);
            this.Controls.Add(this.cBox_Server);
            this.Controls.Add(this.grpBox_Output);
            this.Controls.Add(this.grpBox_Databases);
            this.Controls.Add(this.cBox_Environment);
            this.Controls.Add(this.lbl_Environment);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.grpBox_Output.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem taskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oneOffBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serversToolStripMenuItem;
        private System.Windows.Forms.Label lbl_Environment;
        private System.Windows.Forms.ComboBox cBox_Environment;
        private System.Windows.Forms.GroupBox grpBox_Databases;
        private System.Windows.Forms.GroupBox grpBox_Output;
        private System.Windows.Forms.ComboBox cBox_Server;
        private System.Windows.Forms.Label lbl_Server;
        private System.Windows.Forms.Label lbl_Output;
        private System.Windows.Forms.RadioButton rBtn_Backup;
        private System.Windows.Forms.RadioButton rBtn_Restore;
        private System.Windows.Forms.RadioButton rBtn_Refresh;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_Execute;
        private System.ComponentModel.BackgroundWorker backupbackgroundWorker;
        private System.Windows.Forms.Button startAsynButton;
        private System.Windows.Forms.Button cancelAsynButton;
        private System.Windows.Forms.Label lbl_DestServer;
        private System.Windows.Forms.ComboBox cBox_DestServer;
        private System.Windows.Forms.Label lbl_DestEnvironment;
        private System.Windows.Forms.ComboBox cBox_DestEnvironment;
        private System.ComponentModel.BackgroundWorker restorebackgroundWorker;
    }
}

