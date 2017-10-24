// Code by Pepe
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.EnIPStack;
using System.Text;
using System.Windows.Forms;

namespace EnIPExplorer
{
    public partial class ExplicitMessages : Form
    {

        public class ComboboxItem
        {
            public ComboboxItem(EnIPRemoteDevice rd)
            {
                RemoteDevice = rd;
                Text = rd.IPAdd().ToString() + " - " + rd.ProductName;
            }
            string Text;
            public EnIPRemoteDevice RemoteDevice { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        
        private TreeView _tv;
        private List<EnIPRemoteDevice> _selfCreatedRemoteDevicesOverIp = new List<EnIPRemoteDevice>();

        public ExplicitMessages(TreeView tv)
        {
            InitializeComponent();

            _tv = tv;
            refreshDevices();
            string[] names;

            // services
            CIPServiceCodes[] values1 = (CIPServiceCodes[])Enum.GetValues(typeof(CIPServiceCodes));
            names = Enum.GetNames(typeof(CIPServiceCodes));
            for (int i = 0; i < names.Length; i++)
            {
                cb_service.Items.Add(((int)values1[i]).ToString("X2") + " - " + names[i]);
            }
            if (cb_service.Items.Count > 0)
                cb_service.SelectedIndex = 0;
            else
                cb_service.Text = "1";

            // classes
            CIPObjectLibrary[] values2 = (CIPObjectLibrary[])Enum.GetValues(typeof(CIPObjectLibrary));
            names = Enum.GetNames(typeof(CIPObjectLibrary));
            for (int i = 0; i < names.Length; i++)
            {
                cb_class.Items.Add(((int)values2[i]).ToString("X2") + " - " + names[i]);
            }
            if (cb_class.Items.Count > 0)
                cb_class.SelectedIndex = 0;
            else
                cb_class.Text = "1";
            tb_instance.Text = "0";
        }

        private void ExplicitMessages_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (EnIPRemoteDevice rd in _selfCreatedRemoteDevicesOverIp)
                rd.Disconnect();
        }

        private void b_refresh_Click(object sender, EventArgs e)
        {
            refreshDevices();
        }

        private void refreshDevices()
        {
            // devices
            List<EnIPRemoteDevice> lrd = new List<EnIPRemoteDevice>();
            foreach (TreeNode tn in _tv.Nodes)
                if (tn.Tag is EnIPRemoteDevice)
                    lrd.Add((EnIPRemoteDevice)tn.Tag);
            foreach (EnIPRemoteDevice rd in lrd)
                cb_device.Items.Add(new ComboboxItem(rd));
            if (cb_device.Items.Count > 0)
                cb_device.SelectedIndex = 0;
        }

        #region helper

        //should only be called with positive numbers: https://stackoverflow.com/questions/11650222/minimum-number-of-bytes-that-can-contain-an-integer-value
        private static byte CalcBytes(long value)
        {
            if (value == 0)
                return 1;
            UInt32 bitLength = 0;
            while (value > 0)
            {
                bitLength++;
                value >>= 1;
            }
            return (byte)(Math.Ceiling(bitLength * 1.0 / 8));
        }

        private static List<byte> ItemPath(CipLogicalType lt, UInt32 value)
        {
            List<byte> lb = new List<byte>();
            byte temp = CalcBytes(value); // maximal 32 bytes = 4 -> UInt32 value
            lb.Add( (byte) (((byte)CipSegmentTypes.LogicalSegment) | ((byte)( ((byte)lt) | ((byte)(temp / 2))))) ); // 1,2,4 => 0,1,2   or  LogicalType  or LogicalSegment
            if (temp > 1) // padbyte
                lb.Add(0);
            byte[] xy = BitConverter.GetBytes(value); 
            for (int i = 0; i < temp; i++) // add possible smallest representation
                lb.Add(xy[i]);
            return lb;
        }
        enum CipSegmentTypes : byte { PortSegment=0, LogicalSegment=32, NetwortkSegment=64, SymbolicSegment=96, DataSegment= 128, DataTypeC62= 160, DataTypeC61=192, Reserved = 224}
        enum CipSize : byte { U8 = 0, U16 = 1, U32 = 2, Reserved = 3 }
        enum CipLogicalType : byte { ClassID = 0, InstanceID = 4, MemberID = 8, ConnectionPoint = 12, AttributeId =16, Special =20, ServiceID=24, ExtendedLogical=28 }

