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
using System.Net.EnIPStack.ObjectsLibrary;
using System.Collections;

namespace EnIPExplorer
{
    public partial class MainForm : Form
    {
        EnIPClient client;
        EnIPNetworkStatus LastReadNetworkStatus;

        Type[] UserTypeDecoders;
        List<UserType> UserTypeList;

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

            devicesTreeView.ShowNodeToolTips = Properties.Settings.Default.ShowNodeToolTip;

            if (Properties.Settings.Default.PeriodicUpdateRate > 0)
            {
                tmrUpdate.Interval = Math.Max(200, Properties.Settings.Default.PeriodicUpdateRate);
                tmrUpdate.Enabled = true;
            }
            else
                tmrUpdate.Enabled = false;

            devicesTreeView.TreeViewNodeSorter = new NodeSorter();
            
            UserDecoderMgmt();
        }

        void UserDecoderMgmt()
        {
            // Remove all dynamic elements
            for (int i = decodeAttributAsToolStripMenuItem.DropDownItems.Count-1; i > 2; i--)
                decodeAttributAsToolStripMenuItem.DropDownItems.RemoveAt(i);
            // Gets all UserDecoders
            try
            {
                UserTypeList = UserType.LoadUserTypes(Properties.Settings.Default.UserAttributsDecodersFile);

                UserTypesCompiler Compiler = new UserTypesCompiler();
                UserTypeDecoders = Compiler.GetUserTypeDecoders(UserTypeList);

                foreach (Type t in UserTypeDecoders)
                {
                    ToolStripMenuItem menu = new ToolStripMenuItem(t.Name);
                    menu.Click += new EventHandler(DecodeMenuItem_Click);
                    menu.Tag = t;
                    decodeAttributAsToolStripMenuItem.DropDownItems.Add(menu);
                }
            }
            catch
            {
                Trace.TraceError("Problem with the user type compiler");
            }
        }

