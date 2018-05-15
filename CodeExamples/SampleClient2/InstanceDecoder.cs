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
using System.Net.EnIPStack.ObjectsLibrary;

namespace SampleClient2
{
    public class InstanceDecoder : CIPObject
    {
        // Since all Attributs from 1 to 3 are not given
        // this class cannot be used for full instance
        // decoding, but only for the attribut 3
        // Here attributs are not nullable, because we know a
        // value is always present (not an optional attribut)

        [CIPAttributId(3)]
        public UInt16 AnalogInput { get; set; }
        [CIPAttributId(3)]
        public UInt16 Frequency { get; set; }
        [CIPAttributId(3)]
        public UInt16 Current { get; set; }
        [CIPAttributId(3)]
        public UInt16 Voltage { get; set; }
   
        public InstanceDecoder() { AttIdMax = 3; }

        public override bool DecodeAttr(int AttrNum, ref int Idx, byte[] b)
        {
            switch (AttrNum)
            {
                case 3:
                    AnalogInput = GetUInt16(ref Idx, b).Value;
                    Frequency = GetUInt16(ref Idx, b).Value;
                    Current = GetUInt16(ref Idx, b).Value;
                    Voltage = GetUInt16(ref Idx, b).Value;
                    return true;               
            }

            return false;
        }

        public void Encode(byte[] b)
        {
            int Idx=0;
            SetUInt16(ref Idx, b, AnalogInput);
            SetUInt16(ref Idx, b, Frequency);
            SetUInt16(ref Idx, b, Current);
            SetUInt16(ref Idx, b, Voltage);
        }

    }
}
