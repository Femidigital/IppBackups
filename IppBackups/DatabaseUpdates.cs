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
using System.IO;
using System.Text.RegularExpressions;

namespace IppBackups
{
    public partial class DatabaseUpdates : Form
    {
        XmlDocument doc;
        string cur_database = "";
        string _cur_Db = "";
        string dbName = "";
        string tblName = "";
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
        Label lastRowMark = new Label() { Text = "" };

        string sXmlFile = "";
        bool afterWhile = false;
        bool scriptFromTreeView = false;
        XmlNode startNode;
        TreeNode cloneNode;
        XmlNode copyNode;
        XmlNode targetNode;
        Image delImage = Image.FromFile("..\\..\\Resources\\Images\\delete.png");
        Image acceptImage = Image.FromFile("..\\..\\Resources\\Images\\accept.png");
        Image addImage = Image.FromFile("..\\..\\Resources\\Images\\add2.png");
        PictureBox[] newRowBtn = new PictureBox[5];
        PictureBox[] delRowBtnPic = new PictureBox[5];
        PictureBox addRowBtnPic = new PictureBox();
        PictureBox acceptBtnPic = new PictureBox();
        Button[] delRowBtn = new Button[5];
        Button addRowBtn = new Button();
        Button acceptBtn = new Button();


        string[] NumericDataTypes = new string[] { "bigint", "bit", "decimal", "int", "money", "numberic", "smallint", "smallmoney", "tinyint", "float", "real" };
        string[] DateAndTimeDataTypes = new string[] { "date", "datetime2", "datetime", "datetimeoffset", "smalldatetime", "time" };
        string[] CharacterDataTypes = new string[] { "char", "text", "varchar", "nchar", "ntext", "nvarchar", "uniqueidentifier" };
        string[] BinaryStringsDataTypes = new string[] { "binary", "image", "varbinary" };
        string[] OtherDataTypes = new string[] { "cursor", "hierarchyid", "sql_variant", "table", "timestamp", "xml", "SpatialTypes" };

        public static string[] blackList = {"--",";--",";","/*","*/","@@","@",
                                           "char","nchar","varchar","nvarchar",
                                           "alter","begin","cast","create","cursor","declare","delete","drop","end","exec","execute",
                                           "fetch","insert","kill","open",
                                           "select", "sys","sysobjects","syscolumns",
                                           "table","update"};

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

