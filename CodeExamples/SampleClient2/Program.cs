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
using System.Net.EnIPStack;
using System.Threading;
using System.Net;

namespace SampleClient2
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

        }

        //  Read 4.104.3 on an Eurotherm Epack
        public static void TestReadLoop()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("172.20.54.119"), 0xAF12);
            EnIPRemoteDevice EPack = new EnIPRemoteDevice(ep);

            // class 4, Instance 100, attribut 3 for all Input data
            EnIPClass Class4 = new EnIPClass(EPack, 4);
            // here we gives the decoder class (subclass of CIPObject) in the 3rd parameter
            EnIPInstance Instance100 = new EnIPInstance(Class4, 100, typeof(InstanceDecoder));
            EnIPAttribut AllInputs = new EnIPAttribut(Instance100, 3);

            EPack.autoConnect = false;
            EPack.autoRegisterSession = true;

            for (; ; )
            {
                // Connect or try re-connect, could be made with a long delay
                if (!EPack.IsConnected())
                    EPack.Connect();
                if (!EPack.IsConnected())
                    return;

                // all data will be put in the byte[] RawData member of EnIPInstanceAttribut 
                // ... but decoded in the DecoderMemeber
                if (AllInputs.ReadDataFromNetwork() == EnIPNetworkStatus.OnLine)
                {
                    // rawdata basic technic
                    // Console.WriteLine((AllInputs.RawData[1] << 8) | AllInputs.RawData[0]);
                    InstanceDecoder decoded = (InstanceDecoder)AllInputs.DecodedMembers;
                    Console.WriteLine(decoded.AnalogInput);
                    Console.WriteLine(decoded.Frequency/10.0);
                    // and so on
                }

                Thread.Sleep(200);

                // If a Forward Open is done such as in Class1Sample application sample,
                // the decoder is always called : a first ReadDataFromNetwork is mandatory
                // before that.
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

            // Close pending connection
            device.Dispose();
        }
    }
}
