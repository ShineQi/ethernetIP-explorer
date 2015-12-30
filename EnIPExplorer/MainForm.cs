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
using System.Net;
using System.IO;

namespace EnIPExplorer
{
    public partial class MainForm : Form
    {
        EnIPClient client;

        public MainForm()
        {
            InitializeComponent();
            Trace.Listeners.Add(new MyTraceListener(this));

            Size s = Properties.Settings.Default.GUI_FormSize;
            if (s!= Size.Empty)
                this.Size = s;
            this.WindowState = Properties.Settings.Default.GUI_State;

            if (Properties.Settings.Default.DefaultTreeFile == "")
                Properties.Settings.Default.DefaultTreeFile = Application.StartupPath+"\\SampleTree.csv";
        }

        // Each time we received a response to udp broadcast or udp/tcp unicast ListIdentity
        void On_DeviceArrival(EnIPRemoteDevice device)
        {
            if (InvokeRequired)
            {
                Invoke(new DeviceArrivalHandler(On_DeviceArrival), new object[] { device });
                return;
            }

            // Device already present ?
            foreach (TreeNode tn in devicesTreeView.Nodes)
            {
                EnIPRemoteDevice oldRemoteDevice = (EnIPRemoteDevice)tn.Tag;
                if (oldRemoteDevice.Equals(device))
                {
                    oldRemoteDevice.CopyData(device);
                    // change the icon if needed
                    if (tn.SelectedImageIndex != 0)
                        tn.ImageIndex = tn.SelectedImageIndex = 0;
                    // change the Text maybe
                    tn.Text = device.IPString() + " - " + device.ProductName;
                    
                    if (devicesTreeView.SelectedNode == tn) devicesTreeView.SelectedNode = null;

                    return;
                }
            }

            TreeNode tn2 = new TreeNode(device.IPString() + " - " + device.ProductName, 0, 0);
            tn2.Tag = device;
            devicesTreeView.Nodes.Add(tn2);

            // Queue connection
            ThreadPool.QueueUserWorkItem((o) => device.Connect());
        }

        // fit an Icon according to the selected node
        private int Classe2Ico(CIPObjectLibrary clId)
        {
            switch (clId)
            {
                case CIPObjectLibrary.Identity:
                    return 4;
                case CIPObjectLibrary.MessageRouter:
                    return 5;
                case CIPObjectLibrary.ConnectionManager:
                    return 8;
                case CIPObjectLibrary.Port:
                case CIPObjectLibrary.TCPIPInterface:
                case CIPObjectLibrary.EtherNetLink:
                case CIPObjectLibrary.ControlNet:
                case CIPObjectLibrary.DeviceNet:
                case CIPObjectLibrary.Modbus:
                    return 6;
                case CIPObjectLibrary.Assembly :
                    return 7;
                default: return 3;
            }
        }

        // A new Class inside the Treeview : name & icon
        private TreeNode ClassToTreeNode(EnIPClass Class)
        {
            TreeNode tn;

            // Known classes
            if (Enum.IsDefined(typeof(CIPObjectLibrary), Class.Id))
            {
                CIPObjectLibrary cipobj = (CIPObjectLibrary)Class.Id;
                int img = Classe2Ico(cipobj);
                tn = new TreeNode(cipobj.ToString()+" #"+ Class.Id.ToString(), img, img);

                // Special classes with the known instance(s)
                if ((Class.Id == 1) || (Class.Id == 2) || (Class.Id == 0xF4) || (Class.Id == 0xF5) || (Class.Id == 0xF6))
                {
                    EnIPClassInstance instance = new EnIPClassInstance(Class, 1);
                    TreeNode tnI = new TreeNode("Instance #1", 9, 9);
                    tnI.Tag = instance;
                    tn.Nodes.Add(tnI);
                }
            }
            else
                tn = new TreeNode("Proprietary #" + Class.Id.ToString(), 2, 2);

            tn.Tag = Class;
            return tn;
        }

        // Put the Online of Offline icon to the RemoteDevice in the TreeView
        private void CurrentRemoteDeviceIcon(EnIPNetworkStatus status)
        {
            TreeNode tn = devicesTreeView.SelectedNode;
            while (tn.Parent != null)
                tn = tn.Parent;

            if ((tn.SelectedImageIndex == 0) && (status == EnIPNetworkStatus.OffLine))
                tn.ImageIndex = tn.SelectedImageIndex = 1;
            else if ((tn.SelectedImageIndex != 0) && (status != EnIPNetworkStatus.OffLine))
                tn.ImageIndex = tn.SelectedImageIndex = 0;

        }

