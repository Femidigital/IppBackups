﻿namespace IppBackups
{
    partial class Settings
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.lbl_BackupLocation = new System.Windows.Forms.Label();
            this.tBox_BackupLocation = new System.Windows.Forms.TextBox();
            this.btn_BackupDir = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tView_Servers = new System.Windows.Forms.TreeView();
            this.gBox_ServDetails = new System.Windows.Forms.GroupBox();
            this.txtBox_AzureKey = new System.Windows.Forms.TextBox();
            this.lbl_AzureKey = new System.Windows.Forms.Label();
            this.tBox_Port = new System.Windows.Forms.TextBox();
            this.lbl_Port = new System.Windows.Forms.Label();
            this.tBox_Instance = new System.Windows.Forms.TextBox();
            this.lbl_Instance = new System.Windows.Forms.Label();
            this.tBox_Password = new System.Windows.Forms.TextBox();
            this.tBox_Username = new System.Windows.Forms.TextBox();
            this.lbl_Password = new System.Windows.Forms.Label();
            this.lbl_Username = new System.Windows.Forms.Label();
            this.tBox_IPaddress = new System.Windows.Forms.TextBox();
            this.lbl_IPaddress = new System.Windows.Forms.Label();
            this.tBox_ServerName = new System.Windows.Forms.TextBox();
            this.lbl_ServerName = new System.Windows.Forms.Label();
            this.gBox_EnvInfo = new System.Windows.Forms.GroupBox();
            this.lbl_Environment = new System.Windows.Forms.Label();
            this.tBox_Environment = new System.Windows.Forms.TextBox();
            this.tBox_LogFiles = new System.Windows.Forms.TextBox();
            this.tBox_DataFile = new System.Windows.Forms.TextBox();
            this.lbl_LogFile = new System.Windows.Forms.Label();
            this.lbl_DataFile = new System.Windows.Forms.Label();
            this.btn_Close = new System.Windows.Forms.Button();
            this.tvServers_imageList = new System.Windows.Forms.ImageList(this.components);
            this.btn_Apply = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.lbl_Notice = new System.Windows.Forms.Label();
            this.chkBox_Azure = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.gBox_ServDetails.SuspendLayout();
            this.gBox_EnvInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_BackupLocation
            // 
            this.lbl_BackupLocation.AutoSize = true;
            this.lbl_BackupLocation.Location = new System.Drawing.Point(5, 271);
            this.lbl_BackupLocation.Name = "lbl_BackupLocation";
            this.lbl_BackupLocation.Size = new System.Drawing.Size(88, 17);
            this.lbl_BackupLocation.TabIndex = 0;
            this.lbl_BackupLocation.Text = "Backup Path";
            // 
            // tBox_BackupLocation
            // 
            this.tBox_BackupLocation.Location = new System.Drawing.Point(99, 268);
            this.tBox_BackupLocation.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_BackupLocation.Name = "tBox_BackupLocation";
            this.tBox_BackupLocation.Size = new System.Drawing.Size(149, 22);
            this.tBox_BackupLocation.TabIndex = 1;
            this.tBox_BackupLocation.TextChanged += new System.EventHandler(this.tBox_BackupLocation_TextChanged);
            // 
            // btn_BackupDir
            // 
            this.btn_BackupDir.Location = new System.Drawing.Point(237, 268);
            this.btn_BackupDir.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_BackupDir.Name = "btn_BackupDir";
            this.btn_BackupDir.Size = new System.Drawing.Size(47, 25);
            this.btn_BackupDir.TabIndex = 2;
            this.btn_BackupDir.Text = "...";
            this.btn_BackupDir.UseVisualStyleBackColor = true;
            this.btn_BackupDir.Click += new System.EventHandler(this.btn_BackupDir_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tView_Servers);
            this.groupBox1.Location = new System.Drawing.Point(15, 11);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(227, 514);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Servers";
            // 
            // tView_Servers
            // 
            this.tView_Servers.Location = new System.Drawing.Point(3, 18);
            this.tView_Servers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tView_Servers.Name = "tView_Servers";
            this.tView_Servers.Size = new System.Drawing.Size(121, 98);
            this.tView_Servers.TabIndex = 0;
            // 
            // gBox_ServDetails
            // 
            this.gBox_ServDetails.Controls.Add(this.chkBox_Azure);
            this.gBox_ServDetails.Controls.Add(this.txtBox_AzureKey);
            this.gBox_ServDetails.Controls.Add(this.lbl_AzureKey);
            this.gBox_ServDetails.Controls.Add(this.tBox_Port);
            this.gBox_ServDetails.Controls.Add(this.lbl_Port);
            this.gBox_ServDetails.Controls.Add(this.tBox_Instance);
            this.gBox_ServDetails.Controls.Add(this.lbl_Instance);
            this.gBox_ServDetails.Controls.Add(this.tBox_Password);
            this.gBox_ServDetails.Controls.Add(this.tBox_Username);
            this.gBox_ServDetails.Controls.Add(this.lbl_Password);
            this.gBox_ServDetails.Controls.Add(this.lbl_Username);
            this.gBox_ServDetails.Controls.Add(this.tBox_IPaddress);
            this.gBox_ServDetails.Controls.Add(this.btn_BackupDir);
            this.gBox_ServDetails.Controls.Add(this.tBox_BackupLocation);
            this.gBox_ServDetails.Controls.Add(this.lbl_IPaddress);
            this.gBox_ServDetails.Controls.Add(this.lbl_BackupLocation);
            this.gBox_ServDetails.Controls.Add(this.tBox_ServerName);
            this.gBox_ServDetails.Controls.Add(this.lbl_ServerName);
            this.gBox_ServDetails.Location = new System.Drawing.Point(259, 11);
            this.gBox_ServDetails.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gBox_ServDetails.Name = "gBox_ServDetails";
            this.gBox_ServDetails.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gBox_ServDetails.Size = new System.Drawing.Size(325, 339);
            this.gBox_ServDetails.TabIndex = 4;
            this.gBox_ServDetails.TabStop = false;
            this.gBox_ServDetails.Text = "Server Details";
            // 
            // txtBox_AzureKey
            // 
            this.txtBox_AzureKey.Location = new System.Drawing.Point(101, 304);
            this.txtBox_AzureKey.Name = "txtBox_AzureKey";
            this.txtBox_AzureKey.Size = new System.Drawing.Size(183, 22);
            this.txtBox_AzureKey.TabIndex = 9;
            // 
            // lbl_AzureKey
            // 
            this.lbl_AzureKey.AutoSize = true;
            this.lbl_AzureKey.Location = new System.Drawing.Point(5, 307);
            this.lbl_AzureKey.Name = "lbl_AzureKey";
            this.lbl_AzureKey.Size = new System.Drawing.Size(73, 17);
            this.lbl_AzureKey.TabIndex = 12;
            this.lbl_AzureKey.Text = "Azure Key";
            // 
            // tBox_Port
            // 
            this.tBox_Port.Location = new System.Drawing.Point(101, 153);
            this.tBox_Port.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_Port.Name = "tBox_Port";
            this.tBox_Port.Size = new System.Drawing.Size(94, 22);
            this.tBox_Port.TabIndex = 11;
            // 
            // lbl_Port
            // 
            this.lbl_Port.AutoSize = true;
            this.lbl_Port.Location = new System.Drawing.Point(9, 153);
            this.lbl_Port.Name = "lbl_Port";
            this.lbl_Port.Size = new System.Drawing.Size(34, 17);
            this.lbl_Port.TabIndex = 10;
            this.lbl_Port.Text = "Port";
            // 
            // tBox_Instance
            // 
            this.tBox_Instance.Location = new System.Drawing.Point(101, 68);
            this.tBox_Instance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_Instance.Name = "tBox_Instance";
            this.tBox_Instance.Size = new System.Drawing.Size(181, 22);
            this.tBox_Instance.TabIndex = 9;
            // 
            // lbl_Instance
            // 
            this.lbl_Instance.AutoSize = true;
            this.lbl_Instance.Location = new System.Drawing.Point(11, 71);
            this.lbl_Instance.Name = "lbl_Instance";
            this.lbl_Instance.Size = new System.Drawing.Size(61, 17);
            this.lbl_Instance.TabIndex = 8;
            this.lbl_Instance.Text = "Instance";
            // 
            // tBox_Password
            // 
            this.tBox_Password.Location = new System.Drawing.Point(100, 234);
            this.tBox_Password.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_Password.Name = "tBox_Password";
            this.tBox_Password.Size = new System.Drawing.Size(181, 22);
            this.tBox_Password.TabIndex = 7;
            this.tBox_Password.UseSystemPasswordChar = true;
            this.tBox_Password.TextChanged += new System.EventHandler(this.tBox_Password_TextChanged);
            // 
            // tBox_Username
            // 
            this.tBox_Username.Location = new System.Drawing.Point(101, 194);
            this.tBox_Username.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_Username.Name = "tBox_Username";
            this.tBox_Username.Size = new System.Drawing.Size(181, 22);
            this.tBox_Username.TabIndex = 6;
            this.tBox_Username.TextChanged += new System.EventHandler(this.tBox_Username_TextChanged);
            // 
            // lbl_Password
            // 
            this.lbl_Password.AutoSize = true;
            this.lbl_Password.Location = new System.Drawing.Point(5, 239);
            this.lbl_Password.Name = "lbl_Password";
            this.lbl_Password.Size = new System.Drawing.Size(69, 17);
            this.lbl_Password.TabIndex = 5;
            this.lbl_Password.Text = "Password";
            // 
            // lbl_Username
            // 
            this.lbl_Username.AutoSize = true;
            this.lbl_Username.Location = new System.Drawing.Point(5, 199);
            this.lbl_Username.Name = "lbl_Username";
            this.lbl_Username.Size = new System.Drawing.Size(73, 17);
            this.lbl_Username.TabIndex = 4;
            this.lbl_Username.Text = "Username";
            // 
            // tBox_IPaddress
            // 
            this.tBox_IPaddress.Location = new System.Drawing.Point(104, 110);
            this.tBox_IPaddress.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_IPaddress.Name = "tBox_IPaddress";
            this.tBox_IPaddress.Size = new System.Drawing.Size(181, 22);
            this.tBox_IPaddress.TabIndex = 3;
            this.tBox_IPaddress.TextChanged += new System.EventHandler(this.tBox_IPaddress_TextChanged);
            this.tBox_IPaddress.Validating += new System.ComponentModel.CancelEventHandler(this.tBox_IPaddress_Validating);
            this.tBox_IPaddress.Validated += new System.EventHandler(this.tBox_IPaddress_Validated);
            // 
            // lbl_IPaddress
            // 
            this.lbl_IPaddress.AutoSize = true;
            this.lbl_IPaddress.Location = new System.Drawing.Point(9, 114);
            this.lbl_IPaddress.Name = "lbl_IPaddress";
            this.lbl_IPaddress.Size = new System.Drawing.Size(76, 17);
            this.lbl_IPaddress.TabIndex = 2;
            this.lbl_IPaddress.Text = "IP Address";
            // 
            // tBox_ServerName
            // 
            this.tBox_ServerName.Location = new System.Drawing.Point(101, 30);
            this.tBox_ServerName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_ServerName.Name = "tBox_ServerName";
            this.tBox_ServerName.Size = new System.Drawing.Size(181, 22);
            this.tBox_ServerName.TabIndex = 1;
            // 
            // lbl_ServerName
            // 
            this.lbl_ServerName.AutoSize = true;
            this.lbl_ServerName.Location = new System.Drawing.Point(4, 33);
            this.lbl_ServerName.Name = "lbl_ServerName";
            this.lbl_ServerName.Size = new System.Drawing.Size(91, 17);
            this.lbl_ServerName.TabIndex = 0;
            this.lbl_ServerName.Text = "Server Name";
            // 
            // gBox_EnvInfo
            // 
            this.gBox_EnvInfo.Controls.Add(this.lbl_Environment);
            this.gBox_EnvInfo.Controls.Add(this.tBox_Environment);
            this.gBox_EnvInfo.Controls.Add(this.tBox_LogFiles);
            this.gBox_EnvInfo.Controls.Add(this.tBox_DataFile);
            this.gBox_EnvInfo.Controls.Add(this.lbl_LogFile);
            this.gBox_EnvInfo.Controls.Add(this.lbl_DataFile);
            this.gBox_EnvInfo.Location = new System.Drawing.Point(259, 367);
            this.gBox_EnvInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gBox_EnvInfo.Name = "gBox_EnvInfo";
            this.gBox_EnvInfo.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gBox_EnvInfo.Size = new System.Drawing.Size(325, 158);
            this.gBox_EnvInfo.TabIndex = 5;
            this.gBox_EnvInfo.TabStop = false;
            this.gBox_EnvInfo.Text = "Environment Info";
            // 
            // lbl_Environment
            // 
            this.lbl_Environment.AutoSize = true;
            this.lbl_Environment.Location = new System.Drawing.Point(17, 35);
            this.lbl_Environment.Name = "lbl_Environment";
            this.lbl_Environment.Size = new System.Drawing.Size(87, 17);
            this.lbl_Environment.TabIndex = 5;
            this.lbl_Environment.Text = "Environment";
            // 
            // tBox_Environment
            // 
            this.tBox_Environment.Location = new System.Drawing.Point(114, 30);
            this.tBox_Environment.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_Environment.Name = "tBox_Environment";
            this.tBox_Environment.Size = new System.Drawing.Size(181, 22);
            this.tBox_Environment.TabIndex = 4;
            // 
            // tBox_LogFiles
            // 
            this.tBox_LogFiles.Location = new System.Drawing.Point(91, 112);
            this.tBox_LogFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_LogFiles.Name = "tBox_LogFiles";
            this.tBox_LogFiles.Size = new System.Drawing.Size(204, 22);
            this.tBox_LogFiles.TabIndex = 3;
            this.tBox_LogFiles.TextChanged += new System.EventHandler(this.tBox_LogFiles_TextChanged);
            // 
            // tBox_DataFile
            // 
            this.tBox_DataFile.Location = new System.Drawing.Point(91, 71);
            this.tBox_DataFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tBox_DataFile.Name = "tBox_DataFile";
            this.tBox_DataFile.Size = new System.Drawing.Size(204, 22);
            this.tBox_DataFile.TabIndex = 2;
            this.tBox_DataFile.TextChanged += new System.EventHandler(this.tBox_DataFile_TextChanged);
            // 
            // lbl_LogFile
            // 
            this.lbl_LogFile.AutoSize = true;
            this.lbl_LogFile.Location = new System.Drawing.Point(19, 116);
            this.lbl_LogFile.Name = "lbl_LogFile";
            this.lbl_LogFile.Size = new System.Drawing.Size(65, 17);
            this.lbl_LogFile.TabIndex = 1;
            this.lbl_LogFile.Text = "Log Files";
            // 
            // lbl_DataFile
            // 
            this.lbl_DataFile.AutoSize = true;
            this.lbl_DataFile.Location = new System.Drawing.Point(17, 80);
            this.lbl_DataFile.Name = "lbl_DataFile";
            this.lbl_DataFile.Size = new System.Drawing.Size(71, 17);
            this.lbl_DataFile.TabIndex = 0;
            this.lbl_DataFile.Text = "Data Files";
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(440, 559);
            this.btn_Close.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(100, 28);
            this.btn_Close.TabIndex = 6;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // tvServers_imageList
            // 
            this.tvServers_imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("tvServers_imageList.ImageStream")));
            this.tvServers_imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.tvServers_imageList.Images.SetKeyName(0, "big_tree.jpg");
            this.tvServers_imageList.Images.SetKeyName(1, "Player Play.png");
            this.tvServers_imageList.Images.SetKeyName(2, "server.jpg");
            this.tvServers_imageList.Images.SetKeyName(3, "server_2.jpg");
            this.tvServers_imageList.Images.SetKeyName(4, "Gear.ico");
            this.tvServers_imageList.Images.SetKeyName(5, "gear02.png");
            // 
            // btn_Apply
            // 
            this.btn_Apply.Location = new System.Drawing.Point(264, 559);
            this.btn_Apply.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Apply.Name = "btn_Apply";
            this.btn_Apply.Size = new System.Drawing.Size(100, 28);
            this.btn_Apply.TabIndex = 7;
            this.btn_Apply.Text = "Apply";
            this.btn_Apply.UseVisualStyleBackColor = true;
            this.btn_Apply.Click += new System.EventHandler(this.btn_Apply_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // lbl_Notice
            // 
            this.lbl_Notice.AutoSize = true;
            this.lbl_Notice.Location = new System.Drawing.Point(15, 538);
            this.lbl_Notice.Name = "lbl_Notice";
            this.lbl_Notice.Size = new System.Drawing.Size(423, 17);
            this.lbl_Notice.TabIndex = 8;
            this.lbl_Notice.Text = "Note: All changes to settings will require restarting the application.";
            // 
            // chkBox_Azure
            // 
            this.chkBox_Azure.AutoSize = true;
            this.chkBox_Azure.Location = new System.Drawing.Point(215, 153);
            this.chkBox_Azure.Name = "chkBox_Azure";
            this.chkBox_Azure.Size = new System.Drawing.Size(88, 21);
            this.chkBox_Azure.TabIndex = 13;
            this.chkBox_Azure.Text = "To Azure";
            this.chkBox_Azure.UseVisualStyleBackColor = true;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 600);
            this.Controls.Add(this.lbl_Notice);
            this.Controls.Add(this.btn_Apply);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.gBox_EnvInfo);
            this.Controls.Add(this.gBox_ServDetails);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Settings";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.gBox_ServDetails.ResumeLayout(false);
            this.gBox_ServDetails.PerformLayout();
            this.gBox_EnvInfo.ResumeLayout(false);
            this.gBox_EnvInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }





        #endregion

        private System.Windows.Forms.Label lbl_BackupLocation;
        private System.Windows.Forms.TextBox tBox_BackupLocation;
        private System.Windows.Forms.Button btn_BackupDir;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gBox_ServDetails;
        private System.Windows.Forms.TextBox tBox_Password;
        private System.Windows.Forms.TextBox tBox_Username;
        private System.Windows.Forms.Label lbl_Password;
        private System.Windows.Forms.Label lbl_Username;
        private System.Windows.Forms.TextBox tBox_IPaddress;
        private System.Windows.Forms.Label lbl_IPaddress;
        private System.Windows.Forms.TextBox tBox_ServerName;
        private System.Windows.Forms.Label lbl_ServerName;
        private System.Windows.Forms.GroupBox gBox_EnvInfo;
        private System.Windows.Forms.TreeView tView_Servers;
        private System.Windows.Forms.Label lbl_Environment;
        private System.Windows.Forms.TextBox tBox_Environment;
        private System.Windows.Forms.TextBox tBox_LogFiles;
        private System.Windows.Forms.TextBox tBox_DataFile;
        private System.Windows.Forms.Label lbl_LogFile;
        private System.Windows.Forms.Label lbl_DataFile;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.ImageList tvServers_imageList;
        private System.Windows.Forms.Button btn_Apply;
        private System.Windows.Forms.TextBox tBox_Instance;
        private System.Windows.Forms.Label lbl_Instance;
        private System.Windows.Forms.TextBox tBox_Port;
        private System.Windows.Forms.Label lbl_Port;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label lbl_Notice;
        private System.Windows.Forms.TextBox txtBox_AzureKey;
        private System.Windows.Forms.Label lbl_AzureKey;
        private System.Windows.Forms.CheckBox chkBox_Azure;
    }
}