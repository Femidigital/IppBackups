using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IppBackups
{
    public partial class DatabaseUpdates : Form
    {
        string cur_database = "";
        string script = "";
        string useStmt = "USE [";
        string tbl = "";
        int x, y;
        

        public DatabaseUpdates(string database, string env)
        {
            InitializeComponent();

            //Button btn_Add = new Button();
            //btn_Add.Text = "+";
            //bnt_Add.Click += new EventHandler(Mouse_Click);

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
            int sqBracket = script.LastIndexOf("]");
            int scriptLength = script.Length;
            //script = script.Replace(script.Substring(script.LastIndexOf(']'),script.Length), "");
            script = script.Replace(script.Substring(script.LastIndexOf("]"), script.Length - sqBracket), " ");
            script += "\nUPDATE " + tbl + "\n";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Replace_CheckedChanged(object sender, EventArgs e)
        {
            int sqBracket = script.LastIndexOf("]");
            int scriptLength = script.Length;

            script = script.Replace(script.Substring(script.LastIndexOf("]"), script.Length - sqBracket), " ");
            script += "\nREPLACE ";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Insert_CheckedChanged(object sender, EventArgs e)
        {
            int sqBracket = script.LastIndexOf("]");
            int scriptLength = script.Length;

            script = script.Replace(script.Substring(script.LastIndexOf("]"), script.Length - sqBracket), " ");
            script += "\nINSERT VALUES ";
            rTxtBox_Script.Text = script;
        }

        private void rBtn_Delete_CheckedChanged(object sender, EventArgs e)
        {
            int sqBracket = script.LastIndexOf("]");
            int scriptLength = script.Length;

            script = script.Replace(script.Substring(script.LastIndexOf("]"), script.Length - sqBracket), " ");
            script += "\nDELETE FROM ";
            rTxtBox_Script.Text = script;
        }

        private void tlp_ScriptBuilder_MouseClick(object sender, MouseEventArgs e)
        {
            int row = 0;
            int verticalOffset = 0;
            int y = tlp_ScriptBuilder.RowCount;

           // rTxtBox_Script.Text += "\n" + sender.ToString() + "was clicked \n";
           // rTxtBox_Script.Text += "\n" + e.ToString() + "was clicked \n";

            tlp_ScriptBuilder.RowCount++;
            tlp_ScriptBuilder.RowStyles.Insert(tlp_ScriptBuilder.RowCount - 2, new RowStyle(SizeType.AutoSize));
            tlp_ScriptBuilder.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 0, y - 1);
            tlp_ScriptBuilder.Controls.Add(new ComboBox() { Text = "Logical:", Anchor = AnchorStyles.Left, AutoSize = true }, 1, y - 1);
            tlp_ScriptBuilder.Controls.Add(new ComboBox() { Text = "Feild: ", Anchor = AnchorStyles.None, AutoSize = true }, 2, y - 1);
            tlp_ScriptBuilder.Controls.Add(new ComboBox() { Text = "Operator: ", Anchor = AnchorStyles.None, AutoSize = true }, 3, y - 1);
            tlp_ScriptBuilder.Controls.Add(new TextBox() { Text = ": ", Anchor = AnchorStyles.None, AutoSize = true }, 4, y - 1);

           // y++;
            //tlp_ScriptBuilder.Controls.Add(new Button() { Name = "btnAdd", Text = "+", Anchor = AnchorStyles.None, AutoSize = true }, 0, y);
        }
    }
}
