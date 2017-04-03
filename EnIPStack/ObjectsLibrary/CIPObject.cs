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
using System.Net.NetworkInformation;

namespace System.Net.EnIPStack.ObjectsLibrary
{
    // base class used into the propertyGrid container to displays decoded members
    // also used to decode rawdata
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class CIPObject : ICustomTypeDescriptor
    {

        public abstract bool SetRawBytes(byte[] b);
        public abstract byte[] GetRawBytes();

        public bool? GetBool(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx) return null;
            var ret = (buf[Idx] == 1);
            Idx+=1;
            return ret;
        }
        public byte? GetByte(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx) return null;
            var ret = buf[Idx];
            Idx += 1;
            return ret;
        }
        public UInt16? GetUInt16(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 1) return null;
            var ret = BitConverter.ToUInt16(buf, Idx);
            Idx+=2;
            return ret;
        }
        public UInt32? GetUInt32(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 3) return null;
            var ret = BitConverter.ToUInt32(buf, Idx);
            Idx += 4;
            return ret;
        }
        public UInt64? GetUInt64(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 7) return null;
            var ret = BitConverter.ToUInt64(buf, Idx);
            Idx += 8;
            return ret;
        }
        public sbyte? GetSByte(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx) return null;
            var ret = (sbyte) buf[Idx];
            Idx += 1;
            return ret;
        }
        public Int16? GetInt16(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 1) return null;
            var ret = BitConverter.ToInt16(buf, Idx);
            Idx += 2;
            return ret;
        }
        public Int32? GetInt32(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 3) return null;
            var ret = BitConverter.ToInt32(buf, Idx);
            Idx += 4;
            return ret;
        }
        public Int64? GetInt64(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 7) return null;
            var ret = BitConverter.ToInt64(buf, Idx);
            Idx += 8;
            return ret;
        }
        public String GetString(ref int Idx, byte[] buf)
        {
            UInt16? t = GetUInt16(ref Idx, buf);
            if ((t != null) && (buf.Length >= Idx + t.Value))
            {
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                String s=iso.GetString(buf, Idx, t.Value);
                Idx += t.Value;
                return s;
            }
            return null;
        }
        public String GetShortString(ref int Idx, byte[] buf)
        {
            Byte? t = GetByte(ref Idx, buf);
            if ((t != null) && (buf.Length >= Idx + t.Value))
            {
                Encoding iso = Encoding.GetEncoding("ISO-8859-1");
                String s = iso.GetString(buf, Idx, t.Value);
                Idx += t.Value;
                return s;
            }
            return null;
        }

        public IPAddress GetIPAddress(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 3) return null;

            byte[] b_ip = new byte[4]; ;
            Array.Copy(buf, Idx, b_ip, 0, 4);
            Idx += 4;
            Array.Reverse(b_ip);
            return new IPAddress(b_ip);
        }

        public PhysicalAddress GetPhysicalAddress(ref int Idx, byte[] buf)
        {
            if (buf.Length < Idx + 5) return null;

            byte[] b_eth = new byte[6]; ;
            Array.Copy(buf, Idx, b_eth, 0, 6);
            Idx += 6;
            //Array.Reverse(b_ip);
            return new PhysicalAddress(b_eth);
    }

        #region CustomTypeDescriptor
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this, true);
            return props;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion

    }
}
