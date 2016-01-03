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
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace System.Net.EnIPStack
{
    public delegate void DeviceArrivalHandler(EnIPRemoteDevice device);

    public enum EnIPNetworkStatus { OnLine, OnLineReadRejected, OnLineWriteRejected, OnLineForwardOpenReject, OffLine  };

    public class EnIPClient
    {
        public EnIPUDPTransport udp;
        int TcpTimeout;

        public event DeviceArrivalHandler DeviceArrival;

        // Local endpoint is important for broadcast messages
        // When more than one interface are present, broadcast
        // requests are sent on the first one, not all !
        public EnIPClient(String End_point, int TcpTimeout=100)
        {
            this.TcpTimeout = TcpTimeout;
            udp = new EnIPUDPTransport(End_point, 0);
            udp.EncapMessageReceived += new EncapMessageReceivedHandler(on_MessageReceived);
        }

        void on_MessageReceived(object sender, byte[] packet, Encapsulation_Packet EncapPacket, int offset, int msg_length, System.Net.IPEndPoint remote_address)
        {
            // ListIdentity response
            if ((EncapPacket.Command == EncapsulationCommands.ListIdentity) && (EncapPacket.Length != 0) && EncapPacket.IsOK)
            {
                if (DeviceArrival != null)
                {
                    int NbDevices = BitConverter.ToUInt16(packet, offset);

                    offset += 2;
                    for (int i = 0; i < NbDevices; i++)
                    {
                        EnIPRemoteDevice device = new EnIPRemoteDevice(remote_address, TcpTimeout, packet, ref offset);
                        DeviceArrival(device);
                    }
                }
            }
        }

        // Unicast ListIdentity
        public void DiscoverServers(IPEndPoint ep)
        {
            Encapsulation_Packet p = new Encapsulation_Packet(EncapsulationCommands.ListIdentity);
            p.Command = EncapsulationCommands.ListIdentity;
            udp.Send(p, ep);
            Trace.WriteLine("Send ListIdentity to "+ep.Address.ToString());
        }
        // Broadcast ListIdentity
        public void DiscoverServers()
        {
            DiscoverServers(udp.GetBroadcastAddress());
        }
    }

    public class EnIPRemoteDevice
    {
        // Data comming from the reply to ListIdentity query
        // get set are used by the property grid in EnIPExplorer
        public ushort DataLength;
        public ushort EncapsulationVersion { get; set; }
        private EnIPSocketAddress SocketAddress;
        public ushort VendorId { get; set; }
        public ushort DeviceType { get; set; }
        public ushort ProductCode { get; set; }
        public string Revision { get { return _Revision[0].ToString() + "." + _Revision[1].ToString(); } set { } }
        public byte[] _Revision = new byte[2];
        public short Status { get; set; }
        public uint SerialNumber { get; set; }
        public string ProductName { get; set; }
        public IdentityObjectState State { get; set; }

        private IPEndPoint ep;
        // Not a property to avoid browsable in propertyGrid, also [Browsable(false)] could be used
        public string IPString() { return ep.Address.ToString(); }

        public bool autoConnect = true;
        public bool autoRegisterSession = true;

        private UInt32 SessionHandle=0; // When Register Session is set

        private EnIPTCPClientTransport Tcpclient;

        private object LockTransaction = new object();

        // A global packet for response frames
        private byte[] packet = new byte[1500];

        public List<EnIPClass> SupportedClassLists = new List<EnIPClass>();

        public event DeviceArrivalHandler DeviceArrival;

        private void FromListIdentityResponse(byte[] DataArray, ref int Offset)
        {
            Offset += 2; // 0x000C 

            DataLength = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            EncapsulationVersion = BitConverter.ToUInt16(DataArray, Offset);
            Offset += 2;

            // Maybe it should be used in place of the ep
            // if a host embbed more than one device, sure it sends different tcp/udp port ?
            // FIXME if you know.
            SocketAddress = new EnIPSocketAddress(DataArray, ref Offset);

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

            State = (IdentityObjectState)DataArray[Offset];

            Offset += 1;
        }
        // The remote udp endpoint is given here, it's also the tcp one
        // This constuctor is used with the ListIdentity response buffer
        // No local endpoint given here, the TCP/IP stack should do the job
        // if more than one interface is present
        public EnIPRemoteDevice(IPEndPoint ep, int TcpTimeout, byte[] DataArray, ref int Offset)
        {
            this.ep = ep;
            Tcpclient = new EnIPTCPClientTransport(TcpTimeout);
            FromListIdentityResponse(DataArray, ref Offset);
        }

        public EnIPRemoteDevice(IPEndPoint ep, int TcpTimeout=100)
        {
            this.ep = ep;
            Tcpclient = new EnIPTCPClientTransport(TcpTimeout);
            ProductName = "";
        }

        public void CopyData(EnIPRemoteDevice newset)
        {
            DataLength = newset.DataLength;
            EncapsulationVersion = newset.EncapsulationVersion;
            SocketAddress = newset.SocketAddress;
            VendorId = newset.VendorId;
            DeviceType=newset.DeviceType;
            ProductCode = newset.ProductCode;
            _Revision = newset._Revision;
            Status = newset.Status;
            SerialNumber = newset.SerialNumber;
            ProductName = newset.ProductName;
            State = newset.State;
        }

        // Certainly here if SocketAddress is fullfil it could be the
        // value to test
        // FIXME if you know.
        public bool Equals(EnIPRemoteDevice other)
        {
            return ep.Equals(other.ep);
        }

        public bool IsConnected()
        {
            return Tcpclient.IsConnected();
        }

        public bool Connect()
        {
            if (Tcpclient.IsConnected() == true) return true;

            SessionHandle = 0;

            lock (LockTransaction)
                return Tcpclient.Connect(ep);
        }

        public void Disconnect()
        {
            SessionHandle = 0;

            lock (LockTransaction)
                Tcpclient.Disconnect();
        }

        // Unicast TCP ListIdentity for remote device, not UDP it's my choice because in such way 
        // firewall could be configured only for TCP port (TCP is required for the others exchanges)
        public bool DiscoverServer()
        {
            if (autoConnect) Connect();

            try
            {
                if (Tcpclient.IsConnected())
                {
                    Encapsulation_Packet p = new Encapsulation_Packet(EncapsulationCommands.ListIdentity);
                    p.Command = EncapsulationCommands.ListIdentity;

                    int Length;
                    int Offset = 0;
                    Encapsulation_Packet Encapacket;

                    lock (LockTransaction)
                        Length = Tcpclient.SendReceive(p, out Encapacket, out Offset, ref packet);

                     Trace.WriteLine("Send ListIdentity to " + ep.Address.ToString());

                    if (Length < 26) return false; // never appears in a normal situation

                    if ((Encapacket.Command == EncapsulationCommands.ListIdentity) && (Encapacket.Length != 0) && Encapacket.IsOK)
                    {
                        Offset += 2;
                        FromListIdentityResponse(packet, ref Offset);
                        if (DeviceArrival != null)
                            DeviceArrival(this);
                        return true;
                    }
                    else
                        Trace.WriteLine("Unicast TCP ListIdentity fail");
                }
            }
            catch 
            {
                Trace.WriteLine("Unicast TCP ListIdentity fail");
            }

            return false;
        }

        // Needed for a lot of operations
        private void RegisterSession()
        {
            if (autoConnect) Connect();

            if ((Tcpclient.IsConnected() == true) && (SessionHandle == 0))
            {
                byte[] b = new byte[] { 1, 0, 0, 0 };
                Encapsulation_Packet p = new Encapsulation_Packet(EncapsulationCommands.RegisterSession, 0, b);

                int ret;
                Encapsulation_Packet rep;
                int Offset = 0;

                lock (LockTransaction)
                    ret = Tcpclient.SendReceive(p, out rep, out Offset, ref packet);

                if (ret == 28)
                    if (rep.IsOK)
                        SessionHandle = rep.Sessionhandle;
            }
        }

        public EnIPNetworkStatus SendUCMM_RR_Packet(byte[] DataPath, CIPServiceCodes Service, byte[] data, ref int Offset, ref int Lenght, out byte[] packet)
        {
            packet = this.packet;

            if (autoRegisterSession) RegisterSession();
            if (SessionHandle == 0) return EnIPNetworkStatus.OffLine;

            try
            {
                UCMM_RR_Packet m = new UCMM_RR_Packet(Service, true, DataPath, data);
                Encapsulation_Packet p = new Encapsulation_Packet(EncapsulationCommands.SendRRData, SessionHandle, m.toByteArray());
                
                Encapsulation_Packet rep;
                Offset = 0;

                lock (LockTransaction)
                    Lenght = Tcpclient.SendReceive(p, out rep, out Offset, ref packet);

                String ErrorMsg="TCP mistake";

                if (Lenght > 24)
                {
                    if ((rep.IsOK) && (rep.Command == EncapsulationCommands.SendRRData))
                    {
                        m = new UCMM_RR_Packet(packet, ref Offset, Lenght);
                        if ((m.IsOK) && (m.IsService(Service)))
                        {
                            // all is OK, and Offset is ready set at the beginning of data[]
                            return EnIPNetworkStatus.OnLine;
                        }
                        else
                            ErrorMsg = m.GeneralStatus.ToString();
                    }
                    else
                        ErrorMsg = rep.Status.ToString();
                }

                Trace.WriteLine(Service.ToString() + " : " + ErrorMsg + " - Node " + EnIPPath.GetPath(DataPath) + " - Endpoint " + ep.ToString());

                if (ErrorMsg == "TCP mistake")
                    return EnIPNetworkStatus.OffLine;

                if (Service == CIPServiceCodes.SetAttributeSingle)
                    return EnIPNetworkStatus.OnLineWriteRejected;
                else
                    return EnIPNetworkStatus.OnLineReadRejected;
            }
            catch
            {
                Trace.TraceWarning("Error while sending reques to endpoint "+ep.ToString());
                return EnIPNetworkStatus.OffLine;
            }
        }

        public EnIPNetworkStatus SetClassInstanceAttribut_Data(byte[] DataPath, CIPServiceCodes Service, byte[] data, ref int Offset, ref int Lenght, out byte[] packet)
        {
            return SendUCMM_RR_Packet(DataPath, Service, data, ref Offset, ref Lenght, out packet);
        }
        public EnIPNetworkStatus GetClassInstanceAttribut_Data(byte[] ClassDataPath, CIPServiceCodes Service, ref int Offset, ref int Lenght, out byte[] packet)
        {
            return SendUCMM_RR_Packet(ClassDataPath, Service, null, ref Offset, ref Lenght, out packet);
        }

        public List<EnIPClass> GetObjectList()
        {
            SupportedClassLists.Clear();

            if (autoRegisterSession) RegisterSession();
            if (SessionHandle == 0) return null;

            // Class 2, Instance 1, Attribut 1
            byte[] MessageRouterObjectList = EnIPPath.GetPath("2.1.1");

            int Lenght = 0;
            int Offset = 0;

            if (GetClassInstanceAttribut_Data(MessageRouterObjectList, CIPServiceCodes.GetAttributeSingle, ref Offset, ref Lenght, out packet) == EnIPNetworkStatus.OnLine)
            {
                ushort NbClasses = BitConverter.ToUInt16(packet, Offset);
                Offset += 2;
                for (int i = 0; i < NbClasses; i++)
                {
                    SupportedClassLists.Add(new EnIPClass(this, BitConverter.ToUInt16(packet, Offset)));
                    Offset += 2;
                }
            }
            return SupportedClassLists;
        }

        public void UnRegisterSession()
        {
            if (SessionHandle != 0)
            {
                Encapsulation_Packet p = new Encapsulation_Packet(EncapsulationCommands.RegisterSession, SessionHandle);

                lock (LockTransaction)
                    Tcpclient.Send(p);

                SessionHandle = 0;
            }
        }
    }

    // Device data dictionnary top hierarchy 
    public abstract class EnIPCIPObject
    {
        // set is present to shows not greyed in the property grid
        public ushort Id { get; set; }
        public EnIPNetworkStatus Status { get; set; }
        public object DecodedMembers { get; set; }
        public byte[] RawData { get; set; }

        public abstract EnIPNetworkStatus ReadDataFromNetwork();
        public abstract EnIPNetworkStatus WriteDataToNetwork();

        public EnIPRemoteDevice RemoteDevice;

        protected EnIPNetworkStatus ReadDataFromNetwork(byte[] Path, CIPServiceCodes Service)
        {
            int Offset = 0;
            int Lenght = 0;
            byte[] packet;
            Status = RemoteDevice.GetClassInstanceAttribut_Data(Path, Service, ref Offset, ref Lenght, out packet);

            if (Status == EnIPNetworkStatus.OnLine)
            {
                RawData = new byte[Lenght - Offset];
                Array.Copy(packet, Offset, RawData, 0, Lenght - Offset);
            }
            return Status;
        }

        protected EnIPNetworkStatus WriteDataToNetwork(byte[] Path, CIPServiceCodes Service)
        {
            int Offset = 0;
            int Lenght = 0;
            byte[] packet;

            Status = RemoteDevice.SetClassInstanceAttribut_Data(Path, Service, RawData, ref Offset, ref Lenght, out packet);

            return Status;
        }
    }
    
    public class EnIPClass : EnIPCIPObject
    {

        public EnIPClass(EnIPRemoteDevice RemoteDevice, ushort Id)
        {
            this.Id = Id;
            this.RemoteDevice = RemoteDevice;
            Status=EnIPNetworkStatus.OffLine;
        }

        public override EnIPNetworkStatus WriteDataToNetwork()
        {
            return EnIPNetworkStatus.OnLineWriteRejected;
        }

        public override EnIPNetworkStatus ReadDataFromNetwork()
        {
            byte[] ClassDataPath = EnIPPath.GetPath(Id, 0, null);
            return ReadDataFromNetwork(ClassDataPath, CIPServiceCodes.GetAttributesAll);
        }
    }

    public class EnIPInstance : EnIPCIPObject
    {
        public EnIPClass Class;

        public EnIPInstance(EnIPClass Class, ushort Id)
        {
            this.Id = Id;
            this.Class = Class;
            this.RemoteDevice = Class.RemoteDevice;
            Status = EnIPNetworkStatus.OffLine;
        }

        public override EnIPNetworkStatus WriteDataToNetwork()
        {
            return EnIPNetworkStatus.OnLineWriteRejected;
        }

        public override EnIPNetworkStatus ReadDataFromNetwork()
        {
            byte[] DataPath = EnIPPath.GetPath(Class.Id, Id, null);
            return ReadDataFromNetwork(DataPath, CIPServiceCodes.GetAttributesAll);
        }

        public EnIPNetworkStatus GetClassInstanceAttributList()
        {
            byte[] DataPath = EnIPPath.GetPath(Class.Id, Id, null);

            int Offset = 0;
            int Lenght = 0;
            byte[] packet;

            Status = RemoteDevice.GetClassInstanceAttribut_Data(DataPath, CIPServiceCodes.GetAttributeList, ref Offset, ref Lenght, out packet);

            return Status;
        }

        // Never tested, certainly not like this
        public bool CreateRemoteInstance()
        {
            byte[] ClassDataPath = EnIPPath.GetPath(Class.Id, Id, null);

            int Offset = 0;
            int Lenght = 0;
            byte[] packet;

            Status = RemoteDevice.SendUCMM_RR_Packet(ClassDataPath, CIPServiceCodes.Create, RawData, ref Offset, ref Lenght, out packet);

            if (Status == EnIPNetworkStatus.OnLine)
                return true;
            else
                return false;
        }
    }

    public delegate void T2OEventHandler(EnIPAttribut sender);

    public class EnIPAttribut : EnIPCIPObject
    {
        public EnIPInstance Instance;
        // Forward Open
        public uint T2O_ConnectionId, O2T_ConnectionId;
        // It got the required data to close the previous ForwardOpen
        ForwardClose_Packet closePkt;

        public event T2OEventHandler T2OEvent;

        public EnIPAttribut(EnIPInstance Instance, ushort Id)
        {
            this.Id = Id;
            this.Instance = Instance;
            this.RemoteDevice = Instance.RemoteDevice;
            Status = EnIPNetworkStatus.OffLine;
        }

        public override EnIPNetworkStatus WriteDataToNetwork()
        {
            byte[] DataPath = EnIPPath.GetPath(Instance.Class.Id, Instance.Id, Id);
            return WriteDataToNetwork(DataPath, CIPServiceCodes.SetAttributeSingle);
        }

        public override EnIPNetworkStatus ReadDataFromNetwork()
        {
            byte[] DataPath = EnIPPath.GetPath(Instance.Class.Id, Instance.Id, Id);
            return ReadDataFromNetwork(DataPath, CIPServiceCodes.GetAttributeSingle);
        }
        // Coming from an udp class1 device, with a previous ForwardOpen action
        public void On_ItemMessageReceived(object sender, byte[] packet, SequencedAddressItem ItemPacket, int offset, int msg_length, IPEndPoint remote_address)
        {           
            if (ItemPacket.ConnectionId != T2O_ConnectionId) return;
            RawData = new byte[msg_length - offset];
            Array.Copy(packet, offset, RawData, 0, RawData.Length);

            if (T2OEvent != null)
                T2OEvent(this);
        }

        public EnIPNetworkStatus ForwardOpen(bool p2p, bool T2O, bool O2T, uint CycleTime, int DurationSecond)
        {
            if (RawData==null) return EnIPNetworkStatus.OnLineForwardOpenReject;

            byte[] DataPath = EnIPPath.GetPath(Instance.Class.Id, Instance.Id, Id);
            ForwardOpen_Packet FwPkt = new ForwardOpen_Packet(DataPath, p2p, T2O, O2T, (ushort)RawData.Length);
            if (T2O)
            {
                T2O_ConnectionId = FwPkt.T2O_ConnectionId;
                // Change ForwardOpen_Packet default value
                FwPkt.T2O_RPI = CycleTime * 1000;
                FwPkt.O2T_RPI = 0;
            }
            if (O2T)
            {
                O2T_ConnectionId = FwPkt.O2T_ConnectionId;
                // Change ForwardOpen_Packet default value
                FwPkt.T2O_RPI = 0;
                FwPkt.O2T_RPI = CycleTime * 1000;
            }

            if (CycleTime == 0) FwPkt.SetTriggerType(TransportClassTriggerAttribute.ChangeOfState);

            int Offset = 0;
            int Lenght = 0;
            byte[] packet;

            Status = RemoteDevice.SendUCMM_RR_Packet(EnIPPath.GetPath(6, 1), CIPServiceCodes.ForwardOpen, FwPkt.toByteArray(), ref Offset, ref Lenght, out packet);
            // Offset is ready set at the beginning of data : ie ForwardOpen_Packet response
            
            closePkt = new ForwardClose_Packet(FwPkt);

            // Send a close later
            if ((Status == EnIPNetworkStatus.OnLine)&&(DurationSecond>=0))
            {
                byte[] closePktnew = closePkt.toByteArray();

                ThreadPool.QueueUserWorkItem(
                    (o) =>
                    {
                        Thread.Sleep(DurationSecond * 1000);

                        int Offset2 = 0;
                        int Lenght2 = 0;

                        RemoteDevice.SendUCMM_RR_Packet(EnIPPath.GetPath(6, 1), CIPServiceCodes.ForwardClose, closePktnew, ref Offset2, ref Lenght2, out packet);
                    }
                );
            }
            return Status;
        }

        public EnIPNetworkStatus ForwardClose()
        {
            int Offset = 0;
            int Lenght = 0;
            byte[] packet;
            return RemoteDevice.SendUCMM_RR_Packet(EnIPPath.GetPath(6, 1), CIPServiceCodes.ForwardClose, closePkt.toByteArray(), ref Offset, ref Lenght, out packet);
        }
    }
}

