using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppBackups
{
    public partial class Form1
    {
        private void InitializeCustomEvents()
        {
            this.cBox_Server.DisplayMember = "Name";
            this.cBox_Server.ValueMember = "Id";
            this.rTxtBox_Output.TextChanged += rTxtBox_Output_TextChanged;


            this.rBtn_Restore.CheckedChanged += new System.EventHandler(this.rBtn_Restore_CheckedChanged);
        }

        void rTxtBox_Output_TextChanged(object sender, EventArgs e)
        {
            rTxtBox_Output.ScrollToCaret();
            //throw new NotImplementedException();
        }
    }
}
