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
        }
    }
}
