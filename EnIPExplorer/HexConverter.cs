/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2018 Frederic Chaxel <fchaxel@free.fr>
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
using System.Globalization;

namespace EnIPExplorer
{
    // a class used by the PropertyGrid class to display different integer T as Hex
    public class HexTypeConverter<T> : TypeConverter
    {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) 
                return true; 
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string)) 
                return true; 
            else
                return base.CanConvertTo(context, destinationType);            
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {

            if (destinationType == typeof(string))
            {
                if (Properties.Settings.Default.IntegerHexDisplay == false)
                    return value.ToString();

                int TypeByteNumber = 2*System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

                string Formater = "0x{0:X"+TypeByteNumber+"}";
                return string.Format(Formater, value); // convert.ToString with base 16 not working for signed type 

                /*
                if ((type == typeof(Byte)) || (type == typeof(SByte)))
                {
                    return string.Format("0x{0:X2}", value); // convert.ToString not working for SByte
                }
                if ((type == typeof(UInt16)) ||( type == typeof(Int16)))
                {
                    return string.Format("0x{0:X4}", value);
                }
                 
                ...
                 
                }*/
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {

            if (value.GetType() == typeof(string))
            {
                NumberStyles style;
                if (Properties.Settings.Default.IntegerHexDisplay == true)
                    style = System.Globalization.NumberStyles.HexNumber;
                else
                    style = System.Globalization.NumberStyles.Integer;

                string input = (string)value;
                if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    input = input.Substring(2);

                try
                {
                    return (T)typeof(T).GetMethod("Parse", new Type[] { typeof(string), typeof(NumberStyles), typeof(CultureInfo) }).Invoke(null, new object[] { input, style, culture });
                    /*
                       return Byte.Parse(input, style, culture);
                       return SByte.Parse(input, style, culture);
                       ...
                    */
                }
                catch 
                { 
                    return "nada"; // a wrong type value !
                }
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    } 
}
