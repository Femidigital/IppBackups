using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO;
using Tools;


namespace IppBackups
{
    public partial class Form1 : Form
    {
        private string ConfigFileName = ConfigurationManager.AppSettings["ConfigFileName"];
        private string sSettingPath = ConfigurationManager.AppSettings["SettingsPath"];
        string sXmlFile = "";
        string backupDestination = @"";
        string curSrv = "";
        string curSrvInstance = "";
        string serverName = "";
        string sUsername = "";
        string sPassword = "";
        string r_curSrv = "";
        string r_curSrvInstance = "";
        string r_serverName = "";
        string r_sUsername = "";
        string r_sPassword = "";

        enum Environments
        {
            TEST = 0,
            Dev = 1,
            MIG = 2,
            CI = 3,
            QA = 4,
            UAT = 5,
            PPD = 6,
            PROD = 7
        };

        List<string> databaseList = new List<string>();

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
            backupbackgroundWorker.WorkerReportsProgress = true;
            backupbackgroundWorker.WorkerSupportsCancellation = true;
            LoadValuesFromSettings();
        }

        public void LoadValuesFromSettings()
        {
            if ( sSettingPath == "" )
            {
                //MessageBox.Show("Empty Configuration");
                //sXmlFile = "..\\..\\" + ConfigFileName;
                sXmlFile = ConfigFileName;
            }
            {
                //sXmlFile = sSettingPath + ConfigFileName;
                XmlDocument doc = new XmlDocument();
                doc.Load(sXmlFile);

                XmlNodeList server = doc.SelectNodes("Servers/Server");

                foreach (XmlNode xServer in server)
                {
                    cBox_Server.Items.Add(xServer.Attributes["name"].Value);
                    cBox_DestServer.Items.Add(xServer.Attributes["name"].Value);
                }

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

            XmlDocument doc2 = new XmlDocument();
            doc2.Load(sXmlFile);
            
            XmlNodeList environment = doc2.SelectNodes("Servers/Server/Environment");
            foreach (XmlNode xEnvironment in environment)
            {
                if (xEnvironment.InnerText == sName)
                {
                    curServer = xEnvironment.ParentNode;
                    curSrv = curServer.Attributes["name"].Value;
                    curSrvInstance = curServer.Attributes["instance"].Value;
                    sUsername = curServer.Attributes["user"].Value;
                    sPassword = curServer.Attributes["password"].Value;
                    backupDestination = curServer.Attributes["backups"].Value;

                    lbl_Output.Text += "Connecting to " + sName + " on " + curSrv +".'\n";
                }
            }

            Microsoft.SqlServer.Management.Smo.Server selectedServer = new Microsoft.SqlServer.Management.Smo.Server(curSrvInstance);
            selectedServer.ConnectionContext.LoginSecure = false;
            selectedServer.ConnectionContext.Login = sUsername;
            selectedServer.ConnectionContext.Password = sPassword;
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
                    //columnCount = (selectedDb / rowSize);
                    //cBox_Server.Items.Add(db.Name);
                    CheckBox box = new CheckBox();
                    box.CheckedChanged += box_CheckedChanged;
                    box.Tag = db.Name.ToString();
                    box.Text = db.Name.ToString();
                    box.AutoSize = true;
                    //box.Location = new Point(10 + (columnCount * 150) , (selectedDb % rowSize) * 20);
                    //box.Location = new Point(10 + (columnCount * 150), (grpBox_Databases.DisplayRectangle.Top + 10) + (selectedDb % rowSize) * 20);
                    box.Location = new Point(10 + (columnCount * 150), (grpBox_Databases.DisplayRectangle.Top + 10) + (selectedDb % rowSize) * 20);
                    grpBox_Databases.Controls.Add(box);

                    selectedDb++;                    
                }
                lbl_Output.Text += "Connected successfully.\n";
            }
            catch(Exception ex)
            {
                lbl_Output.Text += "Error Connecting to " + sName + " on " + curSrv + ".'\n";
                lbl_Output.Text += "'\t" + ex.Message + ".'\n";
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
                lbl_Output.Text += "Creating Database Data Subfolder: " + dbDataSubFolderPath + ".\n";
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(dbLogSubFolderPath))
            {
                lbl_Output.Text += "Creating Database Log Subfolder: " + dbLogSubFolderPath + ".\n";
                Directory.CreateDirectory(dbLogSubFolderPath);
            }

            DirectoryInfo SourcefilePaths = new DirectoryInfo(filePath.Substring(0,filePath.LastIndexOf("\\")));
            FileInfo[] Files = SourcefilePaths.GetFiles("*.*");
            foreach (FileInfo file in Files)
            {
                lbl_Output.Text += file.Name + "\n";
            }
                                
