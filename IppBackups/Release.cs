using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Common;
using System.IO;

namespace IppBackups
{
    public partial class Release : Form
    {
        string _serverInstance;
        string _db;
        string _env;
        string _expr = "TOKEN-DbName";
        string _openingBrackets = "[[";
        string _closingBrackets = "]]";

        public Release(string srvInstance, string database, string env)
        {
            InitializeComponent();
            _serverInstance = srvInstance;
            _db = database;
            _env = env;
            openFileDialog1.InitialDirectory = @"\\10.103.109.159\Builds\bin";
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            rTxtBox_Output.Text += "Updating Database entries for " + _db + "...\n";

            string sqlConnectionString = "Data Source=" + _serverInstance + "; Initial Catalog=" + _db + "; Integrated Security=SSPI;";
            string scriptFile = "UpdateDatabaseEntries" + _db.Substring(_db.IndexOf("-") + 1) + "-" + _env + ".sql";
            FileInfo file = new FileInfo(scriptFile);
            string script = file.OpenText().ReadToEnd();
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(script, conn);
            rTxtBox_Output.Text += "Loading file from: " + scriptFile + "\n";
            //ServerConnection connection = new ServerConnection(serverInstance);
            //Server sqlServer = new Server(connection);
            try
            {
                //sqlServer.ConnectionContext.ExecuteNonQuery(script);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlServerManagementException ex)
            {
                // TODO: Change font color
                rTxtBox_Output.Text += ex.InnerException + "\n";
            }
            catch (SqlException ex)
            {
                // TODO: Change font color
                rTxtBox_Output.Text += ex.InnerException + "\n";
            }
            rTxtBox_Output.Text += "Update completed...\n";
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Path_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                tBox_Path.Text = openFileDialog1.FileName;
            }
        }
    }
}
