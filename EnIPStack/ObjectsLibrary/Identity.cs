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

namespace System.Net.EnIPStack.ObjectsLibrary
{
    public class CIP_Identity_class:CIPObject
    {
        public UInt16? Revision { get; set; }
        public UInt16? MaxInstance { get; set; }

        public override string ToString()
        {
            return "Identity class";
        }
        public override bool SetRawBytes(byte[] b)
        {
            return true;   
        }
        // maybe
        public override byte[] GetRawBytes()
        {
            return null;
        }
    }
    public class CIP_Identity_instance : CIPObject
    {
        public UInt32? Status { get; set; }
        public UInt32? ConfigurationCapability { get; set; }
        public UInt32? ConfigurationControl { get; set; }

        public override string ToString()
        {
            return "Identity instance";
        }

        public override bool SetRawBytes(byte[] b)
        {
            return true;
        }
        // maybe
        public override byte[] GetRawBytes()
        {
            return null;
        }
    }
}