        private static List<byte> GetPath(UInt16 Class, UInt32 Instance, UInt16? Attribute = null) // EnIPBase.cs:GetPath is wrong
        { // see Apendix C: Data Management: 
            List<byte> lb = new List<byte>();
            lb.AddRange(ItemPath(CipLogicalType.ClassID, Class));
            lb.AddRange(ItemPath(CipLogicalType.InstanceID, Instance)); // instance = 0 -> class level
            if(Attribute != null)
                lb.AddRange(ItemPath(CipLogicalType.AttributeId, Attribute.Value));
            return lb;
        }

        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ");
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                try { bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16); }
                catch { }
            return bytes;
        }

        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        #endregion  

        // Updating the send textBox
        private void refreshSendWindow()
        {
            List<byte> lb = new List<byte>();
            lb.Add(getServiceId());
            List<byte> path = GetPath(getClasseId(), getInstanceId(), getAttributeId());
            lb.Add(((byte)(path.Count/2))); // size in words not bytes
            lb.AddRange(path);
            lb.AddRange(getExtraData());
            tb_send.Text = ByteArrayToString(lb.ToArray());
        }

        private void b_send_Click(object sender, EventArgs e)
        {
            refreshSendWindow();

            List<byte> lb = new List<byte>();
            lb.Add(getServiceId());
            List<byte> path = GetPath(getClasseId(), getInstanceId(), getAttributeId());
            lb.Add(((byte)(path.Count / 2))); // size in words not bytes
            lb.AddRange(path);
            lb.AddRange(getExtraData());


            int Lenght = 0;
            int Offset = 0;
            byte[] msg = lb.ToArray();

            byte[] pack;
            byte[] data = getExtraData().ToArray();

            getDevice().SendUCMM_RR_Packet(msg, ((CIPServiceCodes)getServiceId()), data, ref Offset, ref Lenght, out pack);

            if(Lenght > 40) // 40 is normally the offset from Encapsulation Header to CipHeader ... iterate over the items in the enip message instaed would be an option and maybe something like "getUnconnectedDataItem"
                tb_received.Text = ByteArrayToString(SubArray(pack, 40, Lenght- 40));


            //  EnIPRemoteDevice remotedevice = new EnIPRemoteDevice(new System.Net.IPEndPoint(IPAddress.Parse(Strs[0]), 0xAF12), Properties.Settings.Default.TCP_LAN_Timeout);
            //   remotedevice.SendUCMM_RR_Packet(byte[] DataPath, CIPServiceCodes Service, byte[] data, ref int Offset, ref int Lenght, out byte[] packet)
        }

