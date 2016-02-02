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
using System.Diagnostics;

namespace System.Net.EnIPStack
{
    // Volume 1 : C-1.4.2 Logical Segment
    // Remember for 16 bits address : (0x21 or 0x25 or 0x31) - 0x00 - 0xPF - 0xpf
    // also a pad 0x00 must be set for 32 bits address. No supported here.
    public static class EnIPPath
    {
        private static void Fit(byte[] path, ref int offset, ushort value, byte code)
        {
            if (value > 255)
            {
                path[offset] = (byte)(code|0x1);
                path[offset + 2] = (byte)((value & 0xFF00) >> 8);
                path[offset + 3] = (byte)(value & 0xFF);
                offset += 4;
            }
            else
            {
                path[offset] = (byte)code;
                path[offset + 1] = (byte)(value & 0xFF);
                offset += 2;
            }
        }

        // Instance seems to be only byte
        // FIXME
        public static byte[] GetPath(ushort Class, ushort Instance, ushort? Attribut=null)
        {

            byte[] path = new byte[12];

            int size=0;
            Fit(path,ref size,Class,0x20);

            // It seems that this Instance value is always required : 0 is used to access class data
            // Volume 1 : Figure 1-2.5 Instance #0 Example
            Fit(path, ref size, Instance, 0x24);
   
            if (Attribut != null)
                Fit(path, ref size, Attribut.Value, 0x30);

            byte[] Ret = new byte[size];
            Array.Copy(path, Ret, size);

            return Ret;
        }

        // Given in the form Class.Instance or Class.Instance.Attribut
        // for Class data should be Class.0
        public static byte[] GetPath(String path)
        {
            String[] s=path.Split('.');
            if (s.Length==3)
                return GetPath(Convert.ToUInt16(s[0]), Convert.ToUInt16(s[1]), Convert.ToUInt16(s[2]));
            if (s.Length == 2)
                return GetPath(Convert.ToUInt16(s[0]), Convert.ToUInt16(s[1]), null);
            return null;
        }

        // Base on Volume 1 : Figure C-1.3 Port Segment Encoding
        // & Table C-1.2 Port Segment Examples
        // & Volume 2 : 3-3.7 Connection Path
        // IPendPoint in the format x.x.x.x:x, port is optional
        private static byte[] GetExtendedPath(String IPendPoint, byte[] LogicalSeg)
        {
            byte[] PortSegment = Encoding.ASCII.GetBytes(IPendPoint);

            int IPlenght = PortSegment.Length;
            if ((IPlenght % 2) != 0) IPlenght++;

            byte[] FullPath = new byte[LogicalSeg.Length + IPlenght + 2];

            // to be FIXED : Port number !
            FullPath[0] = 0x15;
            FullPath[1] = (byte)IPendPoint.Length;
            Array.Copy(PortSegment, 0, FullPath, 2, PortSegment.Length);
            Array.Copy(LogicalSeg, 0, FullPath, 2 + IPlenght, LogicalSeg.Length);

            return FullPath;
        }

        // Add a Data Member to the Path
        public static byte[] AddDataSegment(byte[] ExtendedPath, byte[] Data)
        {
            byte[] FullPath = new byte[Data.Length + ExtendedPath.Length + 2];
            Array.Copy(ExtendedPath, FullPath, ExtendedPath.Length);
            FullPath[ExtendedPath.Length] = 0x80;
            FullPath[ExtendedPath.Length + 1] = (byte)(Data.Length / 2 + (Data.Length % 2));
            Array.Copy(Data, 0, FullPath, ExtendedPath.Length + 2, Data.Length);

            return FullPath;
        }
        public static byte[] GetExtendedPath(String IPendPoint, String LogicalSegment)
        {
            byte[] LogicalSeg = GetPath(LogicalSegment);
            byte[] ExtendedPath = GetExtendedPath(IPendPoint, LogicalSeg);
            return ExtendedPath;
        }
        public static byte[] GetExtendedPath(String IPAdress, ushort Class, ushort Instance, ushort? Attribut = null)
        {
            byte[] LogicalSeg = GetPath(Class, Instance, Attribut);
            byte[] ExtendedPath = GetExtendedPath(IPAdress, LogicalSeg);
            return ExtendedPath;
        }

