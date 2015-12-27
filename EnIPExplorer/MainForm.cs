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
using System.Threading;
using System.Globalization;

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

            Size s = Properties.Settings.Default.GUI_FormSize;
            if (s!= Size.Empty)
                this.Size = s;

        }

        // Each time we received a reponse to udp brodcast ou unicast ListIdentity
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
            TreeNode tn = new TreeNode(device.IPString() + " - " + device.ProductName, 0, 0);
            tn.Tag = device;
            devicesTreeView.Nodes.Add(tn);

        }

        // fit an Icon according to the selected node
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

        // A new Class inside the Treeview : name & icon
        private TreeNode ClassToTreeNode(EnIPClass Class)
        {
            TreeNode tn;

            if (Enum.IsDefined(typeof(CIPObjectLibrary), Class.Id))
            {
                CIPObjectLibrary cipobj = (CIPObjectLibrary)Class.Id;
                int img = Classe2Ico(cipobj);
                tn = new TreeNode(cipobj.ToString(), img, img);

                if ((Class.Id == 1) || (Class.Id == 2) || (Class.Id == 0xF5) || (Class.Id == 0xF6))
                {
                    EnIPClassInstance instance = new EnIPClassInstance(Class, 1);
                    TreeNode tnI = new TreeNode("Instance #1", 9, 9);
                    tnI.Tag = instance;
                    tn.Nodes.Add(tnI);

                }
            }
            else
            {
                tn = new TreeNode("Proprietary " + Class.Id.ToString() + " (0x" + Class.Id.ToString("X2") + ")", 1, 1);
            }

            tn.Tag = Class;

            return tn;
        }

        // All selection on the TreeView
        // Popup menu adaptation
        // Generate network activity to read the selected element
        private void devicesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

            // A Device : top level
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
                    e.Node.Nodes.Add(ClassToTreeNode(clId));
                }
                e.Node.Expand();

                popupAddAToolStripMenuItem.Visible = true;
            }
            // A Class
            else if (e.Node.Tag is EnIPClass)
            {
                EnIPClass EnClass = (EnIPClass)e.Node.Tag;
                EnClass.GetClassData();
                propertyGrid.SelectedObject = EnClass;
                propertyGrid.ExpandAllGridItems();

                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;

            }
            // An Instance
            else if (e.Node.Tag is EnIPClassInstance)
            {
                EnIPClassInstance Instance = (EnIPClassInstance)e.Node.Tag;
                Instance.GetClassInstanceData();
                propertyGrid.SelectedObject = Instance;
                propertyGrid.ExpandAllGridItems();

                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = true;
            }
            // An Attribut
            else if (e.Node.Tag is EnIPInstanceAttribut)
            {
                EnIPInstanceAttribut Att = (EnIPInstanceAttribut)e.Node.Tag;
                Att.GetInstanceAttributData();
                propertyGrid.SelectedObject = Att;
                propertyGrid.ExpandAllGridItems();

                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = true;
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string readme_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(MainForm).Assembly.Location), "README.txt");
            try { System.Diagnostics.Process.Start(readme_path); } catch { }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Ethernet/IP Explorer - EnIPExplorer\nVersion Alpha " + this.GetType().Assembly.GetName().Version + "\nBy Frederic Chaxel - Copyright 2016\n" +
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
            if (client == null) return;
            client.DiscoverServers();
        }

        private void openInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                var Input =
                    new GenericInputBox<ComboBox>("Local Interface", "IP address",
                         (o) =>
                         {
                             string[] local_endpoints = GetAvailableIps();
                             o.Items.AddRange(local_endpoints);
                         });

                DialogResult res = Input.ShowDialog();

                if (res != DialogResult.OK) return;
                String userinput = Input.genericInput.Text;

                try
                {
                    client = new EnIPClient(userinput, Properties.Settings.Default.TCP_LAN_Timeout);
                    client.DeviceArrival += new DeviceArrivalHandler(On_DeviceArrival);

                    client.DiscoverServers();

                    openInterfaceToolStripMenuItem.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("Local address unavailable", "Error in Open Interface", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static string[] GetAvailableIps()
        {
            List<string> ips = new List<string>();
            System.Net.NetworkInformation.NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (System.Net.NetworkInformation.NetworkInterface inf in interfaces)
            {
                if (!inf.IsReceiveOnly && inf.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && inf.SupportsMulticast && inf.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                {
                    System.Net.NetworkInformation.IPInterfaceProperties ipinfo = inf.GetIPProperties();
                    if (ipinfo.GatewayAddresses == null || ipinfo.GatewayAddresses.Count == 0 || (ipinfo.GatewayAddresses.Count == 1 && ipinfo.GatewayAddresses[0].Address.ToString() == "0.0.0.0")) continue;
                    foreach (System.Net.NetworkInformation.UnicastIPAddressInformation addr in ipinfo.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ips.Add(addr.Address.ToString());
                        }
                    }
                }
            }
            return ips.ToArray();
        }

        private void addClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPRemoteDevice) break;
            }

            int Numbase = 1;
            foreach (TreeNode t in tn.Nodes)
            {
                int num = (t.Tag as EnIPClass).Id;
                Numbase = Math.Max(num + 1, Numbase);
            }

            var Input =
                new GenericInputBox<NumericUpDown>("Add Class", "Class Id :",
                     (o) =>
                     {
                         o.Minimum = 1; o.Maximum = 65535; o.Value = Numbase;
                     });

            DialogResult res = Input.ShowDialog();

            if (res == DialogResult.OK)
            {
                byte Id = (byte)Input.genericInput.Value;
                EnIPClass Class = new EnIPClass(tn.Tag as EnIPRemoteDevice, Id);
                tn.Nodes.Add(ClassToTreeNode(Class));
                tn.Expand();
            }
        }

        private void addClassInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPClass) break;
            }

            int Numbase = 1;
            foreach (TreeNode t in tn.Nodes)
            {
                int num = (t.Tag as EnIPClassInstance).Id;
                Numbase = Math.Max(num + 1, Numbase);
            }

            var Input =
                new GenericInputBox<NumericUpDown>("Add Instance", "Instance Id :",
                     (o) =>
                     {
                         o.Minimum = 1; o.Maximum = 255; o.Value = Numbase;
                     });

            DialogResult res = Input.ShowDialog();

            if (res == DialogResult.OK)
            {
                byte Id = (byte)Input.genericInput.Value;
                EnIPClassInstance instance = new EnIPClassInstance(tn.Tag as EnIPClass, Id);
                TreeNode tnI = new TreeNode("Instance #"+Id.ToString(), 9, 9);
                tnI.Tag = instance;
                tn.Nodes.Add(tnI);
                tn.Expand();
            }

        }

        private void addInstanceAttributToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPClassInstance) break;
            }

            int Numbase = 1;
            foreach (TreeNode t in tn.Nodes)
            {
                int num = (t.Tag as EnIPInstanceAttribut).Id;
                Numbase = Math.Max(num + 1, Numbase);
            }

            var Input =
                new GenericInputBox<NumericUpDown>("Add Attribut", "Attribut Id :",
                     (o) =>
                     {
                         o.Minimum = 1; o.Maximum = 255; o.Value = Numbase;
                     });

            DialogResult res = Input.ShowDialog();

            if (res == DialogResult.OK)
            {
                byte Id = (byte)Input.genericInput.Value;
                EnIPInstanceAttribut att = new EnIPInstanceAttribut(tn.Tag as EnIPClassInstance, Id);
                TreeNode tnI = new TreeNode("Attribut #"+Id.ToString(), 8, 8);
                tnI.Tag = att;
                tn.Nodes.Add(tnI);
                tn.Expand();
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
                Properties.Settings.Default.GUI_FormSize = this.Size;
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        // change in properties Grid, a Byte in the Raw Data 
        // send it to the device if it's an attribut value
        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if ((e.ChangedItem.Parent != null) && (e.ChangedItem.Parent.Label == "RawData") && (devicesTreeView.SelectedNode.Tag is EnIPInstanceAttribut))
            {
                EnIPInstanceAttribut v = (EnIPInstanceAttribut)devicesTreeView.SelectedNode.Tag;
                if (v.SetInstanceAttributData()==true)
                    Trace.WriteLine("Write OK");
            }
            else
                Trace.WriteLine("Modifications are not taken into account at this level");
        }

        // Add a new device not discovery using the broadcast technic : outside the local net
        private void addRemoteDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (client==null) return;

            var Input =
                new GenericInputBox<TextBox>("Remote device", "IP address",
                     (o) =>
                     {
                     });

            DialogResult res = Input.ShowDialog();

            if (res != DialogResult.OK) return;
        }

        // Delete a Device, a class, an instance, an attribut
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;
            if (tn == null) return;

            DialogResult deleteOK = DialogResult.OK;

            if (tn.Tag is EnIPRemoteDevice)
            {
                // confirm
                if (Properties.Settings.Default.ConfirmDeleteDevice)
                    deleteOK = MessageBox.Show("Delete this device", tn.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (deleteOK == DialogResult.OK)
                {
                    EnIPRemoteDevice device=(tn.Tag as EnIPRemoteDevice);
                    
                    device.Disconnect();                    // close the tcp connection
                    devicesTreeView.Nodes.Remove(tn);       // remove from the tree
                    servers.Remove(device);                 // remove from the list
                }
            }
            else
            {
                // confirm
                if (Properties.Settings.Default.ConfirmDeleteOthers)
                    deleteOK = MessageBox.Show("Delete", tn.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (deleteOK == DialogResult.OK)
                    devicesTreeView.Nodes.Remove(tn);   // only remove from the list
            }
        }

        // Remove the Log content
        private void LogText_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LogText.Text = "";
        }

        #region PopupMenu

        private void popupDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem_Click(null, null);
        }

        private void popupAddCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addClassToolStripMenuItem_Click(null, null);
        }

        private void popupAddIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addClassInstanceToolStripMenuItem_Click(null, null);
        }

        private void popupAddAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addInstanceAttributToolStripMenuItem_Click(null, null);
        }

        #endregion
    }

    // Coming from Yabe @ Sourceforge 
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
