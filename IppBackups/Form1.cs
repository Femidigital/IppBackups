using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Specialized;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Wmi;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.IO;
using Tools;


namespace IppBackups
{

    public partial class Form1 : Form
    {
        private Servers _servers;
        private Servers _servers2;

        private ServerX _selectedServer;
        private ServerX _selectedDestServer;
        private string ConfigFileName = ConfigurationManager.AppSettings["ConfigFileName"];
        private string sSettingPath = ConfigurationManager.AppSettings["SettingsPath"];
        string sXmlFile = "";
        string backupDestination = @"";
        string curSrv = "";
        string curSrvInstance = "";
        string curSrvInstanceToConnect = "";
        string curEnv = "";
        string curDb = "";
        string sPort = "";
        string serverName = "";
        string sUsername = "";
        string sPassword = "";
        string r_curSrv = "";
        string r_env = "";
        string r_curSrvInstance = "";
        string r_serverName = "";
        string r_sUsername = "";
        string r_sPassword = "";
        string serverInstanceToRestoreTo = "";
        string scriptLocation = "..\\..\\SQL_Scripts\\";
        Dictionary<string, bool> backStatus = new Dictionary<string, bool>();

        enum Environment
        {
            TEST = 0,
            DEV = 1,
            MIG = 2,
            CI = 3,
            QA = 4,
            UAT = 5,
            PPD = 6,
            PROD = 7
        };

        List<string> databaseList = new List<string>();
        int iLastIndexSelected = 0;
        int iLastDestIndexSelected = 0;

        public Form1()
        {
            this.Hide();
            SplashScreen oSplash = new SplashScreen();
            oSplash.Show();
            oSplash.Update();
            //Thread.Sleep(3000);
            Thread.Sleep(1000);
            oSplash.Close();
            this.Visible = true;

            InitializeComponent();
            InitializeCustomEvents();
            backupbackgroundWorker.WorkerReportsProgress = true;
            backupbackgroundWorker.WorkerSupportsCancellation = true;
            LoadValuesFromSettings();
        }

        public void LoadValuesFromSettings()
        {
            if (sSettingPath == "")
            {
                //MessageBox.Show("Empty Configuration");
                sXmlFile = "..\\..\\" + ConfigFileName;
                //sXmlFile = ConfigFileName;
            }
            {
                sXmlFile = sSettingPath + ConfigFileName;
                XmlDocument doc = new XmlDocument();
                doc.Load(sXmlFile);

                XmlNodeList server = doc.SelectNodes("Servers/Server");
                _servers = new Servers();
                _servers2 = new Servers();
                var i = 1;
                foreach (XmlNode xServer in server)
                {
                    var svr = new ServerX { Id = i, Name = xServer.Attributes["name"].Value, IP = xServer.Attributes["ip"].Value };
                    // this needs to read the instances on this server
                    foreach (XmlNode xInstance in xServer.ChildNodes)
                    {
                        var inst = new Instance();
                        inst.xInstance = xInstance.Attributes["instance"].Value;
                        inst.Port = xInstance.Attributes["port"].Value;
                        inst.User = xInstance.Attributes["user"].Value;
                        inst.Password = xInstance.Attributes["password"].Value;
                        inst.Backups = xInstance.Attributes["backups"].Value;
                        //svr.Instances.Add(xInstance);
                        //svr.Instances.Add(inst);
                        // read the envs for this server
                        foreach (XmlNode xEnvironment in xInstance.ChildNodes)
                        {
                            var env = new Environments { Name = xEnvironment.InnerText, data = xEnvironment.Attributes["data"].Value, log = xEnvironment.Attributes["log"].Value };
                            //env.Name = xEnvironment.InnerText;

                            // svr.Instances.Environments.Add(xEnvironment.InnerText);
                            //inst.Environments.Add(xEnvironment.InnerText);
                            inst.Environments.Add(env);
                        }

                        svr.Instances.Add(inst);
                    }
                    _servers.Add(svr);
                    _servers2.Add(svr);
                    //cBox_Server.Items.Add(xServer.Attributes["name"].Value);
                    //cBox_DestServer.Items.Add(xServer.Attributes["name"].Value);
                    //cBox_Server.Items.Add(svr);
                    //cBox_DestServer.Items.Add(svr);
                    i++;
                }

                cBox_Server.DataSource = _servers;
                cBox_DestServer.DataSource = _servers2;


                //XmlNodeList environment = doc.SelectNodes("Servers/Server/Environment");
                //foreach (XmlNode xEnvironment in environment)
                //{
                //    cBox_Environment.Items.Add(xEnvironment.InnerText);
                //}
            }
        }

        private void ConnectToBackupServer()
        {
            Microsoft.SqlServer.Management.Smo.Server selectedServer = new Microsoft.SqlServer.Management.Smo.Server(r_curSrvInstance);
            selectedServer.ConnectionContext.LoginSecure = false;
            selectedServer.ConnectionContext.Login = sUsername;
            selectedServer.ConnectionContext.Password = sPassword;
        }

        private void ConnectToRestoreServer()
        {
            Microsoft.SqlServer.Management.Smo.Server selected_rServer = new Microsoft.SqlServer.Management.Smo.Server(curSrvInstance);
            selected_rServer.ConnectionContext.LoginSecure = false;
            selected_rServer.ConnectionContext.Login = r_sUsername;
            selected_rServer.ConnectionContext.Password = r_sPassword;
        }