        string IdStr(int Id)
        {
            if (Properties.Settings.Default.IdHexDisplay)
                return Properties.Settings.Default.IdHexPrefix + Convert.ToString(Id, 16).ToUpper();
            else
                return Id.ToString();
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
                    tn.Text = device.IPAdd().ToString() + " - " + device.ProductName;
                    
                    if (devicesTreeView.SelectedNode == tn) devicesTreeView.SelectedNode = null;

                    return;
                }
            }

            TreeNode tn2 = new TreeNode(device.IPAdd().ToString() + " - " + device.ProductName, 0, 0);
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
                tn = new TreeNode(cipobj.ToString()+" #"+ IdStr(Class.Id), img, img);

                // Special classes with the known instance(s)
                if ((Class.Id == 1) || (Class.Id == 2) || (Class.Id == 0xF4) || (Class.Id == 0xF5) || (Class.Id == 0xF6))
                {
                    EnIPInstance instance = new EnIPInstance(Class, 1); // Instance 1
                    TreeNode tnI = new TreeNode("Instance #"+IdStr(1), 9, 9);
                    tnI.Tag = instance;
                    tnI.ToolTipText = "Node " + IdStr(Class.Id)+"."+IdStr(1);
                    tn.Nodes.Add(tnI);
                }
            }
            else
                tn = new TreeNode("Proprietary #" + IdStr(Class.Id), 2, 2);

            tn.ToolTipText = "Node "+IdStr(Class.Id);
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
            popupForwardToolStripMenuItem.Visible = false;
            popuForward2ToolStripMenuItem.Visible = false;

            // It's a Device : top level
            if (e.Node.Tag is EnIPRemoteDevice)
            {
                EnIPRemoteDevice device = (EnIPRemoteDevice)e.Node.Tag;

                propertyGrid.SelectedObject = device;

                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = false;
                popupAddAToolStripMenuItem.Visible = false;
                decodeAttributAsToolStripMenuItem.Visible = false;

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
                String txt = device.IPAdd().ToString() + " - " + device.ProductName;
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
                ReadRet = EnClass.ReadDataFromNetwork();
                LastReadNetworkStatus = EnIPNetworkStatus.OffLine; // to avoid periodic reading
                // In the Grid
                propertyGrid.SelectedObject = EnClass;
                propertyGrid.ExpandAllGridItems();
                // Popup menu adaptation
                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = false;
                decodeAttributAsToolStripMenuItem.Visible = false;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Class";

            }
            // It's an Instance
            else if (e.Node.Tag is EnIPInstance)
            {
                // Read it from the remote devie
                EnIPInstance Instance = (EnIPInstance)e.Node.Tag;

                LastReadNetworkStatus = ReadRet = Instance.ReadDataFromNetwork();

                // remove properties litse filter based on CIPAttribut
                // in order to show all atrbiuts in the property grid
                if (Instance.DecodedMembers != null)
                    Instance.DecodedMembers.FilterAttribut(-1); 

                LastReadNetworkStatus=ReadRet = Instance.ReadDataFromNetwork();
                // In the Grid
                propertyGrid.SelectedObject = Instance;
                propertyGrid.ExpandAllGridItems();
                // Popup menu adaptation
                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = true;
                decodeAttributAsToolStripMenuItem.Visible = false;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Instance";
            }
            // It's an Attribut
            else if (e.Node.Tag is EnIPAttribut)
            {
                // Read it from the remote devie
                EnIPAttribut Att = (EnIPAttribut)e.Node.Tag;

                LastReadNetworkStatus=ReadRet = Att.ReadDataFromNetwork();

                // filter properties list for only the given attribut
                if (Att.DecodedMembers != null)
                    Att.DecodedMembers.FilterAttribut(Att.Id); 

                // In the Grid
                propertyGrid.SelectedObject = Att;
                propertyGrid.ExpandAllGridItems();
                // Popup menu adaptation
                popupAddCToolStripMenuItem.Visible = true;
                popupAddIToolStripMenuItem.Visible = true;
                popupAddAToolStripMenuItem.Visible = true;
                popupForwardToolStripMenuItem.Visible = true;
                popuForward2ToolStripMenuItem.Visible = true;
                decodeAttributAsToolStripMenuItem.Visible = true;
                popupDeleteToolStripMenuItem.Text = deleteToolStripMenuItem.Text = "Delete current Attribute";
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
            MessageBox.Show(this, "Ethernet/IP Explorer - EnIPExplorer\nVersion Beta " + this.GetType().Assembly.GetName().Version + "\nBy Frederic Chaxel - Copyright 2016,2017\n" +
                "\nReferences:\n\t http://sourceforge.net/projects/EnIPExplorer" +
                "\n\t http://sourceforge.net/projects/yetanotherbacnetexplorer/" +
                "\n\t http://www.famfamfam.com/"+
                "\n\t http://www.jrsoftware.org/isinfo.php"
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
                    new GenericInputBoxExtended<ComboBox>("Local Interface", "IP address",
                         (o) =>
                         {
                             string[] local_endpoints = GetAvailableIps();
                             o.Items.AddRange(local_endpoints);
                             o.Text = Properties.Settings.Default.DefaultIPInterface;
                         });

                DialogResult res = Input.ShowDialog();

                if (res != DialogResult.OK) return;
                String userinput = Input.genericInput.Text;
                Properties.Settings.Default.DefaultIPInterface = userinput;

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
                new GenericInputBoxExtended<NumericUpDown>("Add Class", "Class Id :",
                     (o) =>
                     {
                         o.Minimum = 1; o.Maximum = 65535; o.Value = Numbase;
                         o.Hexadecimal = Properties.Settings.Default.IdHexDisplay;
                         ToolTip tt = new ToolTip();
                         tt.AutoPopDelay = 32767;
                         // Helper two columns tooltip with the object Id list 
                         StringBuilder sb = new StringBuilder();
                         int i = 0;
                         foreach (CIPObjectLibrary en in Enum.GetValues(typeof(CIPObjectLibrary)))
                         {
                             String s = IdStr((int)en) + " : " + en.ToString();
                             if (i == 0)
                             {
                                 // 1, 2,3 or 4 tab
                                 sb.Append(s + "\t");
                                 if (s.Length < 10) sb.Append('\t');
                                 if (s.Length < 17) sb.Append('\t');
                                 if (s.Length < 29) sb.Append('\t');
                             }
                             else
                                 sb.Append(s + Environment.NewLine);
                             i = ~i;
                         }

                         tt.SetToolTip(o, sb.ToString());
                     },
                     (o) =>
                     {
                         ushort Id = (ushort)o.Value;
                         EnIPClass Class = new EnIPClass(tn.Tag as EnIPRemoteDevice, Id);
                         tn.Nodes.Add(ClassToTreeNode(Class));
                         tn.Expand();
                         if (o.Value!=o.Maximum) o.Value++;
                     });

            DialogResult res = Input.ShowDialog();

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
                int num = (t.Tag as EnIPInstance).Id;
                Numbase = Math.Max(num + 1, Numbase);
            }

            var Input =
                new GenericInputBoxExtended<NumericUpDown>("Add Instance", "Instance Id :",
                     (o) =>
                     {
                         o.Minimum = 1; o.Maximum = 65535; o.Value = Numbase;
                         o.Hexadecimal = Properties.Settings.Default.IdHexDisplay;
                     },
                     (o) =>
                     {
                         ushort Id = (ushort)o.Value;
                         EnIPClass cl = (EnIPClass)tn.Tag;
                         EnIPInstance instance = new EnIPInstance(cl, Id);
                         TreeNode tnI = new TreeNode("Instance #" + IdStr(Id), 9, 9);
                         tnI.Tag = instance;
                         tnI.ToolTipText = "Node " + IdStr(cl.Id) + "." + IdStr(Id);

                         // speed me ... automatic add Attribut 3 (Data - Array of Bytes) to all Assembly instances
                         if (cl.Id == (ushort)CIPObjectLibrary.Assembly)
                         {
                             EnIPAttribut att = new EnIPAttribut(instance, 3);
                             TreeNode tnI2 = new TreeNode("Attribute #" + IdStr(3), 10, 10);
                             tnI2.Tag = att;
                             tnI2.ToolTipText = "Node " + IdStr(cl.Id) + "." + IdStr(instance.Id) + ".3";
                             tnI.Nodes.Add(tnI2);
                         }

                         tn.Nodes.Add(tnI);
                         tn.Expand();
                         tnI.Expand();

                         if (o.Value != o.Maximum) o.Value++;
                     });

            DialogResult res = Input.ShowDialog();            
        }

        // Menu Item
        private void addInstanceAttributToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode tn = devicesTreeView.SelectedNode;

            // look the node or upper
            for (; ; )
            {
                if (tn == null) return;
                if (tn.Tag is EnIPInstance) break;
                tn = tn.Parent;
            }

            int Numbase = 1;
            foreach (TreeNode t in tn.Nodes)
            {
                int num = (t.Tag as EnIPAttribut).Id;
                Numbase = Math.Max(num + 1, Numbase);
            }

            var Input =
                new GenericInputBoxExtended<NumericUpDown>("Add Attribute", "Attribute Id :",
                     (o) =>
                     {
                         o.Minimum = 1; o.Maximum = 65535; o.Value = Numbase;
                         o.Hexadecimal = Properties.Settings.Default.IdHexDisplay;
                     },
                     (o) =>
                     {
                         ushort Id = (ushort)o.Value;
                         EnIPInstance ist = (EnIPInstance)tn.Tag;
                         EnIPAttribut att = new EnIPAttribut(ist, Id);
                         TreeNode tnI = new TreeNode("Attribute #" + IdStr(Id), 10, 10);
                         tnI.Tag = att;
                         tnI.ToolTipText = "Node " + IdStr((tn.Parent.Tag as EnIPClass).Id) + "." + IdStr(ist.Id) + "." + IdStr(Id);
                         tn.Nodes.Add(tnI);
                         tn.Expand();
                         if (o.Value != o.Maximum) o.Value++;
                     });

            DialogResult res = Input.ShowDialog();

        }

        // Menu Item
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog dlg = new SettingsDialog();
            dlg.SelectedObject = Properties.Settings.Default;
            dlg.ShowDialog(this);
            devicesTreeView.ShowNodeToolTips = Properties.Settings.Default.ShowNodeToolTip;

            if (Properties.Settings.Default.PeriodicUpdateRate > 0)
            {
                tmrUpdate.Interval = Math.Max(200,Properties.Settings.Default.PeriodicUpdateRate);
                tmrUpdate.Enabled = true;
            }
            else
                tmrUpdate.Enabled = false;
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
            if ((e.ChangedItem.Parent != null) && (e.ChangedItem.Parent.Label == "RawData") && (devicesTreeView.SelectedNode.Tag is EnIPAttribut))
            {
                EnIPAttribut v = (EnIPAttribut)devicesTreeView.SelectedNode.Tag;
                if (v.WriteDataToNetwork()==EnIPNetworkStatus.OnLine)
                    Trace.WriteLine("Write OK");
            }
            else
                if ((e.ChangedItem.Parent != null) && (e.ChangedItem.Parent.Label == "DecodedMembers") && (devicesTreeView.SelectedNode.Tag is EnIPAttribut))
                {
                    EnIPAttribut v = (EnIPAttribut)devicesTreeView.SelectedNode.Tag;
                    if (v.EncodeFromDecodedMembers() == true) // encoding is done into the previous RawByte (and same size)
                    {
                        if (v.WriteDataToNetwork() == EnIPNetworkStatus.OnLine)
                            Trace.WriteLine("Write OK");
                    }
                    else
                        Trace.WriteLine("Encoding not allow here or error during the encoding process, nothing written");
                }
                else
                    Trace.WriteLine("Modifications are not taken into account here");

            readAgainToolStripMenuItem_Click(null,null);
        }

        // Menu Item
        private TreeNode AddRemoteDevice(EnIPRemoteDevice remotedevice)
        {
            foreach (TreeNode tn in devicesTreeView.Nodes)
                if ((tn.Tag as EnIPRemoteDevice).Equals(remotedevice))
                    return tn;

            TreeNode tn2 = new TreeNode(remotedevice.IPAdd().ToString() + " - " + remotedevice.ProductName, 1, 1);
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
                new GenericInputBoxExtended<TextBox>("Remote device", "IP address",
                     (o) =>
                     {
                         o.Text = Properties.Settings.Default.DefaultRemoteDevice;
                     });
           
            DialogResult res = Input.ShowDialog();
 
            if (res != DialogResult.OK) return;

            try
            {
                EnIPRemoteDevice remotedevice = new EnIPRemoteDevice(new System.Net.IPEndPoint(IPAddress.Parse(Input.genericInput.Text), 0xAF12), Properties.Settings.Default.TCP_WAN_TimeOut);
                remotedevice.ProductName = "EnIPExplorer temporary ProductName";
                AddRemoteDevice(remotedevice);

                remotedevice.DeviceArrival += new DeviceArrivalHandler(On_DeviceArrival);
                remotedevice.DiscoverServer();

                Properties.Settings.Default.DefaultRemoteDevice = Input.genericInput.Text;
            }
            catch
            {
                Trace.WriteLine("Error with IP : " + Input.genericInput.Text);
            }

        }

        // Menu Item
        private void explicitMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ExplicitMessages(devicesTreeView).ShowDialog();
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

            if ((devicesTreeView.SelectedNode==null)||(!(tn.Tag is EnIPCIPObject))) return;

            var Input =
                new GenericInputBoxExtended<TextBox>("Rename", "New name",
                     (o) =>
                     {
                         o.Text = tn.Text;
                         o.SelectAll();
                     });
            DialogResult res = Input.ShowDialog();

            if (res == DialogResult.OK)
                tn.Text = Input.genericInput.Text;
        }

        // Menu Item
        private void readAgainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (propertyGrid.SelectedObject is EnIPCIPObject)
            {
                LastReadNetworkStatus = (propertyGrid.SelectedObject as EnIPCIPObject).ReadDataFromNetwork();
                propertyGrid.Refresh();
            }
            else
                LastReadNetworkStatus = EnIPNetworkStatus.OffLine;
        }

        // Menu Item
        private void ForwardOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(propertyGrid.SelectedObject is EnIPAttribut)) return;

            bool p2p = true;

            if ((sender == multicastToolStripMenuItem) || (sender == popupMulticastToolStripMenuItem))
                p2p = false;

            EnIPAttribut att = (EnIPAttribut)propertyGrid.SelectedObject;

            int Duration = Properties.Settings.Default.ForwardOpenDuration_s;
            if (Duration <= 0) Duration = 1;
            if (Duration > 60) Duration = 60;
            if (att.ForwardOpen(p2p, true, false, Properties.Settings.Default.ForwardOpenPeriod_ms, Duration)==EnIPNetworkStatus.OnLine)
                Trace.WriteLine("ForwardOpen T->O OK, good luck with Wireshark or Class1 client sample source code");

        }
        // Menu Item
        private void sendForwardOpenTOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(propertyGrid.SelectedObject is EnIPAttribut)) return;

            EnIPAttribut att = (EnIPAttribut)propertyGrid.SelectedObject;

            if (att.RemoteDevice.VendorId==40)
                if (MessageBox.Show("Wago PLC, do it at your own risk,\r\n this action could destroy it,\r\n Cancel please it's better", "Takes care", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                    == DialogResult.Cancel) return;


            int Duration = Properties.Settings.Default.ForwardOpenDuration_s;
            if (Duration <= 0) Duration = 1;
            if (Duration > 60) Duration = 60;
            if (att.ForwardOpen(true, false, true, 100, Duration) == EnIPNetworkStatus.OnLine)
                 Trace.WriteLine("ForwardOpen O->T OK, close will be sent in "+Duration.ToString()+" seconds");
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

                sw.WriteLine("Device IP"+s+"Name"+s+"Class"+s+"ClassName"+s+"Instance"+s+"InstanceName"+s+"Attribute"+s+"AttributeName");
                sw.WriteLine("// EnIPExplorer Device Tree, can be modified with a spreadsheet");

                foreach (TreeNode tn in devicesTreeView.Nodes)
                {
                    EnIPRemoteDevice remote = (EnIPRemoteDevice)tn.Tag;
                    sw.WriteLine(remote.IPAdd().ToString() + s + remote.ProductName);
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
            TreeNode ParentDeviceTreeNode = null, ParentClassTreeNode = null, ParentInstanceTreeNode = null;
            EnIPRemoteDevice remotedevice = null; EnIPClass Class = null; EnIPInstance Instance = null;
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
                                remotedevice = new EnIPRemoteDevice(new System.Net.IPEndPoint(IPAddress.Parse(Strs[0]), 0xAF12), Properties.Settings.Default.TCP_LAN_Timeout);
                                remotedevice.ProductName = Strs[1];
                                ParentDeviceTreeNode = AddRemoteDevice(remotedevice);
                                break;
                            case 4: // A class
                                Class = new EnIPClass(remotedevice, Convert.ToUInt16(Strs[2]));
                                int ico;
                                if (Enum.IsDefined(typeof(CIPObjectLibrary), Class.Id))
                                    ico = Classe2Ico((CIPObjectLibrary)Class.Id);
                                else
                                    ico = 2;
                                ParentClassTreeNode = AddeNode(ParentDeviceTreeNode, Class, Strs[3], ico);
                                ParentClassTreeNode.ToolTipText = "Node " + IdStr(Class.Id);
                                break;
                            case 6: // An instance
                                Instance = new EnIPInstance(Class, Convert.ToByte(Strs[4]));
                                ParentInstanceTreeNode = AddeNode(ParentClassTreeNode, Instance, Strs[5], 9);
                                ParentInstanceTreeNode.ToolTipText = ParentClassTreeNode.ToolTipText + "." + IdStr(Instance.Id);
                                break;
                            case 8: // An attribut
                                EnIPAttribut Attribut = new EnIPAttribut(Instance, Convert.ToByte(Strs[6]));
                                TreeNode tnAtt=AddeNode(ParentInstanceTreeNode, Attribut, Strs[7], 10);
                                tnAtt.ToolTipText = ParentInstanceTreeNode.ToolTipText + "." + IdStr(Attribut.Id);
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
        // Menu Item
        private void editAttributsDecodersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DecoderEditor editor = new DecoderEditor(UserTypeList);
            DialogResult result=editor.ShowDialog();

            if (result == DialogResult.OK)
                UserDecoderMgmt();
            
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
        
        private void DecodeMenuItem_Click(object sender, EventArgs e)
        {
            if ((devicesTreeView.SelectedNode==null) ||
             (!(devicesTreeView.SelectedNode.Tag is EnIPAttribut))) return; // hoops !

            EnIPAttribut attribut = (EnIPAttribut)devicesTreeView.SelectedNode.Tag;

            if (sender == defaultToolStripMenuItem)
            {
                // sset to null, next Read will put back the default decoder
                attribut.DecodedMembers = null;
            }
            else
                if (sender == arrayOfUINTToolStripMenuItem)
                {
                    attribut.DecodedMembers = new CIPUInt16Array();
                }
                else
                {
                    ToolStripMenuItem menustrip=(ToolStripMenuItem)sender;
                    attribut.DecodedMembers = (CIPObject)Activator.CreateInstance((Type)menustrip.Tag);
                }

            readAgainToolStripMenuItem_Click(null,null);
        }

        #endregion

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (LastReadNetworkStatus == EnIPNetworkStatus.OnLine)
                readAgainToolStripMenuItem_Click(null, null);
        }

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

    // In order to sort the treeview
    class NodeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = (TreeNode)x;
            TreeNode ty = (TreeNode)y;
              
            #pragma warning disable 0618
            if (tx.Tag is EnIPRemoteDevice) // Top level
                return (tx.Tag as EnIPRemoteDevice).IPAdd().Address.CompareTo((ty.Tag as EnIPRemoteDevice).IPAdd().Address);
            else
                return (tx.Tag as EnIPCIPObject).Id.CompareTo((ty.Tag as EnIPCIPObject).Id);
            #pragma warning restore 0618
        }
    }
}