        public static string GetPath(byte[] path)
        {
            StringBuilder sb=new StringBuilder();

            int i = 0;
            do
            {
                if (i != 0) sb.Append('.');
                // Missing 32 bits elements
                if ((path[i] & 3) == 1)
                {

                    sb = sb.Append((path[i + 2] << 8 | path[i + 3]).ToString());
                    i += 4;
                }
                else
                {
                    sb = sb.Append(path[i + 1].ToString());
                    i += 2;
                }
            } while (i < path.Length);

            return sb.ToString();
        }
    }

    // Volume 2 : Table 2-3.1 Encapsulation Packet
    // No explicit information to distinguish between a request and a reply
    public class Encapsulation_Packet
    {
        public EncapsulationCommands Command;
        public UInt16 Length;
        public UInt32 Sessionhandle;
        //  Volume 2 : Table 2-3.3 Error Codes - 0x0000 Success, others value error
        public EncapsulationStatus Status = EncapsulationStatus.Invalid_Session_Handle;
        // byte copy of the request into the response
        public byte[] SenderContext = new byte[8];
        public UInt32 Options;
        // Not used in the EncapsulationPacket receive objects
        public byte[] Encapsulateddata=null;

        public bool IsOK { get { return Status == EncapsulationStatus.Success; } }

        public Encapsulation_Packet(EncapsulationCommands Command, uint Sessionhandle=0, byte[] Encapsulateddata=null) 
        {
            this.Command = Command;
            this.Sessionhandle = Sessionhandle;
            this.Encapsulateddata = Encapsulateddata;
            if (Encapsulateddata != null)
                Length = (UInt16)Encapsulateddata.Length;
            else
                Length = 0;
        }
    
        // From network
        public Encapsulation_Packet(byte[] Packet, ref int Offset, int Length)
        {
            ushort Cmd=BitConverter.ToUInt16(Packet, Offset);

            if (!(Enum.IsDefined(typeof(EncapsulationCommands), Cmd)))
            {
                Status = EncapsulationStatus.Unsupported_Command;
                return;
            }

            Command = (EncapsulationCommands)Cmd;
            Offset += 2;
            this.Length = BitConverter.ToUInt16(Packet, Offset);

            if (Length < 24 + this.Length)
            {
                Status = EncapsulationStatus.Invalid_Length;
                return;
            }

            Offset += 2;
            Sessionhandle = BitConverter.ToUInt32(Packet, Offset);
            Offset += 4;
            Status = (EncapsulationStatus)BitConverter.ToUInt32(Packet, Offset);
            Offset += 4;
            Array.Copy(Packet, Offset, SenderContext, 0, 8);
            Offset += 8;
            Options = BitConverter.ToUInt32(Packet, Offset);
            Offset += 4;  // value 24
        }

        public byte[] toByteArray(EncapsulationStatus Status = EncapsulationStatus.Success)
        {
            byte[] ret = new byte[24 + Length];
            
            Array.Copy(BitConverter.GetBytes((ushort)Command), 0, ret, 0, 2);
            Array.Copy(BitConverter.GetBytes(Length), 0, ret, 2, 2);
            Array.Copy(BitConverter.GetBytes(Sessionhandle), 0, ret, 4, 4);
            Array.Copy(BitConverter.GetBytes((uint)Status), 0, ret, 8, 4);
            Array.Copy(SenderContext, 0, ret, 12, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(Options), 0, ret, 20, 4);
            if (Encapsulateddata!=null)
                Array.Copy(Encapsulateddata, 0, ret, 24, Encapsulateddata.Length);
            return ret;
        }
    }   

    // Volume 1 : paragraph 2-4 Message Router Request/Response Formats
    public class UCMM_RR_Packet
    {
        // Partial Header
        public ushort ItemCount = 2;
        public CommonPacketItemIdNumbers IdemId = CommonPacketItemIdNumbers.UnConnectedDataItem;
        public ushort DataLength;