            //tlp_ScriptBuilder.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            addRowBtnPic.Size = new Size(16, 16);
            addRowBtnPic.ImageLocation = "..\\..\\Resources\\Images\\add2.png";
            addRowBtnPic.Click += new EventHandler(addRowBtnPic_Click);
            addRowBtn.Anchor = AnchorStyles.Left;
            tlp_ScriptBuilder.Controls.Add(addRowBtnPic, 0, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Logical:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Feild: ", Anchor = AnchorStyles.None, AutoSize = true }, 2, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Operator: ", Anchor = AnchorStyles.None, AutoSize = true }, 3, 0);
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "Value: ", Anchor = AnchorStyles.None, AutoSize = true }, 4, 0);

            //lastRowMark.Size = new Size(20, 20);
            //lastRowMark.Image = acceptImage;
            //tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, tlp_ScriptBuilder.RowCount - 1);
            acceptBtnPic.Size = new Size(16, 16);
            acceptBtnPic.ImageLocation = "..\\..\\Resources\\Images\\accept.png";
            tlp_ScriptBuilder.Controls.Add(acceptBtnPic, 0, tlp_ScriptBuilder.RowCount - 1);
            acceptBtnPic.Click += new EventHandler(acceptBtnPic_Click);

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
            /*operand.Add(">");
            operand.Add("=>");
            operand.Add("<");
            operand.Add("<=");
            operand.Add("BETWEEN");
            operand.Add("ISNULL");
            operand.Add("IS NOT NULL");*/

            for (int i = 0; i < max_row; i++)
            {
                delRowBtnPic[i] = new PictureBox();
                delRowBtnPic[i].Size = new Size(16, 16);
                delRowBtnPic[i].Anchor = AnchorStyles.Left;
                delRowBtnPic[i].ImageLocation = "";


                //delRowBtn[i] = new Button();
                //delRowBtn[i].Click += new EventHandler(delRowBtn_Click);


                /*rowLabel[i] = new Label();
                rowLabel[i].AutoSize = true;
                rowLabel[i].Anchor = AnchorStyles.Left;
                rowLabel[i].Size = new Size(20, 20);
                rowLabel[i].Image = delImage;
                rowLabel[i].Text = "";
                //rowLabel[i].Text = i.ToString();*/

                cBox_Logic[i] = new ComboBox();
                cBox_Logic[i].Items.AddRange(logic.ToArray());

                cBox_Operand[i] = new ComboBox();
                cBox_Operand[i].Items.AddRange(operand.ToArray());

                cBox_Field[i] = new ComboBox();
                cBox_Field[i].Items.AddRange(field.ToArray());

                txtBox_Value[i] = new TextBox();
                txtBox_Value[i].Anchor = AnchorStyles.Left;
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

            foreach (Table tbl in db.Tables)
            {
                cBox_Tables.Items.Add(tbl);
            }

            // cBox_Tables.DataSource = db.Tables;


        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
            this.Close();
        }

        private void btn_Commit_Click(object sender, EventArgs e)
        {
            XmlNode root = doc.DocumentElement;
            bool whereClause = false;
            //string tblName = "";
            // this will create a .sql script file.
            if (scriptFromTreeView)
            {
                MessageBox.Show("Update selected node in TreeView");
                tblName = tViewScripts.SelectedNode.Parent.Parent.Text;

                XmlNode node = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens/ReplaceToken[@name='" + tViewScripts.SelectedNode.Text + "']");
            }
            else
            {
                //string dbName = cur_database;
                //dbName = dbName.Substring(dbName.IndexOf("-") + 1, dbName.Length - (dbName.IndexOf("-") + 1));
                //tblName = cBox_Tables.SelectedItem.ToString();
                //tblName = tblName.Substring(7, tblName.Length - 8);

                doc.Load(".\\Scripts\\DatabaseUpdateValues.xml");
                //doc.Load("..\\..\\Scripts\\DatabaseUpdateValues.xml");

                //XmlNode node = doc.SelectSingleNode("//Databases/Database[@name='" + cur_database +"']/Tables/Table[@name='" + cBox_Tables.SelectedItem + "']/Environments/Environment[@name='" + cur_environment +"']");
                XmlNode node = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']");

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
                else
                {
                    // Override existing ReplaceToken Node.
                    XmlNode rtNodeToReplace = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/ReplaceToken[@name='" + tblName + "']");
                    XmlNode ftNodeToReplace = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToken[@name='" + tblName + "']");

                    int scriptLine = tlp_ScriptBuilder.RowCount;
                    int rtNodeCount = rtNodeToReplace.ChildNodes.Count;
                    int ftNodeCount = ftNodeToReplace.ChildNodes.Count;
                    int i = 0;

                    foreach (XmlNode token in rtNodeToReplace.ChildNodes )
                    {
                        if (i > 0)
                        {
                            token.Attributes["set"].Value = cBox_Logic[i].SelectedItem.ToString();                            
                        }
                        else
                        {
                            token.Attributes["set"].Value = "";
                        }
                        token.Attributes["columnName"].Value = cBox_Field[i].SelectedItem.ToString();
                        token.Attributes["operand"].Value = cBox_Operand[i].SelectedItem.ToString();
                        token.Attributes["value"].Value = txtBox_Value[i].Text;
                        i++;
                    }

                    foreach (XmlNode token in ftNodeToReplace.ChildNodes)
                    {
                        token.Attributes["set"].Value = cBox_Logic[i].SelectedItem.ToString();
                        token.Attributes["columnName"].Value = cBox_Field[i].SelectedItem.ToString();
                        token.Attributes["operand"].Value = cBox_Operand[i].SelectedItem.ToString();
                        token.Attributes["value"].Value = txtBox_Value[i].Text;
                        i++;
                    }
                }

                doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                //doc.Save("..\\..\\Scripts\\DatabaseUpdateValues.xml");    
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
            for (int i = 0; i < (tlp_ScriptBuilder.RowCount - min_rowCount); i++)
            {
                if (cBox_Logic[i].SelectedItem == null)
                {
                    //script += "SET " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText("\nSET ", Color.Blue);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    if (NumericDataTypes.Contains(fieldDatatypes[i].ToString()))
                    {
                        rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                    }
                    else if (CharacterDataTypes.Contains(fieldDatatypes[i].ToString()))
                    {
                        rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                    }
                    else if (DateAndTimeDataTypes.Contains(fieldDatatypes[i].ToString()))
                    {
                        rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                    }

                }
                else if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    //script += " , " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                    rTxtBox_Script.AppendText(", ", Color.Green);
                    rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                    rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                    if (NumericDataTypes.Contains(fieldDatatypes[i].ToString()))
                    {
                        rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                    }
                    else if (CharacterDataTypes.Contains(fieldDatatypes[i].ToString()))
                    {
                        rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                    }
                    else if (DateAndTimeDataTypes.Contains(fieldDatatypes[i].ToString()))
                    {
                        rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                    }
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    if (CheckSQL_Syntax())
                    {
                        //script += "\nWHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                        rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + "", Color.Green);
                        if (NumericDataTypes.Contains(fieldDatatypes[i].ToString()))
                        {
                            rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                        }
                        else if (CharacterDataTypes.Contains(fieldDatatypes[i].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                        else if (DateAndTimeDataTypes.Contains(fieldDatatypes[i].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
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
                            ScriptContent(tlp_ScriptBuilder.RowCount);
                        }

                        int i = y - min_rowCount;

                        if (i < max_row)
                        {
                            tlp_ScriptBuilder.RowCount++;
                            tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));

                            delRowBtnPic[i] = new PictureBox();
                            delRowBtnPic[i].Size = new Size(16, 16);                            
                            delRowBtnPic[i].ImageLocation = "..\\..\\Resources\\Images\\delete.png";
                            delRowBtnPic[i].Click += new EventHandler(delRowBtn_Click);
                            delRowBtnPic[i].Anchor = AnchorStyles.Left;
                            tlp_ScriptBuilder.Controls.Add(delRowBtnPic[i], 0, y - 1);
                            TableLayoutPanelCellPosition pos2 = tlp_ScriptBuilder.GetCellPosition(delRowBtnPic[i]);
                            delRowBtnPic[i].Width = tlp_ScriptBuilder.GetColumnWidths()[pos2.Column] - 2;

                            /*rowLabel[i].Size = new Size(20,20);
                            rowLabel[i].Image = delImage;
                            rowLabel[i].Text = " ";
                            tlp_ScriptBuilder.Controls.Add(rowLabel[i], 0, y - 1);
                            //rowLabel[i].Text = "";
                            //rowLabel[i].Text = i.ToString();*/

                            if (y > min_rowCount || rBtn_Delete.Checked)
                                tlp_ScriptBuilder.Controls.Add(cBox_Logic[i], 1, y - 1);
                            tlp_ScriptBuilder.Controls.Add(cBox_Field[i], 2, y - 1);
                            tlp_ScriptBuilder.Controls.Add(cBox_Operand[i], 3, y - 1);

                            txtBox_Value[i].Anchor = AnchorStyles.Left;
                            tlp_ScriptBuilder.Controls.Add(txtBox_Value[i], 4, y - 1);
                            TableLayoutPanelCellPosition pos = tlp_ScriptBuilder.GetCellPosition(txtBox_Value[i]);
                            //txtBox_Value[i].Tag = i;
                            txtBox_Value[i].Width = tlp_ScriptBuilder.GetColumnWidths()[pos.Column] - 20;
                            //txtBox_Value[i].Validating += new System.ComponentModel.CancelEventHandler(this.txtBox_Value_Validating);
                            //txtBox_Value[i].Validated += new System.EventHandler(this.txtBox_Value_Validated);

                            //tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, y);
                            //acceptBtnPic.Click += new EventHandler(acceptBtnPic_Click);
                            acceptBtnPic.Size = new Size(16, 16);
                            tlp_ScriptBuilder.Controls.Add(acceptBtnPic, 0, y);

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

        private void delRowBtn_Click(object sender, EventArgs e)
        {

            int row_index_to_remove = int.Parse((sender as PictureBox).Tag.ToString()) + 1;

            if (row_index_to_remove >= tlp_ScriptBuilder.RowCount)
            {
                return;
            }

            // delete all controls of row that needs to be deleted.
            for (int i = 0; i < tlp_ScriptBuilder.ColumnCount; i++)
            {
                var control = tlp_ScriptBuilder.GetControlFromPosition(i, row_index_to_remove);
                tlp_ScriptBuilder.Controls.Remove(control);

                if(i == 0)
                {
                    delRowBtnPic[row_index_to_remove - 1].Dispose();
                }
                else if (i == 1)
                {
                    cBox_Logic[row_index_to_remove - 1].Dispose();
                }
                else if (i == 2)
                {
                    cBox_Field[row_index_to_remove - 1].Dispose();
                }
                else if (i == 3)
                {
                    cBox_Operand[row_index_to_remove - 1].Dispose();
                }
                else if (i == 4)
                {
                    txtBox_Value[row_index_to_remove - 1].Dispose();
                }
            }
            
            // move up row controls that comes after row that needs to be removed.
            for (int i = row_index_to_remove + 1; i < tlp_ScriptBuilder.RowCount; i++)
            {
                for (int j = 0; j < tlp_ScriptBuilder.ColumnCount; j++)
                {
                    var control = tlp_ScriptBuilder.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        tlp_ScriptBuilder.SetRow(control, i - 1);
                    }
                }
            }

            // remove the last row
            tlp_ScriptBuilder.RowStyles.RemoveAt(tlp_ScriptBuilder.RowCount - 1);
            tlp_ScriptBuilder.RowCount--;
            UpdateScriptWindow();

            // script DML statement

            var checkedButton = grpBox_DML.Controls.OfType<RadioButton>()
                        .FirstOrDefault(r => r.Checked);
            
            rTxtBox_Script.AppendText("\n" + checkedButton.Text + "", Color.Blue);
            rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Green);

            for (int i = 0; i < tlp_ScriptBuilder.RowCount - 1; i++)
            {
                int cur_rowToProcess = i + 3;
                if (i < tlp_ScriptBuilder.RowCount - 1)
                {
                    ScriptContent(cur_rowToProcess);
                }
            }


            // Remove empty elements from the control arrays.
            //cBox_Logic = cBox_Logic.Where(x => !cBox_Logic.IsNullOrEmpty(x)).ToArray();


            // Update the script file.
            SaveScriptFile();
        }

        private void newRowBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("New Row Button Clicked");
        }

        private void addRowBtnPic_Click(object sender, EventArgs e)
        {
            tlp_ScriptBuilder_MouseClick(addRowBtnPic, (MouseEventArgs)e);
        }

        private void acceptBtnPic_Click(object sender, EventArgs e)
        {
            for (int j = 0; j <= tlp_ScriptBuilder.RowCount - 3; j++)
            {
                if (cBox_Field[j].SelectedIndex != -1)
                {
                    errorProvider1.SetError(txtBox_Value[j], "");
                }
            }

            if ( validateInputValues() && CheckInputForInjection() )
                {
                    bool envFound = false;
                    bool tblFound = false;
                    bool queryFound = false;
                    bool whereClause = false;
                    XmlNode updateNode;
                    XmlNode tblXmlNode;
                    XmlNode envsXmlNode;
                    XmlNode envXmlNode;
                    XmlNode repNode;
                    XmlNode tokensNode;
                    string queryName = "";

                    string strMsg = String.Format("Are you sure you want to commit these pending changes?");

                    string selectedTbl = cBox_Tables.SelectedItem.ToString();
                    queryName = selectedTbl.Substring(7, selectedTbl.Length - 8);


                    if (tlp_ScriptBuilder.RowCount - min_rowCount > 1)
                    {
                        if (MessageBox.Show(strMsg, "Close Window", MessageBoxButtons.YesNo) != DialogResult.No)
                        {
                            if (tViewScripts.SelectedNode != null && tViewScripts.SelectedNode.Tag == "ReplaceToken")
                            {
                                queryFound = true;
                                string selectedNode = tViewScripts.SelectedNode.Text;
                                //MessageBox.Show("Updating " + selectedNode);
                            }
                            else
                            {
                                foreach (TreeNode tn in tViewScripts.Nodes)
                                {
                                    foreach (TreeNode cn in tn.Nodes)
                                    {
                                        //if (tViewScripts.Nodes.ContainsKey(cur_environment))
                                        if (cn.Tag == "Table" && cn.Text == queryName)
                                        {
                                            tblFound = true;
                                            //MessageBox.Show(cBox_Tables.SelectedText + " table already exists");
                                            foreach (TreeNode en in cn.Nodes)
                                            {
                                                if (en.Tag == "Environment" && en.Text == cur_environment)
                                                {
                                                    envFound = true;
                                                    //MessageBox.Show(cur_environment + "already exits");

                                                    foreach (TreeNode qn in en.Nodes)
                                                    {
                                                        if (qn.Tag == "ReplaceToken" && qn.Text == queryName)
                                                        {
                                                            queryFound = true;
                                                            //MessageBox.Show(queryName + " query node already exists, you want to overwrite it?");
                                                        }
                                                        else
                                                        {
                                                            queryFound = false;
                                                            //MessageBox.Show("Creating a new query node");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    envFound = false;
                                                    MessageBox.Show("Creating new environment node as " + cur_environment);

                                                    TreeNode qNode = new TreeNode(queryName);
                                                    qNode.Tag = "ReplaceToken";

                                                    TreeNode envNode = new TreeNode(cur_environment);
                                                    envNode.Tag = "Environment";

                                                    envNode.Nodes.Add(qNode);
                                                    //en.Nodes.Add(envNode);

                                                    TreeNode parentNode = tViewScripts.SelectedNode ?? tViewScripts.Nodes[0];
                                                    if (parentNode != null)
                                                    {
                                                        //parentNode.Nodes.Add(envNode);
                                                        cn.Nodes.Add(envNode);
                                                        //parentNode.SelectedNode = tblNode;
                                                        tViewScripts.SelectedNode = envNode;
                                                        envNode.ExpandAll();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            tblFound = false;
                                            //MessageBox.Show("Creating new table node as " + queryName);
                                            TreeNode tblNode = new TreeNode(queryName);
                                            tblNode.Tag = "Table";

                                            TreeNode qNode = new TreeNode(queryName);
                                            qNode.Tag = "ReplaceToken";

                                            TreeNode envNode = new TreeNode(cur_environment);
                                            envNode.Tag = "Environment";

                                            envNode.Nodes.Add(qNode);
                                            tblNode.Nodes.Add(envNode);

                                            TreeNode parentNode = tViewScripts.SelectedNode ?? tViewScripts.Nodes[0];
                                            if (parentNode != null)
                                            {
                                                parentNode.Nodes.Add(tblNode);
                                                //parentNode.SelectedNode = tblNode;
                                                tViewScripts.SelectedNode = tblNode;
                                                tblNode.ExpandAll();
                                            }
                                        }
                                    }
                                }
                            }
                            // Update the xml file with newly created node.                    
                            updateNode = doc.SelectSingleNode("Databases/Database[@name='" + cur_database + "']");
                            if (!queryFound)
                            {
                                int NoToken = tlp_ScriptBuilder.RowCount - 3;
                                XmlNode[] token = new XmlNode[NoToken];

                                //Create a new Tokens Node
                                XmlNode tokens = doc.CreateNode(XmlNodeType.Element, "Tokens", null);

                                //MessageBox.Show("Creating new query XML node");
                                repNode = doc.CreateNode(XmlNodeType.Element, "ReplaceToken", null);
                                XmlAttribute repName = doc.CreateAttribute("name");
                                repName.Value = tblName;
                                XmlAttribute repType = doc.CreateAttribute("type");
                                repType.Value = "";
                                XmlAttribute repDML = doc.CreateAttribute("dml");

                                var checkedButton = grpBox_DML.Controls.OfType<RadioButton>()
                                    .FirstOrDefault(r => r.Checked);
                                repDML.Value = checkedButton.Text;

                                repNode.Attributes.Append(repName);
                                repNode.Attributes.Append(repType);
                                repNode.Attributes.Append(repDML);

                                /* add tokens */
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

                                    if (!whereClause)
                                    {
                                        repNode.AppendChild(token[i]);
                                    }
                                    else
                                    {
                                        filterToken.AppendChild(token[i]);
                                    }

                                    //}
                                }
                                /* end add tokens */


                                if (!envFound)
                                {
                                    envsXmlNode = doc.CreateNode(XmlNodeType.Element, "Environments", null);
                                    envXmlNode = doc.CreateNode(XmlNodeType.Element, "Environment", null);
                                    XmlAttribute envNameAtt = doc.CreateAttribute("name");
                                    envNameAtt.Value = cur_environment;
                                    envXmlNode.Attributes.Append(envNameAtt);

                                    tokensNode = doc.CreateNode(XmlNodeType.Element, "Tokens", null);

                                    tokensNode.AppendChild(repNode);
                                    tokensNode.AppendChild(filterToken);
                                    envXmlNode.AppendChild(tokensNode);


                                    if (!tblFound)
                                    {
                                        tblXmlNode = doc.CreateNode(XmlNodeType.Element, "Table", null);
                                        XmlAttribute tblNameAtt = doc.CreateAttribute("name");
                                        tblNameAtt.Value = queryName;
                                        tblXmlNode.Attributes.Append(tblNameAtt);

                                        envsXmlNode.AppendChild(envXmlNode);
                                        tblXmlNode.AppendChild(envsXmlNode);

                                        doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables").InsertAfter(tblXmlNode, doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables").LastChild);


                                    }
                                    else
                                    {
                                        MessageBox.Show("Updating existing table XML node");
                                        updateNode = doc.SelectSingleNode("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + queryName + "']/Environments/Environment");
                                        doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments").InsertAfter(envXmlNode, doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments").LastChild);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Updating existing environment XML node");
                                    doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + cur_environment + "']/Tokens").InsertAfter(repNode, doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + cur_environment + "']/Tokens").LastChild);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Updating existing query XML node");
                                scriptFromTreeView = false;
                                btn_Commit.PerformClick();

                            }

                            doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");

                            // Update script file
                            SaveScriptFile();
                        }
                        //else
                        //{
                        //    Close();
                        //}
                    }
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
                    if (i == cur_Row - 1 && count > 1)
                    {
                        goodToGo = false;
                    }
                }
            }
            return goodToGo;
        }

        private bool validateInputValues()
        {
            int errCount = 0;
            bool goodToGo = false;
            string resultString;
            var args = new System.ComponentModel.CancelEventArgs();
            //errorProvider1.Clear();

            if (tlp_ScriptBuilder.RowCount > min_rowCount)
            {
                //for(int i = min_rowCount + 1; i <tlp_ScriptBuilder.RowCount - 1; i++)
                for (int i = 0; i < tlp_ScriptBuilder.RowCount - min_rowCount; i++)
                {
                    if (cBox_Field[i].SelectedIndex != -1)
                    {
                        string sqlDataType = fieldDatatypes[cBox_Field[i].SelectedIndex];
                        string chkValue = txtBox_Value[i].Text;
                        // validate value in tBox_Value[i] against cBox_Field[i] datatype.
                        /*if (txtBox_Value[i].Text = Convert.ChangeType(txtBox_Value[i].Text,fieldDatatypes[i]))
                        {

                        }*/
                        if (NumericDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]))
                        {
                            //"bigint", "bit", "decimal", "int", "money", "numberic", "smallint", "smallmoney", "tinyint", "float", "real"
                            //MessageBox.Show("Validating numberic datatype for " + fieldDatatypes[cBox_Field[i].SelectedIndex]);

                            switch (sqlDataType)
                            {
                                case "bigint":
                                    long longText;
                                    if (Int64.TryParse(chkValue, out longText) == true)
                                    {
                                        goodToGo = true;
                                        txtBox_Value[i].Tag = "";
                                        errorProvider1.SetError(txtBox_Value[i], "");
                                    }
                                    else
                                    {
                                        //MessageBox.Show(chkValue + " is not a valid " + sqlDataType + " value");
                                        goodToGo = false;
                                        errCount++;
                                        txtBox_Value[i].Tag = i;
                                        txtBox_Value_Validating(txtBox_Value[i], args);
                                        //txtBox_Value_Validating(txtBox_Value[i]);
                                    }
                                    break;
                                case "bit":
                                    bool boolText;
                                    //var isBool = Boolean.TryParse(chkValue, out boolText);
                                    //if (Boolean.TryParse(chkValue, out boolText) == true)
                                    var isBool = int.Parse(chkValue) == 1;
                                    if (isBool)
                                    {
                                        goodToGo = true;
                                        txtBox_Value[i].Tag = "";
                                        errorProvider1.SetError(txtBox_Value[i], "");
                                    }
                                    else
                                    {
                                        goodToGo = false;
                                        errCount++;
                                        txtBox_Value[i].Tag = i;
                                        txtBox_Value_Validating(txtBox_Value[i], args);
                                    }
                                    break;
                                case "decimal":
                                case "int":
                                    int intText;
                                    if (Int32.TryParse(chkValue, out intText) == true)
                                    {
                                        goodToGo = true;
                                        txtBox_Value[i].Tag = "";
                                        errorProvider1.SetError(txtBox_Value[i], "");
                                    }
                                    else
                                    {
                                        goodToGo = false;
                                        errCount++;
                                        txtBox_Value[i].Tag = i;
                                        txtBox_Value_Validating(txtBox_Value[i], args);
                                    }
                                    break;                       
                                case "smallint":
                                    Int16 smallText;
                                    if (Int16.TryParse(chkValue, out smallText) == true)
                                    {
                                        goodToGo = true;
                                        txtBox_Value[i].Tag = "";
                                        errorProvider1.SetError(txtBox_Value[i], "");
                                    }
                                    else
                                    {
                                        goodToGo = false;
                                        errCount++;
                                        txtBox_Value[i].Tag = i;
                                        //txtBox_Value_Validating(txtBox_Value[i], (CancelEventArgs) e);
                                        txtBox_Value_Validating(txtBox_Value[i], args);
                                    }
                                    break;
                                case "numberic":
                                case "money":
                                case "smallmoney":
                                    Decimal decimalText;
                                    if (Decimal.TryParse(chkValue, out decimalText) == true)
                                    {
                                        goodToGo = true;
                                        txtBox_Value[i].Tag = "";
                                        errorProvider1.SetError(txtBox_Value[i], "");
                                    }
                                    else
                                    {
                                        goodToGo = false;
                                        errCount++;
                                        txtBox_Value[i].Tag = i;
                                        txtBox_Value_Validating(txtBox_Value[i], args);
                                    }
                                    break;
                                case "tinyint":
                                case "float":
                                case "real":
                                    MessageBox.Show("Validating Decimal datatype for " + fieldDatatypes[cBox_Field[i].SelectedIndex]);
                                    break;
                                MessageBox.Show(txtBox_Value[i].Text + "is not a valid " + sqlDataType);
                                goodToGo = false;
                                errCount++;
                                txtBox_Value_Validating(txtBox_Value[i], args);
                                break;
                            }
                        }
                        else if (DateAndTimeDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]))
                        {
                            //MessageBox.Show("Validating datetime datatype for " + fieldDatatypes[cBox_Field[i].SelectedIndex]);
                            DateTime testDate = DateTime.MinValue;
                            System.Data.SqlTypes.SqlDateTime sdt;

                            if (DateTime.TryParse(chkValue, out testDate))
                            {
                                try
                                {
                                    //take advantage of the native conversion
                                    sdt = new System.Data.SqlTypes.SqlDateTime(testDate);
                                    goodToGo = true;
                                    txtBox_Value[i].Tag = "";
                                    errorProvider1.SetError(txtBox_Value[i], "");
                                }
                                catch(System.Data.SqlTypes.SqlTypeException ex)
                                {
                                    goodToGo = false;
                                }

                            }
                            else
                            {
                                goodToGo = false;
                                txtBox_Value[i].Tag = i;
                                errCount++;
                                txtBox_Value_Validating(txtBox_Value[i], args);
                            }
                        }
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]))
                        {
                            Guid newGuid;

                            if (Guid.TryParse(txtBox_Value[i].Text, out newGuid) == true)
                            {
                                goodToGo = true;
                                txtBox_Value[i].Tag = "";
                                errorProvider1.SetError(txtBox_Value[i], "");
                            }
                            else
                            {
                                goodToGo = false;
                                errCount++;
                                txtBox_Value[i].Tag = i;
                                txtBox_Value_Validating(txtBox_Value[i], args);
                            }
                        }
                        else if (CharacterDataTypes.Contains( fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("("))))
                        {
                            string maxLength = sqlDataType.Substring(sqlDataType.IndexOf("(") + 1, sqlDataType.Length - (sqlDataType.IndexOf("(") + 2));

                            if (chkValue.Length < Int32.Parse(maxLength))
                            {
                                goodToGo = true;
                                txtBox_Value[i].Tag = "";
                                errorProvider1.SetError(txtBox_Value[i], "");
                            }
                            else
                            {
                                goodToGo = false;
                                errCount++;
                                txtBox_Value[i].Tag = i;
                                txtBox_Value_Validating(txtBox_Value[i], args);
                            }
                        }
                        else if (BinaryStringsDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]))
                        {
                            MessageBox.Show("Validating binary datatype for " + fieldDatatypes[cBox_Field[i].SelectedIndex]);
                        }
                        else if (OtherDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]))
                        {
                            MessageBox.Show("Validating other datatype for " + fieldDatatypes[cBox_Field[i].SelectedIndex]);
                        }
                    }
                }
            }

            return (errCount < 1);
        }

        private bool CheckInputForInjection()
        {
            int sqlInjection = 0;
            for (int i = 0; i < tlp_ScriptBuilder.RowCount - min_rowCount; i++ )
            {
                if (cBox_Field[i].SelectedIndex != null)
                {
                    string curString = txtBox_Value[i].Text;
                    foreach (string s in txtBox_Value[i].Text.Split(' '))
                    {
                        for(int j =0; j < blackList.Length; j++)
                        {
                            if ((s.IndexOf(blackList[j], StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                // Handle the discovery of suspicious SQL characters here.
                                errorProvider1.SetError(txtBox_Value[i], "Contains black listed phrase");
                                sqlInjection++;
                            }
                        }
                    }
                }
            }
                return (sqlInjection < 1);
        }

        private void ScriptContent(int lastRowMarker)
        {
            //int i = tlp_ScriptBuilder.RowCount - (min_rowCount + 1);
            int i = lastRowMarker - (min_rowCount + 1);

            if (rBtn_Update.Checked)
            {
                if (cBox_Logic[i].SelectedItem == null)
                {
                    // Check if cBox_Field[i] has as selected value
                    if (cBox_Field[i].SelectedIndex > -1)
                    {
                        // script += "SET " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                        rTxtBox_Script.AppendText("SET ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                    
                        // Check Field datatype, and determine value assignment type.
                        if (NumericDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                        }
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                        else if(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Contains("("))
                        {
                            int posFound =   fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("(");
                            string strippedBackect = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, posFound);

                            if (CharacterDataTypes.Contains(strippedBackect))
                            {
                                rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                            }
                        }
                        else if (DateAndTimeDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                    }
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

                    // Check if cBox_Field[i] has as selected value
                    if (cBox_Field[i].SelectedIndex > -1)
                    {                      
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                        // Check Field datatype, and determine value assignment type.
                        if (NumericDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                        }
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                        else if (fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Contains("("))
                        {
                            int posFound = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("(");
                            string strippedBackect = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, posFound);

                            if (CharacterDataTypes.Contains(strippedBackect))
                            {
                                rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                            }
                        }
                        else if (DateAndTimeDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                    }
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    if (CheckSQL_Syntax())
                    {
                        //script += "\nWHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;
                        rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);

                        // Check if cBox_Field[i] has as selected value
                        if (cBox_Field[i].SelectedIndex > -1)
                        {
                            rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                            rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                            // Check Field datatype, and determine value assignment type.
                            if (NumericDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                            {
                                rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                            }
                            else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                            {
                                rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                            }
                            else if (fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Contains("("))
                            {
                                int posFound = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("(");
                                string strippedBackect = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, posFound);

                                if (CharacterDataTypes.Contains(strippedBackect))
                                {
                                    rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                                }
                            }
                            else if (DateAndTimeDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                            {
                                rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                            }
                        }
                    }
                }
            }
            else if (rBtn_Delete.Checked)
            {
                if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE")
                {
                    //script += cBox_Logic[i].SelectedItem + " " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;

                    // Check if cBox_Field[i] has as selected value
                    if (cBox_Field[i].SelectedIndex > -1)
                    {
                        rTxtBox_Script.AppendText("" + cBox_Logic[i].SelectedItem + " ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText("" + cBox_Operand[i].SelectedItem + " ", Color.Green);
                        if (NumericDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                        }
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                        else if (fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Contains("("))
                        {
                            int posFound = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("(");
                            string strippedBackect = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, posFound);

                            if (CharacterDataTypes.Contains(strippedBackect))
                            {
                                rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                            }
                        }
                        else if (DateAndTimeDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                    }
                }
                else if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    //script += "WHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;

                    // Check if cBox_Field[i] has as selected value
                    if (cBox_Field[i].SelectedIndex > -1)
                    {
                        rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Field[i].SelectedItem + " ", Color.Green);
                        rTxtBox_Script.AppendText(" " + cBox_Operand[i].SelectedItem + " ", Color.Green);
                        if (NumericDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("" + txtBox_Value[i].Text + " ", Color.Black);
                        }
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                        else if (fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Contains("("))
                        {
                            int posFound = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("(");
                            string strippedBackect = fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, posFound);

                            if (CharacterDataTypes.Contains(strippedBackect))
                            {
                                rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                            }
                        }
                        else if (DateAndTimeDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString()))
                        {
                            rTxtBox_Script.AppendText("'" + txtBox_Value[i].Text + "'", Color.Black);
                        }
                    }
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
            field.Clear();
            fieldDatatypes.Clear();
            for (int i = 0; i < max_row; i++)
            {
                cBox_Field[i].Items.Clear();
            }

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
                        if (column.DataType.ToString().ToUpper() == "NVARCHAR")
                        {
                            if (column.DataType.MaximumLength == -1)
                            {
                                fieldDatatypes.Add(column.DataType.ToString() + "(max)");
                            }
                            else
                            {
                                fieldDatatypes.Add(column.DataType.ToString() + "(" + column.DataType.MaximumLength + ")");
                            }
                        }
                        else
                        {
                            fieldDatatypes.Add(column.DataType.ToString());
                        }
                        //field.Add(column.Name, column.DataType.ToString());
                    }
                }
            }

            for (int i = 0; i < max_row; i++)
            {
                cBox_Field[i].Items.AddRange(field.ToArray());
            }

            // Strip dbo square brackets from table name.
            dbName = cur_database;
            dbName = dbName.Substring(dbName.IndexOf("-") + 1, dbName.Length - (dbName.IndexOf("-") + 1));
            tblName = cBox_Tables.SelectedItem.ToString();
            tblName = tblName.Substring(7, tblName.Length - 8);

        }

        private void ClearScriptBuilder()
        {
            int j;
            for (int i = tlp_ScriptBuilder.RowCount; i > 2; i--)
            {
                j = i - min_rowCount;
                if (j >= 1)
                {
                    tlp_ScriptBuilder.Controls.Remove(delRowBtnPic[j - 1]);
                    delRowBtnPic[j - 1].Text = "";
                    tlp_ScriptBuilder.Controls.Remove(cBox_Logic[j - 1]);
                    cBox_Logic[j - 1].SelectedIndex = -1;
                    tlp_ScriptBuilder.Controls.Remove(cBox_Field[j - 1]);
                    cBox_Field[j - 1].SelectedIndex = -1;
                    tlp_ScriptBuilder.Controls.Remove(cBox_Operand[j - 1]);
                    cBox_Operand[j - 1].SelectedIndex = -1;
                    tlp_ScriptBuilder.Controls.Remove(txtBox_Value[j - 1]);
                    txtBox_Value[j - 1].Text = "";
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

            for (int i = 0; i < currentlyUsedRow - 2; i++)
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
            _cur_Db = cur_Database.Substring(dashPos, cur_Database.Length - dashPos);

            //label1.Text = _cur_Db + "\n";
            TreeNode childNode;

            sXmlFile = ".\\Scripts\\DatabaseUpdateValues.xml";

            //XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);

            //XmlNodeList curDatabase = doc.SelectNodes("Databases/Database[@name='" + _cur_Db + "']/Tables/Table");
            XmlNodeList curDatabase = doc.SelectNodes("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table");

            //startNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']");
            startNode = doc.SelectSingleNode("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')" + "='" + _cur_Db.ToLower() + "']");

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


                    foreach (XmlNode tab in xNode.ChildNodes)
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

            if (rBtn_Replace.Checked)
            {
                rTxtBox_Script.AppendText("\nDECLARE ", Color.Blue);
                rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem, Color.Black);
                rTxtBox_Script.AppendText(" " + fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() + "", Color.Blue);
                /*if (fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "NVARCHAR")
                {
                    rTxtBox_Script.AppendText("(" + fieldDatatypes[cBox_Field[0].SelectedIndex].Length + ")");
                }*/
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
                    sel_environment = e.Node.Parent.Text;

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

                                    /*rowLabel[i].Size = new Size(20, 20);
                                    rowLabel[i].Image = delImage;
                                    tlp_ScriptBuilder.Controls.Add(rowLabel[i], 0, y - 1);*/
                                    //rowLabel[i].Text = i.ToString();
                                    delRowBtnPic[i].Size = new Size(19, 19);
                                    delRowBtnPic[i].Tag = i;
                                    delRowBtnPic[i].ImageLocation = "..\\..\\Resources\\Images\\delete.png";
                                    delRowBtnPic[i].Click += new EventHandler(delRowBtn_Click);
                                    tlp_ScriptBuilder.Controls.Add(delRowBtnPic[i], 0, y - 1);
                                    if (i > min_rowCount - 2 || rBtn_Delete.Checked)
                                        tlp_ScriptBuilder.Controls.Add(cBox_Logic[i], 1, y - 1);
                                    tlp_ScriptBuilder.Controls.Add(cBox_Field[i], 2, y - 1);
                                    tlp_ScriptBuilder.Controls.Add(cBox_Operand[i], 3, y - 1);
                                    //txtBox_Value[i].Text = fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper();
                                    tlp_ScriptBuilder.Controls.Add(txtBox_Value[i], 4, y - 1);
                                    TableLayoutPanelCellPosition pos = tlp_ScriptBuilder.GetCellPosition(txtBox_Value[i]);
                                    txtBox_Value[i].Width = tlp_ScriptBuilder.GetColumnWidths()[pos.Column];


                                    //tlp_ScriptBuilder.Controls.Add(lastRowMark, 0, y);
                                    tlp_ScriptBuilder.Controls.Add(acceptBtnPic, 0, y);
                                    ScriptContent(tlp_ScriptBuilder.RowCount);
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
                        if (cloneNode == null || cloneNode.Tag != "Environment")
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
                    else if (tViewScripts.SelectedNode.Tag == "ReplaceToken")
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

            else if (tViewScripts.SelectedNode.Tag == "Environment")
            {
                bool exists = false;
                XmlNode envNode = doc.CreateNode(XmlNodeType.Element, "Environment", null);

                XmlAttribute nameAttri = doc.CreateAttribute("name");
                nameAttri.Value = tViewScripts.SelectedNode.Text;

                envNode.Attributes.Append(nameAttri);

                //Create a new Tokens Node
                XmlNode tokens = doc.CreateNode(XmlNodeType.Element, "Tokens", null);
                //envNode.AppendChild(tokens);

                foreach (XmlNode dbNode in doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments"))
                {
                    if (dbNode.Attributes["name"].Value == tViewScripts.SelectedNode.Text)
                    {
                        exists = true;
                    }
                }

                if (!exists)
                {
                    if (copyNode != null)
                    {
                        envNode.AppendChild(copyNode);
                    }
                    doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments").InsertAfter(envNode, doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments").LastChild);
                }

                doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                startNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']");
            }
            else if (tViewScripts.SelectedNode.Tag == "ReplaceToken")
            {
                bool exists = false;
                XmlNode repToken = doc.CreateNode(XmlNodeType.Element, "ReplaceToken", null);

                XmlAttribute nameAttri = doc.CreateAttribute("name");
                nameAttri.Value = tViewScripts.SelectedNode.Text;

                XmlAttribute typeAttri = doc.CreateAttribute("type");
                typeAttri.Value = "";

                XmlAttribute dmlAttri = doc.CreateAttribute("dml");
                dmlAttri.Value = "";

                repToken.Attributes.Append(nameAttri);
                repToken.Attributes.Append(typeAttri);
                repToken.Attributes.Append(dmlAttri);

                foreach (XmlNode tblNode in doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']"))
                {
                    if (tblNode.Name == "Tokens")
                    {
                        exists = true;
                    }
                }

                if (!exists)
                {
                    XmlNode tokenNode = doc.CreateNode(XmlNodeType.Element, "Tokens", null);
                    tokenNode.AppendChild(repToken);
                    doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']").InsertAfter(tokenNode, doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']").LastChild);

                }
                else
                {
                    doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens").InsertAfter(repToken, doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens").LastChild);
                }
                doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                startNode = doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']");
            }
            else if (tViewScripts.SelectedNode.Tag == "Table")
            {
                XmlNode updateNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']");
                targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']/");
                targetNode.AppendChild(copyNode);

                updateNode = targetNode;
                doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
            }
            else
            {
                //tBox_Environment.Text = node.Text;
            }

            btn_Commit.Enabled = true;
            copyNode = null;
            cloneNode = null;
            targetNode = null;
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
                string queryName = "";

                if (cBox_Tables.SelectedItem == null)
                {
                    queryName = "New Query";
                }
                else
                {
                    string selectedTbl = cBox_Tables.SelectedItem.ToString();
                    queryName = selectedTbl.Substring(7, selectedTbl.Length - 8);
                }
                //TreeNode newNode = new TreeNode("New Query");
                TreeNode newNode = new TreeNode(queryName);
                newNode.Tag = "ReplaceToken";

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
                XmlNode clickedNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens");

                copyNode = clickedNode.CloneNode(true);

                TreeNode clickNode = tViewScripts.SelectedNode;
                cloneNode = (TreeNode)clickNode.Clone();
            }
            else if (m.Text.ToLower() == "copy environment")
            {
                XmlNode clickedXmlNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']/Tokens");
                copyNode = clickedXmlNode.CloneNode(true);

                TreeNode clickedNode = tViewScripts.SelectedNode;
                cloneNode = (TreeNode)clickedNode.Clone();
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
                    newNode.Tag = "Environment";

                    tViewScripts.SelectedNode.Nodes.Add(newNode);
                    newNode.Nodes.Add(cloneNode.LastNode);
                    tViewScripts.SelectedNode = newNode;

                    tViewScripts.LabelEdit = true;

                    if (!newNode.IsEditing)
                    {
                        newNode.BeginEdit();
                    }
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
                    XmlNode updateNode = doc.SelectSingleNode("Databases/Database[@name='" + bare_database + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']");
                    //targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']/Tokens");
                    targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']");
                    targetNode.AppendChild(copyNode);

                    updateNode = targetNode;

                    tViewScripts.SelectedNode.Nodes.Add(cloneNode);
                    //newNode.Nodes.Add(cloneNode.LastNode);
                    tViewScripts.SelectedNode = cloneNode;
                }
                doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
            }
            else if (m.Text.ToLower() == "remove environment")
            {
                string strMsg = String.Format("Are you sure you want to remove the Environment : {0} and all its queries?", tViewScripts.SelectedNode.Text);
                actionText = tViewScripts.SelectedNode.Text;
                if (MessageBox.Show(strMsg, "Close Application", MessageBoxButtons.YesNo) != DialogResult.No)
                {
                    RemoveItemFromXml("Environment", actionText);
                    tViewScripts.Nodes.Remove(tViewScripts.SelectedNode);
                }
            }
            else if (m.Text.ToLower() == "remove query")
            {
                string strMsg = String.Format("This will remove {0} query from your configuration. Confirm?", tViewScripts.SelectedNode.Text);
                actionText = "Remove Query clicked";
                if (MessageBox.Show(strMsg, "Close Application", MessageBoxButtons.YesNo) != DialogResult.No)
                {
                    string toDelete = tViewScripts.SelectedNode.Text;
                    //e.Cancel = true;

                    //XDocument xdoc = XDocument.Load(sXmlFile);
                    XDocument xdoc = XDocument.Load(".\\Scripts\\DatabaseUpdateValues.xml");

                    IEnumerable<XElement> queryTokens =
                        from node in xdoc.Descendants("ReplaceToken")
                        .Union(xdoc.Descendants("FilterToken"))
                        let queries = node
                        where queries != null && queries.Attribute("name").Value == tViewScripts.SelectedNode.Text && queries.Parent.Parent.Attribute("name").Value == tViewScripts.SelectedNode.Parent.Text
                        select queries;

                    queryTokens.ToList().ForEach(x => x.Remove());

                    xdoc.Save(".\\Scripts\\DatabaseUpdateValues.xml");

                    tViewScripts.Nodes.Remove(tViewScripts.SelectedNode);
                    MessageBox.Show("Removed " + toDelete);
                }
            }
        }

        private void RemoveItemFromXml(string nType, string itemToDelete)
        {
            //XDocument xdoc = XDocument.Load(sXmlFile);
            XDocument xdoc = XDocument.Load(".\\Scripts\\DatabaseUpdateValues.xml");

            var q = from node in xdoc.Descendants(nType)
                    let pAttr = node.Parent.Attribute("name")
                    let attr = node.Attribute("name")
                    where (attr != null && attr.Value == itemToDelete) || (attr.Value == itemToDelete && pAttr.Value == tViewScripts.SelectedNode.Parent.Text)
                    select node;

            q.ToList().ForEach(x => x.Remove());
            // xdoc.Save(sXmlFile);
            xdoc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
            MessageBox.Show("Removed " + itemToDelete);
        }

        private void SaveScriptFile()
        {
            string scriptDirectory = "\\Script\\";
            string scriptFile = _cur_Db + "_" + tblName + "_" + cur_environment + ".sql";
            string scriptFileLocation = scriptDirectory + scriptFile;

            if (File.Exists(scriptFile))
            {
                MessageBox.Show("Deleting existing file and re-creating " + scriptFile);
                File.Delete(scriptFile);
            }
            else
            {
                MessageBox.Show("Creating new script file: " + scriptFile);
            }

            System.IO.StreamWriter sqlFile = new StreamWriter(scriptFile);
            sqlFile.WriteLine(rTxtBox_Script.Text);
            sqlFile.Flush();
            sqlFile.Close();
        }

        //private void txtBox_Value_Validating(object sender, CancelEventArgs e)
        private void txtBox_Value_Validating(object sender, CancelEventArgs e)
        {
            //int row_index_to_validate = int.Parse((sender as TextBox).Tag.ToString()) + 1;

            for (int i = 0; i < tlp_ScriptBuilder.RowCount - 2; i++)
            {
                if (txtBox_Value[i].Tag != null)
                {
                    //if (sender.ToString() == txtBox_Value[i].Text.ToString())
                    if (txtBox_Value[i].Tag.ToString() == i.ToString())
                    {
                        //errorProvider1.SetError(txtBox_Value[i], "Valid Datatype is required");
                        errorProvider1.SetError(txtBox_Value[i], txtBox_Value[i].Text + "is not a valid " + fieldDatatypes[cBox_Field[i].SelectedIndex]);
                        //e.Cancel = true;
                        //return;
                    }
                }
            }
        }

        private void txtBox_Value_Validated(object sender, EventArgs e)
        {

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
