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
using System.ComponentModel;

namespace System.Net.EnIPStack.ObjectsLibrary
{
    // CIP_TCPIPInterface_class not required, nothing new than in CIPObjectBaseClass

    public class CIP_TCPIPInterface_instance : CIPObject
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class TCPIPInterface_Configuration
        {
            public string IP_Address { get; set; } // string because IPAddress a greyed in the property grid
            public string NetMask { get; set; }
            public string Gateway_Address { get; set; }
            public string Name_Server_1 { get; set; }
            public string Name_Server_2 { get; set; }
            public string Domain_Name { get; set; }
            public override string ToString() { return ""; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class TCPIPMcastConfig
        {
            public Byte? Alloc_Control { get; set; }
            public Byte? Reserved { get; set; }
            public UInt16? Num_Mcast { get; set; }
            public String Mcast_Start_Addr { get; set; }
            public override string ToString() { return ""; }
        }

        [CIPAttributId(1)]
        public UInt32? Status { get; set; }
        [CIPAttributId(2)]
        public UInt32? Configuration_Capability { get; set; }
        [CIPAttributId(3)]
        public UInt32? Configuration_Control { get; set; }
        [CIPAttributId(4)]
        public UInt16? Path_Size { get; set; }
        [CIPAttributId(4)]
        public string PhysicalObjectLinkPath { get; set; }
        [CIPAttributId(5)]
        public TCPIPInterface_Configuration Interface_Configuration { get; set; }       
        [CIPAttributId(6)]
        public string Host_Name { get; set; }
        [CIPAttributId(7)]
        public Byte[] Safety_Network_Number { get; set; }
        [CIPAttributId(8)]
        public Byte? TTL_Value { get; set; }
        [CIPAttributId(9)]
        public TCPIPMcastConfig Mcast_Config { get; set; }

        public CIP_TCPIPInterface_instance() { AttIdMax = 9; }

        public override string ToString()
        {
            if (FilteredAttribut == -1)
                return "TCPIPInterface instance";
            else
                return "TCPIPInterface instance attribut #" + FilteredAttribut.ToString();
        }

        public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b)
        {
            switch (AttrNum)
            {
                case 1:
                    Status = GetUInt32(ref Idx, b);
                    return true;
                case 2:
                    Configuration_Capability = GetUInt32(ref Idx, b);
                    return true;
                case 3:
                    Configuration_Control = GetUInt32(ref Idx, b);
                    return true;
                case 4:
                    Path_Size = GetUInt16(ref Idx, b);
                    if (Path_Size.Value != 0)
                    {
                        byte[] _Path = new byte[Path_Size.Value * 2];
                        Array.Copy(b, Idx, _Path, 0, Path_Size.Value * 2);
                        Idx += Path_Size.Value * 2;
                        PhysicalObjectLinkPath = EnIPPath.GetPath(_Path);
                    }
                    return true;
                case 5:
                    Interface_Configuration = new TCPIPInterface_Configuration
                    {
                        IP_Address = GetIPAddress(ref Idx, b).ToString(),
                        NetMask = GetIPAddress(ref Idx, b).ToString(),
                        Gateway_Address = GetIPAddress(ref Idx, b).ToString(),
                        Name_Server_1 = GetIPAddress(ref Idx, b).ToString(),
                        Name_Server_2 = GetIPAddress(ref Idx, b).ToString(),
                        Domain_Name = GetString(ref Idx, b)
                    };
                    if ((Interface_Configuration.Domain_Name.Length % 2) != 0) Idx++; // padd to even number of characters
                    return true;
                case 6:
                    Host_Name = GetString(ref Idx, b);
                    if ((Host_Name.Length % 2) != 0) Idx++; // padd to even number of characters
                    return true;
                case 7:
                    if (b.Length >= Idx + 5)
                    {
                        Safety_Network_Number = new byte[6];
                        Array.Copy(b, Idx, Safety_Network_Number, 0, 6);
                        Idx += 6;
                    }
                    return true;
                case 8:
                    TTL_Value = GetByte(ref Idx, b);
                    return true;
                case 9:
                    Mcast_Config = new TCPIPMcastConfig
                    {
                        Alloc_Control = GetByte(ref Idx, b),
                        Reserved = GetByte(ref Idx, b),
                        Num_Mcast = GetUInt16(ref Idx, b),
                        Mcast_Start_Addr = GetIPAddress(ref Idx, b).ToString(),
                    };
                    return true;
            }

            return false;
        }
    }
}
