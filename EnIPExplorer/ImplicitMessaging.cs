using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.EnIPStack;
using System.Net;
using System.Net.EnIPStack.ObjectsLibrary;
using System.Reflection;

namespace EnIPExplorer
{
    public partial class ImplicitMessaging : Form
    {
        EnIPAttribut Config, Input, Output;
        EnIPRemoteDevice device;
        ForwardClose_Packet FwclosePacket = null;
        String MCastAddress=null;

        public ImplicitMessaging(TreeView devicetreeView)
        {
            InitializeComponent();

            ClassView.ImageList = devicetreeView.ImageList;

            TreeNode baseNode = devicetreeView.SelectedNode;
            while (baseNode.Parent != null) baseNode = baseNode.Parent;

            device = (EnIPRemoteDevice)baseNode.Tag;

            foreach (TreeNode t in baseNode.Nodes)
                ClassView.Nodes.Add((TreeNode)t.Clone());
            
            ClassView.ExpandAll();

            ClassView.ItemDrag += new ItemDragEventHandler(ClassView_ItemDrag);

            this.Text = "Implicit Messaging with " + baseNode.Text;

            try
            {
                IPEndPoint LocalEp = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.DefaultIPInterface), 0x8AE);
                // It's not a problem to do this with more than one remote device,
                // the underlying udp socket is static
                device.Class1Activate(LocalEp);
            }
            catch { }

        }

        void GetMultiCastAdress()
        {
            try
            {
                EnIPClass Class245 = new EnIPClass(device, 245);
                EnIPInstance Instance1 = new EnIPInstance(Class245, 1, typeof(CIP_TCPIPInterface_instance));
                Instance1.ReadDataFromNetwork();

                MCastAddress=(Instance1.DecodedMembers as CIP_TCPIPInterface_instance).Mcast_Config.Mcast_Start_Addr;
            }
            catch {}
        }

        void ClassView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Today allows only Attributs but basically all others
            // nodes type can be used for ForwardOpen
            // ... not seems to be implemented !
            if ((e.Item as TreeNode).Tag is EnIPAttribut)
                DoDragDrop(e.Item, DragDropEffects.Move); 

        }

        private void _DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }


        private void _DragDrop(DragEventArgs e, Label lbl, PropertyGrid Pg, string Txt, ref EnIPAttribut cipObj)
        {
            TreeNode t = (TreeNode)e.Data.GetData(typeof(TreeNode));
            cipObj = (EnIPAttribut)t.Tag;
            lbl.Text = Txt + "\r\nNode " + cipObj.GetStrPath();
            cipObj.ReadDataFromNetwork();

            Pg.SelectedObject = cipObj;
            Pg.ExpandAllGridItems();
        }

        private void Config_DragDrop(object sender, DragEventArgs e)
        {
            _DragDrop(e, labelConfig, propertyGridConfig, "Configuration", ref Config);
        }

        private void Output_DragDrop(object sender, DragEventArgs e)
        {
            _DragDrop(e, labelOutput, propertyGridOutput, "Output (O->T)", ref Output);
        }

        private void Input_DragDrop(object sender, DragEventArgs e)
        {
            _DragDrop(e, labelInput, propertyGridInput, "Input (T->O)", ref Input);
        }

        private void buttonFw_Click(object sender, EventArgs e)
        {
            if (!checkP2P.Checked) // multicast mode, get first @ by device query
                if (MCastAddress == null)
                {
                    GetMultiCastAdress();
                    if (MCastAddress != null)
                        device.Class1AddMulticast(MCastAddress); // I will be possible to Join the 32 consecutives @ to be sure
                }

            if (FwclosePacket == null)
            {
                // CycleTime in microseconds
                EnIPNetworkStatus result = device.ForwardOpen(Config, Output, Input, out FwclosePacket, (uint)(CycleTime.Value * 1000), checkP2P.Checked, checkWriteConfig.Checked);

                if (result == EnIPNetworkStatus.OnLine)
                {
                    buttonFw.Text = "Forward Close";
                    tmrO2T.Interval = (int)CycleTime.Value; // here in ms it's a Windows timer
                    tmrO2T.Enabled = true;

                    if (Input!=null)
                        Input.T2OEvent += new T2OEventHandler(Input_T2OEvent);
                }
                else
                    FwclosePacket = null;
            }

            else
            {
                tmrO2T.Enabled = false;
                device.ForwardClose(FwclosePacket);
                buttonFw.Text = "(Large)Forward Open";
                FwclosePacket = null;
                if (Input != null)
                    Input.T2OEvent -= new T2OEventHandler(Input_T2OEvent);
                ImgInputActivity.Visible = false;
            }
        }
     
        void Input_T2OEvent(EnIPAttribut sender)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<EnIPAttribut>(Input_T2OEvent), new object[] { sender });
                return;
            }
            //propertyGridInput.Refresh();
            MainForm.SoftRefreshPropertyGrid(propertyGridInput);
            ImgInputActivity.Visible = !ImgInputActivity.Visible;
        }

        private void tmrO2I_Tick(object sender, EventArgs e)
        {
            Output.Class1UpdateO2T();
        }

        private void ImplicitMessaging_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FwclosePacket!=null)
                device.ForwardClose(FwclosePacket);
        }

        private void ImplicitMessaging_Load(object sender, EventArgs e)
        {
            if (device.VendorId == 40)
                MessageBox.Show("Wago PLC, do it at your own risk,\r\n some configurations could destroy it.\r\n Cancel it's better", "Takes care", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void propertyGridOutput_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // Modification in a Decoded field : copy it into the Raw field
            if ((e.ChangedItem.Parent != null) && (e.ChangedItem.Parent.Label == "DecodedMembers"))
            {
                Output.EncodeFromDecodedMembers();
                propertyGridOutput.Refresh();
            }
        }

        private void propertyGridConfig_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // Modification in a Decoded field : copy it into the Raw field
            if ((e.ChangedItem.Parent != null) && (e.ChangedItem.Parent.Label == "DecodedMembers"))
            {
                Config.EncodeFromDecodedMembers();
                propertyGridConfig.Refresh();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

    }
}