        // All selection on the TreeView.
        // Generate network activity to read the selected element.
        private void devicesTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            devicesTreeView.SelectedNode=e.Node;
        }

        private void devicesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (client == null) return;

            EnIPNetworkStatus ReadRet = EnIPNetworkStatus.OnLine;

            Cursor Memcurs = this.Cursor;

            this.Cursor = Cursors.WaitCursor;

            // It's a Device : top level
            if (e.Node.Tag is EnIPRemoteDevice)
            {
                EnIPRemoteDevice device = (EnIPRemoteDevice)e.Node.Tag;

                propertyGrid.SelectedObject = device;

                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = false;
                popupAddAToolStripMenuItem.Visible = false;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Device";

                if (device.SupportedClassLists.Count == 0) // certainly never discovers
                {
                    if (device.IsConnected() == false)
                    {
                        device.Connect();
                        if (device.IsConnected() == false)
                        {
                            propertyGrid.Enabled = false;
                            CurrentRemoteDeviceIcon(EnIPNetworkStatus.OffLine);
                            this.Cursor = Memcurs;
                            return;
                        }
                    }

                    // never discovers
                    if (device.DataLength == 0)
                        device.DiscoverServer();

                    device.GetObjectList();
                    propertyGrid.Enabled = true;
                }

                // change the Text maybe
                String txt = device.IPString() + " - " + device.ProductName;
                if (e.Node.Text!=txt)
                    e.Node.Text = txt;

                foreach (EnIPClass clId in device.SupportedClassLists)
                {
                    bool alreadyexist = false;
                    foreach (TreeNode tn in e.Node.Nodes)
                        if ((tn.Tag as EnIPClass).Id == clId.Id)
                        {
                            alreadyexist = true;
                            break;
                        }

                    if (!alreadyexist)
                        e.Node.Nodes.Add(ClassToTreeNode(clId));                    
                }
                e.Node.Expand();
            }

            // It's a Class
            else if (e.Node.Tag is EnIPClass)
            {
                // Read it from the remote devie
                EnIPClass EnClass = (EnIPClass)e.Node.Tag;
                ReadRet=EnClass.GetClassData();
                // In the Grid
                propertyGrid.SelectedObject = EnClass;
                propertyGrid.ExpandAllGridItems();
                // Popup menu adaptation
                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = false;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Class";

            }
            // It's an Instance
            else if (e.Node.Tag is EnIPClassInstance)
            {
                // Read it from the remote devie
                EnIPClassInstance Instance = (EnIPClassInstance)e.Node.Tag;
                ReadRet=Instance.GetClassInstanceData();
                // In the Grid
                propertyGrid.SelectedObject = Instance;
                propertyGrid.ExpandAllGridItems();
                // Popup menu adaptation
                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = true;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Instance";
            }
            // It's an Attribut
            else if (e.Node.Tag is EnIPInstanceAttribut)
            {
                // Read it from the remote devie
                EnIPInstanceAttribut Att = (EnIPInstanceAttribut)e.Node.Tag;
                ReadRet=Att.GetInstanceAttributData();
                // In the Grid
                propertyGrid.SelectedObject = Att;
                propertyGrid.ExpandAllGridItems();
                // Popup menu adaptation
                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = true;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Attribut";
            }