        // High bit 0 for query, 1 for response
        public byte Service;

        // Only for response packet
        public CIPGeneralSatusCode GeneralStatus;
        public byte AdditionalStatus_Size;
        public ushort[] AdditionalStatus;

        // Only for request packet
        public byte[] Path;
        public byte[] Data;

        public bool IsOK { get { return GeneralStatus == CIPGeneralSatusCode.Success; } }

        public UCMM_RR_Packet(CIPServiceCodes Service, bool IsRequest, byte[] Path, byte[] Data) 
        {            
            this.Service = (byte)Service;
            if (!IsRequest)
                this.Service = (byte)(this.Service | 0x80);

            this.Path = Path;
            this.Data = Data;
        }

        public bool IsService(CIPServiceCodes Service)
        {
            byte s=(byte)(this.Service & 0x7F);

            if (s == (byte)Service) return true;

            if ((this.Service > 0x80 )&& (s==(byte)CIPServiceCodes.UnconnectedSend))
                return true;
            
            return false;
        }

        public bool IsResponse  { get { return Service > 0x80; } }
        public bool IsQuery { get { return Service < 0x80; } }

        // up to now it's only a response paquet decoding
        public UCMM_RR_Packet(byte[] DataArray, ref int Offset, int Lenght)
        {
            if ((Offset + 20) > Lenght)
                GeneralStatus = CIPGeneralSatusCode.Not_enough_data;

            // Skip 16 bytes of the Command specific data
            // Volume 2 : Table 3-2.1 UCMM Request & Table 3-2.2 UCMM Reply
            Offset += 16;

            Service = DataArray[Offset];
            Offset += 1;

            //Skip reserved byte
            Offset += 1;

            GeneralStatus = (CIPGeneralSatusCode)DataArray[Offset]; // only 0 is OK
            Offset += 1;

            AdditionalStatus_Size = DataArray[Offset];
            Offset += 1;

            if ((Offset + AdditionalStatus_Size *2) > Lenght)
                GeneralStatus = CIPGeneralSatusCode.Not_enough_data;

            if (AdditionalStatus_Size > 0)
            {
                AdditionalStatus = new ushort[AdditionalStatus_Size];
                for (int i = 0; i < AdditionalStatus_Size; i++)
                {
                    AdditionalStatus[i] = BitConverter.ToUInt16(DataArray, Offset);
                    Offset += 2;
                }
            }
        }

        // up to now it's only a request paquet
        public byte[] toByteArray()
        {
            if ((Path == null) || ((Path.Length%2)!=0))
            {
                Trace.TraceError("Request_Path is not OK");
                return null;
            }

            DataLength = (ushort)(2 + Path.Length + (Data == null ? 0 : Data.Length));

            // Volume 2 : Table 3-2.1 UCMM Request
            byte[] retVal = new byte[10 + 6 + DataLength];
            Array.Copy(BitConverter.GetBytes(ItemCount), 0, retVal, 6, 2);

            Array.Copy(BitConverter.GetBytes((ushort)this.IdemId), 0, retVal, 12, 2);

            Array.Copy(BitConverter.GetBytes(DataLength), 0, retVal, 14, 2);

            retVal[16] = Service;
            retVal[17] = (byte)(Path.Length >> 1);

            Array.Copy(Path, 0, retVal, 10+8, Path.Length);

            if (Data != null)
                Array.Copy(Data, 0, retVal, 10 + 8 + Path.Length, Data.Length);

            return retVal;
        }        
    }

    // Volume 1 : Table 3-5.16 Forward_Open
    // class for both request and response
    // A lot of ushort in the bullshit specification, but byte in fact
    // .. Codesys 3.5 EIP scanner, help a lot
    public class ForwardOpen_Packet
    {
        // TimeOut (duration) in ms = 2^Priority_TimeTick * Timeout_Ticks
        // So with Priority_TimeTick=10, Timeout_Ticks is ~ the number of seconds
        // FIXME:
        // I don't understand the usage, with Wago Plc it's not a timeout for the
        // continuous udp flow.
        public byte Priority_TimeTick=10;
        public byte Timeout_Ticks=10;

