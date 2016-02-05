using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Wmi;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;

namespace IppBackups
{
    public partial class DatabaseUpdates : Form
    {
        string cur_database = "";
        string script = "";
        string useStmt = "USE [";
        string tbl = "";
        int max_row = 5;
        List<string> logic = new List<string>();
        List<string> operand = new List<string>();
        List<string> field = new List<string>();
        int x, y;
        Microsoft.SqlServer.Management.Smo.Server svr;
        Database db;
        string _svrInstance;
        ComboBox[] cBox_Logic = new ComboBox[5];
        ComboBox[] cBox_Operand = new ComboBox[5];
        ComboBox[] cBox_Field = new ComboBox[5];
        Label[] rowLabel = new Label[5];
        TextBox[] txtBox_Value = new TextBox[5];
        
        

        public DatabaseUpdates(string curInstance, string database, string env)
        {
            InitializeComponent();

            //Button btn_Add = new Button();
            //btn_Add.Text = "+";
            //bnt_Add.Click += new EventHandler(Mouse_Click);

            getTables(curInstance, database);

            lbl_DatabaseName.Text = database;

            tlp_ScriptBuilder.AutoSize = true;
            tlp_ScriptBuilder.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tlp_ScriptBuilder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlp_ScriptBuilder.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddRows;

            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Logical:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Feild: ", Anchor = AnchorStyles.None, AutoSize = true }, 2, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Operator: ", Anchor = AnchorStyles.None, AutoSize = true }, 3, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Value: ", Anchor = AnchorStyles.None, AutoSize = true }, 4, 0);

           // tlp_ScriptBuilder.Controls.Add(new Button() { Name= "btnAdd", Text ="+", Anchor = AnchorStyles.None, AutoSize = true }, 0,1);
            

            cur_database = database;

            script = useStmt + cur_database + "]\n";
            rTxtBox_Script.Text += script;

            logic.Add("AND");
            logic.Add("OR");
            logic.Add("WHERE");

            operand.Add("=");
            operand.Add("<>");

            for(int i = 0; i < max_row; i++)
            {
                rowLabel[i] = new Label();
                rowLabel[i].AutoSize = true;
                rowLabel[i].Anchor = AnchorStyles.Left;

                cBox_Logic[i] = new ComboBox();
                cBox_Logic[i].Items.AddRange(logic.ToArray());

                cBox_Operand[i] = new ComboBox();
                cBox_Operand[i].Items.AddRange(operand.ToArray());

                cBox_Field[i] = new ComboBox();
                cBox_Field[i].Items.AddRange(field.ToArray());

                txtBox_Value[i] = new TextBox();
                
            }
        }

        private void getTables(string svrInstance, string database)
        {
            svr = new Server();
            //svr.Databases[database];
            _svrInstance = svrInstance;

            ServerConnection connection = new ServerConnection(svrInstance);
            Server sqlServer = new Server(connection);

            db = sqlServer.Databases[database];

            foreach ( Table tbl in db.Tables)
            {
                cBox_Tables.Items.Add(tbl);
            }

           // cBox_Tables.DataSource = db.Tables;
            

        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Commit_Click(object sender, EventArgs e)
        {
            // this will create a .sql script file.
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {

        }

        private void rBtn_Update_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScriptWindow();
            script += "\nUPDATE " + tbl + "\n";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Replace_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScriptWindow();
            script += "\nREPLACE ";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Insert_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScriptWindow();
            script += "\nINSERT VALUES ";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Delete_CheckedChanged(object sender, EventArgs e)
        {
            UpdateScriptWindow();
            script += "\nDELETE FROM " + tbl + "\n"; ;
            rTxtBox_Script.Text = script;
        }

        private void UpdateScriptWindow()
        {
            int sqBracket = script.LastIndexOf("]") + 1;

            int scriptLength = script.Length;
            script = script.Replace(script.Substring(sqBracket,scriptLength - sqBracket), "");
            //script = script.Replace(script.Substring(script.LastIndexOf("]"), script.Length - sqBracket), " ");
        }

        private void tlp_ScriptBuilder_MouseClick(object sender, MouseEventArgs e)
        {
            int row = 0;
            int verticalOffset = 0;
            int y = tlp_ScriptBuilder.RowCount;
            int i = y - 2;

            //rowLabel = new Label[max_row];

            //cBox_Logic = new ComboBox[max_row];
            //cBox_Logic[y].Items.AddRange(logic.ToArray());

            //cBox_Operand = new ComboBox[max_row];
            //cBox_Operand[y].Items.AddRange(operand.ToArray());

            //cBox_Field = new ComboBox[max_row];
            //cBox_Field[y].Items.AddRange(field.ToArray());

            //txtBox_Value = new TextBox[max_row];

           // rTxtBox_Script.Text += "\n" + sender.ToString() + "was clicked \n";
           // rTxtBox_Script.Text += "\n" + e.ToString() + "was clicked \n";
            if (i < max_row)
            {
                tlp_ScriptBuilder.RowCount++;
                tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));
                // tlp_ScriptBuilder.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 0, y - 1); //rowLabel
                //rowLabel[y].AutoSize = true;
                //rowLabel[y].Anchor = AnchorStyles.Left;
                tlp_ScriptBuilder.Controls.Add(rowLabel[i], 0, y - 1);
                if (y > 2)
                    tlp_ScriptBuilder.Controls.Add(cBox_Logic[i], 1, y - 1);
                tlp_ScriptBuilder.Controls.Add(cBox_Field[i], 2, y - 1);
                tlp_ScriptBuilder.Controls.Add(cBox_Operand[i], 3, y - 1);
                //tlp_ScriptBuilder.Controls.Add(new TextBox() { Text = "", Anchor = AnchorStyles.None, AutoSize = true }, 4, y - 1); // txtBox_Value
                tlp_ScriptBuilder.Controls.Add(txtBox_Value[i], 4, y - 1);

                //if (y > 2)
                //    UpdateScriptWindow();
            }

        }

        private void cBox_Tables_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            tbl = cBox_Tables.SelectedItem.ToString();

            //foreach( Column cl in db.Tables[Column])
            //{
            //    rTxtBox_Script.Text += cl.ToString();
            //}

            ServerConnection connection = new ServerConnection(_svrInstance);
            Server sqlServer = new Server(connection);

            foreach (Table table in db.Tables)
            {
                // do something with each table here...
                string tableName = table.Name;

                if (tbl == "[dbo].[" + tableName + "]")
                //if (tableName == tbl)
                {
                    foreach (Column column in table.Columns)
                    {
                        // do something with each column for that table

                        field.Add(column.Name);
                    }
                }
            }
        }

        private void ClearScriptBuilder()
        {
            for( int i = tlp_ScriptBuilder.RowCount; i > 2; i-- )
            {
                tlp_ScriptBuilder.Controls.Remove(rowLabel[i]);
                tlp_ScriptBuilder.Controls.Remove(cBox_Logic[i]);
                tlp_ScriptBuilder.Controls.Remove(cBox_Field[i]);
                tlp_ScriptBuilder.Controls.Remove(cBox_Operand[i]);
                tlp_ScriptBuilder.Controls.Remove(txtBox_Value[i]);
                //tlp_ScriptBuilder.RowCount--;
                i--;
            }
        }
            
    }
}
