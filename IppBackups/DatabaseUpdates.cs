﻿using System;
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
        XmlDocument doc;
        string cur_database = "";
        string cur_environment = "";
        string sel_environment = "";
        string script = "";
        string useStmt = "USE ";
        string tbl = "";
        int max_row = 5;
        int min_rowCount = 2;
        List<string> logic = new List<string>();
        List<string> operand = new List<string>();
        List<string> field = new List<string>();
        List<string> fieldDatatypes = new List<string>();
        //List<KeyValuePair<string, string>> field = new List<KeyValuePair<string, string>>();
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
        bool afterWhile = false;
        bool scriptFromTreeView = false;
        XmlNode startNode;
        TreeNode cloneNode;

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
        

        public DatabaseUpdates(string curInstance, string database, string env)
        {
            InitializeComponent();
            //this.DoubleBuffered = true;
            doc = new XmlDocument();

            LoadValuesFromSettings(database);

            getTables(curInstance, database);

            lbl_DatabaseName.Text = database;
            cur_environment = env;

            tViewScripts.NodeMouseClick += 
                new TreeNodeMouseClickEventHandler(tViewScripts_NodeMouseClick);

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
            rTxtBox_Script.AppendText("[" + cur_database + "]", Color.Green);

            logic.Add("AND");
            logic.Add("OR");
            logic.Add("WHERE");
            logic.Add("WITH");


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
            XmlNode root = doc.DocumentElement;
            bool whereClause = false;
            string tblName = "";
            // this will create a .sql script file.
            if (scriptFromTreeView)
            {
                MessageBox.Show("Update selected node in TreeView");
                tblName = tViewScripts.SelectedNode.Parent.Parent.Text;
                
                XmlNode node = doc.SelectSingleNode("//Databases/Database[@name='" + cur_database + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens/ReplaceToken[@name='"+ tViewScripts.SelectedNode.Text + "']");
            }
            else
            {
                string dbName = cur_database;
                dbName = dbName.Substring(dbName.IndexOf("-") + 1, dbName.Length - (dbName.IndexOf("-") + 1));
                tblName = cBox_Tables.SelectedItem.ToString();
                tblName = tblName.Substring(7, tblName.Length - 8);

                //doc.Load(".\\Scripts\\DatabaseUpdateValues.xml");
                doc.Load("..\\..\\Scripts\\DatabaseUpdateValues.xml");

                //XmlNode node = doc.SelectSingleNode("//Databases/Database[@name='" + cur_database +"']/Tables/Table[@name='" + cBox_Tables.SelectedItem + "']/Environments/Environment[@name='" + cur_environment +"']");
                XmlNode node = doc.SelectSingleNode("//Databases/Database[@name='" + cur_database + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']");
                         
                if (node == null)
                {
                    int NoToken = tlp_ScriptBuilder.RowCount - 3;
                    XmlNode[] token = new XmlNode[NoToken];

                    //Create a new Database node to attach.
                    XmlNode database = doc.CreateNode(XmlNodeType.Element, "Database", null);

                    XmlAttribute name = doc.CreateAttribute("name");
                    name.Value = dbName;

                    database.Attributes.Append(name);

                    XmlNode tables = doc.CreateNode(XmlNodeType.Element, "Tables", null);

                    // Create a new Table Node 
                    XmlNode table = doc.CreateNode(XmlNodeType.Element, "Table", null);

                    XmlAttribute tblNode = doc.CreateAttribute("name");
                    tblNode.Value = tblName;

                    table.Attributes.Append(tblNode);

                    XmlNode environments = doc.CreateNode(XmlNodeType.Element, "Environments", null);

                    // Create a newEnvironment Node
                    XmlNode envNode = doc.CreateNode(XmlNodeType.Element, "Environment", null);

                    XmlAttribute envName = doc.CreateAttribute("name");
                    envName.Value = cur_environment;

                    envNode.Attributes.Append(envName);

                    //Create a new Tokens Node
                    XmlNode tokens = doc.CreateNode(XmlNodeType.Element, "Tokens", null);

                    //Create a new ReplaceToken Node
                    XmlNode replaceToken = doc.CreateNode(XmlNodeType.Element, "ReplaceToken", null);

                    XmlAttribute repTokenName = doc.CreateAttribute("name");
                    repTokenName.Value = tblName;

                    XmlAttribute repTokenType = doc.CreateAttribute("type");
                    repTokenType.Value = "ColumnToken";

                    XmlAttribute repTokenDML = doc.CreateAttribute("dml");
                    var checkedButton = grpBox_DML.Controls.OfType<RadioButton>()
                        .FirstOrDefault(r => r.Checked);
                    repTokenDML.Value = checkedButton.Text;

                    replaceToken.Attributes.Append(repTokenName);
                    replaceToken.Attributes.Append(repTokenType);
                    replaceToken.Attributes.Append(repTokenDML);


                    //Create a new FilterToken Node
                    XmlNode filterToken = doc.CreateNode(XmlNodeType.Element, "FilterToken", null);

                    XmlAttribute filTokenName = doc.CreateAttribute("name");
                    filTokenName.Value = tblName;

                    XmlAttribute filTokenType = doc.CreateAttribute("type");
                    filTokenType.Value = "FilterToken";

                    filterToken.Attributes.Append(filTokenName);
                    filterToken.Attributes.Append(filTokenType);

                    for (int i = 0; i < tlp_ScriptBuilder.RowCount - 3; i++)
                    {
                        token[i] = doc.CreateNode(XmlNodeType.Element, "Token", null);

                        XmlAttribute set = doc.CreateAttribute("set");

                        if (cBox_Logic[i].SelectedItem != null)
                        {
                            set.Value = (string)cBox_Logic[i].SelectedItem;
                            if (set.Value == "WHERE")
                            {
                                whereClause = true;
                            }
                        }
                        else
                        {
                            set.Value = "";
                        }

                        XmlAttribute columnName = doc.CreateAttribute("columnName");
                        columnName.Value = (string)cBox_Field[i].SelectedItem;

                        XmlAttribute operand = doc.CreateAttribute("operand");
                        operand.Value = (string)cBox_Operand[i].SelectedItem;

                        XmlAttribute value = doc.CreateAttribute("value");
                        value.Value = txtBox_Value[i].Text;

                        token[i].Attributes.Append(set);
                        token[i].Attributes.Append(columnName);
                        token[i].Attributes.Append(operand);
                        token[i].Attributes.Append(value);

                        if (node != null)
                        {
                            node.AppendChild(token[i]);
                        }
                        else
                        {
                            if (!whereClause)
                            {
                                replaceToken.AppendChild(token[i]);
                            }
                            else
                            {
                                filterToken.AppendChild(token[i]);
                            }

                        }
                    }

                    tokens.AppendChild(replaceToken);
                    tokens.AppendChild(filterToken);
                    envNode.AppendChild(tokens);
                    environments.AppendChild(envNode);
                    table.AppendChild(environments);
                    tables.AppendChild(table);
                    database.AppendChild(tables);

                    // Attach the token to the new database node.
                    //root.InsertAfter(token[i], root.LastChild);
                    doc.GetElementsByTagName("Databases")[0].InsertAfter(database, doc.GetElementsByTagName("Databases")[0].LastChild);
                }

                //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                doc.Save("..\\..\\Scripts\\DatabaseUpdateValues.xml");    
            }

        }

        private void btn_Add_Click(object sender, EventArgs e)
        {

        }

        private void rBtn_Update_CheckedChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            UpdateScriptWindow();
           // script += "\nUPDATE " + tbl + "\n";
            //rTxtBox_Script.AppendText(useStmt, Color.Blue);
            //rTxtBox_Script.AppendText("[" + cur_database + "]\n", Color.Green);
            rTxtBox_Script.AppendText("\nUPDATE ", Color.Blue);
            rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Green);
            for (int i = 0; i < (tlp_ScriptBuilder.RowCount - min_rowCount); i++ )
            {
                if ( cBox_Logic[i].SelectedItem == null)
                {
                    //script += "SET " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("\nSET ", Color.Blue);
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
                    if (CheckSQL_Syntax())
                    {
                        //script += "\nWHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                        rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                        rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                    }
                }
            }

            //rTxtBox_Script.Text = script;
        }

        private void rBtn_Replace_CheckedChanged(object sender, EventArgs e)
        {
            ClearScriptBuilder();
            UpdateScriptWindow();

            rTxtBox_Script.AppendText("Build script then click Generate after", Color.Black);

            //rTxtBox_Script.AppendText("\nDECLARE ", Color.Blue);
            //rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem, Color.Black);
            //rTxtBox_Script.AppendText("DataType",Color.Black);
            //rTxtBox_Script.AppendText("\nSET ", Color.Blue);
            //rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " = (", Color.Black);
            //rTxtBox_Script.AppendText(" SELECT ", Color.Black);
            //rTxtBox_Script.AppendText("TOP ", Color.Orange);
            //rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem, Color.Black);
            //rTxtBox_Script.AppendText(" FROM ", Color.Blue);
            //rTxtBox_Script.AppendText("" + cBox_Tables.SelectedItem, Color.Blue);
            //rTxtBox_Script.AppendText(" (NOLOCK)", Color.Black);
            //rTxtBox_Script.AppendText("\n");
            //rTxtBox_Script.AppendText("SELECT", Color.Blue);
            //rTxtBox_Script.AppendText("@" + cBox_Field[0].SelectedItem + " =", Color.Black);
            //rTxtBox_Script.AppendText(" SUBSTRING ", Color.Pink);
            //rTxtBox_Script.AppendText("(@Current" + cBox_Field[0].SelectedItem + " ,", Color.Black);
            //rTxtBox_Script.AppendText(" 0", Color.Orange);
            //rTxtBox_Script.AppendText(" , PATINDEX(", Color.Pink);

            //rTxtBox_Script.AppendText("'" + txtBox_Value[0].Text + "'", Color.Red);
            //rTxtBox_Script.AppendText(" , @Current" + cBox_Field[0] + "))\n");
            
            //rTxtBox_Script.AppendText("\nUPDATE ", Color.Pink);
            //rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Black);
            //rTxtBox_Script.AppendText("SET ", Color.Blue);
            //rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem + " = ", Color.Black);
            //rTxtBox_Script.AppendText(" REPLACE", Color.Pink);
            //rTxtBox_Script.AppendText("([" + cBox_Field[0].SelectedItem + "], @Current" + cBox_Field[0].SelectedItem + ",", Color.Black);
            //rTxtBox_Script.AppendText("'" + txtBox_Value[1].Text + "'", Color.Red);
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
            //script += "\nDELETE FROM " + tbl + "\n"; ;
            rTxtBox_Script.AppendText("\nDELETE", Color.Blue);
            rTxtBox_Script.AppendText(" " + tbl + "", Color.Green);
            //rTxtBox_Script.Text = script;
        }

        private void UpdateScriptWindow()
        {
            var text = "";  // Holds the text of current line being looped.
            var startindex = 0; // The position where selection starts.
            var endindex = 0;   // The lenght of selection.

            for (int i = 0; i < rTxtBox_Script.Lines.Length; i++)   //Loops through each line of text in RichTextBox
            {
                if (i > 0)
                {
                    text = rTxtBox_Script.Lines[i]; //Stores current line of text.
                    if (text.Length > 0)
                    {
                        startindex = rTxtBox_Script.GetFirstCharIndexFromLine(i);   //If match is found the index of first char of that line is stored in startindex.
                        endindex = text.Length; // Gets the length of line till semicolon and stores it in endindex.
                        rTxtBox_Script.Select(startindex, endindex);    //Selects the text.
                        rTxtBox_Script.Text = rTxtBox_Script.Text.Replace(rTxtBox_Script.SelectedText, string.Empty);   //Replaces the text with empty string.
                    }
                }
            }
            rTxtBox_Script.Text = "";
            rTxtBox_Script.AppendText(useStmt, Color.Blue);
            if (scriptFromTreeView)
            {
                int pos = cur_database.IndexOf("-");
                string old_environment = cur_database.Substring(0, pos);
                cur_database = cur_database.Replace(old_environment, sel_environment);
            }
            rTxtBox_Script.AppendText("[" + cur_database + "]\n", Color.Green);
        }
       
        private void tlp_ScriptBuilder_MouseClick(object sender, MouseEventArgs e)
        {            
            int row = 0;
            int verticalOffset = 0;
            int y = tlp_ScriptBuilder.RowCount;

            if (CanAddNewRow() && CheckSQL_Syntax())
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
            else
            {
                MessageBox.Show("SQL Syntax error");
            }

        }

        private bool CheckSQL_Syntax()
        {
            int count = 0;
            bool goodToGo = true;

            int cur_Row = tlp_ScriptBuilder.RowCount - min_rowCount;

            for (int i = 0; i <= cur_Row - 1; i++)
            {
                if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    count++;
                    afterWhile = true;
                    if (i == cur_Row - 1  && count > 1)
                    {
                        goodToGo = false;
                    }
                }
            }
            return goodToGo;
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
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    if (CheckSQL_Syntax() && afterWhile == false)
                    {
                        rTxtBox_Script.AppendText(", ", Color.Green);
                    }
                    else
                    {
                        rTxtBox_Script.AppendText("" + cBox_Logic[i].SelectedItem + " ", Color.Green);
                    }
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    if (CheckSQL_Syntax())
                    {
                        //script += "\nWHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                        rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                    }
                }
            }
            else if (rBtn_Delete.Checked)
            {
               if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    //script += cBox_Logic[i].SelectedItem + " " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("" + cBox_Logic[i].SelectedItem + " ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText("" + cBox_Operand[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    //script += "WHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
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
                        fieldDatatypes.Add(column.DataType.ToString());
                        //field.Add(column.Name, column.DataType.ToString());
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

            //XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);

            XmlNodeList curDatabase = doc.SelectNodes("Databases/Database[@name='" + _cur_Db + "']/Tables/Table");
            //XmlNode startNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']");
            startNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']");

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
            int j = 0;

            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                //for (i = 0; i <= inXmlNode.ChildNodes.Count - 1; i++)
                foreach (XmlNode xNode in inXmlNode.ChildNodes)
                {
                   // xNode = inXmlNode.ChildNodes[i];
                    

                   foreach(XmlNode tab in xNode.ChildNodes)
                   {
                       //foreach (XmlNode col in tab.ChildNodes)
                       //{
                           if (tab.Name == "Table")
                           {
                               inTreeNode.Nodes.Add(new TreeNode(tab.Attributes["name"].Value));
                               tNode = inTreeNode.Nodes[i];
                               tNode.Tag = "Table";
                               AddNode(tab, tNode);
                               i++;
                           }
                           else if (tab.Name == "Environment")
                           {
                               inTreeNode.Nodes.Add(new TreeNode(tab.Attributes["name"].Value));
                               tNode = inTreeNode.Nodes[i];
                               tNode.Tag = "Environment";
                               AddNode(tab, tNode);
                               i++;

                           }
                           else if (tab.Name == "ReplaceToken")
                           {
                               inTreeNode.Nodes.Add(new TreeNode(tab.Attributes["name"].Value));
                               tNode = inTreeNode.Nodes[i];
                               tNode.Tag = "ReplaceToken";
                               AddNode(tab, tNode);
                           }
                       //}
                   }
                }
            }
        }

        private void btn_Generate_Click(object sender, EventArgs e)
        {
            UpdateScriptWindow();

            if( rBtn_Replace.Checked)
            {
                rTxtBox_Script.AppendText("\nDECLARE ", Color.Blue);
                rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem, Color.Black);
                rTxtBox_Script.AppendText(" " + fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() + "", Color.Blue);
                rTxtBox_Script.AppendText("\nSET ", Color.Blue);
                rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " = (", Color.Black);
                rTxtBox_Script.AppendText(" SELECT ", Color.Black);
                rTxtBox_Script.AppendText("TOP ", Color.Orange);
                rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem, Color.Black);
                rTxtBox_Script.AppendText(" FROM ", Color.Blue);
                rTxtBox_Script.AppendText("" + cBox_Tables.SelectedItem, Color.Blue);
                rTxtBox_Script.AppendText(" (NOLOCK)", Color.Black);
                rTxtBox_Script.AppendText("\n");
                rTxtBox_Script.AppendText("SELECT", Color.Blue);
                rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " =", Color.Black);
                rTxtBox_Script.AppendText(" SUBSTRING ", Color.Pink);
                rTxtBox_Script.AppendText("(@Current" + cBox_Field[0].SelectedItem + " ,", Color.Black);
                rTxtBox_Script.AppendText(" 0", Color.Orange);
                rTxtBox_Script.AppendText(" , PATINDEX(", Color.Pink);

                rTxtBox_Script.AppendText("'" + txtBox_Value[0].Text + "'", Color.Red);
                rTxtBox_Script.AppendText(" , @Current" + cBox_Field[0].SelectedItem + "))\n");

                rTxtBox_Script.AppendText("\nUPDATE ", Color.Pink);
                rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Black);
                rTxtBox_Script.AppendText("SET ", Color.Blue);
                rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem + " = ", Color.Black);
                rTxtBox_Script.AppendText(" REPLACE", Color.Pink);
                rTxtBox_Script.AppendText("([" + cBox_Field[0].SelectedItem + "], @Current" + cBox_Field[0].SelectedItem + ",", Color.Black);
                rTxtBox_Script.AppendText("'" + txtBox_Value[1].Text + "')", Color.Red);
            }
        }

        void tViewScripts_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            scriptFromTreeView = true;
            tlp_ScriptBuilder.SuspendLayout();
            ClearScriptBuilder();
            //UpdateScriptWindow();            
            
            XmlNodeList nNode;

            if (e.Node.Parent != null)
            {
                if (Enum.IsDefined(typeof(Environment), e.Node.Parent.Text))
                {
                    //sel_environment = e.Node.Parent.Text;
                    sel_environment = e.Node.Text;
                    //nNode = startNode.SelectNodes("Tables/Table/Environments/Environment[@name='" + e.Node.Parent.Text + "']/Tokens/ReplaceToken[@name='" + e.Node.Text + "']");
                    nNode = startNode.SelectNodes("Tables/Table/Environments/Environment[@name='" + e.Node.Parent.Text + "']/Tokens");
                    UpdateScriptWindow();

                    foreach (XmlNode tokens in nNode)
                    {
                        int y;
                        int i;
                        foreach (XmlNode replaceNode in tokens.ChildNodes)
                        {
                            cBox_Tables.SelectedIndex = cBox_Tables.FindString("[dbo].[" + e.Node.Parent.Parent.Text + "]");

                            if (replaceNode.Attributes["name"].Value == e.Node.Text)
                            {                                
                                //tlp_ScriptBuilder.StopPaint();
                                if (replaceNode.Name == "ReplaceToken")
                                {
                                    if (replaceNode.Attributes["dml"].Value.ToLower() == "update")
                                    {
                                        rBtn_Update.Checked = true;
                                        if (!rTxtBox_Script.Text.Contains("UPDATE"))
                                        {
                                            rTxtBox_Script.AppendText("\nUPDATE ", Color.Blue);
                                            rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Green);                                            
                                        }
                                    }
                                    else if (replaceNode.Attributes["dml"].Value.ToLower() == "delete")
                                    {
                                        rBtn_Delete.Checked = true;
                                        if (!rTxtBox_Script.Text.Contains("DELETE"))
                                        {
                                            rTxtBox_Script.AppendText("\nDELETE ", Color.Blue);
                                            rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Green);
                                        }
                                    }
                                    else if (replaceNode.Attributes["dml"].Value.ToLower() == "replace")
                                    {
                                        rBtn_Replace.Checked = true;
                                    }
                                }

                                foreach (XmlNode token in replaceNode.ChildNodes)
                                {
                                    y = tlp_ScriptBuilder.RowCount;
                                    i = y - min_rowCount;
                                    cBox_Logic[i].SelectedIndex = cBox_Logic[i].Items.IndexOf(token.Attributes["set"].Value);
                                    cBox_Field[i].SelectedIndex = cBox_Field[i].Items.IndexOf(token.Attributes["columnName"].Value);
                                    cBox_Operand[i].SelectedIndex = cBox_Operand[i].Items.IndexOf(token.Attributes["operand"].Value);
                                    txtBox_Value[i].Text = token.Attributes["value"].Value;

                                    tlp_ScriptBuilder.RowCount++;
                                    //int y = tlp_ScriptBuilder.RowCount;
                                    tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));

                                    tlp_ScriptBuilder.Controls.Add(rowLabel[i], 0, y - 1);
                                    rowLabel[i].Text = i.ToString();
                                    if (i > min_rowCount - 2 || rBtn_Delete.Checked)
                                        tlp_ScriptBuilder.Controls.Add(cBox_Logic[i], 1, y - 1);
                                    tlp_ScriptBuilder.Controls.Add(cBox_Field[i], 2, y - 1);
                                    tlp_ScriptBuilder.Controls.Add(cBox_Operand[i], 3, y - 1);

                                    tlp_ScriptBuilder.Controls.Add(txtBox_Value[i], 4, y - 1);
                                    TableLayoutPanelCellPosition pos = tlp_ScriptBuilder.GetCellPosition(txtBox_Value[i]);
                                    txtBox_Value[i].Width = tlp_ScriptBuilder.GetColumnWidths()[pos.Column];

                                    tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, y);
                                    ScriptContent();
                                    i++;
                                }
                                if (rBtn_Replace.Checked)
                                    btn_Generate.PerformClick();
                                //rTxtBox_Script.Text = rTxtBox_Script.Text.Replace(cur_environment, e.Node.Parent.Text);
                            }
                        }
                    }

                }
            }
            tlp_ScriptBuilder.ResumeLayout();
        }

        private void tViewScripts_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tViewScripts.SelectedNode = tViewScripts.GetNodeAt(e.X, e.Y);

                int x = tViewScripts.SelectedNode.Bounds.X + tViewScripts.SelectedNode.Bounds.Width + 600;
                int y = tViewScripts.SelectedNode.Bounds.Y + tViewScripts.SelectedNode.Bounds.Width + 30;
                Point nodePos = new Point(x, y);

                if (tViewScripts.SelectedNode != null)
                {
                    ContextMenu cm = new ContextMenu();

                    if (tViewScripts.SelectedNode.Parent == null)
                    {
                        AddMenuItem(cm, "New Table");
                    }
                    else if (tViewScripts.SelectedNode.Tag == "Table" || tViewScripts.SelectedNode.Parent.Text == "<Table></Table>")
                    {
                        if (cloneNode == null)
                        {
                            AddMenuItem(cm, "New Environment");
                            AddMenuItem(cm, "Edit Table");
                            AddMenuItem(cm, "Remove Table");
                        }
                        else
                        {
                            AddMenuItem(cm, "New Environment");
                            AddMenuItem(cm, "Paste Environment");
                            AddMenuItem(cm, "Edit Table");
                            AddMenuItem(cm, "Remove Table");                           
                        }
                    }
                    else if (tViewScripts.SelectedNode.Tag == "Environment")
                    {
                        if (cloneNode == null) 
                        {
                            AddMenuItem(cm, "New Query");
                            AddMenuItem(cm, "Edit Environment");
                            AddMenuItem(cm, "Copy Environment");
                            AddMenuItem(cm, "Remove Environment");
                        }
                        else
                        {
                            AddMenuItem(cm, "New Query");
                            AddMenuItem(cm, "Edit Environment");
                            AddMenuItem(cm, "Paste Query");
                            AddMenuItem(cm, "Remove Environment");                            
                        }
                    }
                    else if(tViewScripts.SelectedNode.Tag == "ReplaceToken")
                    {
                        AddMenuItem(cm, "Edit Query");
                        AddMenuItem(cm, "Copy Query");
                        AddMenuItem(cm, "Remove Query");
                       
                    }
                    else if (tViewScripts.SelectedNode.Parent.Text != "Table" && tViewScripts.SelectedNode.Tag != "Environment" && tViewScripts.SelectedNode.Parent != null)
                    {
                        AddMenuItem(cm, "New Environment");
                        AddMenuItem(cm, "Edit Table");
                        AddMenuItem(cm, "Remove Table");
                    }

                    cm.Show(this, nodePos);
                }
            }
        }

        private void tViewScripts_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            this.BeginInvoke(new Action(() => afterAfterEdit(e.Node)));
        }

        private void afterAfterEdit(TreeNode node)
        {
            if (tViewScripts.SelectedNode.Parent.Text == "Database")
            {
                //tBox_ServerName.Text = node.Text;
            }
            else if (tViewScripts.SelectedNode.Parent.Tag == "Table")
            {
               // tBox_Instance.Text = node.Text;
            }
            else
            {
                //tBox_Environment.Text = node.Text;
            }

            btn_Commit.Enabled = true;
        }

        private MenuItem AddMenuItem(ContextMenu cm, string text)
        {
            MenuItem item = new MenuItem(text, new System.EventHandler(this.cmItemClick));
            // item.Tag = context;
            cm.MenuItems.Add(item);
            return item;
        }

        private void cmItemClick(object sender, EventArgs e)
        {
            string actionText = "";
            MenuItem m = (MenuItem)sender;

            if (m.Text.ToLower() == "new environment")
            {
                //bServer = true;
                actionText = "New Environment";

                TreeNode newNode = new TreeNode("New Environment");
                newNode.Tag = "Environment";

                tViewScripts.SelectedNode.Nodes.Add(newNode);
                tViewScripts.SelectedNode = newNode;
                tViewScripts.LabelEdit = true;

                if (!newNode.IsEditing)
                {
                    newNode.BeginEdit();
                   // EnableServerDetails();
                }
            }
            else if (m.Text.ToLower() == "new query")
            {
                //bServer = true;
                actionText = "New Query";

                TreeNode newNode = new TreeNode("New Query");

                tViewScripts.SelectedNode.Nodes.Add(newNode);
                tViewScripts.SelectedNode = newNode;
                tViewScripts.LabelEdit = true;

                if (!newNode.IsEditing)
                {
                    newNode.BeginEdit();
                    // EnableServerDetails();
                }

            }
            else if (m.Text.ToLower() == "copy query")
            {
                TreeNode clickNode = tViewScripts.SelectedNode;
                cloneNode = (TreeNode)clickNode.Clone();
                
                //cloneNode = tViewScripts.SelectedNode;
            }
            else if (m.Text.ToLower() == "copy environment")
            {
                TreeNode clickedNode = tViewScripts.SelectedNode;
                cloneNode = (TreeNode)clickedNode.Clone();

                //cloneNode = tViewScripts.Nodes[tViewScripts;
                //cloneNode = tViewScripts.SelectedNode;
            }
            else if (m.Text.ToLower() == "paste environment")
            {
                var startindex = 0; // The position where selection starts.
                var endindex = 0;   // The lenght of selection.
                string bare_database = "";

                int pos = cur_database.IndexOf("-") + 1;
                string old_environment = cur_database.Substring(0, pos);
                //cur_database = cur_database.Replace(old_environment, sel_environment);
                bare_database = cur_database.Replace(old_environment, "");

                if (tViewScripts.SelectedNode.Tag == "Table")
                {
                    TreeNode newNode = new TreeNode("New Environment");
                    //XmlNode startPasteNode;
                    //startPasteNode = startNode.SelectSingleNode("/Tables/Table[@name='" + tViewScripts.SelectedNode + "']/Environments");
                    //AddNode(startPasteNode, cloneNode);
                    //startNode = doc.SelectSingleNode("/Databases/Database[@name='" + bare_database + "']/Tables/Table[@name='" + tViewScripts.SelectedNode + "']/Environments");
                    //AddNode(startNode, cloneNode);
                    //cloneNode.Text += "*";
                    
                    tViewScripts.SelectedNode.Nodes.Add(newNode);
                    newNode.Nodes.Add(cloneNode.LastNode);
                    tViewScripts.SelectedNode = newNode;
                }
                
            }
            else if (m.Text.ToLower() == "paste query")
            {                
                var startindex = 0; // The position where selection starts.
                var endindex = 0;   // The lenght of selection.
                string bare_database = "";

                int pos = cur_database.IndexOf("-") + 1;
                string old_environment = cur_database.Substring(0, pos);
                //cur_database = cur_database.Replace(old_environment, sel_environment);
                bare_database = cur_database.Replace(old_environment, "");

                if (tViewScripts.SelectedNode.Tag == "Environment")
                {
                    tViewScripts.SelectedNode.Nodes.Add(cloneNode);
                    //newNode.Nodes.Add(cloneNode.LastNode);
                    tViewScripts.SelectedNode = cloneNode;
                }
            }
        }

        private void RemoveItemFromXml(string nType, string itemToDelete)
        {
            XDocument xdoc = XDocument.Load(sXmlFile);

            var q = from node in xdoc.Descendants(nType)
                    let pAttr = node.Parent.Attribute("name")
                    let attr = node.Attribute("name")
                    let inst = node.Attribute("instance")
                    where (attr != null && attr.Value == itemToDelete) || (inst.Value == itemToDelete && pAttr.Value == tViewScripts.SelectedNode.Text)
                    select node;

            q.ToList().ForEach(x => x.Remove());
            xdoc.Save(sXmlFile);
            MessageBox.Show("Removed " + itemToDelete);
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
