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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace System.Net.EnIPStack
{
    // Volume 1 : C-1.4.2 Logical Segment
    // Remember for 16 bits address : (0x21 or 0x25 or 0x31) - 0x00 - 0xPF - 0xpf
    // also a pad 0x00 must be set  for 32 bits address
    public static class Path
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

        public static byte[] GetPath(ushort Class, ushort? Instance=null, ushort? Attribut=null)
        {

            byte[] path = new byte[12];

            int size=0;
            Fit(path,ref size,Class,0x20);

            // It seems that this Instance value is required : 0 is used to access class data
            if (Instance != null)
                Fit(path, ref size, Instance.Value, 0x24);
   
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
    // no explicit information to distinguish between a request and a reply
    public class EncapsulationPacket
    {
        public UInt16 Command;
        public UInt16 Length;
        public UInt32 Sessionhandle;
        //  Volume 2 : Table 2-3.3 Error Codes - 0x0000 Success, others value error
        public UInt32 Status;
        // byte copy of the request into the response
        public byte[] SenderContext = new byte[8];
        public UInt32 Options;
        // Not used in the EncapsulationPacket receive objects
        public byte[] Encapsulateddata=null;

        public bool IsOK { get { return Status == 0; } }

        public EncapsulationPacket(EncapsulationCommands Command, uint Sessionhandle=0, byte[] Encapsulateddata=null) 
        {
            this.Command = (UInt16)Command;
            this.Sessionhandle = Sessionhandle;
            this.Encapsulateddata = Encapsulateddata;
            if (Encapsulateddata != null)
                Length = (UInt16)Encapsulateddata.Length;
        }
    
        // From network
        public EncapsulationPacket(byte[] Packet, ref int Offset)
        {
            Command = BitConverter.ToUInt16(Packet, Offset);
            Offset += 2;
            Length = BitConverter.ToUInt16(Packet, Offset);

            if (Packet.Length < 24 + Length)
            {
                Trace.TraceWarning("Bad encapsulation packet size");
                throw new Exception("Bad packet size");
            }

            Offset += 2;
            Sessionhandle = BitConverter.ToUInt32(Packet, Offset);
            Offset += 4;
            Status = BitConverter.ToUInt32(Packet, Offset);
            Offset += 4;
            Array.Copy(Packet, Offset, SenderContext, 0, 8);
            Offset += 8;
            Options = BitConverter.ToUInt32(Packet, Offset);
            Offset += 4;  // value 24
        }

        public byte[] toByteArray()
        {
            byte[] ret = new byte[24 + Length];
            
            Buffer.BlockCopy(BitConverter.GetBytes(Command), 0, ret, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, ret, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Sessionhandle), 0, ret, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Status), 0, ret, 8, 4);
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
        public byte Service;

        // Only for response packet
        public byte GeneralStatus;
        public byte AdditionalStatus_Size;
        public ushort[] AdditionalStatus;

        // Only for request packet
        public byte[] Path;
        public byte[] Data;

        public bool IsOK { get { return GeneralStatus == 0; } }

        public UCMM_RR_Packet() { } 

        public UCMM_RR_Packet(byte[] DataArray, ref int Offset)
        {
            // Skip 16 bytes
            Offset += 16;

            Service = DataArray[Offset];
            Offset += 1;

            //Skip reserved byte
            Offset += 1;

            GeneralStatus = DataArray[Offset]; // only 0 is OK
            Offset += 1;

            AdditionalStatus_Size = DataArray[Offset];
            Offset++;

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

        public byte[] toByteArray()
        {
            if ((Path == null) || ((Path.Length%2)!=0))
            {
                Trace.TraceError("Request_Path is not OK");
                throw new Exception("Request_Path is not OK");
            }

            byte[] retVal = new byte[10 + 6 + 2 + Path.Length + (Data == null ? 0 : Data.Length)];

            retVal[6] = 2;
            retVal[12] = 0xB2;
            retVal[14] = (byte)(2 + Path.Length+(Data==null ? 0 : Data.Length));

            retVal[16] = Service;
            retVal[17] = (byte)(Path.Length >> 1);

            Array.Copy(Path, 0, retVal, 10+8, Path.Length);

            if (Data != null)
                Array.Copy(Data, 0, retVal, 10 + 8 + Path.Length, Data.Length);

            return retVal;
        }        
    }

    public class SocketAddress
    {
        public short sin_family;
        public ushort sin_port;
        public uint sin_addr;

        // Too small for IPV6 !
        public byte[] sin_zero = new byte[8];

        public byte[] toByteArray()
        {
            byte[] retVal = new byte[16];

            Buffer.BlockCopy(BitConverter.GetBytes(sin_family), 0, retVal, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(sin_port), 0, retVal, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(sin_addr), 0, retVal, 4, 4);

            return retVal;
        }

        public SocketAddress(byte[] DataArray, ref int Offset)
        {
            sin_family = BitConverter.ToInt16(DataArray, Offset);
            sin_port = BitConverter.ToUInt16(DataArray, Offset + 2);
            sin_addr = BitConverter.ToUInt32(DataArray, Offset + 4);

            Offset += 16;
        }
    }
}