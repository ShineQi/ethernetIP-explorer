/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2016 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.EnIPStack;

namespace EnIPExplorer
{
    public partial class MainForm : Form
    {
        EnIPClient client;
        List<EnIPRemoteDevice> servers = new List<EnIPRemoteDevice>();

        public MainForm()
        {
            InitializeComponent();
            Trace.Listeners.Add(new MyTraceListener(this));           
        }

        void On_DeviceArrival(EnIPRemoteDevice device)
        {
            if (InvokeRequired)
            {
                Invoke(new DeviceArrivalHandler(On_DeviceArrival), new object[] { device });
                return;
            }

            foreach (EnIPRemoteDevice server in servers)
                if (server.Equals(device)) return;

            servers.Add(device);
            TreeNode tn = new TreeNode(device.ep.Address.ToString() + " - " + device.ProductName, 0, 0);
            tn.Tag = device;
            devicesTreeView.Nodes.Add(tn);

        }

        private int Classe2Ico(CIPObjectLibrary clId)
        {

            switch (clId)
            {
                case CIPObjectLibrary.Identity:
                    return 3;
                case CIPObjectLibrary.MessageRouter:
                    return 4;
                case CIPObjectLibrary.ConnectionManager:
                    return 7;
                case CIPObjectLibrary.Port:
                case CIPObjectLibrary.TCPIPInterface:
                case CIPObjectLibrary.EtherNetLink:
                case CIPObjectLibrary.ControlNet:
                case CIPObjectLibrary.DeviceNet:
                case CIPObjectLibrary.Modbus:
                    return 5;
                case CIPObjectLibrary.Assembly :
                    return 6;
                default: return 2;
            }
            

        }

        private void devicesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is EnIPRemoteDevice)
            {
                EnIPRemoteDevice device = (EnIPRemoteDevice)e.Node.Tag;

                propertyGrid.SelectedObject = device;

                // already done
                if (e.Node.Nodes.Count != 0) return;

                if (device.SupportedClassLists.Count == 0)
                {
                    if (device.IsConnected() == false) device.Connect();
                    device.GetObjectList();
                }

                foreach (EnIPClass clId in device.SupportedClassLists)
                {
                    TreeNode tn;

                    if (Enum.IsDefined(typeof(CIPObjectLibrary), clId.Id))
                    {
                        CIPObjectLibrary cipobj = (CIPObjectLibrary)clId.Id;
                        int img = Classe2Ico(cipobj);
                        tn = new TreeNode(cipobj.ToString(), img, img);

                        if ((clId.Id == 1) || (clId.Id == 2) || (clId.Id == 0xF5) || (clId.Id == 0xF6))
                        {
                            EnIPClassInstance instance = new EnIPClassInstance(clId, 1);
                            TreeNode tnI = new TreeNode("Instance #1", 9, 9);
                            tnI.Tag = instance;
                            tn.Nodes.Add(tnI);

                        }
                    }
                    else
                    {
                        tn = new TreeNode("Proprietary " + clId.Id.ToString() + " (0x" + clId.Id.ToString("X2") + ")", 1, 1);
                    }

                    tn.Tag = clId;

                    e.Node.Nodes.Add(tn);
                }
            }
            else if (e.Node.Tag is EnIPClass)
            {
                EnIPClass EnClass = (EnIPClass)e.Node.Tag;
                EnClass.GetClassData();
                propertyGrid.SelectedObject = EnClass;
            }
            else if (e.Node.Tag is EnIPClassInstance)
            {
                EnIPClassInstance Instance = (EnIPClassInstance)e.Node.Tag;
                Instance.GetClassInstanceData();
                propertyGrid.SelectedObject = Instance;
            }
            else if (e.Node.Tag is EnIPInstanceAttribut)
            {
                EnIPInstanceAttribut Att = (EnIPInstanceAttribut)e.Node.Tag;
                Att.GetInstanceAttributData();
                propertyGrid.SelectedObject = Att;
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string readme_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(MainForm).Assembly.Location), "README.txt");
            try { System.Diagnostics.Process.Start(readme_path); } catch { }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Ethernet/IP Explorer - EnIPExplorer\nVersion " + this.GetType().Assembly.GetName().Version + "\nBy Frederic Chaxel - Copyright 2016\n" +
                "\nReference: http://sourceforge.net/projects/EnIPExplorer" +
                "\nReference: http://sourceforge.net/projects/yetanotherbacnetexplorer/" +
                "\nReference: http://www.famfamfam.com/"+
                "\nReference: http://www.jrsoftware.org/isinfo.php"
                , "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void sendListIdentityDiscoverToolStripMenuItem_Click(object sender, EventArgs e)
        {
                client.DiscoverServers();
        }

        private void openInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                client = new EnIPClient("");
                client.DeviceArrival += new DeviceArrivalHandler(On_DeviceArrival);

                client.DiscoverServers();

                openInterfaceToolStripMenuItem.Enabled = false;
                sendListIdentityDiscoverToolStripMenuItem.Enabled = true;
            }
        }

        private void addClassInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TreeNode tn = devicesTreeView.SelectedNode;

            if (tn == null) return;

            if (!(tn.Tag is EnIPClass)) return;

            GetId form = new GetId("Instance Id :");
            DialogResult res = form.ShowDialog();
            if (res == DialogResult.OK)
            {
                byte Id = (byte)form.Id.Value;
                EnIPClassInstance instance = new EnIPClassInstance(tn.Tag as EnIPClass, Id);
                TreeNode tnI = new TreeNode("Instance #"+Id.ToString(), 9, 9);
                tnI.Tag = instance;
                tn.Nodes.Add(tnI);
            }

        }

        private void addInstanceAttributToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;

            if (tn == null) return;

            if (!(tn.Tag is EnIPClassInstance)) return;

            GetId form = new GetId("Attribut Id :");
            DialogResult res = form.ShowDialog();
            if (res == DialogResult.OK)
            {
                byte Id = (byte)form.Id.Value;
                EnIPInstanceAttribut att = new EnIPInstanceAttribut(tn.Tag as EnIPClassInstance, Id);
                TreeNode tnI = new TreeNode("Attribut #"+Id.ToString(), 8, 8);
                tnI.Tag = att;
                tn.Nodes.Add(tnI);
            }

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog dlg = new SettingsDialog();
            dlg.SelectedObject = Properties.Settings.Default;
            dlg.ShowDialog(this);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch { }
        }

    }

    public class MyTraceListener : TraceListener
    {
        private MainForm m_form;

        public MyTraceListener(MainForm form)
        {
            m_form = form;
        }

        public override void Write(string message)
        {
            if (!m_form.IsHandleCreated) return;

            m_form.BeginInvoke((MethodInvoker)delegate { m_form.LogText.AppendText(message); });
        }

        public override void WriteLine(string message)
        {
            if (!m_form.IsHandleCreated) return;

            m_form.BeginInvoke((MethodInvoker)delegate { m_form.LogText.AppendText(message+ Environment.NewLine); });
        }
    }
}
