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

                if (e.Node.Nodes.Count != 0) return;

                if (device.SupportedClassLists.Count == 0)
                {
                    if (device.IsConnected() == false) device.Connect();
                    device.GetObjectList();
                }

                foreach (ushort clId in device.SupportedClassLists)
                {
                    TreeNode tn;

                    if (Enum.IsDefined(typeof(CIPObjectLibrary), clId))
                    {
                        CIPObjectLibrary cipobj = (CIPObjectLibrary)clId;
                        int img = Classe2Ico(cipobj);
                        tn = new TreeNode(cipobj.ToString(), img, img);
                    }
                    else
                        tn = new TreeNode("Proprietary " + clId.ToString() + " (0x" + clId.ToString("X2") + ")", 1, 1);

                    e.Node.Nodes.Add(tn);
                }
            }
            else
                propertyGrid.SelectedObject = null;
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
                client = new EnIPClient("172.20.10.10");
                client.DeviceArrival += new DeviceArrivalHandler(On_DeviceArrival);

                client.DiscoverServers();

                openInterfaceToolStripMenuItem.Enabled = false;
                sendListIdentityDiscoverToolStripMenuItem.Enabled = true;
            }
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

            m_form.BeginInvoke((MethodInvoker)delegate { m_form.LogText.AppendText(message); });
        }
    }
}