            lbl_Output.Text += "Copying backup file locally \n";
            System.IO.File.Copy(filePath, targetCopy, true);
            lbl_Output.Text += "Copyied backup file locally \n";

            targetCopy = localiseUNCPath(targetCopy);

            Restore sqlRestore = new Restore();

            BackupDeviceItem deviceItem = new BackupDeviceItem(targetCopy, DeviceType.File);
            sqlRestore.Devices.Add(deviceItem);
            sqlRestore.Database = databaseName;
            lbl_Output.Text += "Before connecting to : " + serverInstance + " by : " + userName + " \n";


            ServerConnection connection = new ServerConnection(serverInstance);
            Server sqlServer = new Server(connection);
               
            Database db = sqlServer.Databases[databaseName];
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
                lbl_Output.Text += "About to restore... '\n";
                
                   
                    sqlRestore.SqlRestore(sqlServer);
                }
                catch (Exception ex)
                {
                    lbl_Output.Text += ex.InnerException.Message + "'\n";
                }
                db = sqlServer.Databases[databaseName];
                db.FileGroups[0].Files[0].Rename(databaseName);
                db.LogFiles[0].Rename(databaseName + "_log");
                db.SetOnline();
                sqlServer.Refresh();

        }

        public void RestoreDatabase(String databaseName, String filePath, String serverName, String userName, String password, String dataFilePath, String logFilePath)
        {

            Restore sqlRestore = new Restore();

            BackupDeviceItem deviceItem = new BackupDeviceItem(filePath, DeviceType.File);
            sqlRestore.Devices.Add(deviceItem);
            sqlRestore.Database = databaseName;

            ServerConnection connection = new ServerConnection(serverName, userName, password);
            Server sqlServer = new Server(connection);


            Database db = sqlServer.Databases[databaseName];
            sqlRestore.Action = RestoreActionType.Database;

            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;

            if (!Directory.Exists(@dbDataSubFolderPath))
            {
                lbl_Output.Text += "Creating Database Data Subfolder. '\n";
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                lbl_Output.Text += "Creating Database Log Subfolder. '\n";
                Directory.CreateDirectory(dbLogSubFolderPath);
            }

            //String dataFileLocation = dataFilePath + "\\" + databaseName + "\\" + databaseName + ".mdf";
            //String logFileLocation = logFilePath + "\\" + databaseName + "\\" + databaseName + "_Log.ldf";
            string dataFileLocation = dbDataSubFolderPath + "\\" + databaseName + ".mdf";
            string logFileLocation = dbLogSubFolderPath + "\\" + databaseName + "_Log.ldf";

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
            lbl_Output.Text += "About to restore... '\n";

            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                lbl_Output.Text += ex.InnerException.Message + "'\n";
                //lbl_Output.Text += ex.Message + "'\n";
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

            XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);

            var environment = doc.SelectSingleNode("/Servers/Server[@name='" + cBox_Server.SelectedItem.ToString() + "']");

            foreach (XmlNode xEnvironment in environment.ChildNodes)
            {
                cBox_Environment.Items.Add(xEnvironment.InnerText);
            }
            cBox_Environment.SelectedItem = cBox_Environment.Items[0];
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
                lbl_Output.Text += "Restored... '\n";
            }
            else if (rBtn_Refresh.Checked)
            {
                if ((Environments)Enum.Parse(typeof(Environments), cBox_Environment.Text) >= (Environments)Enum.Parse(typeof(Environments), cBox_DestEnvironment.Text))
                {
                    if (backupbackgroundWorker.IsBusy != true)
                    {
                        // Start the asynchronous operation.
                        backupbackgroundWorker.RunWorkerAsync();
                    }
                    else
                    {
                        lbl_Output.Text += "Refresh environment... '\n";
                    }
                }
                else
                {
                    lbl_Output.Text += "Cannot refresh from a lower environemnt... '\n";
                }
            }
            else
            {
                lbl_Output.Text += "Select an action to perform...'\n";
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
                foreach (string db in databaseList)
                {
                    try
                    {
                        string destPath = backupDestination + "\\" + db + ".bak";
                        lbl_Output.Text += "Starting Backup for " + db + ".'\n";

                        // Perform a time consuming operation and report progress
                        BackupDatabase(db, sUsername, sPassword, curSrvInstance, destPath);
                        //worker.ReportProgress();
                    }
                    catch (Exception ex)
                    {
                        lbl_Output.Text += ex.Message + "'\n";
                    }
                    finally
                    {
                        lbl_Output.Text += "Backup completed...'\n";
                        //restorebackgroundWorker.RunWorkerAsync();
                    }
                }
            }
        }

        private void backupbackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbl_Output.Text += (e.ProgressPercentage.ToString() + "%");
        }

        private void backupbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Cancelled == true)
            {
                lbl_Output.Text += "Canceled! '\n";
            }
            else if (e.Error != null)
            {
                lbl_Output.Text += "Error: " + e.Error.Message;
            }
            else
            {
                lbl_Output.Text += "Done!";
                if ( rBtn_Refresh.Checked)
                {
                    lbl_Output.Text += "Refresh environment... '\n";
                    restorebackgroundWorker.RunWorkerAsync();
                }
            }
        }

        private void restorebackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker restoreWorker = sender as BackgroundWorker;

            if (restoreWorker.CancellationPending == true)
            {
                e.Cancel = true;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(sXmlFile);
                var sServer = doc.SelectSingleNode("/Servers/Server[@name='" + cBox_DestServer.SelectedItem.ToString() + "']");
                var environment = doc.SelectSingleNode("/Servers/Server/Environment[@name='" + cBox_DestEnvironment.SelectedItem.ToString() + "']");
                string srvName = sServer.Attributes["name"].Value;
                string srvInstance = sServer.Attributes["instance"].Value;
                string dataFilePath = environment.Attributes["data"].Value;
                string logFilePath = environment.Attributes["log"].Value;
                string restore_dataFilePath = "\\\\" + cBox_DestServer.Text + "\\" + dataFilePath.Replace(":", "$");
                string restore_logFilePath = "\\\\" + cBox_DestServer.Text + "\\" + logFilePath.Replace(":", "$");
                string localcopy = sServer.Attributes["backups"].Value;

                // try another connection method
                r_sUsername = sServer.Attributes["user"].Value;
                r_sPassword = sServer.Attributes["password"].Value;

                // try another connection method
                Microsoft.SqlServer.Management.Smo.Server selectedRestoreServer = new Microsoft.SqlServer.Management.Smo.Server(srvName);
                selectedRestoreServer.ConnectionContext.LoginSecure = false;
                selectedRestoreServer.ConnectionContext.Login = r_sUsername;
                selectedRestoreServer.ConnectionContext.Password = r_sPassword;

                
                foreach (string db in databaseList)
                {
                    try
                    {
                        string restore_db = db.Replace(cBox_Environment.Text, cBox_DestEnvironment.Text);
                        string filePath = "\\\\" + curSrv + "\\" + backupDestination.Replace(":", "$") + "\\" + db + ".bak";
                        lbl_Output.Text += "Starting restore for " + db + ".'\n";

                        // Perform a time consuming operation and report progress
                        lbl_Output.Text += "Restore : " + db + " database to " + restore_db + " database on : " + filePath + " to : " + restore_dataFilePath + " and : " + restore_logFilePath + "'\n";
                        lbl_Output.Text += "User : " + r_sUsername + "'\n";
                        //RestoreDatabase(restore_db, filePath, srvName, r_sUsername, r_sPassword, dataFilePath, logFilePath);
                        RestoreDatabaseToOscar(restore_db, filePath, srvName, srvInstance ,r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                        //worker.ReportProgress();
                    }
                    catch (Exception ex)
                    {
                        lbl_Output.Text += ex.Message + "'\n";
                    }
                    finally
                    {
                        lbl_Output.Text += "Restore completed...'\n";
                    }
                }
            }
        }

        private void restorebackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            lbl_Output.Text += (e.ProgressPercentage.ToString() + "%");
        }

        private void restorebackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                lbl_Output.Text += "Canceled! '\n";
            }
            else if (e.Error != null)
            {
                lbl_Output.Text += "Error: " + e.Error.Message;
            }
            else
            {
                lbl_Output.Text += "Restore Done! '\n";
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

        private void rBtn_Refresh_CheckedChanged(object sender, EventArgs e)
        {
            if (rBtn_Refresh.Checked == true)
            {
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

            XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);

            // XmlNodeList environment = doc.SelectNodes("Servers/Server['"+cBox_Server.SelectedItem.ToString()+"']");
            var environment = doc.SelectSingleNode("/Servers/Server[@name='" + cBox_DestServer.SelectedItem.ToString() + "']");

            foreach (XmlNode xEnvironment in environment.ChildNodes)
            {
                cBox_DestEnvironment.Items.Add(xEnvironment.InnerText);
            }
            cBox_DestEnvironment.SelectedItem = cBox_DestEnvironment.Items[0];
        }
    }
}
