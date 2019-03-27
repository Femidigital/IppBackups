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
using MsgBox;


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
        string r_port = "";
        string r_env = "";
        string r_curSrvInstance = "";
        string r_serverName = "";
        string r_sUsername = "";
        string r_sPassword = "";
        string serverInstanceToRestoreTo = "";
        string backupSource = "";
        //string scriptLocation = "..\\..\\SQL_Scripts\\";
        string scriptLocation = Directory.Exists(Application.StartupPath + "..\\bin") ? Application.StartupPath + "..\\..\\SQL_Scripts\\" : Application.StartupPath + "..\\Scripts\\";
        Dictionary<string, bool> backStatus = new Dictionary<string, bool>();
        string azureKey = "";
        bool restoreFromAzure = false;
        Microsoft.WindowsAzure.Storage.Auth.StorageCredentials myKey = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials();
        string credentialName = "mycredential";
        string divider = new string('-', 180);
        Backup sqlBackup;
        Restore sqlRestore;
        bool CancelRestore = false;

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
            Thread.Sleep(1000);
            oSplash.Close();
            this.Visible = true;

            InitializeComponent();
            InitializeCustomEvents();
            backupbackgroundWorker.WorkerReportsProgress = true;
            backupbackgroundWorker.WorkerSupportsCancellation = true;
            restorebackgroundWorker.WorkerSupportsCancellation = true;
            LoadValuesFromSettings();
            cancelAsynButton.Enabled = false;
        }

        private string GetSettingsPath()
        {
            string basePath = Application.StartupPath;
            //MessageBox.Show(basePath + "\\" + ConfigFileName);
            string path = File.Exists(basePath + "\\" + ConfigFileName) ? basePath + "\\" + ConfigFileName : "..\\..\\" + ConfigFileName;
            //MessageBox.Show(path);
            return path;

        }

        public void LoadValuesFromSettings()
        {
            String path = Application.ExecutablePath;

            sXmlFile = GetSettingsPath(); //sSettingPath + ConfigFileName;
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

                    if (inst.Backups.Contains("https://"))
                        inst.AzureKey = xInstance.Attributes["azureKey"].Value;

                    foreach (XmlNode xEnvironment in xInstance.ChildNodes)
                    {
                        var env = new Environments { Name = xEnvironment.InnerText, data = xEnvironment.Attributes["data"].Value, log = xEnvironment.Attributes["log"].Value };

                        inst.Environments.Add(env);
                    }

                    svr.Instances.Add(inst);
                }
                _servers.Add(svr);
                _servers2.Add(svr);
                i++;
            }

            cBox_Server.DataSource = _servers;
            cBox_DestServer.DataSource = _servers2;
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

            curSrvInstance = cBox_Server.SelectedItem.ToString();

            for (int i = 0; i < _servers[cBox_Server.SelectedIndex].Instances.Count; i++)
            {
                for (int j = 0; j < _servers[cBox_Server.SelectedIndex].Instances[i].Environments.Count; j++)
                {
                    if (_servers[cBox_Server.SelectedIndex].Instances[i].Environments[j].Name == cBox_Environment.SelectedItem.ToString())
                    {
                        curSrvInstance = _servers[cBox_Server.SelectedIndex].Instances[i].xInstance;
                        curSrv = _servers[cBox_Server.SelectedIndex].Name;
                        sPort = _servers[cBox_Server.SelectedIndex].Instances[i].Port;
                        sUsername = _servers[cBox_Server.SelectedIndex].Instances[i].User;
                        sPassword = _servers[cBox_Server.SelectedIndex].Instances[i].Password;
                        backupDestination = _servers[cBox_Server.SelectedIndex].Instances[i].Backups;


                        curSrvInstanceToConnect += curSrv;
                        if (curSrvInstance != "Default")
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

                        // Check if backupDestion is a URL then assigne value for azureKey
                        if (backupDestination.Contains("https"))
                        {
                            azureKey = _servers[cBox_Server.SelectedIndex].Instances[i].AzureKey;
                        }
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
                        rTxtBox_Output.AppendText("Connected to " + si.Name + " successfully.\n", Color.Black);
                    }
                }
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
            databaseList.Clear();
            string newEnv = (string)cBox_Environment.SelectedItem;
            curEnv = newEnv;
            //comboBox1.Items.Clear();
            getAllDatabases(newEnv);
        }

        public void BackupDatabase(String databaseName, String userName, String password, String serverName, String destinationPath)
        {
            //Backup sqlBackup = new Backup();
            sqlBackup = new Backup();
            Credential credential;

            sqlBackup.Action = BackupActionType.Database;
            sqlBackup.BackupSetDescription = "ArchiveDataBase:" + DateTime.Now.ToShortDateString();
            sqlBackup.BackupSetName = "Archive";

            sqlBackup.Database = databaseName;
            BackupDeviceItem deviceItem;

            //BackupDeviceItem deviceItem = new BackupDeviceItem(destinationPath, DeviceType.File);
            ServerConnection connection = new ServerConnection(serverName, userName, password);
            connection.StatementTimeout = 0;

            Server sqlServer = new Server(connection);

            if (destinationPath.Contains("https:"))
            {
                //string credentialName = "mycredential";
                credentialName = "mycredential";
                destinationPath = destinationPath.Replace("\\", "/");
                deviceItem = new BackupDeviceItem(destinationPath, DeviceType.Url);
                //sqlBackup.CredentialName = credentialName;

                credential = new Credential(sqlServer, credentialName);

                try
                {
                    sqlBackup.Initialize = true;
                    sqlBackup.SkipTapeHeader = true;
                    //credential.Create("cbsbackups", azureKey);
                    //credential.Create();
                }
                catch (ExecutionFailureException ex)
                {
                    rTxtBox_Output.AppendText("Backup already exists...", Color.Red);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("Exception Details " + e.InnerException + "\n", Color.Red);
                }
                finally
                {
                    sqlBackup.CredentialName = credentialName;
                    sqlBackup.FormatMedia = true;
                    //credential.Drop();
                }
            }
            else
            {
                deviceItem = new BackupDeviceItem(destinationPath, DeviceType.File);
                sqlBackup.ExpirationDate = DateTime.Now.AddDays(3);
            }


            Database db = sqlServer.Databases[databaseName];

            //sqlBackup.Initialize = false;
            sqlBackup.Checksum = true;
            sqlBackup.ContinueAfterError = true;
            sqlBackup.CopyOnly = true;
            sqlBackup.CompressionOption = BackupCompressionOptions.On;

            sqlBackup.Devices.Add(deviceItem);
            sqlBackup.Incremental = false;
            sqlBackup.PercentComplete += SqlBackup_PercentComplete;
            sqlBackup.Complete += SqlBackup_Complete;
            //sqlBackup.Abort 

            //sqlBackup.ExpirationDate = DateTime.Now.AddDays(3);
            sqlBackup.LogTruncation = BackupTruncateLogType.NoTruncate;

            //sqlBackup.FormatMedia = true;
            //rTxtBox_Output.AppendText("DEBUG: Before SqlBackup() url is " + destinationPath + "\n...", Color.Purple);
            try
            {
                sqlBackup.SqlBackup(sqlServer);
                backStatus.Add(databaseName, true);
            }
            catch (ExecutionFailureException ex)
            {
                if (ex.InnerException.ToString().Contains("Operation cancelled by user"))
                {
                    rTxtBox_Output.AppendText("\t\tThe backup has been aborted...\n", Color.Red);
                }
                rTxtBox_Output.AppendText("\t\tThe backup has been aborted...\n", Color.Red);
                backStatus.Add(databaseName, false);

            }
            catch (Exception e)
            {
                if (e.InnerException.ToString().Contains("Operation cancelled by user"))
                {
                    rTxtBox_Output.AppendText("\t\tThe backup was aborted...\n", Color.Red);
                    if (rBtn_Refresh.Checked)
                    {
                        CancelRestore = true;
                    }
                    backStatus.Add(databaseName, false);

                }
                else if (e.InnerException.ToString().Contains("There is currently a lease on the blob and no lease ID "))
                {
                    rTxtBox_Output.AppendText("\t\tThere is a lease on the blob for this backed up...\n", Color.Red);
                    backStatus.Add(databaseName, false);
                }
                else
                {
                    rTxtBox_Output.AppendText("\tInner Exception Details " + e.InnerException + "\n", Color.Red);
                    rTxtBox_Output.AppendText("\n\t\tMessage Details " + e.Message + "\n", Color.Red);
                    backStatus.Add(databaseName, false);
                }

            }
        }

        private void SqlBackup_Complete(object sender, ServerMessageEventArgs e)
        {
            //throw new NotImplementedException();
            //backStatus.Add(db, true);
            rTxtBox_Output.AppendText("\tBackup completed...\n", Color.Green);
        }

        private void SqlBackup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            //throw new NotImplementedException();
            if (backupbackgroundWorker.CancellationPending)
            {
                rTxtBox_Output.AppendText("\t\tCancelling pending backup from SqlBackup_PercentComplete...\n", Color.Purple);
                backupbackgroundWorker.CancelAsync();
                return;
            }
            else
            {
                rTxtBox_Output.AppendText("\t\tPercentage completed: " + e.Percent + "%.\n", Color.Black);
            }
        }

        public void RestoreDatabaseToOscar(String databaseName, String filePath, String serverName, String serverInstance, String port, String userName, String password, String dataFilePath, String logFilePath, String localCopyBackup)
        {
            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;
            string CopiedBackup = "\\\\" + serverName + "\\" + System.IO.Path.Combine(localCopyBackup, databaseName + ".bak");
            //string targetCopy = CopiedBackup.Replace(":", "$");
            string targetCopy = "";
            rTxtBox_Output.AppendText("\tRestoring from backup file : " + filePath + "...\n", Color.Black);
            if (filePath.Contains("https://"))
            {
                targetCopy = filePath;
            }
            else
            {
                targetCopy = CopiedBackup.Replace(":", "$");
            }

            if (!Directory.Exists(dbDataSubFolderPath))
            {
                rTxtBox_Output.AppendText("\tCreating Database Data Subfolder: " + dbDataSubFolderPath + ".\n", Color.Black);
                try
                {
                    Directory.CreateDirectory(dbDataSubFolderPath);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("\tCould not create subfolder because " + e.Message + "\n", Color.Red);
                    rTxtBox_Output.AppendText("\t Inner text : " + e.InnerException + "\n", Color.Red);
                }
            }

            if (!Directory.Exists(dbLogSubFolderPath))
            {
                rTxtBox_Output.AppendText("\tCreating Database Log Subfolder: " + dbLogSubFolderPath + ".\n", Color.Black);
                try
                {
                    Directory.CreateDirectory(dbLogSubFolderPath);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("\tCould not create subfolder because " + e.Message + "\n", Color.Red);
                    rTxtBox_Output.AppendText("\t Inner text : " + e.InnerException + "\n", Color.Red);
                }
            }

            if (rBtn_Refresh.Checked)
            {
                //if (File.Exists(targetCopy))
                //{
                //    //lbl_Oupt.Text += "Deleting old backup file... \n";
                //    rTxtBox_Output.AppendText("Deleting old backup file... \n", Color.Black);
                //    File.Delete(targetCopy);
                //}

                //int position = targetCopy.LastIndexOf('\\');
                //string targetDir = targetCopy.Substring(0, position);
                ////lbl_Oupt.Text += "Copying backup file locally \n";
                //rTxtBox_Output.AppendText("Copying backup file locally \n",Color.Black);
                ////lbl_Oupt.Text += "Copying from : " + filePath + "\n";
                //rTxtBox_Output.AppendText("Copying from : " + filePath + "\n", Color.Black);
                ////lbl_Oupt.Text += "Copying to : " + targetCopy + "\n";
                //rTxtBox_Output.AppendText("Copying to : " + targetCopy + "\n", Color.Black);
                //////lbl_Oupt.Text += "Copying to : " + targetDir + "\n";
                //System.IO.File.Copy(filePath, targetCopy, true);

                ////lbl_Oupt.Text += "Copyied backup file locally \n";
                //rTxtBox_Output.AppendText("Copyied backup file locally \n", Color.Black);
            }

            //Restore sqlRestore = new Restore();
            sqlRestore = new Restore();
            BackupDeviceItem deviceItem;

            if (targetCopy.Contains("https://"))
            {
                //targetCopy = localiseUNCPath(targetCopy);
                targetCopy = targetCopy;// + "/" + databaseName + ".bak";
                deviceItem = new BackupDeviceItem(targetCopy, DeviceType.Url);
                sqlRestore.CredentialName = credentialName;
            }
            else
            {
                if (File.Exists(targetCopy))
                {
                    //lbl_Oupt.Text += "Deleting old backup file... \n";
                    rTxtBox_Output.AppendText("\tDeleting old backup file... \n", Color.Black);
                    File.Delete(targetCopy);
                }

                int position = targetCopy.LastIndexOf('\\');
                string targetDir = targetCopy.Substring(0, position);
                //lbl_Oupt.Text += "Copying backup file locally \n";
                rTxtBox_Output.AppendText("\tCopying backup file locally \n", Color.Black);
                //lbl_Oupt.Text += "Copying from : " + filePath + "\n";
                rTxtBox_Output.AppendText("\tCopying from : " + filePath + "\n", Color.Black);
                //lbl_Oupt.Text += "Copying to : " + targetCopy + "\n";
                rTxtBox_Output.AppendText("\tCopying to : " + targetCopy + "\n", Color.Black);
                ////lbl_Oupt.Text += "Copying to : " + targetDir + "\n";
                System.IO.File.Copy(filePath, targetCopy, true);

                //lbl_Oupt.Text += "Copyied backup file locally \n";
                rTxtBox_Output.AppendText("\tCopyied backup file locally \n", Color.Black);
                targetCopy = targetCopy + "\\" + databaseName + ".bak";
                deviceItem = new BackupDeviceItem(targetCopy, DeviceType.File);
            }



            //BackupDeviceItem deviceItem = new BackupDeviceItem(targetCopy, DeviceType.File);
            sqlRestore.Devices.Add(deviceItem);
            sqlRestore.Database = databaseName;
            ////lbl_Oupt.Text += "Before connecting to : " + serverName + " by : " + userName + " \n";

            // rTxtBox_Output.AppendText("DEBUG: Server Name : " + serverName +  " ; Instance : " + serverInstance + " ... \n", Color.Purple);

            serverInstanceToRestoreTo = serverName;
            //serverInstanceToRestoreTo = serverName + "," + port;

            if (serverInstance != "Default")
                serverInstanceToRestoreTo = serverName + "\\" + serverInstance;
            //serverInstanceToRestoreTo = serverName + "\\" + serverInstance + "," + port;
            //ServerConnection connection = new ServerConnection(serverInstance);
            //ServerConnection connection = new ServerConnection(serverInstanceToRestoreTo);
            ServerConnection connection = new ServerConnection(serverInstanceToRestoreTo, userName, password);
            connection.StatementTimeout = 0;
            Server sqlServer = new Server(connection);

            Database db = sqlServer.Databases[databaseName];

            if (db != null)
            {
                rTxtBox_Output.AppendText("\tSetting Database to SingleUser mode... \n", Color.Black);

                try
                {
                    db.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
                    db.Alter(TerminationClause.RollbackTransactionsImmediately);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Alter failed for Database"))
                    {
                        rTxtBox_Output.AppendText("\t\tThe database could either be in Restoring state.\n", Color.Red);
                        return;
                    }
                    rTxtBox_Output.AppendText("\tException: " + ex.Message + "\n", Color.Red);
                }
            }
            else
            {
                rTxtBox_Output.AppendText("\tRestoring as a new database... \n", Color.Black);
            }
            sqlRestore.Action = RestoreActionType.Database;
            sqlRestore.NoRecovery = false;

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
                sqlRestore.PercentComplete += SqlRestore_PercentComplete1;

                //sqlRestore.PercentComplete += new PercentCompleteEventHandler(sqlRestore_PercentComplete);

                rTxtBox_Output.AppendText("\tRestoring database... \n", Color.Black);

                //KillAllConnectionsToDb(serverInstanceToRestoreTo, cBox_DestEnvironment.Text, databaseName);
                sqlRestore.SqlRestore(sqlServer);
                /*rTxtBox_Output.AppendText("\tsqlRestore done...\n", Color.Purple);
                rTxtBox_Output.AppendText("\tSetting Database: " + db.ToString() + " back to MultipleUser mode... '\n", Color.Black);
                DatabaseUserAccess dbState = db.DatabaseOptions.UserAccess;
                rTxtBox_Output.AppendText("\tCurrent database state is " + dbState.ToString() + "\n", Color.Purple);
                db.DatabaseOptions.UserAccess = DatabaseUserAccess.Multiple;
                rTxtBox_Output.AppendText("\tSetting Database TerminationClause... '\n", Color.Black);
                db.Alter(TerminationClause.RollbackTransactionsImmediately);*/
            }
            catch (SmoException ex)
            {
                if (ex.InnerException.ToString().Contains("Operation cancelled by user"))
                {
                    rTxtBox_Output.AppendText("\t\tThe restore was aborted.\n", Color.Red);
                }
                else if (ex.InnerException.ToString().Contains("cannot operate on database"))
                {
                    rTxtBox_Output.AppendText("\t\tDatabase is currently in Restoring state.", Color.Red);
                }
                else
                {
                    rTxtBox_Output.AppendText("\tSMO Exception : " + ex.InnerException + " \n", Color.Red);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("Operation cancelled by user"))
                {
                    rTxtBox_Output.AppendText("\t\tThe restore was aborted.\n", Color.Red);
                }
                else if (ex.InnerException.ToString().Contains("cannot operate on database"))
                {
                    rTxtBox_Output.AppendText("\t\tDatabase is currently in Restoring state.", Color.Red);
                }
                rTxtBox_Output.AppendText("\tException: " + ex.InnerException.Message + "\n", Color.Red);
            }
            finally
            {
                if (db != null)
                {
                    rTxtBox_Output.AppendText("\tSetting Database back to MultipleUser mode... '\n", Color.Black);
                    DatabaseUserAccess dbState = db.DatabaseOptions.UserAccess;
                    
                    db.DatabaseOptions.UserAccess = DatabaseUserAccess.Multiple;
                    rTxtBox_Output.AppendText("\tSetting Database TerminationClause... '\n", Color.Black);
                    db.Alter(TerminationClause.RollbackTransactionsImmediately);
                }
            }
            
            sqlServer.Refresh();
            db = sqlServer.Databases[databaseName];
            if (db.FileGroups[0].Files[0].Name != databaseName)
            {
                rTxtBox_Output.AppendText("\tRenaming Logical files from " + db.FileGroups[0].Files[0].Name + "... '\n", Color.Black);
                db.FileGroups[0].Files[0].Rename(databaseName);
                db.LogFiles[0].Rename(databaseName + "_log");
                rTxtBox_Output.AppendText("\tSuccessfully renamed Logical files to " + db.FileGroups[0].Files[0].Name + ". '\n", Color.Black);
            }


            if (rBtn_Refresh.Checked && cBox_DestEnvironment.Text != "PROD")
            {
                rTxtBox_Output.AppendText("\tSetting non production database to Simple mode...'\n", Color.Black);
                db.RecoveryModel = RecoveryModel.Simple;
                db.Alter();
            }
            else if (rBtn_Restore.Checked && r_env != "PROD")
            {
                rTxtBox_Output.AppendText("\tSetting non production database to Simple mode...'\n", Color.Black);
                db.RecoveryModel = RecoveryModel.Simple;
                db.Alter();
            }

            db.SetOnline();
            sqlServer.Refresh();
            connection.Disconnect();
        }

        private void SqlRestore_PercentComplete1(object sender, PercentCompleteEventArgs e)
        {
            if (!restorebackgroundWorker.CancellationPending)
            {
                restorebackgroundWorker.CancelAsync();
                return;
            }
            else
            {
                rTxtBox_Output.AppendText("\t\tPercentage completed: " + e.Percent + "%.\n", Color.Black);
            }
        }

        public void RestoreDatabase(String databaseName, String filePath, String serverName, String userName, String password, String dataFilePath, String logFilePath)
        {
            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;

            if (!Directory.Exists(@dbDataSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n";
                rTxtBox_Output.AppendText("\tCreating Database Data Subfolder. " + dbDataSubFolderPath + "\n", Color.Black);
                try
                {
                    Directory.CreateDirectory(dbDataSubFolderPath);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("\tCould not create subfolder because " + e.Message + "\n", Color.Purple);
                    rTxtBox_Output.AppendText("\t Inner text : " + e.InnerException + "\n", Color.Purple);
                }
                rTxtBox_Output.AppendText("\tSuccessfully created Database Data Subfolder. " + dbDataSubFolderPath + "\n", Color.Black);
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n";
                rTxtBox_Output.AppendText("\tCreating Database Log Subfolder. " + dbLogSubFolderPath + "\n", Color.Black);
                try
                {
                    Directory.CreateDirectory(dbLogSubFolderPath);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("\tCould not create subfolder because " + e.Message + "\n", Color.Purple);
                    rTxtBox_Output.AppendText("\t Inner text : " + e.InnerException + "\n", Color.Purple);
                }
                rTxtBox_Output.AppendText("\tSuccessfully created Database Log Subfolder. " + dbLogSubFolderPath + "\n", Color.Black);
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
            sqlRestore.NoRecovery = false;
            sqlRestore.ReplaceDatabase = true;
            sqlRestore.Complete += new ServerMessageEventHandler(sqlRestore_Complete);
            sqlRestore.PercentCompleteNotification = 10;
            sqlRestore.PercentComplete += SqlRestore_PercentComplete;
            //sqlRestore.PercentComplete += new PercentCompleteEventHandler(sqlRestore_PercentComplete);
            //lbl_Oupt.Text += "About to restore... \n";
            rTxtBox_Output.AppendText("\tAbout to restore... \n", Color.Black);

            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += ex.InnerException.Message + "\n";
                rTxtBox_Output.AppendText(ex.InnerException.Message + "\n", Color.Red);
                ////lbl_Oupt.Text += ex.Message + "'\n";
            }
            db = sqlServer.Databases[databaseName];
            db.FileGroups[0].Files[0].Rename(databaseName);
            db.LogFiles[0].Rename(databaseName + "_log");
            db.SetOnline();
            sqlServer.Refresh();

        }

        private void SqlRestore_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            if (!restorebackgroundWorker.CancellationPending)
            {
                restorebackgroundWorker.CancelAsync();
                return;
            }
            else
            {
                rTxtBox_Output.AppendText("\t\tPercentage completed: " + e.Percent + "%.\n", Color.Black);
            }
        }

        public void RestoreDatabaseToPrivate(String databaseName, String filePath, String serverName, String serverInstance, String userName, String password, String dataFilePath, String logFilePath, String localCopyBackup)
        {
            rTxtBox_Output.AppendText("\tDEBUG: Inside RestoreDatabaseToPrivate Method: .\n", Color.Purple);
            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;
            string CopiedBackup = "\\\\" + serverName + "\\" + System.IO.Path.Combine(localCopyBackup, databaseName + ".bak");
            string targetCopy = CopiedBackup.Replace(":", "$");

            if (!Directory.Exists(@dbDataSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n";
                rTxtBox_Output.AppendText("\tCreating Database Data Subfolder. " + dbDataSubFolderPath + "\n", Color.Black);
                try
                {
                    Directory.CreateDirectory(dbDataSubFolderPath);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("\tCould not create subfolder because " + e.Message + "\n", Color.Purple);
                    rTxtBox_Output.AppendText("\t Inner text : " + e.InnerException + "\n", Color.Purple);
                }
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                //lbl_Oupt.Text += "Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n";
                rTxtBox_Output.AppendText("\tCreating Database Log Subfolder. " + dbLogSubFolderPath + "\n", Color.Black);
                try
                {
                    Directory.CreateDirectory(dbLogSubFolderPath);
                }
                catch (Exception e)
                {
                    rTxtBox_Output.AppendText("\tCould not create subfolder because " + e.Message + "\n", Color.Purple);
                    rTxtBox_Output.AppendText("\t Inner text : " + e.InnerException + "\n", Color.Purple);
                }
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
            sqlRestore.PercentComplete += SqlRestore_PercentComplete2;
            //sqlRestore.PercentComplete += new PercentCompleteEventHandler(sqlRestore_PercentComplete);
            //lbl_Oupt.Text += "About to restore... \n";
            rTxtBox_Output.AppendText("About to restore... \n", Color.Black);


            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += ex.InnerException.Message + "\n";
                rTxtBox_Output.AppendText(ex.InnerException.Message + "\n", Color.Red);
                ////lbl_Oupt.Text += ex.Message + "'\n";
            }
            db = sqlServer.Databases[databaseName];
            db.FileGroups[0].Files[0].Rename(databaseName);
            db.LogFiles[0].Rename(databaseName + "_log");
            db.SetOnline();
            sqlServer.Refresh();

        }

        private void SqlRestore_PercentComplete2(object sender, PercentCompleteEventArgs e)
        {
            rTxtBox_Output.AppendText("\t\tPercentage completed: " + e.Percent + "%.\n", Color.Black);
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
            rTxtBox_Output.AppendText("\tRestore completed.\n", Color.Green);
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
            Settings oSettings = new Settings(this);
            oSettings.Show();
        }

        private void btn_Execute_Click(object sender, EventArgs e)
        {
            if (databaseList.Count > 0)
            {
                backStatus.Clear();
                btn_Execute.Enabled = false;

                if (rBtn_Backup.Checked)
                {
                    if (backupbackgroundWorker.IsBusy != true)
                    {
                        //cancelAsynButton.Enabled = true;
                        backupbackgroundWorker.WorkerSupportsCancellation = true;
                        // Start the asynchronous operation.
                        backupbackgroundWorker.RunWorkerAsync();
                        //cancelAsynButton.Enabled = false;
                    }

                }
                else if (rBtn_Restore.Checked)
                {
                    backupSource = InputMessageBox();

                    if (string.IsNullOrEmpty(backupSource))
                    {
                        btn_Execute.Enabled = true;
                    }
                    else if (restorebackgroundWorker.IsBusy != true)
                    {
                        rTxtBox_Output.AppendText("Restoring selected database(s) with the specified environment backup(s)... \n", Color.Black);
                        restorebackgroundWorker.RunWorkerAsync();
                    }

                }
                else if (rBtn_Refresh.Checked)
                {
                    if ((Environment)Enum.Parse(typeof(Environment), cBox_Environment.Text) >= (Environment)Enum.Parse(typeof(Environment), cBox_DestEnvironment.Text))
                    {
                        r_env = cBox_DestEnvironment.SelectedItem.ToString();
                        if (backupbackgroundWorker.IsBusy != true)
                        {
                            rTxtBox_Output.AppendText("Backing up selected source database(s)... \n", Color.Black);
                            // Start the asynchronous operation.
                            backupbackgroundWorker.RunWorkerAsync();
                        }
                        else
                        {
                            rTxtBox_Output.AppendText("Refresh environment... \n", Color.Black);
                        }
                    }
                    else
                    {
                        rTxtBox_Output.AppendText("Cannot refresh from a lower environemnt.\n", Color.Red);
                    }
                }
                else
                {
                    rTxtBox_Output.AppendText("Select an action to perform...\n", Color.Red);
                }
            }
            else
            {
                rTxtBox_Output.AppendText("There are no databases selected.\n", Color.Red);
            }
        }

        private string InputMessageBox()
        {
            InputBox.SetLanguage(InputBox.Language.English);
            //Save the DialogResult as res
            DialogResult res = InputBox.ShowDialog("Select Environment you are restoring from:", "Restore From",   //Text message (mandatory), Title (optional)
                InputBox.Icon.Question,                                                                         //Set icon type Error/Exclamation/Question/Warning (default info)
                InputBox.Buttons.OkCancel,                                                                      //Set buttons set OK/OKcancel/YesNo/YesNoCancel (default ok)
                InputBox.Type.ComboBox,                                                                         //Set type ComboBox/TextBox/Nothing (default nothing)
                new string[] { "PROD", "PPD", "UAT", "QA", "CI", "TESTING", "DEV" },                                                        //Set string field as ComboBox items (default null)
                true,                                                                                           //Set visible in taskbar (default false)
                new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold));

            //Check InputBox result
            if (res == System.Windows.Forms.DialogResult.OK || res == System.Windows.Forms.DialogResult.Yes)     //Set font (default by system)
            {
                return InputBox.ResultValue;
                // this.Close();
            }
            return InputBox.ResultValue;
        }

        private Microsoft.WindowsAzure.Storage.Auth.StorageCredentials GetStorageCredentials(string azureKey)
        {
            return new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("bsisqlbackups", azureKey);
        }

        private Microsoft.WindowsAzure.Storage.Auth.StorageCredentials GetAccountName(string accountName)
        {
            return new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(accountName);
        }

        private void VerifyBackupExists()
        {
            /* Check the Destination Backup directory for the Source Backup file */

            foreach (string db in databaseList)
            {
                string restorePath = "";
                string destPath = "";
                r_env = cBox_Environment.SelectedItem.ToString();
                string curContName = "";

                for (int i = 0; i < _servers[cBox_Server.SelectedIndex].Instances.Count; i++)
                {
                    for (int j = 0; j < _servers[cBox_Server.SelectedIndex].Instances[i].Environments.Count; j++)
                    {
                        if (cBox_Environment.Text == _servers[cBox_Server.SelectedIndex].Instances[i].Environments[j].Name)
                        {
                            restorePath = _servers[cBox_Server.SelectedIndex].Instances[i].Backups;
                            if (restorePath.Contains("https://"))
                            {
                                var azureKey = _servers[cBox_Server.SelectedIndex].Instances[i].AzureKey;
                                string bakUrl = _servers[cBox_Server.SelectedIndex].Instances[i].Backups;
                                string[] bakUrlSplit = Regex.Split(bakUrl, "/");
                                string contName = bakUrlSplit[3];

                                curContName = contName;

                                var bla = GetStorageCredentials(azureKey);

                                myKey = bla;

                                restoreFromAzure = true;

                                try
                                {
                                    var abm = new AzureBlobManager(contName, bla);
                                    destPath = restorePath + "/" + db + ".bak";
                                }
                                catch (Exception ex)
                                {
                                    rTxtBox_Output.AppendText("Azure Blob Manager Exception:" + ex.InnerException + "...\n", Color.Red);
                                }
                            }
                            else
                            {
                                destPath = restorePath + "\\" + db + ".bak";
                            }
                        }
                    }
                }

                string destFilePath = destPath;

                if (destFilePath.Contains("https://"))
                {
                    restoreFromAzure = true;
                }
                else
                {
                    destFilePath = "\\\\" + cBox_Server.Text + "\\" + destPath;
                    destFilePath = destFilePath.Replace(':', '$');
                }

                if (restoreFromAzure)
                {
                    AzureBlobManager abm2 = new AzureBlobManager(curContName, myKey);
                    string curDb = db.ToString() + ".bak";

                    if (rBtn_Restore.Checked)
                    {
                        curDb = curDb.Replace(r_env, backupSource);
                    }

                    try
                    {
                        rTxtBox_Output.AppendText("\tChecking Azure container for " + curDb + " ...\n", Color.Black);

                        if (abm2.DoesBlobExist(curContName, curDb))
                        {
                            rTxtBox_Output.AppendText("\tFound existing Backup file in Azure\n", Color.Black);

                            if (!backStatus.ContainsKey(db))
                            {
                                backStatus.Add(db, true);
                            }

                            if (restorebackgroundWorker.IsBusy != true)
                            {
                                restorebackgroundWorker.RunWorkerAsync();
                            }
                        }
                        else
                        {
                            rTxtBox_Output.AppendText("\tBackup file missing in Azure\n", Color.Red);
                        }

                    }
                    catch (Exception ex)
                    {
                        rTxtBox_Output.AppendText("\tException:" + ex.InnerException + "\n", Color.Red);
                    }

                }
                else if (File.Exists(destFilePath))
                {
                    rTxtBox_Output.AppendText("\tBackup file exists for the source database at " + destFilePath + " ...\n", Color.Black);
                    //File.Delete(destPath);
                    if (!backStatus.ContainsKey(db))
                    {
                        backStatus.Add(db, true);
                    }

                    rTxtBox_Output.AppendText("\tRestoring " + cBox_Environment.Text + " environment with selected database(s)... \n", Color.Black);
                    if (restorebackgroundWorker.IsBusy != true)
                    {
                        restorebackgroundWorker.RunWorkerAsync();
                    }
                }
                else
                {
                    rTxtBox_Output.AppendText("\tMissing backup file to restore at " + destFilePath + "...\n", Color.Red);
                }
            }
        }

        // This event handler is where the time-consuming work is done.
        private void backupbackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            // BackgroundWorker worker = sender as BackgroundWorker;

            if (backupbackgroundWorker.CancellationPending == true)
            {
                e.Cancel = true;
                rTxtBox_Output.AppendText("Backup process cancelled at " + DateTime.Now + "\n", Color.Black);
                return;
                //break;
            }
            else
            {
                rTxtBox_Output.AppendText("Backup process started at " + DateTime.Now + "\n", Color.Black);
                foreach (string db in databaseList)
                {
                    if (!backupbackgroundWorker.CancellationPending)
                    {
                        string destPath = "";
                        string destFilePath = "";
                        cancelAsynButton.Enabled = true;

                        if (backupDestination.Contains("https:"))
                        {
                            backupDestination = backupDestination.Replace("\\", "/");
                            destPath = backupDestination + "/" + db + ".bak";
                            destFilePath = backupDestination + "/" + db + ".bak";
                        }
                        else
                        {
                            destPath = backupDestination + "\\" + db + ".bak";
                            destFilePath = "\\\\" + cBox_Server.Text + "\\" + backupDestination + "\\" + db + ".bak";
                            destFilePath = destFilePath.Replace(':', '$');
                        }

                        if (File.Exists(destFilePath))
                        {
                            rTxtBox_Output.AppendText("\tDeleting existing backup file ...\n", Color.Black);
                            File.Delete(destFilePath);
                        }

                        try
                        {
                            rTxtBox_Output.AppendText("\n\tStarting Backup for " + db + "...\n", Color.Black);

                            if (destPath.Contains("https:"))
                            {
                                BackupDatabase(db, sUsername, sPassword, curSrvInstanceToConnect, destPath);
                            }
                            else
                            {
                                BackupDatabase(db, sUsername, sPassword, curSrvInstanceToConnect, destPath);
                            }
                            // backStatus.Add(db, true);
                        }
                        catch (Exception ex)
                        {
                            backStatus.Add(db, false);
                            rTxtBox_Output.AppendText("\tDictionary error after backup \n", Color.Red);
                            rTxtBox_Output.AppendText("\tException: " + ex.Message + "\n", Color.Red);
                            rTxtBox_Output.AppendText("\t\tInner Exception: " + ex.InnerException + "\n", Color.Red);
                        }
                        finally
                        {
                            bool status;
                            if (backStatus.ContainsKey(db))
                            {

                                if (backStatus[db])
                                {
                                    // rTxtBox_Output.AppendText("\tBackup completed...\n", Color.Green);
                                }
                                else
                                {
                                    if (rBtn_Refresh.Checked)
                                    {
                                        rTxtBox_Output.AppendText("\t" + db + " restore will be skipped...\n", Color.Red);
                                    }
                                }
                            }
                        }
                        cancelAsynButton.Enabled = false;
                    }
                    else
                    {
                        backupbackgroundWorker.CancelAsync();
                        break;
                    }
                }
                rTxtBox_Output.AppendText("\nBackup process completed at " + DateTime.Now + "\n", Color.Black);
                cancelAsynButton.Enabled = false;
            }
        }

        private void backupbackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //lbl_Oupt.Text += (e.ProgressPercentage.ToString() + "%");
            rTxtBox_Output.AppendText((e.ProgressPercentage.ToString() + "%"), Color.Black);
        }

        private void backupbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true || backupbackgroundWorker.CancellationPending)
            {
                rTxtBox_Output.AppendText("Canceled! \n", Color.Yellow);
            }
            else if (e.Error != null)
            {
                rTxtBox_Output.AppendText("Error: " + e.Error.Message, Color.Red);
            }
            else
            {
                if (rBtn_Refresh.Checked && !backupbackgroundWorker.CancellationPending)
                {
                    rTxtBox_Output.AppendText("All selected backups completed\n\n", Color.Black);
                    rTxtBox_Output.AppendText(divider + "\n", Color.Black);

                    rTxtBox_Output.AppendText("\nRefreshing " + cBox_DestEnvironment.Text + " environment with selected database(s)... \n", Color.Black);
                    restorebackgroundWorker.RunWorkerAsync();
                }
                else
                {
                    btn_Execute.Enabled = true;
                }
            }
        }

        private void restorebackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!restorebackgroundWorker.CancellationPending && !CancelRestore)
            {
                VerifyBackupExists();

                BackgroundWorker restoreWorker = sender as BackgroundWorker;

                if (restoreWorker.CancellationPending == true || backupbackgroundWorker.CancellationPending == true)
                {
                    rTxtBox_Output.AppendText("\t\tRestore operation cancelled by user...", Color.Purple);
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
                    bool restoringToOscar = false;
                    List<string> oscarServers = new List<string>() { "UK-CHFMIGSQL", "UK-CHDEVSQL01", "UK-CHDEVSQL02", "FDC_TAB", "CHI-7S45842", "CHI-PC0R83SJ", "DEV-SENT01", "UK-DEVEPI7", "BSI16DBS04PRV", "BSI16DBS03PRV", "BSI10DBS03PRV", "UK-CHIBALEL" };

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

                                r_port = inst.Port;
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

                    rTxtBox_Output.AppendText("\nRestore process started at " + DateTime.Now + "\n", Color.Black);
                    foreach (string db in databaseList)
                    {
                        if (backStatus.ContainsKey(db) || rBtn_Restore.Checked)
                        {
                            if (backStatus[db])
                            {
                                //if (rBtn_Restore.Checked)
                                //{
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

                                    //string filePath = "\\\\" + curSrv + "\\" + backupDestination.Replace(":", "$") + "\\" + db + ".bak";
                                    string filePath = "";
                                    if (backupDestination.Contains("https"))
                                    {
                                        //rTxtBox_Output.AppendText("\tRestoring from Azure...\n", Color.Black);
                                        filePath = backupDestination + "/" + db + ".bak";
                                        if (rBtn_Restore.Checked)
                                        {
                                            rTxtBox_Output.AppendText("\tUpdating from soure path in Azure...\n", Color.Black);
                                            filePath = filePath.Replace(cBox_Environment.Text, backupSource);
                                        }
                                        //else
                                        //{
                                        //    filePath = backupDestination + "/" + db + ".bak";
                                        //}
                                    }
                                    else
                                    {
                                        filePath = "\\\\" + curSrv + "\\" + backupDestination.Replace(":", "$") + "\\" + db + ".bak";
                                    }

                                    rTxtBox_Output.AppendText("\n\tStarting restore for " + restore_db + "...\n", Color.Black);

                                    // Perform a time consuming operation and report progress

                                    // TODO: Workout how to identify which domain the server is under
                                    restoringToOscar = oscarServers.Any(restoreToSrv.Contains);
                                    //if (restoreToSrv == "UK-CHFMIGSQL" || restoreToSrv == "UK-CHDEVSQL01" || restoreToSrv == "UK-CHDEVSQL02" || restoreToSrv == "FDC_TAB" || restoreToSrv == "CHI-7S45842" || restoreToSrv == "DEV-SENT01" || restoreToSrv == "UK-DEVEPI7" || restoreToSrv == "BSI16DBS04PRV" || restoreToSrv == "BSI16DBS03PRV" || restoreToSrv == "BSI10DBS03PRV")
                                    if (restoringToOscar)
                                    {
                                        RestoreDatabaseToOscar(restore_db, filePath, srvName, srvInstance, r_port, r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                                    }
                                    else
                                    {
                                        RestoreDatabaseToPrivate(restore_db, filePath, srvName, srvInstance, r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                                    }

                                    //if (restore_db.Contains("BSOL") || restore_db.Contains("CloudAdmin"))
                                    if (restore_db == (restoreToEnv + "-BSOL") || restore_db == (restoreToEnv + "-CloudAdmin"))
                                    {
                                        //Server myServer = new Server(srvInstance);
                                        Server myServer = new Server(srvName);
                                        //lbl_Oupt.Text += "Calling GenerateViewScriptWithDependencies " + srvName +" ...\n";
                                        rTxtBox_Output.AppendText("\tScripting Database Objects with Dependencies for " + srvName + " ...\n", Color.Black);

                                        //GenerateViewScriptWithDependencies(myServer, restore_db);                                    
                                        if (restore_db == (restoreToEnv + "-CloudAdmin"))
                                        {
                                            GenerateDatabaseObjectsScriptWithDependencies(myServer, restore_db, "Synonyms");
                                        }
                                        GenerateDatabaseObjectsScriptWithDependencies(myServer, restore_db, "Views");
                                    }

                                    //if (restore_db.Contains("CloudAdmin") || restore_db.Contains("PersonalData"))
                                    rTxtBox_Output.AppendText("\tApplying post restore scripts if applicable...\n", Color.Black);
                                    int dashIndex = db.IndexOf("-") + 1;
                                    DirectoryInfo d = new DirectoryInfo(scriptLocation);
                                    foreach (var file in d.GetFiles("*.sql"))
                                    {
                                        string dbPart = db.Substring(dashIndex, db.Length - dashIndex);

                                        //if (file.Name.ToLower().Contains("_" + restoreToEnv.ToLower()) && file.Name.ToLower().Contains(dbPart.ToLower() + "_") && restore_db.ToLower().Contains(restoreToEnv.ToLower() + "-") && restore_db.ToLower().Contains("-" + dbPart.ToLower()))
                                        if (file.Name.ToLower().Contains("_" + restoreToEnv.ToLower()) && file.Name.ToLower().Contains(dbPart.ToLower() + "_") && restore_db.ToLower().Contains(restoreToEnv.ToLower() + "-") && restore_db.ToLower().Contains("-" + dbPart.ToLower()) && file.Name.StartsWith(dbPart))
                                        {
                                            //update_DatabaseEntries(srvName, restoreToEnv, restore_db, file.Name);
                                            update_DatabaseEntries(srvName, srvInstance, restoreToEnv, restore_db, file.Name);
                                        }
                                    }
                                    rTxtBox_Output.AppendText("\t" + db + " Successfully restored to " + restoreToEnv + " .\n", Color.Green);
                                }
                                catch (Exception ex)
                                {
                                    rTxtBox_Output.AppendText("\tSomething happened while restoring " + db + "\n", Color.Black);
                                    rTxtBox_Output.AppendText("\tException: " + ex.Message + "\n", Color.Red);
                                    rTxtBox_Output.AppendText("\n\t\tInner exception : " + ex.InnerException + "\n", Color.Red);
                                }
                                finally
                                {
                                    //rTxtBox_Output.AppendText("\t" + db + " Restore completed...\n", Color.Green);
                                }
                            }
                            else
                            {
                                rTxtBox_Output.AppendText("\tSkipping " + db + " restore, because the backup failed...\n", Color.Red);
                            }
                        }
                        else
                        {
                            rTxtBox_Output.AppendText("\n\tThere was a problem with the last " + db + " backup, restore can not be performed...\n\n", Color.Red);
                        }
                    }
                    rTxtBox_Output.AppendText("\nRestore process completed at " + DateTime.Now + "\n", Color.Green);
                }
            }
            else
            {
                rTxtBox_Output.AppendText("\tPending restore cancelled by user...\n", Color.Black);
                CancelRestore = false;
            }
        }

        private void restorebackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //lbl_Oupt.Text += (e.ProgressPercentage.ToString() + "%");
            rTxtBox_Output.AppendText((e.ProgressPercentage.ToString() + "%"), Color.Black);
        }

        private void restorebackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //lbl_Oupt.Text += "Canceled! '\n";
                rTxtBox_Output.AppendText("Canceled! '\n", Color.Yellow);
            }
            else if (e.Error != null)
            {
                // TODO: Change font color
                //lbl_Oupt.Text += "Error: " + e.Error.Message;
                rTxtBox_Output.AppendText("Error: " + e.Error.Message + "\n", Color.Red);
            }
            else
            {
                //lbl_Oupt.Text += "Restore Done! '\n";
                rTxtBox_Output.AppendText("All selected databases has been restored. '\n", Color.Green);
                rTxtBox_Output.AppendText(divider + "\n", Color.Black);
                btn_Execute.Enabled = true;
            }
        }

        private void cancelAsynButton_Click(object sender, EventArgs e)
        {
            if (backupbackgroundWorker.WorkerSupportsCancellation == true && backupbackgroundWorker.IsBusy)
            {
                rTxtBox_Output.AppendText("\t\tUser clicked cancel while backking up...\n", Color.Red);
                // Cancel the asychronous operation.
                try
                {
                    sqlBackup.Abort();
                    rTxtBox_Output.AppendText("\t\tCurrent backup aborted...\n", Color.Red);
                    backupbackgroundWorker.CancelAsync();
                    restorebackgroundWorker.CancelAsync();
                    return;
                }
                catch (Exception ex)
                {
                    rTxtBox_Output.AppendText("\t\tException: " + ex.Message, Color.Red);
                }
            }
            else
            {
                rTxtBox_Output.AppendText("\t\tUser clicked cancel while restoring...\n", Color.Red);
                try
                {
                    sqlRestore.Abort();
                    rTxtBox_Output.AppendText("\t\tCurrent restore aborted...\n", Color.Red);
                    restorebackgroundWorker.CancelAsync();
                    return;
                }
                catch (Exception ex)
                {
                    rTxtBox_Output.AppendText("\t\tException: " + ex.Message, Color.Red);
                }
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

        private void update_DatabaseEntries(string serverName, string serverInstance, string env, string db, string cur_ScriptFile)
        {
            //rTxtBox_Output.AppendText("\tUpdating Database entries for " + db + " using " + cur_ScriptFile + " on server " + serverName + " instance " + serverInstance + "...\n", Color.Black);
            rTxtBox_Output.AppendText("\tExecuting Update_" + cur_ScriptFile + " script on " + db + " ...\n", Color.Black);

            /* Try manipulating the incoming server details to cater for both default and instanced SQL */
            Server srv;
            if (serverInstance != "Default")
            {
                // Connect to the specified instance of SQL Server.
                srv = new Server(serverInstanceToRestoreTo);

                curSrvInstanceToConnect = serverName + "\\" + curSrvInstance;
                curSrvInstance = curSrvInstanceToConnect;
                serverInstance = serverInstanceToRestoreTo;
            }
            else
            {
                // Connect to the default instance of SQL Server.
                srv = new Server();
                serverInstance = serverName;
            }

            string sqlConnectionString = "Data Source=" + serverInstance + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";

            string scriptFile = scriptLocation + cur_ScriptFile;
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);

            conn.Open();

            SqlCommand cmd = new SqlCommand(script, conn);
            cmd.CommandTimeout = 0;

            try
            {
                cmd.ExecuteNonQuery();
                rTxtBox_Output.AppendText("\tScript executed successfully.\n", Color.Black);
                conn.Close();
            }
            catch (SqlServerManagementException e)
            {
                rTxtBox_Output.AppendText("\t\tInside SqlServerManagementExcecption block...\n", Color.Red);
                rTxtBox_Output.AppendText(e.InnerException + "\n", Color.Red);
            }
            catch (SqlException e)
            {
                rTxtBox_Output.AppendText("\t\tInside SqlException...\n", Color.Red);
                rTxtBox_Output.AppendText(e.Message + "\n" + e.StackTrace + "\n", Color.Red);
                rTxtBox_Output.AppendText(e.InnerException + "\n", Color.Red);
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
                        var updatedScript = new object();
                        if (rBtn_Refresh.Checked)
                        {
                            updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                        }
                        else if (rBtn_Restore.Checked)
                        {
                            //updatedScript = Regex.Replace(create_script, Environment.CI | Environment.DEV | Environment.MIG | Environment.PPD | Environment.PROD | Environment.QA | Environment.TEST  + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                            updatedScript = Regex.Replace(create_script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                        }

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
            string destEnv = "";
            System.IO.StreamWriter sqlFile = new StreamWriter("CreateView_" + db + ".sql");

            Server srv;
            if (serverInstanceToRestoreTo != "Default")
            {
                // Connect to the specified instance of SQL Server.
                srv = new Server(serverInstanceToRestoreTo);
            }
            else
            {
                // Connect to the default instance of SQL Server.
                srv = new Server();
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
            rTxtBox_Output.AppendText("\t\tCollating all views for this database...'\n", Color.Black);
            foreach (Microsoft.SqlServer.Management.Smo.View view in restoreDb.Views)
            {
                // Check if the view is not a system view            
                if (!view.IsSystemObject)
                {
                    udvObjs.Add(view.Urn);
                }
            }


            // Creating Dependency Tree
            DependencyTree dtree = scrp.DiscoverDependencies(udvObjs, true);
            DependencyWalker dwalker = new DependencyWalker();
            DependencyCollection dcollect = dwalker.WalkDependencies(dtree);

            StringBuilder sb = new StringBuilder();

            //lbl_Oupt.Text += "Building Dependencies tree for all views...'\n";
            rTxtBox_Output.AppendText("\t\tBuilding Dependencies tree for all views...'\n", Color.Black);

            foreach (DependencyCollectionNode dcoln in dcollect)
            {
                foreach (Microsoft.SqlServer.Management.Smo.View myView in restoreDb.Views)
                {
                    if (myView.Name == dcoln.Urn.GetAttribute("Name"))
                    {
                        if (dcoln.Urn.Type == "View")
                        {
                            rTxtBox_Output.AppendText("\t\t" + dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n", Color.Black);

                            /* Generating IF EXISTS and DROP command for views */
                            StringCollection viewScripts = myView.Script(scriptOptions);

                            foreach (string script in viewScripts)
                            {
                                var updatedScript = "";

                                if (rBtn_Refresh.Checked)
                                {
                                    updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                }
                                else if (rBtn_Restore.Checked)
                                {
                                    //updatedScript = Regex.Replace(create_script, Environment.CI | Environment.DEV | Environment.MIG | Environment.PPD | Environment.PROD | Environment.QA | Environment.TEST  + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                    updatedScript = Regex.Replace(script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                }


                                sqlFile.WriteLine(updatedScript);
                            }

                            /* Generating CREATE VIEW command */
                            viewScripts = myView.Script(scriptOptionsForCreate);
                            foreach (string create_script in viewScripts)
                            {
                                var updatedScript = "";
                                if (rBtn_Refresh.Checked)
                                {
                                    updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                }
                                else if (rBtn_Restore.Checked)
                                {
                                    //updatedScript = Regex.Replace(create_script, Environment.CI | Environment.DEV | Environment.MIG | Environment.PPD | Environment.PROD | Environment.QA | Environment.TEST  + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                    updatedScript = Regex.Replace(create_script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                }

                                sqlFile.WriteLine(updatedScript);
                            }
                        }
                    }
                }
            }

            sqlFile.Write(sb);
            rTxtBox_Output.AppendText("\tView Scripts completed...\n", Color.Black);
            sqlFile.Close();

            if (rBtn_Refresh.Checked)
            {
                destEnv = cBox_DestEnvironment.Text;
            }
            else if (rBtn_Restore.Checked)
            {
                destEnv = cBox_Environment.Text;
            }

            UpdateView(rServer.ToString(), destEnv, db);
        }

        private void GenerateDatabaseObjectsScriptWithDependencies(Server rServer, string db, string dbObj)
        {
            string destEnv = "";
            System.IO.StreamWriter sqlFile = new StreamWriter("Create" + dbObj + "_" + db + ".sql");

            Server srv;
            if (serverInstanceToRestoreTo != "Default")
            {
                // Connect to the specified instance of SQL Server.
                srv = new Server(serverInstanceToRestoreTo);
            }
            else
            {
                // Connect to the default instance of SQL Server.
                srv = new Server();
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
            rTxtBox_Output.AppendText("\tCollating all " + dbObj + " for this database...'\n", Color.Black);

            switch (dbObj)
            {
                case "Views":
                    foreach (Microsoft.SqlServer.Management.Smo.View view in restoreDb.Views)
                    {
                        // Check if the view is not a system view            
                        if (!view.IsSystemObject)
                        {
                            udvObjs.Add(view.Urn);
                        }
                    }
                    break;
                case "Synonyms":
                    foreach (Microsoft.SqlServer.Management.Smo.Synonym synonym in restoreDb.Synonyms)
                    {
                        // Check if the synonym is not a system view
                        //if (!synonym..IsSchemaOwned)
                        //{
                        udvObjs.Add(synonym.Urn);
                        //}
                    }
                    break;
                case "StdProc":
                    foreach (Microsoft.SqlServer.Management.Smo.StoredProcedure stdProc in restoreDb.StoredProcedures)
                    {
                        // Check if the synonym is not a system view
                        if (!stdProc.IsSystemObject)
                        {
                            udvObjs.Add(stdProc.Urn);
                        }
                    }
                    break;
                case "Triggers":
                    foreach (Microsoft.SqlServer.Management.Smo.Trigger trigger in restoreDb.Triggers)
                    {
                        // Check if the synonym is not a system view
                        if (!trigger.IsSystemObject)
                        {
                            udvObjs.Add(trigger.Urn);
                        }
                    }
                    break;
            }
            //}

            // Creating Dependency Tree
            DependencyTree dtree = scrp.DiscoverDependencies(udvObjs, true);
            DependencyWalker dwalker = new DependencyWalker();
            DependencyCollection dcollect = dwalker.WalkDependencies(dtree);

            StringBuilder sb = new StringBuilder();

            rTxtBox_Output.AppendText("\tBuilding Dependencies tree for all " + dbObj + " ...'\n", Color.Black);

            foreach (DependencyCollectionNode dcoln in dcollect)
            {
                switch (dbObj)
                {

                    case "Views":
                        foreach (Microsoft.SqlServer.Management.Smo.View myView in restoreDb.Views)
                        {
                            if (myView.Name == dcoln.Urn.GetAttribute("Name"))
                            {
                                if (dcoln.Urn.Type == "View")
                                {
                                    rTxtBox_Output.AppendText("\t\t" + dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n", Color.Black);

                                    /* Generating IF EXISTS and DROP command for views */
                                    StringCollection viewScripts = myView.Script(scriptOptions);

                                    foreach (string script in viewScripts)
                                    {
                                        var updatedScript = "";

                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }


                                        sqlFile.WriteLine(updatedScript);
                                    }

                                    /* Generating CREATE VIEW command */
                                    viewScripts = myView.Script(scriptOptionsForCreate);
                                    foreach (string create_script in viewScripts)
                                    {
                                        var updatedScript = "";
                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }

                                        sqlFile.WriteLine(updatedScript);
                                    }
                                }
                            }
                        }
                        break;
                    case "Synonyms":
                        foreach (Microsoft.SqlServer.Management.Smo.Synonym mySynonyms in restoreDb.Synonyms)
                        {
                            if (mySynonyms.Name == dcoln.Urn.GetAttribute("Name"))
                            {
                                if (dcoln.Urn.Type == "Synonym")
                                {
                                    rTxtBox_Output.AppendText("\t\t" + dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n", Color.Black);

                                    /* Generating IF EXISTS and DROP command for views */
                                    StringCollection synonymScripts = mySynonyms.Script(scriptOptions);

                                    foreach (string script in synonymScripts)
                                    {
                                        var updatedScript = "";

                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }


                                        sqlFile.WriteLine(updatedScript);
                                    }

                                    /* Generating CREATE VIEW command */
                                    synonymScripts = mySynonyms.Script(scriptOptionsForCreate);
                                    foreach (string create_script in synonymScripts)
                                    {
                                        var updatedScript = "";
                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }

                                        sqlFile.WriteLine(updatedScript);
                                    }
                                }
                            }
                        }
                        break;
                    case "StdProc":
                        foreach (Microsoft.SqlServer.Management.Smo.View myStdProc in restoreDb.StoredProcedures)
                        {
                            if (myStdProc.Name == dcoln.Urn.GetAttribute("Name"))
                            {
                                if (dcoln.Urn.Type == "View")
                                {
                                    rTxtBox_Output.AppendText("\t\t" + dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n", Color.Black);

                                    /* Generating IF EXISTS and DROP command for views */
                                    StringCollection stdProcScripts = myStdProc.Script(scriptOptions);

                                    foreach (string script in stdProcScripts)
                                    {
                                        var updatedScript = "";

                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }


                                        sqlFile.WriteLine(updatedScript);
                                    }

                                    /* Generating CREATE VIEW command */
                                    stdProcScripts = myStdProc.Script(scriptOptionsForCreate);
                                    foreach (string create_script in stdProcScripts)
                                    {
                                        var updatedScript = "";
                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }

                                        sqlFile.WriteLine(updatedScript);
                                    }
                                }
                            }
                        }
                        break;
                    case "Triggers":
                        foreach (Microsoft.SqlServer.Management.Smo.Trigger myTriggers in restoreDb.Triggers)
                        {
                            if (myTriggers.Name == dcoln.Urn.GetAttribute("Name"))
                            {
                                if (dcoln.Urn.Type == "Trigger")
                                {
                                    rTxtBox_Output.AppendText("\t\t" + dcoln.Urn.GetAttribute("Name") + " will be scripted. '\n", Color.Black);

                                    /* Generating IF EXISTS and DROP command for views */
                                    StringCollection triggerScripts = myTriggers.Script(scriptOptions);

                                    foreach (string script in triggerScripts)
                                    {
                                        var updatedScript = "";

                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }


                                        sqlFile.WriteLine(updatedScript);
                                    }

                                    /* Generating CREATE VIEW command */
                                    triggerScripts = myTriggers.Script(scriptOptionsForCreate);
                                    foreach (string create_script in triggerScripts)
                                    {
                                        var updatedScript = "";
                                        if (rBtn_Refresh.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, cBox_Environment.Text + "-", cBox_DestEnvironment.Text + "-", RegexOptions.IgnoreCase);
                                        }
                                        else if (rBtn_Restore.Checked)
                                        {
                                            updatedScript = Regex.Replace(create_script, backupSource + "-", cBox_Environment.Text + "-", RegexOptions.IgnoreCase);
                                        }

                                        sqlFile.WriteLine(updatedScript);
                                    }
                                }
                            }
                        }
                        break;
                }


            }

            sqlFile.Write(sb);
            rTxtBox_Output.AppendText("\t" + dbObj + " Scripts completed.\n", Color.Black);
            sqlFile.Close();

            if (rBtn_Refresh.Checked)
            {
                destEnv = cBox_DestEnvironment.Text;
            }
            else if (rBtn_Restore.Checked)
            {
                destEnv = cBox_Environment.Text;
            }

            UpdateDatabaseObjects(rServer.ToString(), destEnv, db, dbObj);
        }

        private void UpdateView(string serverInstance, string env, string db)
        {
            string sqlConnectionString = "Data Source=" + serverInstanceToRestoreTo + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "CreateView_" + db + ".sql";
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);

            //rTxtBox_Output.AppendText("\tOpening connection to : " + serverInstance + "\n", Color.Black);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);

            rTxtBox_Output.AppendText("\t\tLoading Viewupdate file from: " + scriptFile + "\n", Color.Black);

            try
            {
                int resultSet = 0;
                resultSet = cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException ex)
            {
                rTxtBox_Output.AppendText("\t\tSqlServerManagementException: " + ex.Message + "\n", Color.Red);
            }
            catch (SqlException ex)
            {
                rTxtBox_Output.AppendText("\t\tSqlException: " + ex.Message + "\n", Color.Red);
            }
            catch (Exception ex)
            {
                rTxtBox_Output.AppendText("\t\tException: " + ex.Message + "\n", Color.Red);
            }
        }

        private void UpdateDatabaseObjects(string serverInstance, string env, string db, string dbObj)
        {
            string sqlConnectionString = "Data Source=" + serverInstanceToRestoreTo + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "Create" + dbObj + "_" + db + ".sql";
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);

            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);

            //rTxtBox_Output.AppendText("\tLoading " + dbObj + " update file from: " + scriptFile + "\n", Color.Black);
            rTxtBox_Output.AppendText("\tLoading script to update existing " + dbObj + " ...\n", Color.Black);

            try
            {
                int resultSet = 0;
                resultSet = cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException ex)
            {
                rTxtBox_Output.AppendText("\t\tSqlServerManagementException: " + ex.Message + "\n", Color.Red);
            }
            catch (SqlException ex)
            {
                rTxtBox_Output.AppendText("\t\tSqlException: " + ex.Message + "\n", Color.Red);
            }
            catch (Exception ex)
            {
                rTxtBox_Output.AppendText("\t\tException: " + ex.Message + "\n", Color.Red);
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
            string logs = "";
            rTxtBox_Output.SelectAll();
            rTxtBox_Output.Copy();
            logs = Clipboard.GetText();
            System.IO.StreamWriter logFile = new System.IO.StreamWriter("logs.txt");
            logFile.WriteLine(logs);
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
        public string AzureKey { get; set; }
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

}
