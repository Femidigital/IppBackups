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
using System.Configuration;

namespace IppBackups
{
    
    public partial class Settings : Form
    {
        private string ConfigFileName = ConfigurationManager.AppSettings["ConfigFileName"];
        private string sSettingPath = ConfigurationManager.AppSettings["SettingsPath"];
        XmlDocument doc = new XmlDocument();
        string sXmlFile = "";
        bool bServer = false;
        bool bServer_Edit = false;
        bool bEnvironment = false;
        bool bEnvironment_Edit = false;

        public Settings()
        {
            InitializeComponent();

            InitializeTreeViewEvents();
            
            //sXmlFile = "..\\..\\" + ConfigFileName;
            sXmlFile = ConfigFileName;

            //tView_Servers.ImageList = tvServers_imageList;
            tView_Servers.SetBounds(5, 18, 140, 280);

            DisableServerDetails();
            DisableEnvironmentDetails();
            
            try
            {
                //XmlDocument doc = new XmlDocument();
                doc.Load(sXmlFile);

                tView_Servers.Nodes.Clear();
                tView_Servers.Nodes.Add(new TreeNode(doc.DocumentElement.Name));
                
                TreeNode tNode = new TreeNode();
                tNode = tView_Servers.Nodes[0];

                AddNode(doc.DocumentElement, tNode);
                tView_Servers.ExpandAll();               

            }
            catch(XmlException xmlex)
            {
                MessageBox.Show(xmlex.Message);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;


            if(inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for( i = 0; i <= inXmlNode.ChildNodes.Count - 1; i++ )
                {
                    xNode = inXmlNode.ChildNodes[i];

                    if (xNode.Name == "Server")
                    {
                        inTreeNode.Nodes.Add(new TreeNode(xNode.Attributes["name"].Value));

                        tNode = inTreeNode.Nodes[i];
                        AddNode(xNode, tNode);
                    }
                    else
                    {
                        if(xNode.InnerText != null && xNode.InnerText != "")
                            inTreeNode.Nodes.Add(new TreeNode(xNode.InnerText));
                        //inTreeNode.ImageIndex = 0;
                        //tView_Servers.SelectedNode.ImageIndex = 0;
                    }
                }
            }
            else
            {
                inTreeNode.Text = (inXmlNode.OuterXml).Trim();
                //tView_Servers.SelectedNode.ImageIndex = 1;
            }
        }

        private void btn_BackupDir_Click(object sender, EventArgs e)
        {

        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            if (btn_Apply.Enabled)
            {
                string strMsg = String.Format("Are you sure you want to close this window without applying pending changes?");

                if (MessageBox.Show(strMsg, "Close Window", MessageBoxButtons.YesNo) != DialogResult.No)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        void tView_Servers_NodeMouseClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            /* Now using global variable
            XmlDocument doc = new XmlDocument();
            doc.Load(sXmlFile);
             */

            XmlNode nNode;
            XmlNode pNode;

            if (e.Node.Parent != null)
            {
                if(e.Node.Text == "New Server")
                {
                    tBox_ServerName.Text = "";
                    tBox_Instance.Text = "";
                    tBox_IPaddress.Text = "";
                    tBox_Username.Text = "";
                    tBox_Password.Text = "";
                    tBox_BackupLocation.Text = "";

                    tBox_Environment.Text = "";
                    tBox_DataFile.Text = "";
                    tBox_LogFiles.Text = "";
                }
                else if ( e.Node.Parent.Text == "New Environment")
                {
                    tBox_Environment.Text = "";
                    tBox_DataFile.Text = "";
                    tBox_LogFiles.Text = "";

                    nNode = doc.SelectSingleNode("/Servers/Server[@name='" + e.Node.Parent.Text + "']");

                    tBox_ServerName.Text = nNode.Attributes["name"].Value;
                    tBox_Instance.Text = nNode.Attributes["instance"].Value;
                    tBox_IPaddress.Text = nNode.Attributes["ip"].Value;
                    tBox_Username.Text = nNode.Attributes["user"].Value;
                    tBox_Password.Text = nNode.Attributes["password"].Value;
                    tBox_BackupLocation.Text = nNode.Attributes["backups"].Value;
                }
                else if ( e.Node.Parent.FullPath == "Servers")
                {
                    tBox_Environment.Text = "";
                    tBox_DataFile.Text = "";
                    tBox_LogFiles.Text = "";

                    nNode = doc.SelectSingleNode("/Servers/Server[@name='" + e.Node.Text + "']");

                    tBox_ServerName.Text = nNode.Attributes["name"].Value;
                    tBox_Instance.Text = nNode.Attributes["instance"].Value;
                    tBox_IPaddress.Text = nNode.Attributes["ip"].Value;
                    tBox_Username.Text = nNode.Attributes["user"].Value;
                    tBox_Password.Text = nNode.Attributes["password"].Value;
                    tBox_BackupLocation.Text = nNode.Attributes["backups"].Value;
                }
                else
                {
                    nNode = doc.SelectSingleNode("/Servers/Server/Environment[@name='" + e.Node.Text + "']");
                    pNode = nNode.ParentNode;

                    tBox_ServerName.Text = pNode.Attributes["name"].Value;
                    tBox_Instance.Text = pNode.Attributes["instance"].Value;
                    tBox_IPaddress.Text = pNode.Attributes["ip"].Value;
                    tBox_Username.Text = pNode.Attributes["user"].Value;
                    tBox_BackupLocation.Text = pNode.Attributes["backups"].Value;

                    tBox_Environment.Text = e.Node.Text;
                    tBox_DataFile.Text = nNode.Attributes["data"].Value;
                    tBox_LogFiles.Text = nNode.Attributes["log"].Value;
                }
            }
        }

        void tView_Servers_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
           if(e.Button == MouseButtons.Right)
           {
               tView_Servers.SelectedNode = tView_Servers.GetNodeAt(e.X, e.Y);

               int x = tView_Servers.SelectedNode.Bounds.X + tView_Servers.SelectedNode.Bounds.Width;
               int y = tView_Servers.SelectedNode.Bounds.Y + tView_Servers.SelectedNode.Bounds.Width;
               Point nodePos = new Point(x, y);

               if (tView_Servers.SelectedNode != null)
               {
                   ContextMenu cm = new ContextMenu();

                   if ( tView_Servers.SelectedNode.Parent == null)
                   {
                       AddMenuItem(cm, "New Server");
                   }
                   else if (tView_Servers.SelectedNode.Parent.Text == "Servers" || tView_Servers.SelectedNode.Parent.Text == "<Servers></Servers>")
                   {
                       AddMenuItem(cm, "New Environment");
                       AddMenuItem(cm, "Edit Server");
                       AddMenuItem(cm, "Remove Server");
                   }
                   else if (tView_Servers.SelectedNode.Parent.Text != "Server" && tView_Servers.SelectedNode.Parent != null)
                   {
                       AddMenuItem(cm, "Edit Environment");
                       AddMenuItem(cm, "Remove Environment");
                   }

                   cm.Show(this, nodePos);
               }
           }
        }

        void tView_Servers_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
        {
            this.BeginInvoke(new Action(() => afterAfterEdit(e.Node)));
        }

        private void afterAfterEdit(TreeNode node)
        {
            if (tView_Servers.SelectedNode.Parent.Text == "Servers")
            {
                tBox_ServerName.Text = node.Text;
                //bServer = true;
            }
            else
            {
                tBox_Environment.Text = node.Text;
                //bEnvironment = true;
            }

            btn_Apply.Enabled = true;
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

            if(m.Text.ToLower() == "new server")
            {
                bServer = true;
                actionText = "New Server";
                //string newSrvName = MessageBox("New Server");
                TreeNode newNode = new TreeNode("New Server");

                tView_Servers.SelectedNode.Nodes.Add(newNode);
                tView_Servers.SelectedNode = newNode;
                tView_Servers.LabelEdit = true;

                if(!newNode.IsEditing)
                {
                    newNode.BeginEdit();
                    EnableServerDetails();
                }
                
            }
            else if (m.Text.ToLower() == "new environment")
            {
                bEnvironment = true;
                actionText = "New Environment";

                TreeNode newEnvNode = new TreeNode("New Environment");

                tView_Servers.SelectedNode.Nodes.Add(newEnvNode);
                tView_Servers.SelectedNode = newEnvNode;
                tView_Servers.LabelEdit = true;

                if (!newEnvNode.IsEditing)
                {
                    newEnvNode.BeginEdit();
                    EnableEnvironmentDetails();
                }
            }
            else if (m.Text.ToLower() == "edit server")
            {
                bServer_Edit = true;
                actionText = "Edit Server Click";
                EnableServerDetails();
                //btn_Apply.Enabled = true;
            }
            else if (m.Text.ToLower() == "remove server")
            {
                string strMsg = String.Format("Are you sure you want to remove the Server : {0} and all its environment?", tView_Servers.SelectedNode.Text);
                actionText = tView_Servers.SelectedNode.Text;
                if (MessageBox.Show(strMsg, "Close Application", MessageBoxButtons.YesNo) != DialogResult.No)
                {
                    tView_Servers.Nodes.Remove(tView_Servers.SelectedNode);
                    RemoveItemFromXml("Server", actionText);
                }
            }
            else if (m.Text.ToLower() == "edit environment")
            {
                bEnvironment_Edit = true;
                actionText = "Edit Environment Click";
                EnableEnvironmentDetails();
                //btn_Apply.Enabled = true;
            }
            else if (m.Text.ToLower() == "remove environment")
            {
                string strMsg = String.Format("This will remove {0} environment from your configuration. Confirm?", tView_Servers.SelectedNode.Text);
                actionText = "Remove Environment clicked";
                if (MessageBox.Show(strMsg, "Close Application", MessageBoxButtons.YesNo) != DialogResult.No)
                {
                    string toDelete = tView_Servers.SelectedNode.Text;
                    //e.Cancel = true;
                    tView_Servers.Nodes.Remove(tView_Servers.SelectedNode);

                    XDocument xdoc = XDocument.Load(sXmlFile);
                    var q = from node in xdoc.Descendants("Environment")
                            let attr = node.Attribute("name")
                            where attr != null && attr.Value == toDelete
                            select node;
                    q.ToList().ForEach(x => x.Remove());
                    xdoc.Save(sXmlFile);
                    MessageBox.Show("Removed " + toDelete);
                }
            }

           // MessageBox.Show(actionText);
        }

        private void RemoveItemFromXml(string nType, string itemToDelete)
        {
            XDocument xdoc = XDocument.Load(sXmlFile);
            var q = from node in xdoc.Descendants(nType)
                    let attr = node.Attribute("name")
                    where attr != null && attr.Value == itemToDelete
                    select node;
            q.ToList().ForEach(x => x.Remove());
            xdoc.Save(sXmlFile);
            MessageBox.Show("Removed " + itemToDelete);
        }

        private void EnableServerDetails()
        {
           // tBox_ServerName.Enabled = true;
            tBox_Instance.Enabled = true;
            tBox_IPaddress.Enabled = true;
            tBox_Username.Enabled = true;
            tBox_Password.Enabled = true;
            tBox_BackupLocation.Enabled = true;

            //if (!btn_Apply.Enabled)
            //    btn_Apply.Enabled = false;
        }

        private void DisableServerDetails()
        {
            tBox_ServerName.Enabled = false;
            tBox_Instance.Enabled = false;
            tBox_IPaddress.Enabled = false;
            tBox_Username.Enabled = false;
            tBox_Password.Enabled = false;
            tBox_BackupLocation.Enabled = false;

            //if (btn_Apply.Enabled)
            btn_Apply.Enabled = false;
        }

        private void EnableEnvironmentDetails()
        {
           // tBox_Environment.Enabled = true;
            tBox_DataFile.Enabled = true;
            tBox_LogFiles.Enabled = true;

            //if (!btn_Apply.Enabled)
            //    btn_Apply.Enabled = true;
        }

        private void DisableEnvironmentDetails()
        {
            tBox_Environment.Enabled = false;
            tBox_DataFile.Enabled = false;
            tBox_LogFiles.Enabled = false;
            btn_Apply.Enabled = false;
        }

        private void btn_Apply_Click(object sender, EventArgs e)
        {
            string nodeType = "";
            /* Now using a global variable.
            XmlDocument doc2 = new XmlDocument();
             */
            doc.Load(sXmlFile);

            XmlNode root = doc.DocumentElement;

            //if (bServer)
            //{
            //    nodeType = "Server";
            //}
            //else
            //{
            //    nodeType = "Environemnt";
            //}

            //Create a new node.
            XmlElement elem = doc.CreateElement("Server");

            //Create Attributes.
            XmlAttribute name = doc.CreateAttribute("name");
            name.Value = tBox_ServerName.Text;
            XmlAttribute instance = doc.CreateAttribute("instance");
            instance.Value = tBox_Instance.Text;
            XmlAttribute ip = doc.CreateAttribute("ip");
            ip.Value = tBox_IPaddress.Text;
            XmlAttribute user = doc.CreateAttribute("user");
            user.Value = tBox_Username.Text;
            XmlAttribute password = doc.CreateAttribute("password");
            password.Value = tBox_Password.Text;
            XmlAttribute backups = doc.CreateAttribute("backups");
            backups.Value = tBox_BackupLocation.Text;

            // Create and empty Environment Node under new Server.
            XmlElement elemEnv = doc.CreateElement("Environment");
            elemEnv.InnerText = tBox_Environment.Text;

            //Create Blank Attributes for blank Environment Node.
            XmlAttribute EnvName = doc.CreateAttribute("name");
            EnvName.Value = tBox_Environment.Text;
            XmlAttribute data = doc.CreateAttribute("data");
            data.Value = tBox_DataFile.Text;
            XmlAttribute log = doc.CreateAttribute("log");
            log.Value = tBox_LogFiles.Text;


            elem.Attributes.Append(name);
            elem.Attributes.Append(instance);
            elem.Attributes.Append(ip);
            elem.Attributes.Append(user);
            elem.Attributes.Append(password);
            elem.Attributes.Append(backups);

            elemEnv.Attributes.Append(EnvName);
            elemEnv.Attributes.Append(data);
            elemEnv.Attributes.Append(log);            

            if (bServer)
            {
                //Add the node to the document.
                root.InsertAfter(elem, root.LastChild);
                bServer = false;
            }
            else if ( bServer_Edit )
            {
                XmlNode oldElem = doc.SelectSingleNode("//Server[@name='" + tBox_ServerName.Text + "']");
                oldElem.Attributes["instance"].Value = tBox_Instance.Text;
                oldElem.Attributes["ip"].Value = tBox_IPaddress.Text;
                oldElem.Attributes["user"].Value = tBox_Username.Text;
                oldElem.Attributes["password"].Value = tBox_Password.Text;
                oldElem.Attributes["backups"].Value = tBox_BackupLocation.Text;
               // root.ReplaceChild(elem, oldElem);
                bServer_Edit = false;
            }
            else if ( bEnvironment )
            {
                bEnvironment = false;
            }
            else if ( bEnvironment_Edit)
            {
                XmlNode envNode = doc.SelectSingleNode("//Environment[@name='" + tBox_Environment.Text + "']");
                envNode.Attributes["data"].Value = tBox_DataFile.Text;
                envNode.Attributes["log"].Value = tBox_LogFiles.Text;
                bEnvironment = false;
            }
            
            //XmlNode elemServ = doc.SelectSingleNode("//Server[@name='" + tBox_ServerName.Text + "']");
           
            //elemServ.AppendChild(elemEnv);

            doc.Save(sXmlFile);

            btn_Apply.Enabled = false;
            DisableServerDetails();
            DisableEnvironmentDetails();
            
        }

        private void tBox_Instance_TextChanged(object sender, EventArgs e)
        {
            if (tBox_Instance.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

        private void tBox_IPaddress_TextChanged(object sender, EventArgs e)
        {
            if (tBox_IPaddress.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

        private void tBox_Username_TextChanged(object sender, EventArgs e)
        {
            if (tBox_Username.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

        private void tBox_Password_TextChanged(object sender, EventArgs e)
        {
            if(tBox_Password.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

        private void tBox_BackupLocation_TextChanged(object sender, EventArgs e)
        {
            if(tBox_BackupLocation.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

        private void tBox_DataFile_TextChanged(object sender, EventArgs e)
        {
            if(tBox_DataFile.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }

        private void tBox_LogFiles_TextChanged(object sender, EventArgs e)
        {
            if(tBox_LogFiles.Enabled && !btn_Apply.Enabled)
                btn_Apply.Enabled = true;
        }
    }
}
