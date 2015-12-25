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
using System.Net.EnIPStack;
using System.Threading;
using System.Net;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {

            TestDiscover();
            // Wait a little before next operation
            Thread.Sleep(1000);

            TestReadLoop();
            // TestWrite();
        }

        //  Read the first analog value on a Wago PLC 750-8xx
        public static void TestReadLoop()
        {

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("172.20.54.58"), 0xAF12);
            EnIPRemoteDevice WagoPlc = new EnIPRemoteDevice(ep);

            WagoPlc.Connect();

            if (!WagoPlc.IsConnected()) return;

            // class 103, instance 1, attribut 3
            EnIPClass Class103 = new EnIPClass(WagoPlc, 103);
            EnIPClassInstance Instance1 = new EnIPClassInstance(Class103, 1);
            EnIPInstanceAttribut FirstAnalogInput = new EnIPInstanceAttribut(Instance1, 3);

            for (; ; )
            {
                // all data will be put in the byte[] RawData member of EnIPInstanceAttribut 
                if (FirstAnalogInput.GetInstanceAttributData())
                    Console.WriteLine((FirstAnalogInput.RawData[0] << 8) | FirstAnalogInput.RawData[1]);
                Thread.Sleep(200);
            }
        }

        //  Write %IB2552-%IB2553
        public static void TestWriteLoop()
        {

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("172.20.54.58"), 0xAF12);
            EnIPRemoteDevice WagoPlc = new EnIPRemoteDevice(ep);

            WagoPlc.Connect();

            if (!WagoPlc.IsConnected()) return;

            // class 163, instance 1, attribut 1
            EnIPClass Class163 = new EnIPClass(WagoPlc, 103);
            EnIPClassInstance Instance1 = new EnIPClassInstance(Class163, 1);
            EnIPInstanceAttribut FirstMemoryByte = new EnIPInstanceAttribut(Instance1, 1);

            ushort i = 0;
            for (; ; )
            {
                FirstMemoryByte.RawData=BitConverter.GetBytes(i++);
                if (FirstMemoryByte.SetInstanceAttributData())
                    Console.WriteLine("OK");
                Thread.Sleep(200);
            }
        }

        public static void TestDiscover()
        {
            // Attach the default network interface
            EnIPClient client = new EnIPClient("");
            // Device arrival listener
            client.DeviceArrival += new DeviceArrivalHandler(client_DeviceArrival);
            // Send a broadcast discovery message
            client.DiscoverServers();
        }

        static void client_DeviceArrival(EnIPRemoteDevice device)
        {

            Console.WriteLine("Arrvial of : " + device.ep.Address.ToString() + " - " + device.ProductName);
        }
    }
}
