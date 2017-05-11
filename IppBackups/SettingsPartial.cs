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
using System.Configuration;

namespace IppBackups
{
    public partial class Settings 
    {
        private void InitializeTreeViewEvents()
        {
            this.tView_Servers.NodeMouseClick += tView_Servers_NodeMouseClick;
            this.tView_Servers.MouseUp += tView_Servers_MouseUp;
            this.tView_Servers.AfterLabelEdit += tView_Servers_AfterLabelEdit;
            this.chkBox_Azure.CheckedChanged += new System.EventHandler(this.chkBox_Azure_CheckedChanged);
        }

        private void chkBox_Azure_CheckedChanged(object sender, EventArgs e)
        {
            bool backupToAzure = chkBox_Azure.Checked;

            if(backupToAzure)
            {
                lbl_BackupLocation.Text = "Azure URI";
                txtBox_AzureKey.Enabled = true;
            }
            else
            {
                lbl_BackupLocation.Text = "Backup Path";
                txtBox_AzureKey.Enabled = false;
            }
        }

        private void txtBox_AzureKey_TextChanged(object sender, EventArgs e)
        {
            if (tBox_Instance.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

    }
}
