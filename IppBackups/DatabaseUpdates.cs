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
using System.IO;
using System.Text.RegularExpressions;
using MsgBox;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace IppBackups
{
    public partial class DatabaseUpdates : Form
    {
        XmlDocument doc;
        string basePath = Application.StartupPath;
        string cur_database = "";
        string _cur_Db = "";
        string dbName = "";
        string tblName = "";
        string cur_environment = "";
        string sel_environment = "";
        string sel_table = "";
        string sel_query = "";
        string sel_tag = "";
        string script = "";
        string useStmt = "USE ";
        string tbl = "";
        int max_row = 5;
        int min_rowCount = 2;
        string replaceOption = "";
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
        //string resourcesPath = "";
        //string scriptLocation = "..\\..\\SQL_Scripts\\";
        string resourcesPath = Directory.Exists(Application.StartupPath + "..\\bin") ? "..\\..\\Resources\\" : "..\\Resources\\";
        // scriptLocation = Directory.Exists(basePath + "..\\bin") ? basePath + "..\\..\\SQL_Scripts\\" : basePath + "..\\Scripts";
        string scriptLocation = Directory.Exists(Application.StartupPath + "..\\bin") ? Application.StartupPath + "..\\..\\SQL_Scripts\\" : Application.StartupPath + "..\\Scripts\\";
        //string scriptLocation = "..\\..\\Scripts\\";
        bool afterWhile = false;
        bool scriptFromTreeView = false;
        XmlNode startNode;
        string source_table = "";
        TreeNode cloneNode;
        XmlNode copyNode;
        XmlNode copyFilterNode;
        XmlNode targetNode;
        //Image delImage = Image.FromFile("..\\..\\Resources\\Images\\delete.png");
        //Image acceptImage = Image.FromFile("..\\..\\Resources\\Images\\accept.png");
        //Image addImage = Image.FromFile("..\\..\\Resources\\Images\\add2.png");

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

        int lastRowScripted;

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

        private string GetSettingsPath()
        {
            string basePath = Application.StartupPath;
            //MessageBox.Show(basePath );
            //string path = Directory.Exists(basePath + "..\\..\\bin" ) ? basePath + "..\\..\\Rssources"  : "..\\Resources" ;
            string path = basePath;
            //MessageBox.Show(path);
            return path;

        }


        public DatabaseUpdates(string curInstance, string database, string env)
        {
            InitializeComponent();
            //this.DoubleBuffered = true;
            doc = new XmlDocument();
            scriptLocation = GetSettingsPath();
            scriptLocation += "\\Scripts\\";

            LoadValuesFromSettings(database);

            getTables(curInstance, database);

            lbl_DatabaseName.Text = database;
            cur_environment = env;

            resourcesPath = GetSettingsPath();
            resourcesPath += "\\Resources\\Images\\";

            //Image delImage = Image.FromFile(Path.Combine(Application.StartupPath, resourcesPath) + "delete.png");
            Image delImage = Image.FromFile(Path.Combine(resourcesPath, "delete.png"));
            Image acceptImage = Image.FromFile(Path.Combine(resourcesPath, "accept.png"));
            Image addImage = Image.FromFile(Path.Combine(resourcesPath, "add2.png"));

            tViewScripts.NodeMouseClick +=
                new TreeNodeMouseClickEventHandler(tViewScripts_NodeMouseClick);

            tlp_ScriptBuilder.AutoSize = true;
            tlp_ScriptBuilder.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tlp_ScriptBuilder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tlp_ScriptBuilder.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddRows;

            //tlp_ScriptBuilder.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            addRowBtnPic.Size = new Size(16, 16);
            //addRowBtnPic.ImageLocation = "..\\..\\Resources\\Images\\add2.png";
            addRowBtnPic.ImageLocation = resourcesPath + "add2.png";
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
            //acceptBtnPic.ImageLocation = "..\\..\\Resources\\Images\\accept.png";
            acceptBtnPic.ImageLocation = resourcesPath + "accept.png";
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
                //delRowBtnPic[i].ImageLocation = "";
                delRowBtnPic[i].ImageLocation = resourcesPath;


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
            string logs = "";
            rTxtBox_Script.SelectAll();
            rTxtBox_Script.Copy();
            logs = Clipboard.GetText();
            System.IO.StreamWriter logFile = new System.IO.StreamWriter("logs.txt");
            logFile.WriteLine(logs);
            logFile.Close();
            logFile.Close();
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

                //doc.Load(".\\Scripts\\DatabaseUpdateValues.xml");
                //doc.Load("..\\..\\Scripts\\DatabaseUpdateValues.xml");
                //doc.Load(scriptLocation + sXmlFile);
                doc.Load(sXmlFile);

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
                    //XmlNode rtNodeToReplace = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/ReplaceToken[@name='" + tblName + "']");
                    XmlNode rtNodeToReplace = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/ReplaceToken[@name='" + sel_query + "']");
                    XmlNode ftNodeToReplace = doc.SelectSingleNode("//Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToken[@name='" + sel_query + "']");

                    int scriptLine = tlp_ScriptBuilder.RowCount;
                    int rtNodeCount = rtNodeToReplace.ChildNodes.Count;
                    //int ftNodeCount = ftNodeToReplace.ChildNodes.Count;
                    int i = 0;

                    foreach (XmlNode token in rtNodeToReplace.ChildNodes)
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

                    if (ftNodeToReplace != null)
                    {
                        int ftNodeCount = ftNodeToReplace.ChildNodes.Count;

                        foreach (XmlNode token in ftNodeToReplace.ChildNodes)
                        {
                            token.Attributes["set"].Value = cBox_Logic[i].SelectedItem.ToString();
                            token.Attributes["columnName"].Value = cBox_Field[i].SelectedItem.ToString();
                            token.Attributes["operand"].Value = cBox_Operand[i].SelectedItem.ToString();
                            token.Attributes["value"].Value = txtBox_Value[i].Text;
                            i++;
                        }
                    }

                    // Check if there are any (new) more tokens in the script builder.
                    if (scriptLine - i > 1)
                    {
                        int NoToken = (scriptLine - 2) - i;
                        //XmlNode[] token = new XmlNode[NoToken];
                        XmlNode token;

                        // Add all FilterTokens after the WHERE Clause.
                        for (int j = i; j < scriptLine - 2; j++)
                        {
                            if (cBox_Logic[j].SelectedIndex != -1)
                            {
                                MessageBox.Show("Add row " + j.ToString());

                                token = doc.CreateNode(XmlNodeType.Element, "Token", null);

                                XmlAttribute set = doc.CreateAttribute("set");
                                set.Value = (string)cBox_Logic[j].SelectedItem;

                                XmlAttribute columnName = doc.CreateAttribute("columnName");
                                columnName.Value = (string)cBox_Field[j].SelectedItem;

                                XmlAttribute operand = doc.CreateAttribute("operand");
                                operand.Value = (string)cBox_Operand[j].SelectedItem;

                                XmlAttribute value = doc.CreateAttribute("value");
                                value.Value = txtBox_Value[j].Text;

                                token.Attributes.Append(set);
                                token.Attributes.Append(columnName);
                                token.Attributes.Append(operand);
                                token.Attributes.Append(value);

                                //doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToken[@name='" + tblName + "']").InsertAfter(token, doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToke[@name='" + tblName + "']").LastChild);
                                //var selectedFileterNode = doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToken[@name='" + tblName + "']");
                                var selectedFileterNode = doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToken[@name='" + sel_query + "']");
                                selectedFileterNode.AppendChild(token);
                                doc.Save(sXmlFile);
                            }
                            //ftNodeToReplace
                        }
                    }

                }

                //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                //doc.Save("..\\..\\Scripts\\DatabaseUpdateValues.xml");   
                //doc.Save(scriptLocation + sXmlFile);
                //doc.Load(sXmlFile);
                doc.Save(sXmlFile);
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

            if (!scriptFromTreeView)
            {
                // Using Custom MessageBox
                //Set buttons language Czech/English/German/Slovakian/Spanish (default English)
                InputBox.SetLanguage(InputBox.Language.English);
                //Save the DialogResult as res
                DialogResult res = InputBox.ShowDialog("Select position of the string to replace:", "Replace Position",   //Text message (mandatory), Title (optional)
                    InputBox.Icon.Question,                                                                         //Set icon type Error/Exclamation/Question/Warning (default info)
                    InputBox.Buttons.OkCancel,                                                                      //Set buttons set OK/OKcancel/YesNo/YesNoCancel (default ok)
                    InputBox.Type.ComboBox,                                                                         //Set type ComboBox/TextBox/Nothing (default nothing)
                    new string[] { "Starting", "Wildcard", "Ending" },                                                        //Set string field as ComboBox items (default null)
                    true,                                                                                           //Set visible in taskbar (default false)
                    new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold));                        //Set font (default by system)
                //Check InputBox result
                if (res == System.Windows.Forms.DialogResult.OK || res == System.Windows.Forms.DialogResult.Yes)
                    replaceOption = InputBox.ResultValue;
                //listView1.Items.Add(InputBox.ResultValue);
            }


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
                            //if ((tlp_ScriptBuilder.RowCount - (min_rowCount + 1) == 0 ) && scriptFromTreeView == true)
                            if ((tlp_ScriptBuilder.RowCount - 1 == lastRowScripted) && scriptFromTreeView == true)
                            {
                                //tlp_ScriptBuilder.RowCount++;
                                //return;   
                            }
                            else
                            {
                                ScriptContent(tlp_ScriptBuilder.RowCount);
                            }
                        }

                        int i = y - min_rowCount;

                        if (i < max_row)
                        {
                            tlp_ScriptBuilder.RowCount++;
                            tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));

                            delRowBtnPic[i] = new PictureBox();
                            delRowBtnPic[i].Size = new Size(16, 16);
                            //delRowBtnPic[i].ImageLocation = "..\\..\\Resources\\Images\\delete.png";
                            delRowBtnPic[i].ImageLocation = resourcesPath + "delete.png";
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
            string childNode = cBox_Field[row_index_to_remove - 1].SelectedText;

            if (row_index_to_remove >= tlp_ScriptBuilder.RowCount)
            {
                return;
            }

            // delete all controls of row that needs to be deleted.
            for (int i = 0; i < tlp_ScriptBuilder.ColumnCount; i++)
            {
                var control = tlp_ScriptBuilder.GetControlFromPosition(i, row_index_to_remove);
                tlp_ScriptBuilder.Controls.Remove(control);

                if (i == 0)
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
            // Filter on Current Database name, Current Environment and Current selected table 
            //cBox_Logic = cBox_Logic.Where(x => !cBox_Logic.IsNullOrEmpty(x)).ToArray();

            // Update the XML file for scriptBuilder

            string tblName = cBox_Tables.SelectedItem.ToString();
            tblName = tblName.Substring(7, tblName.Length - 8);

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(sXmlFile);
            //XDocument xdoc = XDocument.Load(@"C:\Users\Alfred\Dropbox\Transfer\LinqXMLTest\DatabaseUpdateValues.xml");

            XmlNode childNodeToDelete;
            childNodeToDelete = xdoc.SelectSingleNode("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/ReplaceToken[@name='" + tblName + "']/Token[@columnName='" + childNode + "']");
            if (childNodeToDelete == null)
            {
                childNodeToDelete = xdoc.SelectSingleNode("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[@name='" + tblName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens/FilterToken[@name='" + tblName + "']/Token[@columnName='" + childNode + "']");
            }
            childNodeToDelete.ParentNode.RemoveChild(childNodeToDelete);
            xdoc.Save(sXmlFile);



            /*var tokens = from token in xdoc.Descendants("Database")
                         let table = (string)token.Element("Tables").Element("Table").Attribute("name")
                         let env = (string)token.Element("Tables").Element("Table").Element("Environments").Element("Environment").Attribute("name")
                         let qToken = (string)token.Element("Tables").Element("Table").Element("Environments").Element("Environment").Element("Tokens").Element("ReplaceToken").Attribute("name")
                         where (table != null && table == tblName) && (env != null && env == cur_environment) && (qToken != null && qToken == tblName)
                         //where ((string)token.Element("Database").Attribute("name") == cur_database) && ((string)token.Element("Table").Attribute("name") == tblName) && ((string)token.Element("Environment").Attribute("name") == cur_environment)
                         select token;
            tokens.ToList().ForEach(x => x.Remove());*/

            /* where (string)token.Element("Table").Element("Environment").Element("ReplaceToken") == ""
             * var q = from node in xdoc.Descendants("Database")
                    let pAttr = node.Attribute("name")
                    let attr = node.Descendants("Table")
                    let catt = node.Descendants("Environment")
                    //let tattr = node.Attribute("name")
                    where (pAttr != null && pAttr.Value == _cur_Db) && (attr != null && attr.Value == tblName) && (catt != null && catt.Value == cur_environment)
                    select node;

            q.ToList().ForEach(x => x.Remove());*/
            //xdoc.Save(sXmlFile);


            // Update the script file.
            //SaveScriptFile();
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

            if (validateInputValues() && CheckInputForInjection())
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

                if (!scriptFromTreeView)
                {
                    sel_query = InputMessageBox();
                }
                //else
                //{
                //    sel_query = queryName;
                //}                    


                if (tlp_ScriptBuilder.RowCount - min_rowCount > 1)
                {
                    if (MessageBox.Show(strMsg, "Close Window", MessageBoxButtons.YesNo) != DialogResult.No)
                    {
                        //if (tViewScripts.SelectedNode != null && tViewScripts.SelectedNode.Tag == "ReplaceToken")
                        if (tViewScripts.SelectedNode != null && sel_tag == "ReplaceToken")
                        {
                            queryFound = true;
                            string selectedNode = tViewScripts.SelectedNode.Text;
                            //MessageBox.Show("Updating " + selectedNode);
                        }
                        else
                        {
                            foreach (TreeNode tn in tViewScripts.Nodes) // tranverse the database node
                            {
                                foreach (TreeNode cn in tn.Nodes)   // iterate through all the table nodes
                                {
                                    //if (tViewScripts.Nodes.ContainsKey(cur_environment))
                                    if (cn.Tag == "Table" && cn.Text == queryName)
                                    {
                                        tblFound = true;

                                        foreach (TreeNode en in cn.Nodes)
                                        {
                                            if (en.Tag == "Environment" && en.Text == cur_environment)
                                            {
                                                envFound = true;

                                                foreach (TreeNode qn in en.Nodes)
                                                {
                                                    if (qn.Tag == "ReplaceToken" && qn.Text == sel_query)
                                                    {
                                                        queryFound = true;
                                                    }
                                                    /*else
                                                    {
                                                        queryFound = false;                                                           
                                                    }*/
                                                }
                                            }
                                            /*else
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
                                            }*/
                                        }
                                    }
                                    /* else
                                     {
                                         tblFound = false;
                                         //MessageBox.Show("Creating new table node as " + queryName);
                                         TreeNode tblNode = new TreeNode(queryName);
                                         tblNode.Tag = "Table";

                                         TreeNode qNode = new TreeNode(sel_query);
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
                                     }*/
                                }
                            }
                        }
                        // Update the xml file with newly created node.                    
                        //updateNode = doc.SelectSingleNode("Databases/Database[@name='" + cur_database + "']");
                        updateNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db + "']");
                        if (!queryFound)
                        {
                            int NoToken = tlp_ScriptBuilder.RowCount - 3;
                            XmlNode[] token = new XmlNode[NoToken];

                            //Create a new Tokens Node
                            XmlNode tokens = doc.CreateNode(XmlNodeType.Element, "Tokens", null);

                            //MessageBox.Show("Creating new query XML node");
                            repNode = doc.CreateNode(XmlNodeType.Element, "ReplaceToken", null);
                            XmlAttribute repName = doc.CreateAttribute("name");
                            //repName.Value = tblName;
                            repName.Value = sel_query;
                            XmlAttribute repType = doc.CreateAttribute("type");
                            repType.Value = replaceOption;

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
                            filTokenName.Value = sel_query;

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
                                sel_environment = cur_environment;
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
                                //doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + queryName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens").InsertAfter(repNode, doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + cur_environment + "']/Tokens").LastChild);
                                doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + cur_environment + "']/Tokens").AppendChild(repNode);
                                if (filterToken != null)
                                {
                                    //doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + queryName + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens").AppendChild(filterToken);
                                    doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + cur_environment + "']/Tokens").AppendChild(filterToken);
                                }
                                if (sel_environment == "")
                                    sel_environment = cur_environment;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Updating existing query XML node");
                            scriptFromTreeView = false;
                            cur_environment = sel_environment;
                            btn_Commit.PerformClick();

                        }

                        doc.Save(sXmlFile);
                        //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");

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
                                catch (System.Data.SqlTypes.SqlTypeException ex)
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
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]) && fieldDatatypes[cBox_Field[i].SelectedIndex] == "uniqueidentifier")
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
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex]) && fieldDatatypes[cBox_Field[i].SelectedIndex] == "ntext")
                        {
                            string maxLength = "4000";

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
                        else if (CharacterDataTypes.Contains(fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().Substring(0, fieldDatatypes[cBox_Field[i].SelectedIndex].ToString().IndexOf("("))))
                        {
                            string maxLength = sqlDataType.Substring(sqlDataType.IndexOf("(") + 1, sqlDataType.Length - (sqlDataType.IndexOf("(") + 2));
                            // check if maxlenght is max, then use 4000.

                            if (maxLength == "max")
                                maxLength = "4000";

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
            for (int i = 0; i < tlp_ScriptBuilder.RowCount - min_rowCount; i++)
            {
                if (cBox_Field[i].SelectedIndex != null)
                {
                    string curString = txtBox_Value[i].Text;
                    foreach (string s in txtBox_Value[i].Text.Split(' '))
                    {
                        for (int j = 0; j < blackList.Length; j++)
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
                if (cBox_Logic[i].SelectedItem == "WHERE")
                {
                    //script += "WHERE " + cBox_Field[i].SelectedItem + " " + cBox_Operand[i].SelectedItem + " " + txtBox_Value[i].Text;

                    // Check if cBox_Field[i] has as selected value
                    if (cBox_Field[i].SelectedIndex > -1)
                    {
                        rTxtBox_Script.AppendText("\nWHERE ", Color.Blue);
                        //}
                        //else
                        //{
                        //    rTxtBox_Script.AppendText(" AND ", Color.Blue);
                        //}
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
                else if (cBox_Logic[i].SelectedItem != null && cBox_Logic[i].SelectedItem != "WHERE" && cBox_Logic[i].SelectedItem != "WITH")
                {
                    //if (CheckSQL_Syntax() && afterWhile == false)
                    //{
                    //    rTxtBox_Script.AppendText(", ", Color.Green);
                    //}
                    //else
                    //{
                    //    rTxtBox_Script.AppendText("" + cBox_Logic[i].SelectedItem + " ", Color.Green);
                    //}

                    // Check if cBox_Field[i] has as selected value
                    if (cBox_Field[i].SelectedIndex > -1)
                    {
                        rTxtBox_Script.AppendText(" AND ", Color.Green);
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
            else if (rBtn_Insert.Checked)
            {

            }

            //rTxtBox_Script.Text = script;
            lastRowScripted = lastRowMarker - 1;
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
                        if (column.DataType.ToString().ToUpper() == "NVARCHAR" || column.DataType.ToString().ToUpper() == "VARCHAR")
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

        static TripleDES CreateDES(string key)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = md5.ComputeHash(Encoding.Unicode.GetBytes(key));
            des.IV = new byte[des.BlockSize / 8];
            return des;
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {
            /*
            Stream myStream = null;
            //System.IO.FileStream fs = new FileStream(();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.Filter = "txt files (*.cbs)|*.cbs|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FileName = "DatabaseUpdateValues.cbs";

            System.IO.FileStream fs = new FileStream(openFileDialog1.FileName,System.IO.FileMode.Create);

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(sXmlFile);
                    fs.Write(info, 0, info.Length);
                    if ((fs != null)  && (openFileDialog1.OpenFile() != null))
                    {
                        using (fs)
                        {
                            // Open file for reading
                            System.IO.FileStream _filename = new FileStream(openFileDialog1.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                            _filename.Write(info, 0, info.Length);
                            fs.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
             */

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = "C:\\";
            saveFileDialog1.Filter = "Export values (*.cbs)|*.cbs|All files (*.*)|*.*";
            saveFileDialog1.Title = "Export the Database Update Values";
            saveFileDialog1.FileName = "DatabaseUpdateValues.cbs";
            saveFileDialog1.ShowDialog();


            if (saveFileDialog1.FileName != "")
            {
                XmlDocument xmlDoc = doc;
                // // Saves the Image via a FileStream created by the OpenFile method.
                // //System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                // //System.IO.FileStream fs = new System.IO.FileStream(saveFileDialog1.FileName, FileMode.Create);

                // //byte[] info = new UTF8Encoding(true).GetBytes(sXmlFile);
                // //byte[] info = new UTF8Encoding(true).GetBytes(doc.OuterXml);
                //// byte[] info = Encoding.Default.GetBytes(doc.OuterXml);
                // //byte[] info = Encoding.Default.GetBytes(xmlDoc.OuterXml);
                // //Encoding.ASCII.GetBytes("FileStream Test");
                // //fs.Write(info, 0, info.Length);

                // // Create a new TripleDES key. 
                TripleDESCryptoServiceProvider tDESkey = new TripleDESCryptoServiceProvider();

                // // Create a new instance of the TrippleDESDocumentEncryption object.
                // //TrippleDESDocumentEncryption xmlTDES = new TrippleDESDocumentEncryption(xmlDoc, tDESkey);
                // TrippleDESDocumentEncryption xmlTDES = new TrippleDESDocumentEncryption(xmlDoc, tDESkey);

                // xmlTDES.Encrypt("Databases");
                // byte[] info = Encoding.Default.GetBytes(xmlTDES.Doc.OuterXml);

                // System.IO.FileStream _filename = new FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                // _filename.Write(info, 0, info.Length);


                // //fs.Close();
                // _filename.Close();
                // tDESkey.Clear();

                /*byte[] buffer = Encryption(xmlDoc.OuterXml, tDESkey.ToString());
                //string b = Convert.ToBase64String(buffer);

                System.IO.FileStream _filename = new FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                _filename.Write(buffer, 0, buffer.Length);


                _filename.Close();*/


                string filedata = xmlDoc.OuterXml;
                filedata = EncryptData(filedata);
                byte[] buffer = Encoding.UTF8.GetBytes(filedata);

                System.IO.FileStream _filename = new FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                _filename.Write(buffer, 0, buffer.Length);


                _filename.Close();
            }

        }

        public string EncryptData(string toEncrypt)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");

            //256-AES key
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            RijndaelManaged rDel = new RijndaelManaged();

            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;

            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public string DecryptData(string toDecrypt)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");

            //AES-256 key
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;

            rDel.Padding = PaddingMode.PKCS7;

            //better lang support
            ICryptoTransform cTransform = rDel.CreateDecryptor();

            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static byte[] Encryption(string PlainText, string key)
        {
            TripleDES des = CreateDES(key);
            ICryptoTransform ct = des.CreateEncryptor();
            byte[] input = Encoding.Default.GetBytes(PlainText);
            //byte[] input = Encoding.UTF32.GetBytes(PlainText);     
            //string s64 = Convert.ToBase64String(Encoding.Default.GetBytes(PlainText));
            //byte[] input = Convert.FromBase64String(s64);
            //byte[] input = Encoding.ASCII.GetBytes(PlainText);                
            //Convert.ToBase64String(input);
            //return input;
            return ct.TransformFinalBlock(input, 0, input.Length);
        }

        private void btn_Import_Click(object sender, EventArgs e)
        {
            string strMsg = "Import will over write existing update values, do you wish to continue?";
            if (MessageBox.Show(strMsg, "Import Warning", MessageBoxButtons.YesNo) != DialogResult.No)
            {
                //string duv_ImportPath = "";
                // Displays a SaveFileDialog so the user can save the DatabaseUpdateValues
                // to the specified location.
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                //saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
                saveFileDialog1.Filter = "Export values (*.cbs)|*.cbs|All files (*.*)|*.*";
                saveFileDialog1.Title = "Import the Database Update Values";
                saveFileDialog1.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (saveFileDialog1.FileName != "")
                {
                    XmlDocument xDoc = new XmlDocument();
                    // xDoc.Load(Path.GetFullPath(saveFileDialog1.FileName));
                    // // Saves the Image via a FileStream created by the OpenFile method.
                    // //System.IO.FileStream fs =
                    //   // (System.IO.FileStream)saveFileDialog1.OpenFile();
                    //// System.IO.FileStream fs = new System.IO.FileStream("D:\\Imports\\DatabaseUpdateValues.cbs", FileMode.Append);
                    //System.IO.FileStream fs = new System.IO.FileStream("D:\\Imports\\DatabaseUpdateValues.cbs", FileMode.Append);
                    System.IO.FileStream fs = new System.IO.FileStream(scriptLocation + "DatabaseUpdateValues.xml", FileMode.Create);

                    // // Saves the Image in the appropriate ImageFormat based upon the
                    // // File type selected in the dialog box.
                    // // NOTE that the FilterIndex property is one-based.
                    // /*
                    // switch (saveFileDialog1.FilterIndex)
                    // {
                    //     case 1:
                    //         this.button2.Image.Save(fs,
                    //            System.Drawing.Imaging.ImageFormat.Jpeg);
                    //         break;

                    //     case 2:
                    //         this.button2.Image.Save(fs,
                    //            System.Drawing.Imaging.ImageFormat.Bmp);
                    //         break;

                    //     case 3:
                    //         this.button2.Image.Save(fs,
                    //            System.Drawing.Imaging.ImageFormat.Gif);
                    //         break;
                    // }

                    //  */

                    // string text = System.IO.File.ReadAllText(saveFileDialog1.FileName);

                    // // Create a new TripleDES key. 
                    TripleDESCryptoServiceProvider tDESkey = new TripleDESCryptoServiceProvider();

                    // // Create a new instance of the TrippleDESDocumentEncryption object.
                    // //TrippleDESDocumentEncryption xmlTDES = new TrippleDESDocumentEncryption(xmlDoc, tDESkey);
                    // TrippleDESDocumentEncryption xmlTDES = new TrippleDESDocumentEncryption(xDoc, tDESkey);

                    // xmlTDES.Decrypt();

                    // string decryptedString = DecryptXml(text, tDESkey.ToString());

                    // // fileStream.Write(uniEncoding.GetBytes(tempString), 0, uniEncoding.GetByteCount(tempString));
                    // //byte[] info = new UTF8Encoding(true).GetBytes(sXmlFile);
                    // //byte[] info = UTF8Encoding.Default.GetBytes(Path.GetFullPath(saveFileDialog1.FileName));
                    // //byte[] info = UTF8Encoding.Default.GetBytes(xDoc.OuterXml);
                    // byte[] info = UTF8Encoding.Default.GetBytes(xmlTDES.Doc.OuterXml);
                    // fs.Write(info, 0, info.Length);
                    // /*
                    // if (File.Exists(scriptFileLocation))
                    // {
                    //     MessageBox.Show("Deleting existing file and re-creating " + scriptFile);
                    //     File.Delete(scriptFileLocation);
                    // }
                    // else
                    // {
                    //     MessageBox.Show("Creating new script file: " + scriptFile);
                    // }

                    // System.IO.StreamWriter sqlFile = new StreamWriter(scriptFileLocation);
                    // sqlFile.WriteLine(rTxtBox_Script.Text);
                    // sqlFile.Flush();
                    // sqlFile.Close();
                    //  */

                    // fs.Close();
                    // xmlTDES.Clear();

                    /*string text = System.IO.File.ReadAllText(saveFileDialog1.FileName);
                    string decryptedText = Decryption(text, tDESkey.ToString());
                    MessageBox.Show(decryptedText);*/

                    string filedata = System.IO.File.ReadAllText(saveFileDialog1.FileName);
                    filedata = DecryptData(filedata);
                    //MessageBox.Show(filedata);
                    byte[] buffer = Encoding.UTF8.GetBytes(filedata);
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Close();
                }
            }
            else
            {
                // Cancel the import process
            }

        }

        public static string Decryption(string CypherText, string key)
        {
            byte[] b = Convert.FromBase64String(CypherText);
            //byte[] b = Encoding.Default.GetBytes(CypherText);
            TripleDES des = CreateDES(key);
            ICryptoTransform ct = des.CreateDecryptor();
            byte[] output = ct.TransformFinalBlock(b, 0, b.Length);
            return Encoding.Default.GetString(output);
        }

        public static string DecryptXml(string input, string key)
        {
            //byte[] inputArray = Convert.FromBase64String(input);
            byte[] inputArray = UTF8Encoding.Default.GetBytes(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
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

            sXmlFile = scriptLocation + "DatabaseUpdateValues.xml";

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
            {
                AddNode(startNode, tNode);
            }
            else
            {
                if (MessageBox.Show("Set up new Database for future value update?", "New Database", MessageBoxButtons.YesNo) != DialogResult.No)
                {
                    XmlNode new_dbNode = doc.CreateNode(XmlNodeType.Element, "Database", null);
                    XmlNode tables = doc.CreateNode(XmlNodeType.Element, "Tables", null);

                    XmlAttribute nameAttri = doc.CreateAttribute("name");
                    nameAttri.Value = _cur_Db;

                    new_dbNode.AppendChild(tables);
                    new_dbNode.Attributes.Append(nameAttri);
                    XmlNodeList existingDatabases = doc.SelectNodes("Databases/Database");
                    doc.SelectSingleNode("/Databases").InsertAfter(new_dbNode, doc.SelectSingleNode("/Databases").LastChild);
                    doc.Save(sXmlFile);
                }
            }
        }

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            DirectoryInfo d = new DirectoryInfo(scriptLocation);
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
                            Font f = new Font("Arial", 8, FontStyle.Bold);
                            Font m = new Font("Arial", 8, FontStyle.Italic);
                            inTreeNode.Nodes.Add(new TreeNode(tab.Attributes["name"].Value));
                            tNode = inTreeNode.Nodes[i];
                            tNode.Tag = "ReplaceToken";
                            //tViewScripts.DrawMode = TreeViewDrawMode.OwnerDrawText;
                            //string scriptFile = _cur_Db + "_" + tblName + "_" + sel_query + "_" + sel_environment + ".sql";
                            string scriptFile = scriptLocation + "\\" + _cur_Db + "_" + tNode.Parent.Parent.Text + "_" + tNode.Text + "_" + tNode.Parent.Text + ".sql";
                            if (File.Exists(scriptFile))
                            {
                                tNode.ForeColor = System.Drawing.Color.Green;
                                tNode.NodeFont = f;
                                tNode.ToolTipText = "Found script";
                            }
                            else
                            {
                                tNode.ForeColor = System.Drawing.Color.Red;
                                tNode.NodeFont = m;
                                tNode.ToolTipText = "Missing script";
                            }
                            AddNode(tab, tNode);
                            i++;
                        }
                        //}
                    }
                }
            }
        }

        private void tViewScripts_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Color nodeColor = Color.Black;
            //if ((e.State & TreeNodeStates.Selected) != 0)
            //    nodeColor = SystemColors.HighlightText;

            TextRenderer.DrawText(e.Graphics,
                                  e.Node.Text,
                                  e.Node.NodeFont,
                                  e.Bounds,
                                  nodeColor,
                                  Color.Empty,
                                  TextFormatFlags.VerticalCenter);
        }

        private void btn_Generate_Click(object sender, EventArgs e)
        {
            UpdateScriptWindow();

            if (rBtn_Replace.Checked)
            {
                rTxtBox_Script.AppendText("\nDECLARE ", Color.Blue);
                rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem, Color.Black);
                if (fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "NTEXT" || fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "TEXT" || fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "IMAGE")
                {
                    rTxtBox_Script.AppendText(" NVARCHAR(4000) ", Color.Blue);
                }
                else
                {
                    rTxtBox_Script.AppendText(" " + fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() + "", Color.Blue);
                }
                /*if (fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "NVARCHAR")
                {
                    rTxtBox_Script.AppendText("(" + fieldDatatypes[cBox_Field[0].SelectedIndex].Length + ")");
                }*/

                switch (replaceOption)
                {
                    case "Starting":
                        rTxtBox_Script.AppendText("\nSET ", Color.Blue);
                        rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " = (", Color.Black);
                        rTxtBox_Script.AppendText(" SELECT ", Color.Black);
                        rTxtBox_Script.AppendText("TOP 1 ", Color.Orange);
                        rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem, Color.Black);
                        rTxtBox_Script.AppendText(" FROM ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Tables.SelectedItem, Color.Blue);
                        rTxtBox_Script.AppendText(" (NOLOCK))", Color.Black);
                        rTxtBox_Script.AppendText("\n");
                        rTxtBox_Script.AppendText("SELECT ", Color.Blue);
                        rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " =", Color.Black);
                        rTxtBox_Script.AppendText(" SUBSTRING ", Color.Pink);
                        rTxtBox_Script.AppendText("(@Current" + cBox_Field[0].SelectedItem + " ,", Color.Black);
                        rTxtBox_Script.AppendText(" 0", Color.Orange);
                        rTxtBox_Script.AppendText(" , PATINDEX(", Color.Pink);

                        rTxtBox_Script.AppendText("'" + txtBox_Value[0].Text + "'", Color.Red);
                        rTxtBox_Script.AppendText(" , @Current" + cBox_Field[0].SelectedItem + "))\n");
                        break;
                    case "Wildcard":
                        rTxtBox_Script.AppendText("\nSET ", Color.Blue);
                        rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " = ", Color.Black);
                        rTxtBox_Script.AppendText("" + txtBox_Value[0].Text + "'", Color.Black);
                        rTxtBox_Script.AppendText("\n");
                        break;
                    case "Ending":
                        rTxtBox_Script.AppendText("\nSET ", Color.Blue);
                        rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " = (", Color.Black);
                        rTxtBox_Script.AppendText("SELECT ", Color.Black);
                        rTxtBox_Script.AppendText("TOP 1", Color.Orange);
                        rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem, Color.Black);
                        rTxtBox_Script.AppendText(" FROM ", Color.Blue);
                        rTxtBox_Script.AppendText("" + cBox_Tables.SelectedItem, Color.Blue);
                        rTxtBox_Script.AppendText(" (NOLOCK)", Color.Black);
                        rTxtBox_Script.AppendText("\n");
                        rTxtBox_Script.AppendText("SELECT ", Color.Blue);
                        rTxtBox_Script.AppendText("@Current" + cBox_Field[0].SelectedItem + " = ", Color.Black);
                        rTxtBox_Script.AppendText(" SUBSTRING ", Color.Pink);
                        rTxtBox_Script.AppendText("(@Current" + cBox_Field[0].SelectedItem + ", ", Color.Black);
                        rTxtBox_Script.AppendText(" 0", Color.Orange);
                        rTxtBox_Script.AppendText("PARTINDEX(", Color.Pink);

                        rTxtBox_Script.AppendText("" + txtBox_Value[0].Text + "", Color.Red);
                        rTxtBox_Script.AppendText(" ,@Current" + cBox_Field[0].SelectedItem + "))\n");
                        break;
                }

                rTxtBox_Script.AppendText("\nUPDATE ", Color.Pink);
                rTxtBox_Script.AppendText(" " + tbl + "\n", Color.Black);
                rTxtBox_Script.AppendText("SET ", Color.Blue);
                rTxtBox_Script.AppendText("" + cBox_Field[0].SelectedItem + " = ", Color.Black);
                rTxtBox_Script.AppendText(" REPLACE", Color.Pink);
                if (fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "NTEXT" || fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "TEXT" || fieldDatatypes[cBox_Field[0].SelectedIndex].ToUpper() == "IMAGE")
                {
                    rTxtBox_Script.AppendText("(CAST([" + cBox_Field[0].SelectedItem + "] AS NVARCHAR(4000)), @Current" + cBox_Field[0].SelectedItem + ",", Color.Black);
                }
                else
                {
                    rTxtBox_Script.AppendText("([" + cBox_Field[0].SelectedItem + "], @Current" + cBox_Field[0].SelectedItem + ",", Color.Black);
                }

                rTxtBox_Script.AppendText("'" + txtBox_Value[1].Text + "')", Color.Red);

                for (int i = 3; i <= tlp_ScriptBuilder.RowCount; i++)
                {
                    int cur_rowToProcess = i;
                    if (i <= tlp_ScriptBuilder.RowCount)
                    {
                        ScriptContent(cur_rowToProcess);
                    }
                }
            }
        }

        void tViewScripts_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // scriptFromTreeView = true;
            afterWhile = false;
            tlp_ScriptBuilder.SuspendLayout();
            ClearScriptBuilder();
            //UpdateScriptWindow();            

            XmlNodeList nNode;

            if (e.Node.Parent != null)
            {
                if (Enum.IsDefined(typeof(Environment), e.Node.Parent.Text))
                {
                    sel_environment = e.Node.Parent.Text;
                    sel_table = e.Node.Parent.Parent.Text;
                    sel_query = e.Node.Text;
                    sel_tag = "ReplaceToken";
                    scriptFromTreeView = true;

                    nNode = startNode.SelectNodes("Tables/Table[@name='" + sel_table + "']/Environments/Environment[@name='" + sel_environment + "']/Tokens");
                    UpdateScriptWindow();

                    foreach (XmlNode tokens in nNode)
                    {
                        int y;
                        int i;
                        foreach (XmlNode replaceNode in tokens.ChildNodes)
                        {
                            cBox_Tables.SelectedIndex = cBox_Tables.FindString("[dbo].[" + sel_table + "]");

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
                                        replaceOption = replaceNode.Attributes["type"].Value;
                                        // btn_Generate.PerformClick();
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

                                    tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));

                                    delRowBtnPic[i].Size = new Size(19, 19);
                                    delRowBtnPic[i].Tag = i;
                                    delRowBtnPic[i].ImageLocation = resourcesPath + "delete.png";
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

                //doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + sel_table + "']/Environments/Environment[@name='" + cur_environment + "']/Tokens").AppendChild(filterToken);
                // "/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments" 
                // book=root.SelectSingleNode("descendant::book[author/last-name='Austen']");
                XmlNode testNode = doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table[@name='" + sel_table + "']/Environments");
                //XmlNode testNode = doc.SelectSingleNode("descendant::Database[Tables/Table[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + sel_table + "']/Environments");
                foreach (XmlNode dbNode in doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table[@name='" + sel_table + "']/Environments"))
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
                    doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table[@name='" + sel_table + "']/Environments").InsertAfter(envNode, doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table[@name='" + sel_table + "']/Environments").LastChild);
                }

                doc.Save(sXmlFile);
                //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                startNode = doc.SelectSingleNode("Databases/Database[@name='" + _cur_Db.ToLower() + "']");
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

                foreach (XmlNode tblNode in doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']"))
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
                    doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']").InsertAfter(tokenNode, doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']").LastChild);

                }
                else
                {
                    doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens").InsertAfter(repToken, doc.SelectSingleNode("/Databases/Database[@name='" + _cur_Db + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens").LastChild);
                }
                doc.Save(sXmlFile);
                //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
                startNode = doc.SelectSingleNode("/Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']");
            }
            else if (tViewScripts.SelectedNode.Tag == "Table")
            {
                XmlNode dbNode = doc.SelectSingleNode("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']");
                if (dbNode != null)
                {
                    XmlNode updateNode = doc.SelectSingleNode("Databases/Database[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') " + "='" + _cur_Db.ToLower() + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']");
                    targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']/");
                    targetNode.AppendChild(copyNode);

                    updateNode = targetNode;
                }
                else
                {
                    XmlNode cur_dbNode = doc.CreateNode(XmlNodeType.Element, "Database", null);
                    doc.SelectSingleNode("/Databases/").InsertAfter(cur_dbNode, doc.SelectSingleNode("/Databases/").LastChild);
                }
                doc.Save(sXmlFile);
                //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
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

            if (m.Text.ToLower() == "new table")
            {
                //bServer = true;
                actionText = "New Table";

                TreeNode newNode = new TreeNode("New Table");
                newNode.Tag = "Table";

                tViewScripts.SelectedNode.Nodes.Add(newNode);
                tViewScripts.SelectedNode = newNode;
                tViewScripts.LabelEdit = true;

                if (!newNode.IsEditing)
                {
                    newNode.BeginEdit();
                    // EnableServerDetails();
                }
            }
            else if (m.Text.ToLower() == "new environment")
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
                string activeNode = tViewScripts.SelectedNode.Tag.ToString();
                queryName = "New Query";

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
                //XmlNode clickedNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens");  //last working version - copies all query
                XmlNode clickedNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens/ReplaceToken[@name='" + tViewScripts.SelectedNode.Text + "']");
                XmlNode clickedNodeFilter = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Tokens/FilterToken[@name='" + tViewScripts.SelectedNode.Text + "']");
                source_table = tViewScripts.SelectedNode.Parent.Parent.Text;

                copyNode = clickedNode.CloneNode(true);

                if (clickedNodeFilter != null)
                {
                    copyFilterNode = clickedNodeFilter.CloneNode(true);
                }

                TreeNode clickNode = tViewScripts.SelectedNode;
                cloneNode = (TreeNode)clickNode.Clone();
            }
            else if (m.Text.ToLower() == "copy environment")
            {
                XmlNode clickedXmlNode = startNode.SelectSingleNode("Tables/Table[@name='" + tViewScripts.SelectedNode.Parent.Text + "']/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']/Tokens");
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

                    sel_table = tViewScripts.SelectedNode.Text;
                    tViewScripts.SelectedNode.Nodes.Add(newNode);
                    //newNode.Nodes.Add(cloneNode.LastNode);
                    newNode.Nodes.Add(cloneNode);
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
                    XmlNode updateNode = doc.SelectSingleNode("Databases/Database[@name='" + bare_database + "']/Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']"); // last working version                   
                    targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']/Tokens");
                    //targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']");

                    if (targetNode != null)
                    {
                        targetNode.AppendChild(copyNode);
                        targetNode.AppendChild(copyFilterNode);
                    }
                    else
                    {
                        // add the Tokens node to the environment node.
                        targetNode = startNode.SelectSingleNode("Tables/Table/Environments/Environment[@name='" + tViewScripts.SelectedNode.Text + "']");
                        XmlNode newTokens;
                        newTokens = doc.CreateNode(XmlNodeType.Element, "Tokens", null);
                        XmlNode tokensNode = targetNode.AppendChild(newTokens);
                        //targetNode.AppendChild(newTokens).AppendChild(copyNode);
                        tokensNode.AppendChild(copyNode);
                        tokensNode.AppendChild(copyFilterNode);
                    }

                    updateNode = targetNode;

                    tViewScripts.SelectedNode.Nodes.Add(cloneNode);
                    //newNode.Nodes.Add(cloneNode.LastNode);
                    tViewScripts.SelectedNode = cloneNode;
                }
                doc.Save(sXmlFile);
                //doc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
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

                    XDocument xdoc = XDocument.Load(sXmlFile);
                    //XDocument xdoc = XDocument.Load(".\\Scripts\\DatabaseUpdateValues.xml");

                    IEnumerable<XElement> queryTokens =
                        from node in xdoc.Descendants("ReplaceToken")
                        .Union(xdoc.Descendants("FilterToken"))
                        let queries = node
                        where queries != null && queries.Attribute("name").Value == tViewScripts.SelectedNode.Text && queries.Parent.Parent.Attribute("name").Value == tViewScripts.SelectedNode.Parent.Text
                        select queries;

                    queryTokens.ToList().ForEach(x => x.Remove());

                    xdoc.Save(sXmlFile);
                    //xdoc.Save(".\\Scripts\\DatabaseUpdateValues.xml");

                    tViewScripts.Nodes.Remove(tViewScripts.SelectedNode);
                    MessageBox.Show("Removed " + toDelete);
                }
            }
        }

        private void RemoveItemFromXml(string nType, string itemToDelete)
        {
            XDocument xdoc = XDocument.Load(sXmlFile);
            //XDocument xdoc = XDocument.Load(".\\Scripts\\DatabaseUpdateValues.xml");

            var q = from node in xdoc.Descendants(nType)
                    let pAttr = node.Parent.Attribute("name")
                    let attr = node.Attribute("name")
                    where (attr != null && attr.Value == itemToDelete) || (attr.Value == itemToDelete && pAttr.Value == tViewScripts.SelectedNode.Parent.Text)
                    select node;

            q.ToList().ForEach(x => x.Remove());
            xdoc.Save(sXmlFile);
            //xdoc.Save(".\\Scripts\\DatabaseUpdateValues.xml");
            MessageBox.Show("Removed " + itemToDelete);
        }

        private void SaveScriptFile()
        {
            //string scriptDirectory = "\\Script\\";
            string scriptDirectory = scriptLocation;
            //string scriptFile = _cur_Db + "_" + tblName + "_" + cur_environment + ".sql";
            string scriptFile = _cur_Db + "_" + tblName + "_" + sel_query + "_" + sel_environment + ".sql";
            string scriptFileLocation = scriptDirectory + scriptFile;

            if (File.Exists(scriptFileLocation))
            {
                MessageBox.Show("Deleting existing file and re-creating " + scriptFile);
                File.Delete(scriptFileLocation);
            }
            else
            {
                MessageBox.Show("Creating new script file: " + scriptFile);
            }

            //rTxtBox_Script.AppendText("\nGO", Color.Blue);
            System.IO.StreamWriter sqlFile = new StreamWriter(scriptFileLocation);
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

        private string InputMessageBox()
        {
            InputBox.SetLanguage(InputBox.Language.English);
            //Save the DialogResult as res
            DialogResult res = InputBox.ShowDialog("Enter Query Name:", "New Query",   //Text message (mandatory), Title (optional)
                InputBox.Icon.Question,                                                                         //Set icon type Error/Exclamation/Question/Warning (default info)
                InputBox.Buttons.OkCancel,                                                                      //Set buttons set OK/OKcancel/YesNo/YesNoCancel (default ok)
                InputBox.Type.TextBox,                                                                         //Set type ComboBox/TextBox/Nothing (default nothing)
                new string[] { "Starting", "Wildcard", "Ending" },                                                        //Set string field as ComboBox items (default null)
                true,                                                                                           //Set visible in taskbar (default false)
                new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold));                        //Set font (default by system)
            //Check InputBox result
            if (res == System.Windows.Forms.DialogResult.OK || res == System.Windows.Forms.DialogResult.Yes)
            {
                return InputBox.ResultValue;
                // this.Close();
            }
            return InputBox.ResultValue;
        }

    }

    class TrippleDESDocumentEncryption
    {
        protected XmlDocument docValue;
        protected TripleDES algValue;

        public TrippleDESDocumentEncryption(XmlDocument Doc, TripleDES Key)
        {
            if (Doc != null)
            {
                docValue = Doc;
            }
            else
            {
                throw new ArgumentNullException("Doc");
            }

            if (Key != null)
            {

                algValue = Key;
            }
            else
            {
                throw new ArgumentNullException("Key");
            }
        }

        public XmlDocument Doc { set { docValue = value; } get { return docValue; } }
        public TripleDES Alg { set { algValue = value; } get { return algValue; } }

        public void Clear()
        {
            if (algValue != null)
            {
                algValue.Clear();
            }
            else
            {
                throw new Exception("No TripleDES key was found to clear.");
            }
        }

        public void Encrypt(string Element)
        {
            // Find the element by name and create a new
            // XmlElement object.
            XmlElement inputElement = docValue.GetElementsByTagName(Element)[0] as XmlElement;

            // If the element was not found, throw an exception.
            if (inputElement == null)
            {
                throw new Exception("The element was not found.");
            }

            // Create a new EncryptedXml object.
            EncryptedXml exml = new EncryptedXml(docValue);

            // Encrypt the element using the symmetric key.
            byte[] rgbOutput = exml.EncryptData(inputElement, algValue, false);

            // Create an EncryptedData object and populate it.
            EncryptedData ed = new EncryptedData();

            // Specify the namespace URI for XML encryption elements.
            ed.Type = EncryptedXml.XmlEncElementUrl;

            // Specify the namespace URI for the TrippleDES algorithm.
            ed.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncTripleDESUrl);

            // Create a CipherData element.
            ed.CipherData = new CipherData();

            // Set the CipherData element to the value of the encrypted XML element.
            ed.CipherData.CipherValue = rgbOutput;

            // Replace the plaintext XML elemnt with an EncryptedData element.
            EncryptedXml.ReplaceElement(inputElement, ed, false);
        }

        public void Decrypt()
        {

            // XmlElement object.
            XmlElement encryptedElement = docValue.GetElementsByTagName("EncryptedData")[0] as XmlElement;

            // If the EncryptedData element was not found, throw an exception.
            if (encryptedElement == null)
            {
                throw new Exception("The EncryptedData element was not found.");
            }

            // Create an EncryptedData object and populate it.
            EncryptedData ed = new EncryptedData();
            ed.LoadXml(encryptedElement);

            // Create a new EncryptedXml object.
            EncryptedXml exml = new EncryptedXml();

            // Decrypt the element using the symmetric key.
            byte[] rgbOutput = exml.DecryptData(ed, algValue);

            // Replace the encryptedData element with the plaintext XML elemnt.
            exml.ReplaceData(encryptedElement, rgbOutput);

        }

        public static void Decrypt(XmlDocument Doc)
        {
            // Check the arguments.  
            if (Doc == null)
                throw new ArgumentNullException("Doc");

            // Create a new EncryptedXml object.
            EncryptedXml exml = new EncryptedXml(Doc);

            // Decrypt the XML document.
            exml.DecryptDocument();

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
