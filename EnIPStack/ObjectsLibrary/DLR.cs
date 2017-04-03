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

    public class CIP_DLR_class : CIPObject
    {
        public UInt16? Revision { get; set; }
        public UInt16? Max_Instance { get; set; }
        public UInt16? Number_of_Instances { get; set; }
        public byte[] Remain_Undecoded_Bytes { get; set; }

        public override string ToString()
        {
            return "DLR class";
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
    public class CIP_DLR_instance : CIPObject
    {
        public byte? Network_Topology  { get; set; }
        public byte? Network_Status { get; set; }

        public string Active_Supervisor_IPAddress { get; set; }
        public string Active_Supervisor_PhysicalAddress { get; set; }

        public UInt32? Capability_Flag { get; set; }


        public byte[] Remain_Undecoded_Bytes { get; set; }

        public override string ToString()
        {
            return "DLR instance";
        }

        public override bool SetRawBytes(byte[] b)
        {
            int Idx = 0;

            Network_Topology = GetByte(ref Idx, b);
            Network_Status = GetByte(ref Idx, b);

            Active_Supervisor_IPAddress = GetIPAddress(ref Idx, b).ToString();
            Active_Supervisor_PhysicalAddress = GetPhysicalAddress(ref Idx, b).ToString();

            Capability_Flag = GetUInt32(ref Idx, b);

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