            propertyGrid.Enabled = (ReadRet==EnIPNetworkStatus.OnLine);
            CurrentRemoteDeviceIcon(ReadRet);
            this.Cursor = Memcurs;
        }

        // Menu Item
        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string readme_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(MainForm).Assembly.Location), "README.txt");
            try { System.Diagnostics.Process.Start(readme_path); } catch { }
        }

        // Menu Item
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Ethernet/IP Explorer - EnIPExplorer\nVersion Beta " + this.GetType().Assembly.GetName().Version + "\nBy Frederic Chaxel - Copyright 2016\n" +
                "\nReference: http://sourceforge.net/projects/EnIPExplorer" +
                "\nReference: http://sourceforge.net/projects/yetanotherbacnetexplorer/" +
                "\nReference: http://www.famfamfam.com/"+
                "\nReference: http://www.jrsoftware.org/isinfo.php"
                , "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Menu Item
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Menu Item
        private void sendListIdentityDiscoverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (client == null) return;
            client.DiscoverServers();
        }

        // Menu Item
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

                devicesTreeView.SelectedNode = null;
                devicesTreeView.CollapseAll();
            }
        }

        // Used to fill the combox with the availaible network interface
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

        // Menu Item
        private void addClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPRemoteDevice) break;
                tn = tn.Parent;
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

        // Menu Item
        private void addClassInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPClass) break;
                tn = tn.Parent;
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

        // Menu Item
        private void addInstanceAttributToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPClassInstance) break;
                tn = tn.Parent;
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
                TreeNode tnI = new TreeNode("Attribut #"+Id.ToString(), 10, 10);
                tnI.Tag = att;
                tn.Nodes.Add(tnI);
                tn.Expand();
            }

        }

        // Menu Item
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog dlg = new SettingsDialog();
            dlg.SelectedObject = Properties.Settings.Default;
            dlg.ShowDialog(this);
        }

        // Save the current Settings
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Properties.Settings.Default.GUI_FormSize = this.Size;
                Properties.Settings.Default.GUI_State = this.WindowState;
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
                if (v.SetInstanceAttributData()==EnIPNetworkStatus.OnLine)
                    Trace.WriteLine("Write OK");
            }
            else
                Trace.WriteLine("Modifications are not taken into account at this level");
        }

        // Menu Item
        private TreeNode AddRemoteDevice(EnIPRemoteDevice remotedevice)
        {
            foreach (TreeNode tn in devicesTreeView.Nodes)
                if ((tn.Tag as EnIPRemoteDevice).Equals(remotedevice))
                    return tn;
            
            TreeNode tn2 = new TreeNode(remotedevice.IPString()+ " - " +remotedevice.ProductName, 1, 1);
            tn2.Tag = remotedevice;
            devicesTreeView.Nodes.Add(tn2);

            return tn2;
        }

        // Menu Item
        // Add a new device not discovery using the broadcast technic : outside the local net
        private void addRemoteDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (client==null) return;

            var Input =
                new GenericInputBox<TextBox>("Remote device", "IP address",
                     (o) =>
                     {
                         o.Text = Properties.Settings.Default.DefaultRemoteDevice;
                     });
           
            DialogResult res = Input.ShowDialog();
 
            if (res != DialogResult.OK) return;

            try
            {
                EnIPRemoteDevice remotedevice = new EnIPRemoteDevice(new System.Net.IPEndPoint(IPAddress.Parse(Input.genericInput.Text), 0xAF12), Properties.Settings.Default.TCP_WAN_TimeOut);

                AddRemoteDevice(remotedevice);

                remotedevice.DeviceArrival += new DeviceArrivalHandler(On_DeviceArrival);
                remotedevice.DiscoverServer();
            }
            catch
            {
                Trace.WriteLine("Error with IP : " + Input.genericInput.Text);
            }

        }

        // Menu Item
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

        // Menu Item
        private void renameCurrentNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn= devicesTreeView.SelectedNode;

            if (!(tn.Tag is EnIPCIPObject)) return;

            var Input =
                new GenericInputBox<TextBox>("Rename", "New name",
                     (o) =>
                     {
                         o.Text = tn.Text;
                         o.SelectAll();
                     });
            DialogResult res = Input.ShowDialog();

            if (res == DialogResult.OK)
                tn.Text = Input.genericInput.Text;
        }

        // Recursive usage
        private void SaveFileEnIPOject(StreamWriter sw, string pre,TreeNodeCollection tnc)
        {
            if (tnc == null) return;
            foreach (TreeNode tn in tnc)
            {
                String s=Properties.Settings.Default.CSVSeparator.ToString();
                EnIPCIPObject obj = (EnIPCIPObject)tn.Tag;
                sw.WriteLine(pre + obj.Id+s+tn.Text);
                SaveFileEnIPOject(sw, pre + s + s, tn.Nodes);
            }
        }
        // Menu Item
        private void saveConfigurationAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = Properties.Settings.Default.DefaultTreeFile;
                dlg.Filter = "csv|*.csv";
                if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;

                string filename = dlg.FileName;
                Properties.Settings.Default.DefaultTreeFile = filename;

                StreamWriter sw = new StreamWriter(filename);

                char s = Properties.Settings.Default.CSVSeparator;

                sw.WriteLine("Device IP" + s + "Name" + s + "Class" + s + "Instance" + s + "Attribut");
                sw.WriteLine("// EnIPExplorer Device Tree, can be modified with a spreadsheet");

                foreach (TreeNode tn in devicesTreeView.Nodes)
                {
                    EnIPRemoteDevice remote = (EnIPRemoteDevice)tn.Tag;
                    sw.WriteLine(remote.IPString() + s + remote.ProductName);
                    SaveFileEnIPOject(sw, s.ToString()+s.ToString(), tn.Nodes);
                }

                sw.Close();
                MessageBox.Show("Done", "File Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("File Error", "EnIPExplorer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        // Usage by Load file menu
        private TreeNode AddeNode(TreeNode Parent, EnIPCIPObject obj, String Name, int iconIdx)
        {
            foreach (TreeNode t in Parent.Nodes)
            {
                // already present, just change the name
                if (((t.Tag as EnIPCIPObject).Id) == obj.Id)
                {
                    t.Text = Name;
                    return t;
                }
            }
            TreeNode tn=new TreeNode(Name,iconIdx,iconIdx);
            tn.Tag=obj;
            Parent.Nodes.Add(tn);
            return tn;
        }

        // Usage by Load file menu
        private void AddNodesFromFile(StreamReader sr, ref int line)
        {
            TreeNode ParentDevice = null, ParentClass = null, ParentInstance = null;

            while (!sr.EndOfStream)
            {
                try
                {
                    String str = sr.ReadLine(); line++;
                    if ((str != null) && (str[0] != '/'))
                    {
                        String[] Strs = str.Split(Properties.Settings.Default.CSVSeparator);
                        int Length = Strs.Length;
                        while (Length != 1)
                            if (Strs[Length - 1] == "")
                                Length--;
                            else
                                break;

                        switch (Length)
                        {
                            case 2: // A device, assume LAN timeout and not WAN
                                EnIPRemoteDevice remotedevice = new EnIPRemoteDevice(new System.Net.IPEndPoint(IPAddress.Parse(Strs[0]), 0xAF12), Properties.Settings.Default.TCP_LAN_Timeout);
                                remotedevice.ProductName = Strs[1];
                                ParentDevice = AddRemoteDevice(remotedevice);
                                break;
                            case 4: // A class
                                EnIPClass Class = new EnIPClass(ParentDevice.Tag as EnIPRemoteDevice, Convert.ToUInt16(Strs[2]));
                                int ico;
                                if (Enum.IsDefined(typeof(CIPObjectLibrary), Class.Id))
                                    ico = Classe2Ico((CIPObjectLibrary)Class.Id);
                                else
                                    ico = 2;
                                ParentClass = AddeNode(ParentDevice, Class, Strs[3], ico);
                                break;
                            case 6: // An instance
                                EnIPClassInstance Instance = new EnIPClassInstance(ParentClass.Tag as EnIPClass, Convert.ToByte(Strs[4]));
                                ParentInstance = AddeNode(ParentClass, Instance, Strs[5], 9);
                                break;
                            case 8: // An attribut
                                EnIPInstanceAttribut Attribut = new EnIPInstanceAttribut(ParentInstance.Tag as EnIPClassInstance, Convert.ToByte(Strs[6]));
                                AddeNode(ParentInstance, Attribut, Strs[7], 10);
                                break;
                            default:
                                throw new Exception("Not the good number of colums");
                        }
                    }
                }
                catch { throw new Exception("Line content error"); }
            }
        }
        // Menu Item
        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = Properties.Settings.Default.DefaultTreeFile;
            dlg.Filter = "csv|*.csv";
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;

            string filename = dlg.FileName;
            Properties.Settings.Default.DefaultTreeFile = filename;

            int line = 1;

            try
            {
                StreamReader sr = new StreamReader(filename);
                sr.ReadLine(); line++;  // put out the first line              
                AddNodesFromFile(sr, ref line);
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Error line "+line.ToString()+"/r/n"+ex.ToString(), "EnIPExplorer", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void popupRenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renameCurrentNodeToolStripMenuItem_Click(null, null);
        }
        #endregion

    }

    // Coming from Yabe @ Sourceforge, by Morten Kvistgaard
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
