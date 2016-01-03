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
            Console.WriteLine("Starting");

            TestDiscover();
            // Wait a little before next operation
            Thread.Sleep(1000);

            TestReadLoop();
            //TestWriteLoop();
        }

        //  Read the first analog value on a Wago PLC 750-8xx
        public static void TestReadLoop()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("172.20.54.58"), 0xAF12);
            EnIPRemoteDevice WagoPlc = new EnIPRemoteDevice(ep);

            // class 103, instance 1, attribut 1
            // could be class 4, Instance 104 (with status) or 107 (wihout), attribut 3 for all Input data
            EnIPClass Class103 = new EnIPClass(WagoPlc, 103);
            EnIPInstance Instance1 = new EnIPInstance(Class103, 1);
            EnIPAttribut FirstAnalogInput = new EnIPAttribut(Instance1, 1);

            WagoPlc.autoConnect = false;
            WagoPlc.autoRegisterSession = true;

            for (; ; )
            {
                // Connect or try re-connect, could be made with a long delay
                if (!WagoPlc.IsConnected())
                    WagoPlc.Connect();
                if (!WagoPlc.IsConnected())
                    return;

                // all data will be put in the byte[] RawData member of EnIPInstanceAttribut 
                if (FirstAnalogInput.ReadDataFromNetwork()==EnIPNetworkStatus.OnLine)
                    Console.WriteLine((FirstAnalogInput.RawData[1] << 8) | FirstAnalogInput.RawData[0]);

                Thread.Sleep(200);
            }
        }

        //  Write %IW1530
        public static void TestWriteLoop()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("172.20.54.58"), 0xAF12);
            EnIPRemoteDevice WagoPlc = new EnIPRemoteDevice(ep);

            // class 166, instance 1, attribut 1
            EnIPClass Class166 = new EnIPClass(WagoPlc, 166);
            EnIPInstance Instance1 = new EnIPInstance(Class166, 1);
            EnIPAttribut FirstMemoryByte = new EnIPAttribut(Instance1, 1);

            // Connect made & retry automatically
            WagoPlc.autoConnect = true;
            WagoPlc.autoRegisterSession = true;

            ushort i = 0;
            for (; ; )
            {               
                FirstMemoryByte.RawData=BitConverter.GetBytes(i++);
                if (FirstMemoryByte.WriteDataToNetwork()==EnIPNetworkStatus.OnLine)
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
            Console.WriteLine("Arrvial of : " + device.IPString() + " - " + device.ProductName);
            // Query the object list : Assembly object, instance 1, atribut 1
            Console.WriteLine("Classes inside :");
            device.GetObjectList();
            foreach (EnIPClass cl in device.SupportedClassLists)
                Console.WriteLine("\t"+((CIPObjectLibrary)cl.Id).ToString());
        }
    }
}
