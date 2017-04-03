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

namespace System.Net.EnIPStack.ObjectsLibrary
{
    public class CIP_TCPIPInterface_class : CIPObject
    {
        public UInt16? Revision { get; set; }
        public UInt16? Max_Instance { get; set; }
        public UInt16? Number_of_Instances { get; set; }
        public byte[] Remain_Undecoded_Bytes { get; set; }

        public override string ToString()
        {
            return "TCPIPInterface class";
        }
        public override bool SetRawBytes(byte[] b)
        {
            int Idx = 0;

            Revision = GetUInt16(ref Idx, b);
            Max_Instance = GetUInt16(ref Idx, b);
            Number_of_Instances = GetUInt16(ref Idx, b);

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
    public class CIP_TCPIPInterface_instance : CIPObject
    {
        public UInt32? Status { get; set; }
        public UInt32? Configuration_Capability { get; set; }
        public UInt32? Configuration_Control { get; set; }
        public UInt16? Path_Size { get; set; }
        public string PhysicalObjectLinkPath { get; set; }
        public string IP_Address { get; set; } // string because IPAddress a greyed in the property grid
        public string NetMask { get; set; }
        public string Gateway_Address { get; set; }
        public string Name_Server_1 { get; set; }
        public string Name_Server_2 { get; set; }
        public string Domain_Name { get; set; }
        public string Host_Name { get; set; }
        public byte[] Remain_Undecoded_Bytes { get; set; }
        
        public override string ToString()
        {
            return "TCPIPInterface instance";
        }

        public override bool SetRawBytes(byte[] b)
        {
            int Idx = 0;

            Status = GetUInt32(ref Idx, b);
            Configuration_Capability = GetUInt32(ref Idx, b);
            Configuration_Control = GetUInt32(ref Idx, b);
            Path_Size = GetUInt16(ref Idx, b);

            /*
            for (int i = 0; i < Path_Size.Value; i++)
            {
                UInt16? _Path;
                _Path = (UInt16)GetUInt16(ref Idx, b);
                if (i!=0)
                    Path = Path + "." + _Path.ToString();
                else
                Path = _Path.ToString();
            }
             * */
            if (Path_Size.Value != 0)
            {
                byte[] _Path = new byte[Path_Size.Value * 2];
                Array.Copy(b, Idx, _Path,0, Path_Size.Value * 2);
                Idx+=Path_Size.Value * 2;
                PhysicalObjectLinkPath = EnIPPath.GetPath(_Path);
            }


            IP_Address = GetIPAddress(ref Idx, b).ToString();
            NetMask = GetIPAddress(ref Idx, b).ToString();
            Gateway_Address = GetIPAddress(ref Idx, b).ToString();
            Name_Server_1 = GetIPAddress(ref Idx, b).ToString();
            Name_Server_2 = GetIPAddress(ref Idx, b).ToString();

            Domain_Name=GetString(ref Idx, b);
            if ((Domain_Name.Length % 2) != 0) Idx++; // padd to even number of characters

            Host_Name = GetString(ref Idx, b);
            if ((Host_Name.Length % 2) != 0) Idx++; // padd to even number of characters

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
