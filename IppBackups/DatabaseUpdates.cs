using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
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
        string useStmt = "USE ";
        string tbl = "";
        int max_row = 5;
        int min_rowCount = 2;
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
        Label lastRowMark = new Label() { Text = "*" };
        string sXmlFile = "";
        

        public DatabaseUpdates(string curInstance, string database, string env)
        {
            InitializeComponent();
            LoadValuesFromSettings(database);

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

            tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, tlp_ScriptBuilder.RowCount - 1);            

            cur_database = database;

            //script = useStmt + cur_database + "]\n";
            //rTxtBox_Script.Text += script;
            rTxtBox_Script.AppendText(useStmt, Color.Blue);
            rTxtBox_Script.AppendText("[" + cur_database + "]\n", Color.Green);

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
                rowLabel[i].Text = i.ToString();

                cBox_Logic[i] = new ComboBox();
                cBox_Logic[i].Items.AddRange(logic.ToArray());

                cBox_Operand[i] = new ComboBox();
                cBox_Operand[i].Items.AddRange(operand.ToArray());

                cBox_Field[i] = new ComboBox();
                cBox_Field[i].Items.AddRange(field.ToArray());

                txtBox_Value[i] = new TextBox();
                txtBox_Value[i].Anchor = AnchorStyles.None;                
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
            ClearScriptBuilder();
            UpdateScriptWindow();
           // script += "\nUPDATE " + tbl + "\n";
            rTxtBox_Script.AppendText("\nUPDATE ", Color.Blue);
            rTxtBox_Script.AppendText(" " + tbl + "\n ", Color.Green);
            for (int i = 0; i < (tlp_ScriptBuilder.RowCount - min_rowCount); i++ )
            {
                if ( cBox_Logic[i].SelectedItem == null)
                {
                    //script += "SET " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("SET ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    //script += " , " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText(", ", Color.Green);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    //script += "\nWHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
            }

            //rTxtBox_Script.Text = script;
        }

        private void rBtn_Replace_CheckedChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            UpdateScriptWindow();
            script += "\nREPLACE ";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Insert_CheckedChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            UpdateScriptWindow();
            script += "\nINSERT VALUES ";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Delete_CheckedChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            UpdateScriptWindow();
            script += "\nDELETE FROM " + tbl + "\n"; ;
            rTxtBox_Script.Text = script;
        }

        private void UpdateScriptWindow()
        {
            script = rTxtBox_Script.Text;
            int sqBracket = script.IndexOf("]") + 1;
            int sqrBracket = rTxtBox_Script.Text.IndexOf("]") + 1;


            int scriptLength = script.Length;
            //script = script.Replace(script.Substring(sqBracket,scriptLength - sqBracket), "");
            rTxtBox_Script.Text = rTxtBox_Script.Text.Replace(script.Substring(sqBracket, scriptLength - sqBracket), " ");
            //rTxtBox_Script.Text = rTxtBox_Script.Text.Replace(rTxtBox_Script.Text.Substring(sqBracket, scriptLength - sqBracket), " ");

            //rTxtBox_Script.SelectionStart = 0;
            //rTxtBox_Script.SelectionLength = rTxtBox_Script.GetFirstCharIndexFromLine(2);
            //rTxtBox_Script.SelectedText = "";
        }

        private void tlp_ScriptBuilder_MouseClick(object sender, MouseEventArgs e)
        {
            int row = 0;
            int verticalOffset = 0;
            int y = tlp_ScriptBuilder.RowCount;

            if (CanAddNewRow())
            {
                if (y >= min_rowCount)
                {
                    if (rBtn_Delete.Checked || rBtn_Insert.Checked || rBtn_Replace.Checked || rBtn_Update.Checked)
                    {
                        //UpdateScriptWindow();
                        if (tlp_ScriptBuilder.RowCount > min_rowCount)
                        {
                            ScriptContent();
                        }

                        int i = y - min_rowCount;

                        if (i < max_row)
                        {
                            tlp_ScriptBuilder.RowCount++;
                            tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));

                            tlp_ScriptBuilder.Controls.Add(rowLabel[i], 0, y - 1);
                            rowLabel[i].Text = i.ToString();
                            if (y > min_rowCount || rBtn_Delete.Checked)
                                tlp_ScriptBuilder.Controls.Add(cBox_Logic[i], 1, y - 1);
                            tlp_ScriptBuilder.Controls.Add(cBox_Field[i], 2, y - 1);
                            tlp_ScriptBuilder.Controls.Add(cBox_Operand[i], 3, y - 1);

                            tlp_ScriptBuilder.Controls.Add(txtBox_Value[i], 4, y - 1);
                            TableLayoutPanelCellPosition pos = tlp_ScriptBuilder.GetCellPosition(txtBox_Value[i]);
                            txtBox_Value[i].Width = tlp_ScriptBuilder.GetColumnWidths()[pos.Column];

                            tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, y);

                        }
                    }
                    else
                    {
                        MessageBox.Show("Select a modification command");
                    }
                }
            }

            //int i = y - min_rowCount;

            //if (i < max_row)
            //{
            //    tlp_ScriptBuilder.RowCount++;
            //    tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));

            //    tlp_ScriptBuilder.Controls.Add(rowLabel[i], 0, y - 1);
            //    rowLabel[i].Text = i.ToString();
            //    if (y > min_rowCount || rBtn_Delete.Checked)
            //        tlp_ScriptBuilder.Controls.Add(cBox_Logic[i], 1, y - 1);
            //    tlp_ScriptBuilder.Controls.Add(cBox_Field[i], 2, y - 1);
            //    tlp_ScriptBuilder.Controls.Add(cBox_Operand[i], 3, y - 1);
                
            //    tlp_ScriptBuilder.Controls.Add(txtBox_Value[i], 4, y - 1);
            //    TableLayoutPanelCellPosition pos = tlp_ScriptBuilder.GetCellPosition(txtBox_Value[i]);
            //    txtBox_Value[i].Width = tlp_ScriptBuilder.GetColumnWidths()[pos.Column];

            //    tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, y);
                
            //}

        }

        private void ScriptContent()
        {
            int i = tlp_ScriptBuilder.RowCount - (min_rowCount + 1);

            if (rBtn_Update.Checked)
            {                
                if (cBox_Logic[i].SelectedItem == null)
                {
                   // script += "SET " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("SET ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    //script += " , " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText(", ", Color.Green);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    //script += "\nWHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
            }
            else if (rBtn_Delete.Checked)
            {
               if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    script += cBox_Logic[i].SelectedItem + " " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    script += "WHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                }
            }
            else if (rBtn_Replace.Checked)
            {

            }
            else if (rBtn_Insert.Checked)
            {

            }

            //rTxtBox_Script.Text = script;
        }

        private void cBox_Tables_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            
            /* Clear the script window */
            //UpdateScriptWindow();

            tbl = cBox_Tables.SelectedItem.ToString();

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

            for (int i = 0; i < max_row; i++)
            {
                cBox_Field[i].Items.AddRange(field.ToArray());
            }

        }

        private void ClearScriptBuilder()
        {
            int j;
            for( int i = tlp_ScriptBuilder.RowCount; i > 2; i-- )
            {
                j = i - min_rowCount;
                if (j >= 1)
                {
                    tlp_ScriptBuilder.Controls.Remove(rowLabel[j-1]);
                    rowLabel[j-1].Text = "";
                    tlp_ScriptBuilder.Controls.Remove(cBox_Logic[j-1]);
                    cBox_Logic[j-1].SelectedIndex = -1;
                    tlp_ScriptBuilder.Controls.Remove(cBox_Field[j-1]);
                    cBox_Field[j-1].SelectedIndex = -1;
                    tlp_ScriptBuilder.Controls.Remove(cBox_Operand[j-1]);
                    cBox_Operand[j-1].SelectedIndex = -1;
                    tlp_ScriptBuilder.Controls.Remove(txtBox_Value[j-1]);
                    txtBox_Value[j-1].Text = "";
                    //tlp_ScriptBuilder.RowCount--;
                    //i--;
                }
            }
            
            //if (tlp_ScriptBuilder.RowCount <= 2)
            //    tlp_ScriptBuilder.RowCount = tlp_ScriptBuilder.RowCount + 1;
            tlp_ScriptBuilder.RowCount = min_rowCount;
            tlp_ScriptBuilder.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {

        }

        private void btn_Import_Click(object sender, EventArgs e)
        {

        }

        private bool CanAddNewRow()
        {
            bool LastRowCompleted = true;

            int currentlyUsedRow = tlp_ScriptBuilder.RowCount;

            for (int i = 0; i < currentlyUsedRow - 2; i++ )
            {
                if (currentlyUsedRow > 1)
                {
                    if (i == 0)
                    {
                        if (cBox_Field[i].SelectedIndex == -1 || cBox_Operand[i].SelectedIndex == -1 || txtBox_Value[i].Text == "")
                        {
                            LastRowCompleted = false;
                            break;
                        }
                    }
                    else if (cBox_Logic[i].SelectedIndex == -1 || cBox_Field[i].SelectedIndex == -1 || cBox_Operand[i].SelectedIndex == -1 || txtBox_Value[i].Text == "")
                    {
                        LastRowCompleted = false;
                        break;
                    }
                    else
                    {
                        LastRowCompleted = true;
                    }
                }
                //else
                //{
                //    LastRowCompleted = true;
                //}
            }

            return LastRowCompleted;
        }

        public void LoadValuesFromSettings(string cur_Database)
        {
            int dashPos = cur_Database.IndexOf("-") + 1;
            string _cur_Db = cur_Database.Substring(dashPos, cur_Database.Length - dashPos);
                
            //label1.Text = _cur_Db + "\n";
            TreeNode childNode;
            
            sXmlFile = ".\\Scripts\\DatabaseUpdateValues.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);

            XmlNodeList curDatabase = doc.SelectNodes("Databases/Database[@name='" + _cur_Db + "']/Tables/Table");
            XmlNode startNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']");

            tViewScripts.Nodes.Clear();
            tViewScripts.Nodes.Add(new TreeNode(_cur_Db));
            //tViewScripts.Nodes.Add(new TreeNode(startNode));

            TreeNode tNode = new TreeNode();
            tNode = tViewScripts.Nodes[0];

            if (startNode != null)
                AddNode(startNode, tNode);
        }

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            //XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i = 0;

            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                //for (i = 0; i <= inXmlNode.ChildNodes.Count - 1; i++)
                foreach (XmlNode xNode in inXmlNode.ChildNodes)
                {
                   // xNode = inXmlNode.ChildNodes[i];
                    

                   foreach(XmlNode tab in xNode.ChildNodes)
                   {
                       if (tab.Name == "Table")
                       {
                           inTreeNode.Nodes.Add(new TreeNode(tab.Attributes["name"].Value));
                           tNode = inTreeNode.Nodes[i];
                           AddNode(tab, tNode);
                       }
                       else if (tab.Name == "Environment")
                       {
                           inTreeNode.Nodes.Add(new TreeNode(tab.Attributes["name"].Value));
                           tNode = inTreeNode.Nodes[i];
                           AddNode(tab, tNode);
                       }
                   }
                   i++;
                }
            }
        }

        
            
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
