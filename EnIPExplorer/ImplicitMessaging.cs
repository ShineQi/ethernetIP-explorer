using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.EnIPStack;

namespace EnIPExplorer
{
    public partial class ImplicitMessaging : Form
    {
        EnIPAttribut Config, Input, Output;
        EnIPRemoteDevice device;
        ForwardClose_Packet FwclosePacket = null;

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
            _DragDrop(e, labelOutput, propertyGridOutput, "Output", ref Output);
        }

        private void Input_DragDrop(object sender, DragEventArgs e)
        {
            _DragDrop(e, labelInput, propertyGridInput, "Input", ref Input);
        }

        private void buttonFw_Click(object sender, EventArgs e)
        {
            if (FwclosePacket == null)
            {
                EnIPNetworkStatus result = device.ForwardOpen(checkP2P.Checked, Config, Output, Input, 
                                                (uint)CycleTime.Value, out FwclosePacket, checkWriteConfig.Checked);

                if (result == EnIPNetworkStatus.OnLine)
                {
                    buttonFw.Text = "Forward Close";
                    tmrO2I.Enabled = true;
                }
                else
                    FwclosePacket = null;
            }

            else
            {
                tmrO2I.Interval = (int)CycleTime.Value;
                tmrO2I.Enabled = false;
                device.ForwardClose(FwclosePacket);
                buttonFw.Text = "Forward Open";
                FwclosePacket = null;
            }
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

    }
}