        private static uint _ConnectionId;
        public uint O2T_ConnectionId;
        public uint T2O_ConnectionId;

        // Assume only one client in this application
        public static ushort ConnectionSerialNumber= (ushort)new Random().Next(65535);
        public static ushort OriginatorVendorId = 0xFADA;
        public static uint OriginatorSerialNumber = 0x8BADF00D;

        // 0 => *4
        public byte ConnectionTimeoutMultiplier;
        public byte[] Reserved = new byte[3];
        // It's O2T_API for reply, in microseconde
        public uint O2T_RPI=0;
        public ushort O2T_ConnectionParameters;
        // It's T2A_API for reply
        public uint T2O_RPI = 0;
        public ushort T2O_ConnectionParameters;
        // volume 1 : Figure 3-4.2 Transport Class Trigger Attribute
        public byte TransportTrigger;
        public byte Connection_Path_Size;
        public byte[] Connection_Path;

        // Only use for request up to now
        // O2T & T2O could be use at the same time
        // using a Connection_Path with more than 1 reference
        // 1 Path : path is for Consumption & Production
        // 2 Path : First path is for Consumption, second path is for Production.
        public ForwardOpen_Packet(byte[] Connection_Path, bool p2p, bool T2O, bool O2T, ushort datasize, uint? ConnectionId=null) 
        {

            this.Connection_Path = Connection_Path;

            if (ConnectionId == null)
            {
                // volume 2 : 3-3.7.1.3 Pseudo-Random Connection ID Per Connection
                _ConnectionId++;
                _ConnectionId = _ConnectionId | (uint)(new Random().Next(65535) << 16);
            }
            else
                _ConnectionId = ConnectionId.Value;

            if (O2T)
                O2T_ConnectionId = _ConnectionId;
            if (T2O)
                T2O_ConnectionId = _ConnectionId;

            // Volume 1:  chapter 3-5.5.1.1
            /* Sample for wireshark
            T->O Network Connection Parameters: 0x463b
            0... .... .... .... = Owner: Exclusive (0)
            .10. .... .... .... = Connection Type: Point to Point (2)
            .... 01.. .... .... = Priority: High Priority (1)
            .... ..1. .... .... = Connection Size Type: Variable (1)
            .... ...0 0011 1011 = Connection Size: 59
            */
            ushort ConnectionParameters;
            if (p2p) ConnectionParameters = 0x4600; else ConnectionParameters = 0x2600;
            // ConnectionParameters = (ushort)(ConnectionParameters | 0x8000);

            if (T2O)
            {
                T2O_ConnectionParameters = ConnectionParameters;
                // 2 bytes CIP class 1 sequence count + datasize bytes application data
                ConnectionParameters += (ushort)(datasize + 2);
                TransportTrigger = 0x01; // Client class 1, cyclic
            }
            if (O2T)
            {
                O2T_ConnectionParameters = ConnectionParameters;
                // 2 bytes CIP class 1 sequence count + 4 bytes 32-bit real time header + datasize bytes application data
                ConnectionParameters += (ushort)(datasize + 2 + 4);
                TransportTrigger = 0x81; // Server class 1
            }

        }

        public void SetTriggerType(TransportClassTriggerAttribute type)
        {
            TransportTrigger = (byte)((TransportTrigger & 0x8F) | (byte)type);
        }

