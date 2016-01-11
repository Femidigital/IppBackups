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
using Microsoft.SqlServer.Management.Common;
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
        string sPort = "";
        string serverName = "";
        string sUsername = "";
        string sPassword = "";
        string r_curSrv = "";
        string r_curSrvInstance = "";
        string r_serverName = "";
        string r_sUsername = "";
        string r_sPassword = "";
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
                //sXmlFile = sSettingPath + ConfigFileName;
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

                        lbl_Output.Text += "Connecting to " + sName + " on " + curSrv + ".'\n";
                    }
                    
                }
            }
            */

            curSrvInstance = cBox_Server.SelectedItem.ToString();
            //sUsername = _servers[cBox_Server.SelectedIndex].Instances[0].User;
            //sPassword = _servers[cBox_Server.SelectedIndex].Instances[0].Password;
            /* Dinesh's stab at it...*/
            //sPassword = _servers[cBox_Server.SelectedIndex].Instances.Find(x=>x.Environments.FirstOrDefault(env => env.)))   ;
            //var envValue = cBox_Environment.SelectedItem.ToString();
            //var tempinstance = _servers
            /* Dinesh's stab at it */

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
                Microsoft.SqlServer.Management.Smo.Server selectedServer = new Microsoft.SqlServer.Management.Smo.Server();
                lbl_Output.Text += "Connected to " + selectedServer.Information.Version + " successfully.\n";
            }
            else
            {

                Microsoft.SqlServer.Management.Smo.Server selectedServer = new Microsoft.SqlServer.Management.Smo.Server(curSrvInstanceToConnect);
                selectedServer.ConnectionContext.LoginSecure = false;
                selectedServer.ConnectionContext.Login = sUsername;
                selectedServer.ConnectionContext.Password = sPassword;
                //}
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
                    lbl_Output.Text += "Connected to " + sName + " on " + curSrv + " successfully.\n";
                }
                catch (Exception ex)
                {
                    // TODO: Change font color
                    lbl_Output.Text += "Error Connecting to " + sName + " on " + curSrv + " as " + sUsername + ".'\n";
                    lbl_Output.Text += "'\t" + ex.Message + ".'\n";
                }
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

            if (!File.Exists(targetCopy))
            {
                int position = targetCopy.LastIndexOf('\\');
                string targetDir = targetCopy.Substring(0, position);
                lbl_Output.Text += "Copying backup file locally \n";
                lbl_Output.Text += "Copying from : " + filePath + "\n";
                lbl_Output.Text += "Copying to : " + targetCopy + "\n";
                //lbl_Output.Text += "Copying to : " + targetDir + "\n";
                System.IO.File.Copy(filePath, targetCopy, true);

                lbl_Output.Text += "Copyied backup file locally \n";
            }




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
                // TODO: Change font color
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
            string dbDataSubFolderPath = dataFilePath + "\\" + databaseName;
            string dbLogSubFolderPath = logFilePath + "\\" + databaseName;

            if (!Directory.Exists(@dbDataSubFolderPath))
            {
                lbl_Output.Text += "Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n";
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                lbl_Output.Text += "Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n";
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
            lbl_Output.Text += "About to restore... '\n";

            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
                lbl_Output.Text += ex.InnerException.Message + "'\n";
                //lbl_Output.Text += ex.Message + "'\n";
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
                lbl_Output.Text += "Creating Database Data Subfolder. " + dbDataSubFolderPath + "\n";
                Directory.CreateDirectory(dbDataSubFolderPath);
            }

            if (!Directory.Exists(@dbLogSubFolderPath))
            {
                lbl_Output.Text += "Creating Database Log Subfolder. " + dbLogSubFolderPath + "\n";
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
            lbl_Output.Text += "About to restore... '\n";

            try
            {
                sqlRestore.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                // TODO: Change font color
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

            //XmlDocument doc = new XmlDocument();
            //doc.Load(sXmlFile);

            //var environment = doc.SelectSingleNode("/Servers/Server[@name='" + cBox_Server.SelectedItem.ToString() + "']");
            //var environment = doc.SelectSingleNode("/Serers/Server[position()=" + cBox_Server.SelectedIndex + "]");
            //var environment = doc.SelectSingleNode("/Servers/Server[" + cBox_Server.SelectedIndex + /*"][@name='" + cBox_Server.SelectedItem.ToString() +*/ "']");

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
                //lbl_Output.Text += "Restored... '\n";
                lbl_Output.Text += "Restoring " + cBox_DestEnvironment.Text + " environment with selected database(s)... \n";
                restorebackgroundWorker.RunWorkerAsync();
            }
            else if (rBtn_Refresh.Checked)
            {
                if ((Environment)Enum.Parse(typeof(Environment), cBox_Environment.Text) >= (Environment)Enum.Parse(typeof(Environment), cBox_DestEnvironment.Text))
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
                    // TODO: Change font color
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
                // TODO: Fix deleting or relacing existing backup file
                foreach (string db in databaseList)
                {
                    string destPath = backupDestination + "\\" + db + ".bak";

                    if (File.Exists(destPath))
                    {
                        lbl_Output.Text += "Deleting existing backup file ...\n";
                        File.Delete(destPath);
                    }

                    try
                    {
                        lbl_Output.Text += "Starting Backup for " + db + ".\n";

                        // Perform a time consuming operation and report progress
                        //BackupDatabase(db, sUsername, sPassword, curSrvInstance, destPath);
                        BackupDatabase(db, sUsername, sPassword, curSrvInstanceToConnect, destPath);
                        backStatus.Add(db, true);
                        //worker.ReportProgress();
                    }
                    catch (Exception ex)
                    {
                        backStatus.Add(db, false);
                        lbl_Output.Text += ex.Message + "'\n";
                    }
                    finally
                    {
                        bool status;
                        //if (backStatus.TryGetValue(db, out status))
                        if (backStatus[db])
                        {
                            // TODO: Change font color to green
                            lbl_Output.Text += "Backup completed...\n";
                        }
                        else
                        {
                            // TODO: Change font color
                            lbl_Output.Text += "Backup completed with error(s)...\n";
                        }
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
            if (e.Cancelled == true)
            {
                lbl_Output.Text += "Canceled! '\n";
            }
            else if (e.Error != null)
            {
                // TODO: Change font color
                lbl_Output.Text += "Error: " + e.Error.Message;
            }
            else
            {
                lbl_Output.Text += "Done!!!\n\n";
                if (rBtn_Refresh.Checked)
                {
                    lbl_Output.Text += "Refreshing " + cBox_DestEnvironment.Text + " environment with selected database(s)... \n";
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
                string srvInstance = "";
                string dataFilePath = "";
                string logFilePath = "";
                string restore_dataFilePath = "";
                string restore_logFilePath = "";
                string localcopy = "";

                var si2 = cBox_DestServer.SelectedItem as ServerX;
                _selectedDestServer = si2;

                string srvName = _selectedDestServer.Name;

                foreach (var inst in _selectedDestServer.Instances)
                {
                    lbl_Output.Text += "Going through Instances on the Server... \n ";
                    for (int i = 0; i < inst.Environments.Count; i++)
                    {
                        lbl_Output.Text += "Looping through Environments..." + inst.Environments.ElementAt(i).Name + "\n ";
                        if (inst.Environments.ElementAt(i).Name == cBox_DestEnvironment.SelectedItem)
                        {
                            lbl_Output.Text += "Setting user and password ... \n ";
                            string sServer = inst.xInstance;

                            srvInstance = inst.xInstance;
                            r_sUsername = inst.User;
                            r_sPassword = inst.Password;
                            dataFilePath = inst.Environments.ElementAt(i).data;
                            logFilePath = inst.Environments.ElementAt(i).log;
                            restore_dataFilePath = "\\\\" + cBox_DestServer.Text + "\\" + dataFilePath.Replace(":", "$");
                            restore_logFilePath = "\\\\" + cBox_DestServer.Text + "\\" + logFilePath.Replace(":", "$");
                            localcopy = inst.Backups;
                        }
                    }
                }
                //iLastDestIndexSelected = cBox_Server.SelectedIndex;
                /*
                XmlDocument doc = new XmlDocument();
                doc.Load(sXmlFile);
                // Find out why the following line is throwing exception on cBox_DestServer.SelectedItem.ToString().
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
                 */



                // try another connection method
                Microsoft.SqlServer.Management.Smo.Server selectedRestoreServer = new Microsoft.SqlServer.Management.Smo.Server(srvName);
                selectedRestoreServer.ConnectionContext.LoginSecure = false;
                selectedRestoreServer.ConnectionContext.Login = r_sUsername;
                selectedRestoreServer.ConnectionContext.Password = r_sPassword;


                foreach (string db in databaseList)
                {
                    lbl_Output.Text += "Going into the foreach loop... \n ";
                    lbl_Output.Text += "Items in the selected database list :" + databaseList.Count.ToString() + "\n";
                    if (backStatus.ContainsKey(db) || rBtn_Restore.Checked)
                    {
                        lbl_Output.Text += "Got this far first... \n ";
                        if (backStatus[db] || rBtn_Restore.Checked)
                        //if (rBtn_Restore.Checked)
                        {
                            lbl_Output.Text += "Got this far... \n ";
                            try
                            {
                                string restore_db = db.Replace(cBox_Environment.Text, cBox_DestEnvironment.Text);
                                string filePath = "\\\\" + curSrv + "\\" + backupDestination.Replace(":", "$") + "\\" + db + ".bak";
                                lbl_Output.Text += "Starting restore for " + db + ".'\n";

                                // Perform a time consuming operation and report progress
                                lbl_Output.Text += "Restore : " + db + " database to " + restore_db + " database on : " + filePath + " to : " + restore_dataFilePath + " and : " + restore_logFilePath + "'\n";
                                lbl_Output.Text += "User : " + r_sUsername + "'\n";
                                lbl_Output.Text += "Selected destination server  : " + cBox_DestServer.Text + "\n";
                                // TODO: Workout how to identify which domain the server is under
                                if (cBox_DestServer.Text == "UK-CHFMIGSQL" || cBox_DestServer.Text == "UK-CHDEVSQL01" || cBox_DestServer.Text == "UK-CHDEVSQL02" || cBox_DestServer.Text == "FDC_TAB")
                                {
                                    lbl_Output.Text += "Restoring database to OSCAR domain.\n";
                                    RestoreDatabaseToOscar(restore_db, filePath, srvName, srvInstance, r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                                }
                                else
                                {
                                    lbl_Output.Text += "Restoring database to PRIVATE domain.\n";
                                    // RestoreDatabase(restore_db, filePath, srvName, r_sUsername, r_sPassword, dataFilePath, logFilePath);
                                    RestoreDatabaseToPrivate(restore_db, filePath, srvName, srvInstance, r_sUsername, r_sPassword, restore_dataFilePath, restore_logFilePath, localcopy);
                                }

                                //if (restore_db.Contains("BSOL") || restore_db.Contains("CloudAdmin"))
                                if (restore_db == (cBox_DestEnvironment.Text + "-BSOL") || restore_db == (cBox_DestEnvironment.Text + "-CloudAdmin"))
                                {
                                    Server myServer = new Server(srvInstance);
                                    GenerateViewScript(myServer, restore_db);
                                }

                                //if (restore_db.Contains("CloudAdmin") || restore_db.Contains("PersonalData"))
                                if (restore_db == (cBox_DestEnvironment.Text + "-CloudAdmin") || restore_db == (cBox_DestEnvironment.Text + "-PersonalData") || restore_db == (cBox_DestEnvironment.Text + "-Ecommerce"))
                                {
                                    update_DatabaseEntries(srvInstance, cBox_DestEnvironment.Text, restore_db);
                                }
                                //worker.ReportProgress();
                            }
                            catch (Exception ex)
                            {
                                // TODO: Change font color
                                lbl_Output.Text += ex.Message + "'\n";
                            }
                            finally
                            {
                                // TODO: Change font color to green
                                lbl_Output.Text += "Restore completed...'\n";
                            }
                        }
                        else
                        {
                            // TODO: Change font color
                            lbl_Output.Text += "\nThere was a problem with the last " + db + " backup, restore can not be performed...\n";
                        }
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
                // TODO: Change font color
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

        private void rBtn_Restore_CheckedChanged(object sender, EventArgs e)
        {
            if (rBtn_Restore.Checked == true)
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

        private void update_DatabaseEntries(string serverInstance, string env, string db)
        {
            lbl_Output.Text += "Updating Database entries for " + db + "...\n";

            string sqlConnectionString = "Data Source=" + serverInstance + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "UpdateDatabaseEntries" + db.Substring(db.IndexOf("-") + 1) + "-" + env + ".sql";
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);
            lbl_Output.Text += "Loading file from: " + scriptFile + "\n";
            //ServerConnection connection = new ServerConnection(serverInstance);
            //Server sqlServer = new Server(connection);
            try
            {
                //sqlServer.ConnectionContext.ExecuteNonQuery(script);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException e)
            {
                // TODO: Change font color
                lbl_Output.Text += e.InnerException + "\n";
            }
            catch (SqlException e)
            {
                // TODO: Change font color
                lbl_Output.Text += e.InnerException + "\n";
            }
            lbl_Output.Text += "Update completed...\n";
        }

        private void GenerateViewScript(Server rServer, string db)
        {
            Scripter scripter = new Scripter(rServer);
            Database restoreDb = rServer.Databases[db];
            System.IO.StreamWriter sqlFile = new StreamWriter("CreateView_" + db + ".sql");

            lbl_Output.Text += "Generating Script for creating Views in : " + db + " on " + rServer + "\n";
            /* With ScriptingOptions you can specify different scripting options,
             * for example to include IF NOT EXISTS, DROP statements, output location etc */
            ScriptingOptions scriptOptions = new ScriptingOptions();
            scriptOptions.ScriptDrops = true;
            scriptOptions.IncludeIfNotExists = true;

            ScriptingOptions scriptOptionsForCreate = new ScriptingOptions();
            scriptOptionsForCreate.AnsiPadding = true;
            scriptOptionsForCreate.ExtendedProperties = true;
            scriptOptionsForCreate.IncludeIfNotExists = true;

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
            lbl_Output.Text += "View Scripts completed...\n";
            sqlFile.Close();
            UpdateView(rServer.ToString(), cBox_DestEnvironment.Text, db);
        }

        private void UpdateView(string serverInstance, string env, string db)
        {
            string sqlConnectionString = "Data Source=" + cBox_DestServer.Text + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "CreateView_" + db + ".sql";
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            lbl_Output.Text += "Opening connection to : " + serverInstance + "\n";
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);
            lbl_Output.Text += "Loading Viewupdate file from: " + scriptFile + "\n";
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    int resultSet = 0;
                    resultSet = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (SqlServerManagementException ex)
                {
                    // TODO: Change font color
                    lbl_Output.Text += "SME " + ex.Message + "\n";
                }
                catch (SqlException ex)
                {
                    // TODO: Change font color
                    //lbl_Output.ForeColor = Color.Red;
                    //lbl_Output.Text += ex.Message + "\n";
                    continue;
                }
                catch (Exception ex)
                {
                    // TODO: Change font color
                    lbl_Output.Text += "E " + ex.Message + "\n";
                }
            }
        }

        private void KillAllConnectionsToDb(string serverInstance, string env, string db)
        {
            lbl_Output.Text += "Killing all conncetions to " + db + "...\n";

            string sqlConnectionString = "Data Source=" + serverInstance + "; Initial Catalog=" + db + "; Integrated Security=SSPI;";
            string scriptFile = "KillAllConnectionsToDb.sql";
            scriptFile = scriptFile.Replace("ToBeReplaced", db);
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);
            lbl_Output.Text += "Loading file from: " + scriptFile + "\n";

            try
            {
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException e)
            {
                lbl_Output.Text += e.InnerException + "\n";
            }
            catch (SqlException e)
            {
                lbl_Output.Text += e.InnerException + "\n";
            }
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


}
