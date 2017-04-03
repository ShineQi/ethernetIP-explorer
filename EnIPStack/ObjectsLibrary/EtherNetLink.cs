/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2017 Frederic Chaxel <fchaxel@free.fr>
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
using System.Net.NetworkInformation;

namespace System.Net.EnIPStack.ObjectsLibrary
{
    public class CIP_EtherNetLink_class : CIPObject
    {
        public UInt16? Revision { get; set; }
        public UInt16? Max_Instance { get; set; }
        public UInt16? Number_of_Instances { get; set; }
        public byte[] Remain_Undecoded_Bytes { get; set; }

        public override string ToString()
        {
            return "EtherNetLink class";
        }
        public override bool SetRawBytes(byte[] b)
        {
            int Idx = 0;

            Revision = GetUInt16(ref Idx, b);
            Max_Instance = GetUInt16(ref Idx, b);
            Number_of_Instances = GetUInt16(ref Idx, b);

            if (Idx<b.Length)
            {
                Remain_Undecoded_Bytes = new byte[b.Length - Idx];
                Array.Copy(b, Idx, Remain_Undecoded_Bytes, 0, Remain_Undecoded_Bytes.Length);
            }

            return true;
        }
        // maybe
        public override byte[] GetRawBytes()
        {
            return null;
        }
    }
    public class CIP_EtherNetLink_instance : CIPObject
    {
        public UInt32? Interface_Speed { get; set; }
        public UInt32? Interface_Flags { get; set; }
        public string Physical_Address { get; set; }

        public UInt32? In_Octets { get; set; }
        public UInt32? In_Ucast_Packets { get; set; }
        public UInt32? In_NUcast_Packets { get; set; }
        public UInt32? In_Discards { get; set; }
        public UInt32? In_Errors { get; set; }
        public UInt32? In_Unknown_Protos { get; set; }

        public UInt32? Out_Octets { get; set; }
        public UInt32? Out_Ucast_Packets { get; set; }
        public UInt32? Out_NUcast_Packets { get; set; }
        public UInt32? Out_Discards { get; set; }
        public UInt32? Out_Errors { get; set; }

        public UInt32? Alignment_Errors { get; set; }
        public UInt32? FCS_Errors { get; set; }
        public UInt32? Single_Collisions { get; set; }
        public UInt32? Multiple_Collisions { get; set; }
        public UInt32? SQE_Test_Errors { get; set; }
        public UInt32? Deferred_Transmissions { get; set; }
        public UInt32? Late_Collisions { get; set; }
        public UInt32? Excessive_Collisions { get; set; }
        public UInt32? MAC_Transmit_Errors { get; set; }
        public UInt32? Carrier_Sense_Errors { get; set; }
        public UInt32? Frame_Too_Long { get; set; }
        public UInt32? MAC_Receive_Errors { get; set; }

        public UInt16? Control_Bits { get; set; }
        public UInt16? Forced_Interface_Speed { get; set; }
        public byte? Interface_Type { get; set; }
        public byte? Interface_State { get; set; }
        public byte? Admin_State { get; set; }
        public String Interface_Label { get; set; }             

        public byte[] Remain_Undecoded_Bytes { get; set; }

        public override string ToString()
        {
            return "EtherNetLink instance";
        }

        public override bool SetRawBytes(byte[] b)
        {
            int Idx = 0;

            Interface_Speed = GetUInt32(ref Idx, b);
            Interface_Flags = GetUInt32(ref Idx, b);

            Physical_Address = GetPhysicalAddress(ref Idx, b).ToString();

            In_Octets = GetUInt32(ref Idx, b);
            In_Ucast_Packets = GetUInt32(ref Idx, b);
            In_NUcast_Packets= GetUInt32(ref Idx, b);
            In_Discards = GetUInt32(ref Idx, b);
            In_Errors = GetUInt32(ref Idx, b);
            In_Unknown_Protos = GetUInt32(ref Idx, b);

            Out_Octets = GetUInt32(ref Idx, b);
            Out_Ucast_Packets = GetUInt32(ref Idx, b);
            Out_NUcast_Packets = GetUInt32(ref Idx, b);
            Out_Discards = GetUInt32(ref Idx, b);
            Out_Errors = GetUInt32(ref Idx, b);

            Alignment_Errors= GetUInt32(ref Idx, b);
            FCS_Errors= GetUInt32(ref Idx, b);
            Single_Collisions= GetUInt32(ref Idx, b);
            Multiple_Collisions= GetUInt32(ref Idx, b);
            SQE_Test_Errors= GetUInt32(ref Idx, b);
            Deferred_Transmissions= GetUInt32(ref Idx, b);
            Late_Collisions = GetUInt32(ref Idx, b);
            Excessive_Collisions = GetUInt32(ref Idx, b);
            MAC_Transmit_Errors = GetUInt32(ref Idx, b);
            Carrier_Sense_Errors = GetUInt32(ref Idx, b);
            Frame_Too_Long = GetUInt32(ref Idx, b);
            MAC_Receive_Errors = GetUInt32(ref Idx, b);

            Control_Bits = GetUInt16(ref Idx, b);
            Forced_Interface_Speed = GetUInt16(ref Idx, b);

            Interface_Type = GetByte(ref Idx, b);
            Interface_State = GetByte(ref Idx, b);
            Admin_State = GetByte(ref Idx, b);

            Interface_Label = GetShortString(ref Idx, b);

            if (Idx < b.Length)
            {
                Remain_Undecoded_Bytes = new byte[b.Length - Idx];
                Array.Copy(b, Idx, Remain_Undecoded_Bytes, 0, Remain_Undecoded_Bytes.Length);
            }

            return true;
        }
        // maybe
        public override byte[] GetRawBytes()
        {
            return null;
        }
    }
}