        // by now only use for request
        public byte[] toByteArray()
        {
            int PathSize = Connection_Path.Length / 2 + (Connection_Path.Length % 2);
            Connection_Path_Size = (byte)PathSize;

            byte[] fwopen=new byte[36+PathSize*2];

            fwopen[0] = Priority_TimeTick;
            fwopen[1] = Timeout_Ticks;
            Array.Copy(BitConverter.GetBytes(O2T_ConnectionId), 0, fwopen, 2, 4);
            Array.Copy(BitConverter.GetBytes(T2O_ConnectionId), 0, fwopen, 6, 4);
            Array.Copy(BitConverter.GetBytes(ConnectionSerialNumber), 0, fwopen, 10, 2);
            Array.Copy(BitConverter.GetBytes(OriginatorVendorId), 0, fwopen, 12, 2);
            Array.Copy(BitConverter.GetBytes(OriginatorSerialNumber), 0, fwopen, 14, 4);
            fwopen[18] = ConnectionTimeoutMultiplier;
            Array.Copy(Reserved, 0, fwopen, 19, 3);
            Array.Copy(BitConverter.GetBytes(O2T_RPI), 0, fwopen, 22, 4);
            Array.Copy(BitConverter.GetBytes(O2T_ConnectionParameters), 0, fwopen, 26, 2);
            Array.Copy(BitConverter.GetBytes(T2O_RPI), 0, fwopen, 28, 4);
            Array.Copy(BitConverter.GetBytes(T2O_ConnectionParameters), 0, fwopen, 32, 2);
            fwopen[34] = TransportTrigger;
            fwopen[35] = Connection_Path_Size;
            Array.Copy(Connection_Path, 0, fwopen, 36, Connection_Path.Length);

            return fwopen;
        }
    }

    public class ForwardClose_Packet
    {
        ForwardOpen_Packet OrignalPkt;
        public ForwardClose_Packet(ForwardOpen_Packet FwOpen)
        {
            OrignalPkt = FwOpen;
        }
        // by now only use for request
        public byte[] toByteArray()
        {
            byte[] fwclose = new byte[12 + OrignalPkt.Connection_Path_Size * 2];
            fwclose[0] = OrignalPkt.Priority_TimeTick;
            fwclose[1] = OrignalPkt.Timeout_Ticks;
            Array.Copy(BitConverter.GetBytes(ForwardOpen_Packet.ConnectionSerialNumber), 0, fwclose, 2, 2);
            Array.Copy(BitConverter.GetBytes(ForwardOpen_Packet.OriginatorVendorId), 0, fwclose, 4, 2);
            Array.Copy(BitConverter.GetBytes(ForwardOpen_Packet.OriginatorSerialNumber), 0, fwclose, 6, 4);
            fwclose[10] = OrignalPkt.Connection_Path_Size;
            fwclose[11] = 0;
            Array.Copy(OrignalPkt.Connection_Path, 0, fwclose, 12, OrignalPkt.Connection_Path.Length);
            return fwclose;
        }
    }

    // This is here a SequencedAddress + a connected Data Item
    // Receive via Udp after a ForwardOpen
    // Volume 2 : 2-6 Common Packet Format
    public class SequencedAddressItem
    {
        // SequencedAddress
        public ushort TypeId;
        public ushort Lenght=8;
        public uint ConnectionId;
        public uint SequenceNumber;
        // Connected or Unconnected Data Item
        public ushort TypeId2;
        public ushort Lenght2 = 8;
        public ushort SequenceCount; // ??

        public byte[] data;

        public SequencedAddressItem(uint ConnectionId, uint SequenceNumber=0, byte[] data=null)
        {
            this.ConnectionId = ConnectionId;
            this.SequenceNumber = SequenceNumber;
            this.data = data;
        }

        public SequencedAddressItem(byte[] DataArray, ref int Offset, int Lenght)
        {
            // Itemcount=2, by now, could change maybe in this code !
            Offset += 2;
            TypeId=BitConverter.ToUInt16(DataArray, Offset);
            if (TypeId != (ushort)CommonPacketItemIdNumbers.SequencedAddressItem) return;
            Offset += 4;
            ConnectionId = BitConverter.ToUInt32(DataArray, Offset);
            Offset += 4;
            SequenceNumber = BitConverter.ToUInt32(DataArray, Offset);
            Offset += 4;

            TypeId2 = BitConverter.ToUInt16(DataArray, Offset);
            if ((TypeId2 != (ushort)CommonPacketItemIdNumbers.ConnectedDataItem) &&
                (TypeId2 != (ushort)CommonPacketItemIdNumbers.UnConnectedDataItem)) return;

            Offset += 2;
            Lenght2 = BitConverter.ToUInt16(DataArray, Offset);            
            Offset += 2;

            if ((Lenght2 + Offset) != Lenght)
            {
                TypeId = 0; // invalidate the frame
                return;
            }

            if (Lenght2 != 0)
            {
                SequenceCount = BitConverter.ToUInt16(DataArray, Offset);
                Offset += 2;
            }
            // Offset is now at the beginning of the raw data
        }

