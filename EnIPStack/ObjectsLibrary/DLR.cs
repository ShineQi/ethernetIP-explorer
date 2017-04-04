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

    // CIP_DLR_class not required, nothing new than in CIPObjectBaseClass

    public class CIP_DLR_instance : CIPObject
    {
        [CIPAttributId(1)]
        public byte? Network_Topology  { get; set; }
        [CIPAttributId(2)]
        public byte? Network_Status { get; set; }
        [CIPAttributId(3)]
        public string Active_Supervisor_IPAddress { get; set; }
        [CIPAttributId(4)]
        public string Active_Supervisor_PhysicalAddress { get; set; }
        [CIPAttributId(5)]
        public UInt32? Capability_Flag { get; set; }

        public CIP_DLR_instance() { AttIdMax = 5; }

        public override string ToString()
        {
            if (FilteredAttribut == -1)
                return "DLR instance";
            else
                return "DLR instance attribut #" + FilteredAttribut.ToString();
        }

        public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b)
        {
            switch (AttrNum)
            {
                case 1:
                    Network_Topology = GetByte(ref Idx, b);
                    return true;
                case 2:
                    Network_Status = GetByte(ref Idx, b);
                    return true;
                case 3:
                    Active_Supervisor_IPAddress = GetIPAddress(ref Idx, b).ToString();
                    return true;
                case 4:
                    Active_Supervisor_PhysicalAddress = GetPhysicalAddress(ref Idx, b).ToString();
                    return true;
                case 5:
                    Capability_Flag = GetUInt32(ref Idx, b);
                    return true;
            }
            return false;
        }
    }
}