        private void cb_device_Leave(object sender, EventArgs e)
        {
            IPAddress ip;
            if (cb_device.SelectedIndex ==  -1)
            {
                try
                {
                    ip =  IPAddress.Parse(cb_device.Text);
                }
                catch 
                {
                    cb_device.Text = "192.168.1.1"; // not a valid ip
                    cb_device.Focus();
                    return;
                }
                int i = 0;
                foreach (object o in cb_device.Items)
                {
                    if (((ComboboxItem)o).RemoteDevice.IPAdd().Equals(ip))
                    {
                        cb_device.SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }
        }

        private EnIPRemoteDevice getDevice()
        {
            EnIPRemoteDevice remotedevice;
            IPAddress ip;
            if (cb_device.SelectedIndex == -1)
            {
                try
                { 
                    ip = IPAddress.Parse(cb_device.Text);
                }
                catch
                {
                    // no valid ip:
                    cb_device.Text = "192.168.1.1";
                    ip = IPAddress.Parse(cb_device.Text);
                }
                foreach (EnIPRemoteDevice rd in _selfCreatedRemoteDevicesOverIp) // connection to the adapter already opended somewhere here?
                    if(rd.IPAdd().Equals(ip))
                        return rd;
                remotedevice = new EnIPRemoteDevice(new System.Net.IPEndPoint(ip, 0xAF12), Properties.Settings.Default.TCP_LAN_Timeout); // create new one
                _selfCreatedRemoteDevicesOverIp.Add(remotedevice);
            }
            else
            {
                remotedevice = ((ComboboxItem)cb_device.SelectedItem).RemoteDevice;
            }
            return remotedevice;
        }




        private void cb_service_Leave(object sender, EventArgs e)
        {
            byte b;
            if (cb_service.SelectedIndex == -1)
            {
                try
                {
                    Byte.TryParse(cb_service.Text, out b);
                }
                catch
                {
                    if (cb_service.Items.Count > 0)
                        cb_service.SelectedIndex = 0;
                    else
                        cb_service.Text = "1";
                    cb_service.Focus();
                    return;
                }


                CIPServiceCodes[] values1 = (CIPServiceCodes[])Enum.GetValues(typeof(CIPServiceCodes));
                string[] names = Enum.GetNames(typeof(CIPServiceCodes));
                for (int i = 0; i < names.Length; i++)
                {
                    if (((byte)values1[i]) == b)
                    { 
                        cb_service.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private byte getServiceId()
        {
           
            byte b;
            if (cb_service.SelectedIndex == -1)
            {
                if (!(Byte.TryParse(cb_service.Text, out b)))
                { 
                    b = 1;
                    if (cb_service.Items.Count > 0)
                        cb_service.SelectedIndex = 0;
                    else
                    { 
                        cb_service.Text = "1";
                        cb_service_Leave(null, null);
                    }
                }

            }

            else
                b = (byte)((CIPServiceCodes[])Enum.GetValues(typeof(CIPServiceCodes)))[cb_service.SelectedIndex];
            return b;
        }






        private void cb_class_Leave(object sender, EventArgs e)
        {
            UInt16 classId;
            if (cb_class.SelectedIndex == -1)
            {
                try
                {
                    UInt16.TryParse(cb_class.Text, out classId);
                }
                catch
                {
                    if (cb_class.Items.Count > 0)
                        cb_class.SelectedIndex = 0;
                    else
                        cb_class.Text = "1";
                    cb_class.Focus();
                    return;
                }

                // classes
                CIPObjectLibrary[] values2 = (CIPObjectLibrary[])Enum.GetValues(typeof(CIPObjectLibrary));
                string[] names = Enum.GetNames(typeof(CIPObjectLibrary));
                for (int i = 0; i < names.Length; i++)
                {
                    if (((UInt16)values2[i]) == classId)
                    {
                        cb_class.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private UInt16 getClasseId()
        {
            UInt16 classId;
            if (cb_class.SelectedIndex == -1)
            {
                if (! (UInt16.TryParse(cb_class.Text, out classId)))
                { 
                    classId = 1;
                    if (cb_class.Items.Count > 0)
                        cb_class.SelectedIndex = 0;
                    else
                    {
                        cb_class.Text = "1";
                        cb_class_Leave(null, null);
                    }
                }
            }
            else
                classId = (UInt16)((CIPObjectLibrary[])Enum.GetValues(typeof(CIPObjectLibrary)))[cb_class.SelectedIndex];
            return classId;
        }




        private void tb_instance_Leave(object sender, EventArgs e)
        {
            UInt32 instanceId;
            if( ! (UInt32.TryParse(tb_instance.Text, out instanceId) ) )
            { 
                tb_instance.Text = "0";
                tb_instance.Focus();
                return;
            }
        }

        private UInt32 getInstanceId()
        {
            UInt32 instanceId;
            if (!(UInt32.TryParse(tb_instance.Text, out instanceId)))
            { 
                instanceId = 0;
                tb_instance.Text = "0";
            }
            return instanceId;
        }

        private void tb_attribute_Leave(object sender, EventArgs e)
        {
            if (tb_attribute.Text.Equals(""))
                return; // don´t send attributeId
            UInt16 attributeId;
            try
            {
                UInt16.TryParse(cb_class.Text, out attributeId);
            }
            catch
            {
                tb_attribute.Text = "1";
                tb_attribute.Focus();
                return;
            }
        }

        private UInt16? getAttributeId()
        {
            if (tb_attribute.Text.Equals(""))
                return null; // dont use attributeID
            UInt16 attributeId;
            if(!(UInt16.TryParse(cb_class.Text, out attributeId)))
            {
                tb_attribute.Text = "";
                return null;
            }
            return attributeId;
        }

        private void tb_data_Leave(object sender, EventArgs e)
        {
            string[] words = tb_data.Text.Split(' ');
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (string s in words)
                {
                    if (!s.Equals(" ") && (s.Length > 0))
                    {
                        sb.Append(Convert.ToInt32(s, 16).ToString("X2"));
                    }
                }
                sb.Remove(sb.Length - 1, 1);
                tb_data.Text = sb.ToString();
            }
            catch 
            {
                ; // convertion failure -> will be filtered in the next loop
            }

            sb = new StringBuilder();
            
            foreach (char c in tb_data.Text)
            {
                if (c >= '0' && c <= '9'){
                    sb.Append(c);
                    if(sb.Length%2 == 0)
                        sb.Append(" ");
                }
                else
                {
                    if (c >= 'a' && c <= 'f')
                    {
                        sb.Append(c+32);
                        if (sb.Length % 2 == 0)
                            sb.Append(" ");
                    }
                    else
                    {
                        if(c >= 'A' && c <= 'F')
                        {
                            sb.Append(c + 32);
                            if (sb.Length % 2 == 0)
                                sb.Append(" ");
                        }
                    }
                }
            }
            if(sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            tb_data.Text = sb.ToString();
        }

        private List<byte> getExtraData()
        {
            return new List<Byte>(StringToByteArray(tb_data.Text.Replace(" ", "")));
        }


    }
}