        public byte[] toByteArray(byte[] newdata=null)
        {
            byte[] retVal;

            if (newdata != null) data = newdata;    

            if (data == null)
            {
                Lenght2 = 0;
                retVal = new byte[18];
            }
            else
            {
                Lenght2 = (ushort)data.Length;
                retVal = new byte[20 + Lenght2];
            }

            // Itemcount=2
            retVal[0] = 2;
            Array.Copy(BitConverter.GetBytes((ushort)CommonPacketItemIdNumbers.SequencedAddressItem), 0, retVal, 2, 2);
            Array.Copy(BitConverter.GetBytes(Lenght), 0, retVal, 4, 2);
            Array.Copy(BitConverter.GetBytes(ConnectionId), 0, retVal, 6, 4);
            Array.Copy(BitConverter.GetBytes(SequenceNumber), 0, retVal, 10, 4);
            Array.Copy(BitConverter.GetBytes((ushort)CommonPacketItemIdNumbers.ConnectedDataItem), 0, retVal, 14, 2);

            Array.Copy(BitConverter.GetBytes(Lenght2), 0, retVal, 16, 2);

            if (Lenght2 != 0)
            {
                // Don't really understand this sequence count, so put something
                Array.Copy(BitConverter.GetBytes((ushort)SequenceNumber), 0, retVal, 18, 2);
                Array.Copy(data, 0, retVal, 20, Lenght2);
            }

            SequenceNumber++;

            return retVal;
        }

        public bool IsOK { get { return ((TypeId == 0x8002) && (TypeId2 == 0x00b1)); } }
    }

    // Volume 2 : 2-6.3.3 Sockaddr Info Item
    public class EnIPSocketAddress
    {

        public short sin_family;
        public ushort sin_port;
        public uint sin_addr;

        // Too small for IPV6 !
        // public byte[] sin_zero = new byte[8];
        
        public EnIPSocketAddress(IPEndPoint ep)
        {
            sin_family = (short)ep.AddressFamily;
            sin_port = (ushort)ep.Port;
            sin_addr = BitConverter.ToUInt32(ep.Address.GetAddressBytes(),0);
        }
        public EnIPSocketAddress(byte[] DataArray, ref int Offset)
        {
            sin_family = (short)((DataArray[0 + Offset] << 8) + DataArray[1 + Offset]);
            sin_port = (ushort)((DataArray[2 + Offset] << 8) + DataArray[3 + Offset]);
            sin_addr = (uint)((DataArray[7 + Offset] << 24) + (DataArray[6 + Offset] << 16) 
                            + (DataArray[5 + Offset] << 8) + DataArray[4 + Offset]);
            Offset += 16;
        }

        public IPEndPoint toIPEndpoint()
        {
            IPEndPoint ep = new IPEndPoint(new IPAddress(sin_addr), sin_port);
            return ep;
        }

        public byte[] toByteArray()
        {
            byte[] retVal;

            retVal = new byte[16];
           

            retVal[0] = (byte)(sin_family >> 8);
            retVal[1] = (byte)(sin_family & 0xFF);
            retVal[2] = (byte)(sin_port >> 8);
            retVal[3] = (byte)(sin_port & 0xFF);

            retVal[4] = (byte)(sin_addr & 0xFF);
            retVal[5] = (byte)((sin_addr & 0xFF00) >> 8);
            retVal[6] = (byte)((sin_addr & 0xFF0000) >> 16);
            retVal[7] = (byte)((sin_addr & 0xFF000000) >> 24);

            return retVal;
        }
    }
}