        private void getAllDatabases(string sName)
        {

            XmlNode curServer;
            serverName = sName;
            curSrvInstanceToConnect = "";
            Microsoft.SqlServer.Management.Smo.Server selectedServer;

            /*
            XmlDocument doc2 = new XmlDocument();
            doc2.Load(sXmlFile);
            
            XmlNodeList environment = doc2.SelectNodes("Servers/Server/Instance/Environment");  // this needs to point to instances on the server
            foreach (XmlNode xEnvironment in environment)
            {
                if (xEnvironment.InnerText == sName)
                {
                    curServer = xEnvironment.ParentNode;

                    if (curServer.Attributes["name"].Value == cBox_Server.SelectedItem.ToString())
                    {
                        curSrv = curServer.Attributes["name"].Value;
                        curSrvInstance = curServer.Attributes["instance"].Value;
                        sUsername = curServer.Attributes["user"].Value;
                        sPassword = curServer.Attributes["password"].Value;
                        backupDestination = curServer.Attributes["backups"].Value;

                        //lbl_Oupt.Text += "Connecting to " + sName + " on " + curSrv + ".'\n";
                    }
                    
                }
            }
            */

            curSrvInstance = cBox_Server.SelectedItem.ToString();
            //sUsername = _servers[cBox_Server.SelectedIndex].Instances[0].User;
            //sPassword = _servers[cBox_Server.SelectedIndex].Instances[0].Password;

            for (int i = 0; i < _servers[cBox_Server.SelectedIndex].Instances.Count; i++)
            {
                for (int j = 0; j < _servers[cBox_Server.SelectedIndex].Instances[i].Environments.Count; j++)
                {
                    //if (_servers[cBox_Server.SelectedIndex].Instances[i].Environments.Contains(cBox_Environment.SelectedItem.ToString()))
                    if (_servers[cBox_Server.SelectedIndex].Instances[i].Environments[j].Name == cBox_Environment.SelectedItem.ToString())
                    {
                        curSrvInstance = _servers[cBox_Server.SelectedIndex].Instances[i].xInstance;
                        curSrv = _servers[cBox_Server.SelectedIndex].Name;
                        sPort = _servers[cBox_Server.SelectedIndex].Instances[i].Port;
                        sUsername = _servers[cBox_Server.SelectedIndex].Instances[i].User;
                        sPassword = _servers[cBox_Server.SelectedIndex].Instances[i].Password;
                        backupDestination = _servers[cBox_Server.SelectedIndex].Instances[i].Backups;
                        curSrvInstanceToConnect += curSrv;
                        if ( curSrvInstance != "Default" )
                        {
                            curSrvInstanceToConnect += "\\" + curSrvInstance;
                            curSrvInstance = curSrvInstanceToConnect;
                        }
                        
                        if (sPort != "" && sPort != "1433")
                        {
                            curSrvInstanceToConnect += ", " + sPort;
                        }

                        if (sUsername == "")
                            sUsername = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    }
                }
            }


            if (curSrvInstanceToConnect == "LocalHost")
            {
                selectedServer = new Microsoft.SqlServer.Management.Smo.Server();

                if (selectedServer == null)
                {
                    ManagedComputer mc = new ManagedComputer();

                    foreach (ServerInstance si in mc.ServerInstances)
                    {                        
                        //lbl_Oupt.Text += "Connected to " + si.Name + " successfully.\n";
                        rTxtBox_Output.AppendText("Connected to " + si.Name + " successfully.\n", Color.Black);
                    }
                }
               // //lbl_Oupt.Text += "Connected to " + selectedServer.InstanceName + " successfully.\n";
            }
            else
            {

                selectedServer = new Microsoft.SqlServer.Management.Smo.Server(curSrvInstanceToConnect);
                selectedServer.ConnectionContext.LoginSecure = false;
                selectedServer.ConnectionContext.Login = sUsername;
                selectedServer.ConnectionContext.Password = sPassword;
             }
                //ConnectToBackupServer();

                try
                {
                    //lbl_Oupt.Text += "Connected to " + selectedServer.InstanceName.ToString() + " successfully.\n";
                    rTxtBox_Output.AppendText("Connected to " + selectedServer.InstanceName.ToString() + " successfully.\n", Color.Black);
                    int selectedDb = 0;
                    int columnCount = 1;
                    int rowSize = 15;
                    int rowCount = selectedServer.Databases.Count;
                    columnCount = (selectedDb / rowSize);

                    foreach (Database db in selectedServer.Databases)
                    {
                        if ((selectedDb < selectedServer.Databases.Count) && (selectedDb < rowSize))
                        {
                            //selectedDb++;
                        }
                        else
                        {
                            selectedDb = 0;
                            columnCount++;
                        }

                        if (db.Name.Contains(cBox_Environment.Text + "-") || chkBox_ShowAll.Checked)
                        {
                            CheckBox box = new CheckBox();
                            box.CheckedChanged += box_CheckedChanged;
                            box.Tag = db.Name.ToString();
                            box.Text = db.Name.ToString();
                            box.AutoSize = true;
                            box.Location = new Point(10 + (columnCount * 150), (grpBox_Databases.DisplayRectangle.Top + 10) + (selectedDb % rowSize) * 20);
                            grpBox_Databases.Controls.Add(box);

                            selectedDb++;
                        }
                    }
                    //lbl_Oupt.Text += "Connected to " + sName + " on " + curSrv + " successfully.\n";
                    rTxtBox_Output.AppendText("Connected to " + sName + " on " + curSrv + " successfully.\n", Color.Black);
                }
                catch (Exception ex)
                {
                    rTxtBox_Output.AppendText("Error Connecting to " + sName + " on " + curSrv + " as " + sUsername + ".'\n", Color.Red);
                    rTxtBox_Output.AppendText(ex.Message + ".'\n", Color.Red);
                }
        }

