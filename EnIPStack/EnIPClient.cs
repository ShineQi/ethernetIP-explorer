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
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace System.Net.EnIPStack
{
    public delegate void DeviceArrivalHandler(EnIPRemoteDevice device);

    public class EnIPClient
    {
        EnIPUDPTransport udp;

        public event DeviceArrivalHandler DeviceArrival;

        public EnIPClient(String End_point)
        {
            udp = new EnIPUDPTransport(End_point, false);
            udp.Start();
            udp.MessageReceived += new MessageReceivedHandler(on_MessageReceived);
        }

        void on_MessageReceived(object sender, byte[] packet, EncapsulationPacket EncapPacket, int offset, int msg_length, System.Net.IPEndPoint remote_address)
        {
            if ((EncapPacket.Command == (ushort)EncapsulationCommands.ListIdentity) && (EncapPacket.Length != 0) && EncapPacket.IsOK)
            {
                if (DeviceArrival != null)
                {
                    int NbDevices = BitConverter.ToUInt16(packet, offset);

                    offset += 2;
                    for (int i = 0; i < NbDevices; i++)
                    {
                        EnIPRemoteDevice device = new EnIPRemoteDevice(packet, remote_address, ref offset);
                        DeviceArrival(device);
                    }
                }
            }
        }

        public void DiscoverServers()
        {
            EncapsulationPacket p = new EncapsulationPacket(EncapsulationCommands.ListIdentity);
            p.Command = (ushort)EncapsulationCommands.ListIdentity;
            udp.Send(p, udp.GetBroadcastAddress());
        }
    }

    public class EnIPRemoteDevice
    {
        // Data comming from the reply to ListIdentity query
        public ushort Length;
        public ushort EncapsulationVersion { get; set; }
        public SocketAddress SocketAddress;
        public ushort VendorId { get; set; }
        public ushort DeviceType { get; set; }
        public ushort ProductCode { get; set; }
        public string Revision { get { return _Revision[0].ToString() + "." + _Revision[1].ToString(); } set {} }
        public byte[] _Revision = new byte[2];
        public short Status { get; set; }
        public uint SerialNumber { get; set; }
        public string ProductName { get; set; }
        public byte State { get; set; }

        public IPEndPoint ep;
        public UInt32 SessionHandle; // When Register Session is set
        public TcpClient Tcpclient;

        public List<ushort> SupportedClassLists = new List<ushort>();

        // The udp endpoint is given here, it's also the tcp one
        public EnIPRemoteDevice(byte[] DataArray, IPEndPoint ep, ref int Offset)
        {
            this.ep = ep;

            Offset += 2; // 0x000C 

            Length = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            EncapsulationVersion = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            // Maybe it should be used in place of the ep
            // if a host embbed more than one device, sure it sends different tcp/udp port ?
            // FIXME if you know.
            SocketAddress = new SocketAddress(DataArray, ref Offset);

            VendorId = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            DeviceType = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            ProductCode = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            _Revision[0] = DataArray[Offset];
            Offset++;

            _Revision[1] = DataArray[Offset];
            Offset++;

            Status = BitConverter.ToInt16(DataArray, Offset);
            Offset += 2;

            SerialNumber = BitConverter.ToUInt32(DataArray, Offset);
            Offset += 4;

            int strSize = DataArray[Offset];
            Offset += 1;

            ProductName = System.Text.ASCIIEncoding.ASCII.GetString(DataArray, Offset, strSize);
            Offset += strSize;

            State = DataArray[Offset];

            Offset += 1;
        }

        public EnIPRemoteDevice(IPEndPoint ep)
        {
            this.ep = ep;
        }

        public bool Equals(EnIPRemoteDevice other)
        {
            return ((ep.Equals(other.ep)) && (SerialNumber == other.SerialNumber));
        }

        public bool IsConnected() { return (Tcpclient != null);  }

        public void Connect()
        {
            SessionHandle = 0;
            try
            {
                Tcpclient = new TcpClient();
                Tcpclient.Connect(ep);
                Tcpclient.ReceiveTimeout = 100;
            }
            catch
            {
                Tcpclient = null;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (Tcpclient != null)
                {
                    Tcpclient.Close();
                }
            }
            catch { }

            Tcpclient = null;
        }

        // Needed for a lot of operations
        private void RegisterSession()
        {
            if ((Tcpclient != null) && (SessionHandle == 0))
            {
                byte[] b = new byte[] { 1, 0, 0, 0 };
                EncapsulationPacket p = new EncapsulationPacket(EncapsulationCommands.RegisterSession, 0, b);

                byte[] buffer = p.toByteArray();
                Tcpclient.Client.Send(buffer);

                int ret = Tcpclient.Client.Receive(buffer); // re-use of buffer, large enought
                if (ret == 28)
                {
                    int Offset = 0;
                    EncapsulationPacket rep = new EncapsulationPacket(buffer, ref Offset);
                    if (rep.IsOK)
                        SessionHandle = rep.Sessionhandle;
                }
            }
        }

        public void Test()
        {

            if (SessionHandle == 0) RegisterSession();
            if (SessionHandle == 0) return;

            byte[] IdentityObjectAttributName = new byte[] { 0x20, 0x01, 0x24, 0x01, 0x30, 0x07 };
            byte[] MessageRouterObject = new byte[] { 0x20, 0x02, 0x24, 0x01, 0x30, 0x01 };

            // Attribut 3 de la classe (instance 0) = Nombre d'instance .. mais optionel 
            byte[] WagoInput = new byte[] { 0x20, 0x64, 0x24, 0x00, 0x30, 0x03 };
            // Pour addresser la class il faut demander l'instance 0 
            byte[] MessageRouterObjectList = new byte[] { 0x20, 0x02, 0x24, 0x01, 0x30, 0x01 };

            UCMM_RR_Packet m = new UCMM_RR_Packet();
            m.Path = WagoInput;
           // m.Service = (byte)ControlNetService.GetAttributesAll;
            m.Service = (byte)ControlNetService.GetAttributeSingle;

            EncapsulationPacket p = new EncapsulationPacket(EncapsulationCommands.SendRRData, SessionHandle, m.toByteArray());
            Tcpclient.Client.Send(p.toByteArray());

        }

        public List<ushort> GetObjectList()
        {
            SupportedClassLists.Clear();

            if (Tcpclient == null)
                return null;

            if (SessionHandle == 0) RegisterSession();
            if (SessionHandle == 0) return null;

            // Class 2, Instance 1, Attribut 1
            byte[] MessageRouterObjectList = new byte[] { 0x20, 0x02, 0x24, 0x01, 0x30, 0x01 };
            //MessageRouterObjectList = Path.GetPath(0x02, 0x01, 0x01);
            // MessageRouterObjectList = Path.GetPath("2.1.1");

            UCMM_RR_Packet m = new UCMM_RR_Packet();
            m.Path = MessageRouterObjectList;
            m.Service = (byte)ControlNetService.GetAttributeSingle;

            EncapsulationPacket p = new EncapsulationPacket(EncapsulationCommands.SendRRData, SessionHandle, m.toByteArray());
            Tcpclient.Client.Send(p.toByteArray());

            byte[] b = new byte[1500];
            int ret = Tcpclient.Client.Receive(b);
            if (ret >24)
            {
                int Offset=0;
                p = new EncapsulationPacket(b, ref Offset);
                if ((p.IsOK) && (p.Command == (ushort)EncapsulationCommands.SendRRData))
                {
                    m = new UCMM_RR_Packet(b, ref Offset);
                    if (m.IsOK)
                    {
                        ushort NbClasses = BitConverter.ToUInt16(b, Offset);
                        Offset += 2;
                        for (int i = 0; i < NbClasses; i++)
                        {
                            SupportedClassLists.Add(BitConverter.ToUInt16(b, Offset));
                            Offset += 2;
                        }
                    }
                }
             }

            return SupportedClassLists;
        }

        public void UnRegisterSession()
        {
            if ((Tcpclient != null) && (SessionHandle != 0))
            {
                EncapsulationPacket p = new EncapsulationPacket(EncapsulationCommands.RegisterSession, SessionHandle);
                byte[] buffer = p.toByteArray();
                Tcpclient.Client.Send(buffer);
                SessionHandle = 0;
            }
        }
    }
}
