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

    // Common class attribut : 4-4.1 Class Attributes
    public class CIPObjectBaseClass : CIPObject
    {
        [CIPAttributId(1)]
        public UInt16? Revision { get; set; }
        [CIPAttributId(2)]
        public UInt16? Max_Instance { get; set; }
        [CIPAttributId(3)]
        public UInt16? Number_of_Instances { get; set; }
        [CIPAttributId(4)]
        public UInt16? Number_of_Attributes { get; set; }
        [CIPAttributId(4)]
        public UInt16[] Optional_Attributes { get; set; }
        [CIPAttributId(5)]
        public UInt16? Number_of_Services { get; set; }
        [CIPAttributId(5)]
        public UInt16[] Optional_Services { get; set; }
        [CIPAttributId(6)]
        public UInt16? Maximum_ID_Number_Class_Attributes { get; set; }
        [CIPAttributId(7)]
        public UInt16? Maximum_ID_Number_Instance_Attributes { get; set; }

        String Name = "Base";
        public CIPObjectBaseClass() { AttIdMax = 7; }

        public CIPObjectBaseClass(string Name)
        {
            this.Name = Name;
            AttIdMax = 7;
        }

        public override string ToString()
        {
            return "class " + Name;
        }

        public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b)
        {
            switch (AttrNum)
            {
                case 1:
                    Revision = GetUInt16(ref Idx, b);
                    return true;
                case 2:
                    Max_Instance = GetUInt16(ref Idx, b);
                    return true;
                case 3:
                    Number_of_Instances = GetUInt16(ref Idx, b);
                    return true;
                case 4:
                    Number_of_Attributes = GetUInt16(ref Idx, b);
                    if ((Number_of_Attributes != null) && (Number_of_Attributes.Value > 0))
                    {
                        Optional_Attributes = new UInt16[Number_of_Attributes.Value];
                        for (int i = 0; i < Number_of_Attributes.Value; i++)
                            Optional_Attributes[i] = GetUInt16(ref Idx, b).Value;
                    }
                    return true;
                case 5:
                    Number_of_Services = GetUInt16(ref Idx, b);
                    if ((Number_of_Services != null) && (Number_of_Services.Value > 0))
                    {
                        Optional_Services = new UInt16[Number_of_Services.Value];
                        for (int i = 0; i < Number_of_Services.Value; i++)
                            Optional_Services[i] = GetUInt16(ref Idx, b).Value;
                    }
                    return true;
                case 6:
                    Maximum_ID_Number_Class_Attributes = GetUInt16(ref Idx, b);
                    return true;
                case 7:
                    Maximum_ID_Number_Instance_Attributes = GetUInt16(ref Idx, b);
                    return true;
            }

            return false;
        }
    }

    // Only used to fill the Remain_Undecoded_Bytes
    public class CIPBaseUserDecoder : CIPObject
    {
        public override string ToString() { return ""; }
        protected void Finish(int Idx, byte[] b)
        {
            Remain_Undecoded_Bytes = new byte[b.Length - Idx];
            for (int i = 0; i < Remain_Undecoded_Bytes.Length; i++)
                Remain_Undecoded_Bytes[i] = b[Idx++];
        }
    }

    // Only used for Attribut decoding
    public class CIPUInt16Array : CIPBaseUserDecoder
    {
        public UInt16[] UINT { get; set; }

        public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b)
        {
            UINT=new UInt16[b.Length>>1];

            for (int i = 0; i < UINT.Length; i++)
                UINT[i] = GetUInt16(ref Idx, b).Value;

            Idx = UINT.Length * 2;
            Finish(Idx, b);
            return true;
        }
    }

}
