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
    // CIP_EtherNetLink_class not required, nothing new than in CIPObjectBaseClass

    public class CIP_EtherNetLink_instance : CIPObject
    {
        [CIPAttributId(1)]
        public UInt32? Interface_Speed { get; set; }
        [CIPAttributId(2)]
        public UInt32? Interface_Flags { get; set; }
        [CIPAttributId(3)]
        public string Physical_Address { get; set; }

        [CIPAttributId(4)]
        public UInt32? In_Octets { get; set; }
        [CIPAttributId(4)]
        public UInt32? In_Ucast_Packets { get; set; }
        [CIPAttributId(4)]
        public UInt32? In_NUcast_Packets { get; set; }
        [CIPAttributId(4)]
        public UInt32? In_Discards { get; set; }
        [CIPAttributId(4)]
        public UInt32? In_Errors { get; set; }
        [CIPAttributId(4)]
        public UInt32? In_Unknown_Protos { get; set; }
        [CIPAttributId(4)]
        public UInt32? Out_Octets { get; set; }
        [CIPAttributId(4)]
        public UInt32? Out_Ucast_Packets { get; set; }
        [CIPAttributId(4)]
        public UInt32? Out_NUcast_Packets { get; set; }
        [CIPAttributId(4)]
        public UInt32? Out_Discards { get; set; }
        [CIPAttributId(4)]
        public UInt32? Out_Errors { get; set; }

        [CIPAttributId(5)]
        public UInt32? Alignment_Errors { get; set; }
        [CIPAttributId(5)]
        public UInt32? FCS_Errors { get; set; }
        [CIPAttributId(5)]
        public UInt32? Single_Collisions { get; set; }
        [CIPAttributId(5)]
        public UInt32? Multiple_Collisions { get; set; }
        [CIPAttributId(5)]
        public UInt32? SQE_Test_Errors { get; set; }
        [CIPAttributId(5)]
        public UInt32? Deferred_Transmissions { get; set; }
        [CIPAttributId(5)]
        public UInt32? Late_Collisions { get; set; }
        [CIPAttributId(5)]
        public UInt32? Excessive_Collisions { get; set; }
        [CIPAttributId(5)]
        public UInt32? MAC_Transmit_Errors { get; set; }
        [CIPAttributId(5)]
        public UInt32? Carrier_Sense_Errors { get; set; }
        [CIPAttributId(5)]
        public UInt32? Frame_Too_Long { get; set; }
        [CIPAttributId(5)]
        public UInt32? MAC_Receive_Errors { get; set; }

        [CIPAttributId(6)]
        public UInt16? Control_Bits { get; set; }
        [CIPAttributId(6)]
        public UInt16? Forced_Interface_Speed { get; set; }
        [CIPAttributId(7)]
        public byte? Interface_Type { get; set; }
        [CIPAttributId(8)]
        public byte? Interface_State { get; set; }
        [CIPAttributId(9)]
        public byte? Admin_State { get; set; }
        [CIPAttributId(10)]
        public String Interface_Label { get; set; }

        public CIP_EtherNetLink_instance() { AttIdMax = 10; }

        public override string ToString()
        {
            if (FilteredAttribut == -1)
                return "EtherNetLink instance";
            else
                return "EtherNetLink instance attribut #" + FilteredAttribut.ToString();
        }

        public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b)
        {
            switch (AttrNum)
            {
                case 1:
                    Interface_Speed = GetUInt32(ref Idx, b);
                    return true;
                case 2:
                    Interface_Flags = GetUInt32(ref Idx, b);
                    return true;
                case 3:
                    Physical_Address = GetPhysicalAddress(ref Idx, b).ToString();
                    return true;
                case 4:
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
                    return true;
                case 5:
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
                    return true;
                case 6:
                    Control_Bits = GetUInt16(ref Idx, b);
                    Forced_Interface_Speed = GetUInt16(ref Idx, b);
                    return true;
                case 7:
                    Interface_Type = GetByte(ref Idx, b);
                    return true;
                case 8:
                    Interface_State = GetByte(ref Idx, b);
                    return true;
                case 9:
                    Admin_State = GetByte(ref Idx, b);
                    return true;
                case 10:
                    Interface_Label = GetShortString(ref Idx, b);
                    return true;
            }

            return false;
        }
    }
}