        void box_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                databaseList.Add(((CheckBox)sender).Text);
            }
            else
            {
                databaseList.Remove(((CheckBox)sender).Text);
            }
        }

        private void cBox_Environment_SelectedIndexChanged(object sender, EventArgs e)
        {
            grpBox_Databases.Controls.Clear();
            string newEnv = (string)cBox_Environment.SelectedItem;
            curEnv = newEnv;
            //comboBox1.Items.Clear();
            getAllDatabases(newEnv);
        }

        public void BackupDatabase(String databaseName, String userName, String password, String serverName, String destinationPath)
        {
            Backup sqlBackup = new Backup();

            sqlBackup.Action = BackupActionType.Database;
            sqlBackup.BackupSetDescription = "ArchiveDataBase:" + DateTime.Now.ToShortDateString();
            sqlBackup.BackupSetName = "Archive";

            sqlBackup.Database = databaseName;

            BackupDeviceItem deviceItem = new BackupDeviceItem(destinationPath, DeviceType.File);
            ServerConnection connection = new ServerConnection(serverName, userName, password);
            Server sqlServer = new Server(connection);

            Database db = sqlServer.Databases[databaseName];

            sqlBackup.Initialize = true;
            sqlBackup.Checksum = true;
            sqlBackup.ContinueAfterError = true;
            sqlBackup.CopyOnly = true;

            sqlBackup.Devices.Add(deviceItem);
            sqlBackup.Incremental = false;

            sqlBackup.ExpirationDate = DateTime.Now.AddDays(3);
            sqlBackup.LogTruncation = BackupTruncateLogType.NoTruncate;

            sqlBackup.FormatMedia = false;

            sqlBackup.SqlBackup(sqlServer);

        }

        public void RestoreDatabaseToOscar(String databaseName, String filePath, String serverName, String serverInstance, String userName, String password, String dataFilePath, String logFilePath, String localCopyBackup)
        {

            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;
            string CopiedBackup = "\\\\" + serverName + "\\" + System.IO.Path.Combine(localCopyBackup, databaseName + ".bak");
            string targetCopy = CopiedBackup.Replace(":", "$");


            if (!Directory.Exists(dbDataSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Data Subfolder: " + dbDataSubFolderPath + ".\n";
                rTxtBox_Output.AppendText("Creating Database Data Subfolder: " + dbDataSubFolderPath + ".\n", Color.Black);
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(dbLogSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Log Subfolder: " + dbLogSubFolderPath + ".\n";
                rTxtBox_Output.AppendText("Creating Database Log Subfolder: " + dbLogSubFolderPath + ".\n", Color.Black);
                Directory.CreateDirectory(dbLogSubFolderPath);
            }

            if (rBtn_Refresh.Checked)
            {
                if (File.Exists(targetCopy))
                {
                    //lbl_Oupt.Text += "Deleting old backup file... \n";
                    rTxtBox_Output.AppendText("Deleting old backup file... \n", Color.Black);
                    File.Delete(targetCopy);
                }

                int position = targetCopy.LastIndexOf('\\');
                string targetDir = targetCopy.Substring(0, position);
                //lbl_Oupt.Text += "Copying backup file locally \n";
                rTxtBox_Output.AppendText("Copying backup file locally \n",Color.Black);
                //lbl_Oupt.Text += "Copying from : " + filePath + "\n";
                rTxtBox_Output.AppendText("Copying from : " + filePath + "\n", Color.Black);
                //lbl_Oupt.Text += "Copying to : " + targetCopy + "\n";
                rTxtBox_Output.AppendText("Copying to : " + targetCopy + "\n", Color.Black);
                ////lbl_Oupt.Text += "Copying to : " + targetDir + "\n";
                System.IO.File.Copy(filePath, targetCopy, true);

                //lbl_Oupt.Text += "Copyied backup file locally \n";
                rTxtBox_Output.AppendText("Copyied backup file locally \n", Color.Black);
            }

            targetCopy = localiseUNCPath(targetCopy);

            Restore sqlRestore = new Restore();

            BackupDeviceItem deviceItem = new BackupDeviceItem(targetCopy, DeviceType.File);
            sqlRestore.Devices.Add(deviceItem);
            sqlRestore.Database = databaseName;
            ////lbl_Oupt.Text += "Before connecting to : " + serverName + " by : " + userName + " \n";


            serverInstanceToRestoreTo = serverName;
            
            if(serverInstance != "Default")
                serverInstanceToRestoreTo = serverName + "\\" + serverInstance;
            //ServerConnection connection = new ServerConnection(serverInstance);
            //ServerConnection connection = new ServerConnection(serverInstanceToRestoreTo);
            ServerConnection connection = new ServerConnection(serverInstanceToRestoreTo);            
            Server sqlServer = new Server(connection);

            Database db = sqlServer.Databases[databaseName];
            rTxtBox_Output.AppendText("Setting Database to SingleUser mode... '\n", Color.Black);
            db.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
            db.Alter(TerminationClause.RollbackTransactionsImmediately);
            sqlRestore.Action = RestoreActionType.Database;

            String dataFileLocation = dataFilePath + "\\" + databaseName + "\\" + databaseName + ".mdf";
            String logFileLocation = logFilePath + "\\" + databaseName + "\\" + databaseName + "_Log.ldf";
            dataFileLocation = localiseUNCPath(dataFileLocation);
            logFileLocation = localiseUNCPath(logFileLocation);

            try
            {
                db = sqlServer.Databases[databaseName];
                RelocateFile rf = new RelocateFile(databaseName, dataFileLocation);

                System.Data.DataTable logicalRestoreFiles = sqlRestore.ReadFileList(sqlServer);

                sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[0][0].ToString(), dataFileLocation));
                sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[1][0].ToString(), logFileLocation));
                sqlRestore.ReplaceDatabase = true;
                sqlRestore.Complete += new ServerMessageEventHandler(sqlRestore_Complete);
                sqlRestore.PercentCompleteNotification = 10;
                sqlRestore.PercentComplete += new PercentCompleteEventHandler(sqlRestore_PercentComplete);
                //lbl_Oupt.Text += "About to restore... '\n";
                rTxtBox_Output.AppendText("About to restore... '\n", Color.Black);

                //KillAllConnectionsToDb(serverInstanceToRestoreTo, cBox_DestEnvironment.Text, databaseName);
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += ex.InnerException.Message + "'\n";
                rTxtBox_Output.AppendText(ex.InnerException.Message + "'\n", Color.Red);
            }

            db = sqlServer.Databases[databaseName];
            if (db.FileGroups[0].Files[0].Name != databaseName)
            {
                //lbl_Oupt.Text += "Renaming Logical files from " + db.FileGroups[0].Files[0].Name + "... '\n";
                rTxtBox_Output.AppendText("Renaming Logical files from " + db.FileGroups[0].Files[0].Name + "... '\n", Color.Black);
                db.FileGroups[0].Files[0].Rename(databaseName);
                db.LogFiles[0].Rename(databaseName + "_log");
                //lbl_Oupt.Text += "Successfully renamed Logical files to " + db.FileGroups[0].Files[0].Name + "... '\n";
                rTxtBox_Output.AppendText("Successfully renamed Logical files to " + db.FileGroups[0].Files[0].Name + "... '\n", Color.Black);
            }

            //lbl_Oupt.Text += "Resfresh environment is " + r_env + "...'\n";
            rTxtBox_Output.AppendText("Resfresh environment is " + r_env + "...'\n", Color.Black);
            if (r_env != "PROD")
            {
                //lbl_Oupt.Text += "Setting non production database to Simple mode...'\n";
                rTxtBox_Output.AppendText("Setting non production database to Simple mode...'\n", Color.Black);
                db.RecoveryModel = RecoveryModel.Simple;
                db.Alter();
            }

            db.SetOnline();            
            sqlServer.Refresh();
            connection.Disconnect();

        }

        public void RestoreDatabase(String databaseName, String filePath, String serverName, String userName, String password, String dataFilePath, String logFilePath)
        {
            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;

            if (!Directory.Exists(@dbDataSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n";
                rTxtBox_Output.AppendText("Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n",Color.Black);
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n";
                rTxtBox_Output.AppendText("Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n",Color.Black);
                Directory.CreateDirectory(dbLogSubFolderPath);
            }

            //String dataFileLocation = dataFilePath + "\\" + databaseName + "\\" + databaseName + ".mdf";
            //String logFileLocation = logFilePath + "\\" + databaseName + "\\" + databaseName + "_Log.ldf";
            string dataFileLocation = dbDataSubFolderPath + "\\" + databaseName + ".mdf";
            string logFileLocation = dbLogSubFolderPath + "\\" + databaseName + "_Log.ldf";

            Restore sqlRestore = new Restore();

            BackupDeviceItem deviceItem = new BackupDeviceItem(filePath, DeviceType.File);
            sqlRestore.Devices.Add(deviceItem);
            sqlRestore.Database = databaseName;

            ServerConnection connection = new ServerConnection(serverName, userName, password);
            //ServerConnection connection = new ServerConnection(serverName);
            Server sqlServer = new Server(connection);


            Database db = sqlServer.Databases[databaseName];
            sqlRestore.Action = RestoreActionType.Database;

            db = sqlServer.Databases[databaseName];
            RelocateFile rf = new RelocateFile(databaseName, dataFileLocation);

            //sqlRestore.RelocateFiles.Add(new RelocateFile(databaseName, dataFileLocation));
            //sqlRestore.RelocateFiles.Add(new RelocateFile(databaseName + "_log", logFileLocation));


            System.Data.DataTable logicalRestoreFiles = sqlRestore.ReadFileList(sqlServer);
            sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[0][0].ToString(), dataFileLocation));
            sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[1][0].ToString(), logFileLocation));
            sqlRestore.ReplaceDatabase = true;
            sqlRestore.Complete += new ServerMessageEventHandler(sqlRestore_Complete);
            sqlRestore.PercentCompleteNotification = 10;
            sqlRestore.PercentComplete += new PercentCompleteEventHandler(sqlRestore_PercentComplete);
            //lbl_Oupt.Text += "About to restore... \n";
            rTxtBox_Output.AppendText("About to restore... \n",Color.Black);

            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += ex.InnerException.Message + "\n";
                rTxtBox_Output.AppendText(ex.InnerException.Message + "\n",Color.Red);
                ////lbl_Oupt.Text += ex.Message + "'\n";
            }
            db = sqlServer.Databases[databaseName];
            db.FileGroups[0].Files[0].Rename(databaseName);
            db.LogFiles[0].Rename(databaseName + "_log");
            db.SetOnline();
            sqlServer.Refresh();

        }

        public void RestoreDatabaseToPrivate(String databaseName, String filePath, String serverName, String serverInstance, String userName, String password, String dataFilePath, String logFilePath, String localCopyBackup)
        {
            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;
            string CopiedBackup = "\\\\" + serverName + "\\" + System.IO.Path.Combine(localCopyBackup, databaseName + ".bak");
            string targetCopy = CopiedBackup.Replace(":", "$");

            if (!Directory.Exists(@dbDataSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n";
                rTxtBox_Output.AppendText("Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n",Color.Black);
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n";
                rTxtBox_Output.AppendText("Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n",Color.Black);
                Directory.CreateDirectory(dbLogSubFolderPath);
            }

            string dataFileLocation = dbDataSubFolderPath + "\\" + databaseName + ".mdf";
            string logFileLocation = dbLogSubFolderPath + "\\" + databaseName + "_Log.ldf";

            Restore sqlRestore = new Restore();

            BackupDeviceItem deviceItem = new BackupDeviceItem(filePath, DeviceType.File);
            sqlRestore.Devices.Add(deviceItem);
            sqlRestore.Database = databaseName;

            ServerConnection connection = new ServerConnection(serverName, userName, password);

            Server sqlServer = new Server(connection);


            Database db = sqlServer.Databases[databaseName];
            sqlRestore.Action = RestoreActionType.Database;

            db = sqlServer.Databases[databaseName];
            RelocateFile rf = new RelocateFile(databaseName, dataFileLocation);

            System.Data.DataTable logicalRestoreFiles = sqlRestore.ReadFileList(sqlServer);
            sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[0][0].ToString(), dataFileLocation));
            sqlRestore.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[1][0].ToString(), logFileLocation));
            sqlRestore.ReplaceDatabase = true;
            sqlRestore.Complete += new ServerMessageEventHandler(sqlRestore_Complete);
            sqlRestore.PercentCompleteNotification = 10;
            sqlRestore.PercentComplete += new PercentCompleteEventHandler(sqlRestore_PercentComplete);
            //lbl_Oupt.Text += "About to restore... \n";
            rTxtBox_Output.AppendText("About to restore... \n",Color.Black);


            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += ex.InnerException.Message + "\n";
                rTxtBox_Output.AppendText(ex.InnerException.Message + "\n",Color.Red);
                ////lbl_Oupt.Text += ex.Message + "'\n";
            }
            db = sqlServer.Databases[databaseName];
            db.FileGroups[0].Files[0].Rename(databaseName);
            db.LogFiles[0].Rename(databaseName + "_log");
            db.SetOnline();
            sqlServer.Refresh();

        }

        public string localiseUNCPath(string serverPath)
        {
            int firstDollar = serverPath.IndexOf("$") - 1;
            serverPath = serverPath.Substring(firstDollar, serverPath.Length - firstDollar);
            serverPath = serverPath.Replace("$", ":");
            return serverPath;
        }

        public void sqlRestore_Complete(object sender, ServerMessageEventArgs e)
        {

        }

        public void sqlRestore_PercentComplete(object sender, PercentCompleteEventArgs e)
        {

        }

        private void cBox_Server_SelectedIndexChanged(object sender, EventArgs e)
        {
            cBox_Environment.Items.Clear();

            //XmlDocument doc = new XmlDocument();
            //doc.Load(sXmlFile);

            //var environment = doc.SelectSingleNode("/Servers/Server[@name='" + cBox_Server.SelectedItem.ToString() + "']");
            //var environment = doc.SelectSingleNode("/Serers/Server[position()=" + cBox_Server.SelectedIndex + "]");
            //var environment = doc.SelectSingleNode("/Servers/Server[" + cBox_Server.SelectedIndex + /*"][@name='" + cBox_Server.SelectedItem.ToString() +*/ "']");
            //cBox_Environment.Items.Add("All");

            var si = cBox_Server.SelectedItem as ServerX;
            _selectedServer = si;
            // Select_index = cBox_Server.SelectedIndex + 1;
            //var environment = doc.SelectSingleNode("/Servers/Server[" + Select_index + "]");

            foreach (var inst in si.Instances)
            {
                // This needs to iterate through instances on the selected server.
                foreach (var env in inst.Environments)
                {
                    cBox_Environment.Items.Add(env.Name);
                }
            }
            cBox_Environment.SelectedIndex = 0;
            //cBox_Environment.SelectedItem = cBox_Environment.Items[0];
            //TODO: Change Source Server's Selected Item to Select_Index.
            //cBox_Server.SelectedItem = cBox_Server.Items[Select_index];
            //cBox_Server.SelectedItem = cBox_Server.SelectedIndex;
            iLastIndexSelected = cBox_Server.SelectedIndex;

        }

        private void cBox_Server_DropDownClosd(object sender, EventArgs e)
        {
            if (_selectedServer == null) return;
            cBox_Server.SelectedIndex = _selectedServer.Id - 1;
        }

        private void cBox_Server_DropDown(object sender, EventArgs e)
        {
            if (iLastIndexSelected > 0)
                cBox_Server.SelectedIndex = iLastIndexSelected;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void serversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings oSettings = new Settings();
            oSettings.Show();
        }

        private void btn_Execute_Click(object sender, EventArgs e)
        {
            backStatus.Clear();

            if (rBtn_Backup.Checked)
            {
                if (backupbackgroundWorker.IsBusy != true)
                {
                    // Start the asynchronous operation.
                    backupbackgroundWorker.RunWorkerAsync();
                }

            }
            else if (rBtn_Restore.Checked)
            {
                if (restorebackgroundWorker.IsBusy != true)
                {
                    restorebackgroundWorker.RunWorkerAsync();
                }
               
                ////lbl_Oupt.Text += "Restoring " + cBox_DestEnvironment.Text + " environment with selected database(s)... \n";
                //restorebackgroundWorker.RunWorkerAsync();
            }
            else if (rBtn_Refresh.Checked)
            {
                if ((Environment)Enum.Parse(typeof(Environment), cBox_Environment.Text) >= (Environment)Enum.Parse(typeof(Environment), cBox_DestEnvironment.Text))
                {
                    r_env = cBox_DestEnvironment.SelectedItem.ToString();
                    if (backupbackgroundWorker.IsBusy != true)
                    {
                        //lbl_Oupt.Text += "Backing up source database... \n";
                        rTxtBox_Output.AppendText("Backing up source database... \n",Color.Black);
                        // Start the asynchronous operation.
                        backupbackgroundWorker.RunWorkerAsync();
                    }
                    else
                    {
                        //lbl_Oupt.Text += "Refresh environment... \n";
                        rTxtBox_Output.AppendText("Refresh environment... \n",Color.Black);
                    }
                }
                else
                {
                    // TODO: Change font color
                    //lbl_Oupt.Text += "Cannot refresh from a lower environemnt...\n";
                    rTxtBox_Output.AppendText("Cannot refresh from a lower environemnt...\n",Color.Red);
                }
            }
            else
            {
                //lbl_Oupt.Text += "Select an action to perform...\n";
                rTxtBox_Output.AppendText("Select an action to perform...\n",Color.Red);
            }
        }

        private void VerifyBackupExists()
        {
             /* Check the Destination Backup directory for the Source Backup file */
               
                foreach (string db in databaseList)
                {
                    string restorePath = "";
                    string destPath = "";
                    r_env = cBox_Environment.SelectedItem.ToString();

                    for (int i = 0; i < _servers[cBox_Server.SelectedIndex].Instances.Count; i++ )
                    {
                        for (int j = 0; j < _servers[cBox_Server.SelectedIndex].Instances[i].Environments.Count; j++)
                        {
                            if (cBox_Environment.Text == _servers[cBox_Server.SelectedIndex].Instances[i].Environments[j].Name)
                            {
                                restorePath = _servers[cBox_Server.SelectedIndex].Instances[i].Backups;
                                destPath = restorePath + "\\" + db + ".bak";
                            }
                        }
                    }

                    

                    string destFilePath = "\\\\" + cBox_Server.Text + "\\" + destPath;
                    destFilePath = destFilePath.Replace(':', '$');

                    if (File.Exists(destFilePath))
                    {
                        //lbl_Oupt.Text += "Backup file exists for the source database at " + destFilePath + " ...\n";
                        rTxtBox_Output.AppendText("Backup file exists for the source database at " + destFilePath + " ...\n",Color.Black);
                        //File.Delete(destPath);
                        if (!backStatus.ContainsKey(db))
                        {
                            backStatus.Add(db, true);
                        }
                        //lbl_Oupt.Text += "Restoring " + cBox_Environment.Text + " environment with selected database(s)... \n";
                        rTxtBox_Output.AppendText("Restoring " + cBox_Environment.Text + " environment with selected database(s)... \n",Color.Black);
                        if (restorebackgroundWorker.IsBusy != true)
                        {
                            restorebackgroundWorker.RunWorkerAsync();
                        }
                    }        
                    else
                    {
                        //lbl_Oupt.Text += "Missing backup file to restore at " + destFilePath + "...\n";
                        rTxtBox_Output.AppendText("Missing backup file to restore at " + destFilePath + "...\n",Color.Red);
                    }
                }
        }

        // This event handler is where the time-consuming work is done.
        private void backupbackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
                //break;
            }
            else
            {
                // TODO: Fix deleting or relacing existing backup file
                rTxtBox_Output.AppendText("Backup process started at " + DateTime.Now + "\n", Color.Black);
                foreach (string db in databaseList)
                {
                    string destPath = backupDestination + "\\" + db + ".bak";
                    string destFilePath = "\\\\" + cBox_Server.Text + "\\" + backupDestination + "\\" + db + ".bak";
                    destFilePath = destFilePath.Replace(':', '$');
                    
                    /* Trace Comments */
                    //lbl_Oupt.Text += "Backing up to " + destPath + "\n";
                    rTxtBox_Output.AppendText("Backing up to " + destPath + "\n",Color.Black);

                    if (File.Exists(destFilePath))
                    {
                        //lbl_Oupt.Text += "Deleting existing backup file ...\n";
                        rTxtBox_Output.AppendText("Deleting existing backup file ...\n",Color.Black);
                        File.Delete(destFilePath);
                    }

                    try
                    {
                        //lbl_Oupt.Text += "Starting Backup for " + db + ".\n";
                        rTxtBox_Output.AppendText("Starting Backup for " + db + ".\n",Color.Black);

                        // Perform a time consuming operation and report progress
                        //BackupDatabase(db, sUsername, sPassword, curSrvInstance, destPath);
                        BackupDatabase(db, sUsername, sPassword, curSrvInstanceToConnect, destPath);
                        backStatus.Add(db, true);
                        //worker.ReportProgress();
                    }
                    catch (Exception ex)
                    {
                        backStatus.Add(db, false);
                        //lbl_Oupt.Text += ex.Message + "\n";
                        rTxtBox_Output.AppendText(ex.Message + "\n",Color.Black);
                    }
                    finally
                    {
                        bool status;
                        //if (backStatus.TryGetValue(db, out status))
                        if (backStatus[db])
                        {
                            // TODO: Change font color to green
                            //lbl_Oupt.Text += "Backup completed...\n";
                            rTxtBox_Output.AppendText("Backup completed...\n",Color.Black);
                        }
                        else
                        {
                            // TODO: Change font color
                            //lbl_Oupt.Text += "Backup completed with error(s)...\n";
                            rTxtBox_Output.AppendText("Backup completed with error(s)...\n",Color.Red);
                        }
                        //restorebackgroundWorker.RunWorkerAsync();
                    }
                }
                rTxtBox_Output.AppendText("Backup process completed at " + DateTime.Now + "\n", Color.Black);
            }
        }

        private void backupbackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //lbl_Oupt.Text += (e.ProgressPercentage.ToString() + "%");
            rTxtBox_Output.AppendText((e.ProgressPercentage.ToString() + "%"),Color.Black);
        }

        private void backupbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //lbl_Oupt.Text += "Canceled! \n";
                rTxtBox_Output.AppendText("Canceled! \n",Color.Black);
            }
            else if (e.Error != null)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += "Error: " + e.Error.Message;
                rTxtBox_Output.AppendText("Error: " + e.Error.Message , Color.Red);
            }
            else
            {
                //lbl_Oupt.Text += "Done!!!\n\n";
                rTxtBox_Output.AppendText("Done!!!\n\n",Color.Black);
                if (rBtn_Refresh.Checked)
                {
                    //lbl_Oupt.Text += "Refreshing " + cBox_DestEnvironment.Text + " environment with selected database(s)... \n";
                    rTxtBox_Output.AppendText("Refreshing " + cBox_DestEnvironment.Text + " environment with selected database(s)... \n",Color.Black);
                    restorebackgroundWorker.RunWorkerAsync();
                }
            }
        }

        private void restorebackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            VerifyBackupExists();

            BackgroundWorker restoreWorker = sender as BackgroundWorker;

            if (restoreWorker.CancellationPending == true)
            {
                e.Cancel = true;
            }
            else
            {
                string srvInstance = "";
                string dataFilePath = "";
                string logFilePath = "";
                string restore_dataFilePath = "";
                string restore_logFilePath = "";
                string localcopy = "";
                string restoreToSrv = "";
                string restoreToEnv = "";

                ServerX si2 = new ServerX();
                if (rBtn_Refresh.Checked)
                {
                    si2 = cBox_DestServer.SelectedItem as ServerX;
                    restoreToSrv = cBox_DestServer.Text;
                    restoreToEnv = cBox_DestEnvironment.SelectedItem.ToString();
                }
                else if (rBtn_Restore.Checked)
                {
                    si2 = cBox_Server.SelectedItem as ServerX;
                    restoreToSrv = cBox_Server.Text;
                    restoreToEnv = cBox_Environment.SelectedItem.ToString();
                }
                _selectedDestServer = si2;

                string srvName = _selectedDestServer.Name;

                foreach (var inst in _selectedDestServer.Instances)
                {
                    for (int i = 0; i < inst.Environments.Count; i++)
                    {
                        if (inst.Environments.ElementAt(i).Name == restoreToEnv)
                        {
                            string sServer = inst.xInstance;

                            srvInstance = inst.xInstance;
                            r_sUsername = inst.User;
                            r_sPassword = inst.Password;
                            dataFilePath = inst.Environments.ElementAt(i).data;
                            logFilePath = inst.Environments.ElementAt(i).log;
                            restore_dataFilePath = "\\\\" + restoreToSrv + "\\" + dataFilePath.Replace(":", "$");
                            restore_logFilePath = "\\\\" + restoreToSrv + "\\" + logFilePath.Replace(":", "$");
                            localcopy = inst.Backups;
                        }
                    }
                }

                // try another connection method
                Microsoft.SqlServer.Management.Smo.Server selectedRestoreServer = new Microsoft.SqlServer.Management.Smo.Server(srvName);
                selectedRestoreServer.ConnectionContext.LoginSecure = false;
                selectedRestoreServer.ConnectionContext.Login = r_sUsername;
                selectedRestoreServer.ConnectionContext.Password = r_sPassword;

                rTxtBox_Output.AppendText("Restore process started at " + DateTime.Now + "\n", Color.Black);
                foreach (string db in databaseList)
                {
                    if (backStatus.ContainsKey(db) || rBtn_Restore.Checked)
                    {
                        if (backStatus[db] || rBtn_Restore.Checked)
                        //if (rBtn_Restore.Checked)
                        {
                            try
                            {
                                string restore_db = "";
                                if (rBtn_Refresh.Checked)
                                {
                                    restore_db = db.Replace(cBox_Environment.Text, cBox_DestEnvironment.Text);
                                }
                                else if (rBtn_Restore.Checked)
                                {
                                    restore_db = db;
                                }
                                string filePath = "\\\\" + curSrv + "\\" + backupDestination.Replace(":", "$") + "\\" + db + ".bak";
                                //lbl_Oupt.Text += "Starting restore for " + db + ".'\n";
                                rTxtBox_Output.AppendText("Starting restore for " + db + ".'\n",Color.Black);

                                // Perform a time consuming operation and report progress
                                //lbl_Oupt.Text += "Restore : " + db + " database to " + restore_db + " database on : " + filePath + " to : " + restore_dataFilePath + " and : " + restore_logFilePath + "'\n";
                                //lbl_Oupt.Text += "User : " + r_sUsername + "'\n";
                                //lbl_Oupt.Text += "Selected destination server  : " + restoreToSrv + "\n";
                                rTxtBox_Output.AppendText("Restore : " + db + " database to " + restore_db + " database on : " + filePath + " to : " + restore_dataFilePath + " and : " + restore_logFilePath + "\n",Color.Black);
                                rTxtBox_Output.AppendText("User : " + r_sUsername + "\n",Color.Black);
                                rTxtBox_Output.AppendText("Selected destination server  : " + restoreToSrv + "\n",Color.Black);
                                // TODO: Workout how to identify which domain the server is under
                                if (restoreToSrv == "UK-CHFMIGSQL" || restoreToSrv == "UK-CHDEVSQL01" || restoreToSrv == "UK-CHDEVSQL02" || restoreToSrv == "FDC_TAB" || restoreToSrv == "CHI-7S45842" || restoreToSrv == "DEV-SENT01" || restoreToSrv == "BSI16DBS04PRV" || restoreToSrv == "BSI16DBS03PRV" || restoreToSrv == "BSI10DBS03PRV")
                                {
                                    //lbl_Oupt.Text += "Restoring database to OSCAR domain.\n";
                                    rTxtBox_Output.AppendText("Restoring database to OSCAR domain.\n",Color.Black);
                                    RestoreDatabaseToOscar(restore_db, filePath, srvName, srvInstance, r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                                }
                                else
                                {
                                    //lbl_Oupt.Text += "Restoring database to PRIVATE domain.\n";
                                    rTxtBox_Output.AppendText("Restoring database to PRIVATE domain.\n",Color.Black);
                                    // RestoreDatabase(restore_db, filePath, srvName, r_sUsername, r_sPassword, dataFilePath, logFilePath);
                                    RestoreDatabaseToPrivate(restore_db, filePath, srvName, srvInstance, r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                                }

                                //if (restore_db.Contains("BSOL") || restore_db.Contains("CloudAdmin"))
                                if (restore_db == (restoreToEnv + "-BSOL") || restore_db == (restoreToEnv + "-CloudAdmin"))
                                {
                                    //Server myServer = new Server(srvInstance);
                                    Server myServer = new Server(srvName);
                                    //lbl_Oupt.Text += "Calling GenerateViewScriptWithDependencies " + srvName +" ...\n";
                                    rTxtBox_Output.AppendText("Calling GenerateViewScriptWithDependencies " + srvName + " ...\n",Color.Black);
                                    //GenerateViewScript(myServer, restore_db);
                                    GenerateViewScriptWithDependencies(myServer, restore_db);
                                }

                                //if (restore_db.Contains("CloudAdmin") || restore_db.Contains("PersonalData"))

                                int dashIndex = db.IndexOf("-") + 1;
                                DirectoryInfo d = new DirectoryInfo(scriptLocation);
                                foreach(var file in d.GetFiles("*.sql"))
                                {
                                    string dbPart = db.Substring(dashIndex, db.Length - dashIndex);

                                    if (file.Name.ToLower().Contains("_" + restoreToEnv.ToLower()) && file.Name.ToLower().Contains(dbPart.ToLower() + "_") && restore_db.ToLower().Contains(restoreToEnv.ToLower() + "-") && restore_db.ToLower().Contains("-" + dbPart.ToLower()))
                                    {
                                        update_DatabaseEntries(srvName, restoreToEnv, restore_db, file.Name);
                                    }
                                }

                               /* if (restore_db == (restoreToEnv + "-CloudAdmin") || restore_db == (restoreToEnv + "-PersonalData") || restore_db == (restoreToEnv + "-Ecommerce"))
                                {
                                    rTxtBox_Output.AppendText("\n\nUsing hardcoded Values for updates... \n", Color.Red);
                                    //update_DatabaseEntries(srvInstance, cBox_DestEnvironment.Text, restore_db);
                                    update_DatabaseEntries(srvName, restoreToEnv, restore_db);
                                }*/
                                //worker.ReportProgress();
                            }
                            catch (Exception ex)
                            {
                                rTxtBox_Output.AppendText(ex.Message + "\n\n",Color.Red);
                            }
                            finally
                            {
                                rTxtBox_Output.AppendText(db + "Restore completed...\n\n",Color.Black);
                            }
                        }
                        else
                        {
                            rTxtBox_Output.AppendText("\nThere was a problem with the last " + db + " backup, restore can not be performed...\n\n",Color.Red);
                        }
                    }
                }
                rTxtBox_Output.AppendText("Restore process completed at " + DateTime.Now + "\n", Color.Black);
            }
        }

        private void restorebackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //lbl_Oupt.Text += (e.ProgressPercentage.ToString() + "%");
            rTxtBox_Output.AppendText((e.ProgressPercentage.ToString() + "%"),Color.Black);
        }

        private void restorebackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //lbl_Oupt.Text += "Canceled! '\n";
                rTxtBox_Output.AppendText("Canceled! '\n",Color.Black);
            }
            else if (e.Error != null)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += "Error: " + e.Error.Message;
                rTxtBox_Output.AppendText("Error: " + e.Error.Message +"\n", Color.Red);
            }
            else
            {
                //lbl_Oupt.Text += "Restore Done! '\n";
                rTxtBox_Output.AppendText("Restore Done! '\n",Color.Black);
            }
        }

        private void cancelAsynButton_Click(object sender, EventArgs e)
        {
            if (backupbackgroundWorker.WorkerSupportsCancellation == true)
            {
                // Cancel the asychronous operation.
                backupbackgroundWorker.CancelAsync();
            }
        }

        private void rBtn_Restore_CheckedChanged(object sender, EventArgs e)
        {
            if (rBtn_Restore.Checked == true)
            {
                //cBox_DestServer.DataSource = _servers2;
                //cBox_DestServer.Enabled = true;
                //cBox_DestEnvironment.Enabled = true;
                cBox_Server.DataSource = _servers;                
                cBox_DestServer.Enabled = false;
                cBox_DestEnvironment.Enabled = false;
            }
           /* else
            {
                //cBox_DestServer.Enabled = false;
                //cBox_DestEnvironment.Enabled = false;
                cBox_DestServer.Enabled = true;
                cBox_DestEnvironment.Enabled = true;
            }*/
        }

        private void rBtn_Refresh_CheckedChanged(object sender, EventArgs e)
        {
            if (rBtn_Refresh.Checked == true)
            {
                cBox_DestServer.DataSource = _servers2;
                cBox_DestServer.Enabled = true;
                cBox_DestEnvironment.Enabled = true;
            }
            else
            {
                cBox_DestServer.Enabled = false;
                cBox_DestEnvironment.Enabled = false;
            }
        }

        private void cBox_DestServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            cBox_DestEnvironment.Items.Clear();

            var si = cBox_DestServer.SelectedItem as ServerX;
            _selectedServer = si;

            foreach (var inst in si.Instances)
            {
                // This needs to iterate through instances on the selected server.
                foreach (var env in inst.Environments)
                {
                    cBox_DestEnvironment.Items.Add(env.Name);
                }
            }
            cBox_DestEnvironment.SelectedIndex = 0;

            iLastDestIndexSelected = cBox_DestServer.SelectedIndex;

            /*XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);

            int Select_Index = cBox_DestServer.SelectedIndex + 1;

            // XmlNodeList environment = doc.SelectNodes("Servers/Server['"+cBox_Server.SelectedItem.ToString()+"']");
            //var environment = doc.SelectSingleNode("/Servers/Server[@name='" + cBox_DestServer.SelectedItem.ToString() + "']");
            var environment = doc.SelectSingleNode("/Servers/Server[" + Select_Index + "]");

            foreach (XmlNode xEnvironment in environment.ChildNodes)
            {
                cBox_DestEnvironment.Items.Add(xEnvironment.InnerText);
            }
            cBox_DestEnvironment.SelectedItem = cBox_DestEnvironment.Items[0];
            // TODO: Change Destination Server's Selected item to Select_Index.*/
        }

        private void update_DatabaseEntries(string serverInstance, string env, string db, string cur_ScriptFile)
        {
            //lbl_Oupt.Text += "Updating Database entries for " + db + "...\n";
            rTxtBox_Output.AppendText("Updating Database entries for " + db + " using " + cur_ScriptFile  + " on server " + serverInstance + "...\n",Color.Black);

            /* Try manipulating the incoming server details to cater for both default and instanced SQL */
            Server srv;
            if (serverInstanceToRestoreTo != "Default")
            {
                // Connect to the specified instance of SQL Server.
                srv = new Server(serverInstanceToRestoreTo);
                //lbl_Oupt.Text += "Connected to " + serverInstanceToRestoreTo + " specied instance...'\n";
                rTxtBox_Output.AppendText("curSrvInstanceToConnect is " + curSrvInstanceToConnect + "...\n", Color.Black);
                rTxtBox_Output.AppendText("curSrvInstance is " + curSrvInstance + "...\n", Color.Black);
                rTxtBox_Output.AppendText("serverInstance is " + serverInstance + "...\n", Color.Black);
                rTxtBox_Output.AppendText("serverInstanceToRestoreTo is " + serverInstanceToRestoreTo + "...\n", Color.Black);
                curSrvInstanceToConnect += "\\" + curSrvInstance;
                curSrvInstance = curSrvInstanceToConnect;
                serverInstance = serverInstanceToRestoreTo;
                rTxtBox_Output.AppendText("Connected to " + serverInstanceToRestoreTo + " specied instance...'\n", Color.Black);
            }
            //else
            //{
            //    // Connect to the default instance of SQL Server.
            //    srv = new Server();
            //    //lbl_Oupt.Text += "Connected to default instance...'\n";
                
            //    rTxtBox_Output.AppendText("Connected to default instance...'\n", Color.Black);
            //}  
            
            
            /*if (curSrvInstance != "Default")
            {
                rTxtBox_Output.AppendText("Incoming server details is " + serverInstance + "...\n", Color.Black);
                curSrvInstanceToConnect += "\\" + curSrvInstance;
                curSrvInstance = curSrvInstanceToConnect;
            }*/
            /* End of manipulating of incoming server details*/


            string sqlConnectionString = "Data Source=" + serverInstance + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            //string scriptFile = "UpdateDatabaseEntries" + db.Substring(db.IndexOf("-") + 1) + "-" + env + ".sql";
            string scriptFile = scriptLocation + cur_ScriptFile;
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);
            //ServerConnection connection = new ServerConnection(serverInstance);
            //Server sqlServer = new Server(connection);
            try
            {
                //sqlServer.ConnectionContext.ExecuteNonQuery(script);
                cmd.ExecuteNonQuery();
                conn.Close();
                rTxtBox_Output.AppendText("Script executed successfully...\n", Color.Green);
            }
            catch (SqlServerManagementException e)
            {
                rTxtBox_Output.AppendText("Inside SqlServerManagementExcecption block...\n", Color.Red);
                rTxtBox_Output.AppendText(e.InnerException + "\n",Color.Red);
            }
            catch (SqlException e)
            {
                rTxtBox_Output.AppendText("Inside SqlException...\n", Color.Red);
                rTxtBox_Output.AppendText(e.Message + "\n" + e.StackTrace + "\n", Color.Red);
                rTxtBox_Output.AppendText(e.InnerException + "\n",Color.Red);
            }
        }

        private void GenerateViewScript(Server rServer, string db)
        {
            Scripter scripter = new Scripter(rServer);
            Database restoreDb = rServer.Databases[db];
            System.IO.StreamWriter sqlFile = new StreamWriter("CreateView_" + db + ".sql");

            //lbl_Oupt.Text += "Generating Script for creating Views in : " + db + " on " + rServer + "\n";
            /* With ScriptingOptions you can specify different scripting options,
             * for example to include IF NOT EXISTS, DROP statements, output location etc */
            ScriptingOptions scriptOptions = new ScriptingOptions();
            scriptOptions.ScriptDrops = true;
            scriptOptions.IncludeIfNotExists = true;
            scriptOptions.WithDependencies = true;  // Check this line

            ScriptingOptions scriptOptionsForCreate = new ScriptingOptions();
            scriptOptionsForCreate.AnsiPadding = true;
            scriptOptionsForCreate.ExtendedProperties = true;
            scriptOptionsForCreate.IncludeIfNotExists = true;
            scriptOptionsForCreate.WithDependencies = true;  // Check this line

            foreach (Microsoft.SqlServer.Management.Smo.View myView in restoreDb.Views)
            {
                if (!myView.IsSystemObject)
                {
                    /* Generating IF EXISTS and DROP command for views */
                    StringCollection viewScripts = myView.Script(scriptOptions);

                    foreach (string script in viewScripts)
                    {
                        var updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);

                        sqlFile.WriteLine(updatedScript);
                    }

                    /* Generating CREATE VIEW command */
                    viewScripts = myView.Script(scriptOptionsForCreate);
                    foreach (string create_script in viewScripts)
                    {
                        var updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);

                        sqlFile.WriteLine(updatedScript);
                    }
                }


            }
            //lbl_Oupt.Text += "View Scripts completed...\n";
            sqlFile.Close();
            UpdateView(rServer.ToString(), cBox_DestEnvironment.Text, db);
        }

        private void GenerateViewScriptWithDependencies(Server rServer, string db)
        {
            System.IO.StreamWriter sqlFile = new StreamWriter("CreateView_" + db + ".sql");

            Server srv;
            if (serverInstanceToRestoreTo != "Default")
            {
                // Connect to the specified instance of SQL Server.
                srv = new Server(serverInstanceToRestoreTo);
                //lbl_Oupt.Text += "Connected to " + serverInstanceToRestoreTo + " specied instance...'\n";
                rTxtBox_Output.AppendText("Connected to " + serverInstanceToRestoreTo + " specied instance...'\n",Color.Black);
            }
            else
            {
                // Connect to the default instance of SQL Server.
                srv = new Server();
                //lbl_Oupt.Text += "Connected to default instance...'\n";
                rTxtBox_Output.AppendText("Connected to default instance...'\n",Color.Black);
            }            

            // Reference the database.
            Database restoreDb = srv.Databases[db];

            // Define a Scripter object and set the required scripting options.
            Scripter scrp = new Scripter(srv);
            scrp.Options.ScriptDrops = true;
            scrp.Options.IncludeIfNotExists = true;
            scrp.Options.WithDependencies = true;

            ScriptingOptions scriptOptions = new ScriptingOptions();
            scriptOptions.ScriptDrops = true;
            scriptOptions.IncludeIfNotExists = true;

            ScriptingOptions scriptOptionsForCreate = new ScriptingOptions();
            scriptOptionsForCreate.AnsiPadding = true;
            scriptOptionsForCreate.ExtendedProperties = true;
            scriptOptionsForCreate.IncludeIfNotExists = true;

            UrnCollection udvObjs = new UrnCollection();

            // Iterate through the views in database and script each one. Display the script.
            //lbl_Oupt.Text += "Collating all views for this database...'\n";
            rTxtBox_Output.AppendText("Collating all views for this database...'\n",Color.Black);
            foreach( Microsoft.SqlServer.Management.Smo.View view in restoreDb.Views)
            {
                // Check if the view is not a system view            
                if (!view.IsSystemObject)
                {
                    ////lbl_Oupt.Text += "Adding " + view + " to the collection '\n";
                    udvObjs.Add(view.Urn);
                }
            }


            // Creating Dependency Tree
            DependencyTree dtree = scrp.DiscoverDependencies(udvObjs, true);
            DependencyWalker dwalker = new DependencyWalker();
            DependencyCollection dcollect = dwalker.WalkDependencies(dtree);

            StringBuilder sb = new StringBuilder();

            //lbl_Oupt.Text += "Building Dependencies tree for all views...'\n";
            rTxtBox_Output.AppendText("Building Dependencies tree for all views...'\n",Color.Black);

            foreach(DependencyCollectionNode dcoln in dcollect)
            {
                foreach (Microsoft.SqlServer.Management.Smo.View myView in restoreDb.Views)
                {
                if (myView.Name == dcoln.Urn.GetAttribute("Name"))
                {
                    if (dcoln.Urn.Type == "View")
                    {
                        //lbl_Oupt.Text += dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n";
                        rTxtBox_Output.AppendText("" + dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n", Color.Black);

                        /* Generating IF EXISTS and DROP command for views */
                        StringCollection viewScripts = myView.Script(scriptOptions);

                        foreach (string script in viewScripts)
                        {
                            var updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);

                            sqlFile.WriteLine(updatedScript);
                        }

                        /* Generating CREATE VIEW command */
                        viewScripts = myView.Script(scriptOptionsForCreate);
                        foreach (string create_script in viewScripts)
                        {
                            var updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);

                            sqlFile.WriteLine(updatedScript);
                        }
                    }
                }
                }
            }

            sqlFile.Write(sb);
            rTxtBox_Output.AppendText("View Scripts completed...\n",Color.Black);
            sqlFile.Close();
            UpdateView(rServer.ToString(), cBox_DestEnvironment.Text, db);
        }

        private void UpdateView(string serverInstance, string env, string db)
        {
            string sqlConnectionString = "Data Source=" + serverInstanceToRestoreTo + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "CreateView_" + db + ".sql";
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);

            rTxtBox_Output.AppendText("Opening connection to : " + serverInstance + "\n", Color.Black);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);

            rTxtBox_Output.AppendText("Loading Viewupdate file from: " + scriptFile + "\n",Color.Black);

            try
            {
                int resultSet = 0;
                resultSet = cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException ex)
            {
                rTxtBox_Output.AppendText("SME " + ex.Message + "\n",Color.Red);
            }
            catch (SqlException ex)
            {
               rTxtBox_Output.AppendText(ex.Message + "\n",Color.Red);
            }
            catch (Exception ex)
            {
                rTxtBox_Output.AppendText("E " + ex.Message + "\n",Color.Red);
            }
        }

        private void KillAllConnectionsToDb(string serverInstance, string env, string db)
        {
            //lbl_Oupt.Text += "Killing all conncetions to " + db + "...\n";

            string sqlConnectionString = "Data Source=" + serverInstance + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "KillAllConnectionsToDb.sql";
            scriptFile = scriptFile.Replace("ToBeReplaced", db);
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);
            //lbl_Oupt.Text += "Loading file from: " + scriptFile + "\n";

            try
            {
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException e)
            {
                //lbl_Oupt.Text += e.InnerException + "\n";
            }
            catch (SqlException e)
            {
                //lbl_Oupt.Text += e.InnerException + "\n";
            }
        }

        private void chkBox_ShowAll_CheckedChanged(object sender, EventArgs e)
        {
            grpBox_Databases.Controls.Clear();
            string newEnv = (string)cBox_Environment.SelectedItem;
            //comboBox1.Items.Clear();
            getAllDatabases(newEnv);
        }

        private void releaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // string srvInstance, string database, string env
            if (curSrvInstanceToConnect == "" || curDb == "" || curEnv == "")
            {
                MessageBox.Show("Missing some information...");
            }
            else
            {
                Release _release = new Release(curSrvInstanceToConnect, curDb, curEnv);
                _release.Show();
            }
        }

        private void databaseUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseList.Count == 1)
            {
                curDb = databaseList[0];
                if (curDb == "" || curEnv == "")
                {
                    MessageBox.Show("Missing some information...");
                }
                else
                {
                    DatabaseUpdates new_entry = new DatabaseUpdates(curSrvInstanceToConnect, curDb, curEnv);
                    new_entry.Show();
                }
            }
            else 
            {
                MessageBox.Show("Please, select only one database");
            }
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_ClearLogs_Click(object sender, EventArgs e)
        {
            //lbl_Oupt.Text = "";
            rTxtBox_Output.Text = "";
        }
    }

    public class Servers : List<ServerX> { }

    public class ServerX
    {
        public ServerX()
        {
            // Environments = new List<string>();
            Instances = new List<Instance>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public List<Instance> Instances { get; set; }
        //public ICollection<string> Environments { get;set;}
        public override string ToString()
        {
            return Name;
        }
    }

    public class Instance
    {
        public Instance()
        {
            Environments = new List<Environments>();
        }
        public string xInstance { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Backups { get; set; }
        public List<Environments> Environments { get; set; }
        //public override string ToString()
        //{
        //    return xInstance;
        //}
    }

    public class Environments
    {
        public Environments()
        {
        }

        public string Name { get; set; }
        public string data { get; set; }
        public string log { get; set; }
    }

   /* public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }*/
}
