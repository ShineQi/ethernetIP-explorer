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
using System.Net;
using System.Net.EnIPStack;
using System.Threading;

namespace Class1SampleClient2
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Starting");

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("100.75.137.27"), 0xAF12);
            EnIPRemoteDevice OpENer = new EnIPRemoteDevice(ep);
            OpENer.autoConnect = true;
            OpENer.autoRegisterSession = true;

            // class 4, instance 151, attribut 3 : Config Data
            EnIPClass Class4 = new EnIPClass(OpENer, 4);
            EnIPInstance Instance151 = new EnIPInstance(Class4, 151);
            EnIPAttribut Config = new EnIPAttribut(Instance151, 3);

            // class 4, instance 150, attribut 3 : Output Data
            EnIPInstance Instance150 = new EnIPInstance(Class4, 150);
            EnIPAttribut Outputs = new EnIPAttribut(Instance150, 3);

            // class 4, instance 100, attribut 3 : Input Data
            EnIPInstance Instance100 = new EnIPInstance(Class4, 100);
            EnIPAttribut Inputs = new EnIPAttribut(Instance100, 3);

            // Read require, it provides the data size in the RawData field
            // If not, one have to make a new on it with the good size before
            // calling ForwardOpen : Inputs.RawData=new byte[xx]
            Config.ReadDataFromNetwork();
            Inputs.ReadDataFromNetwork();
            Outputs.ReadDataFromNetwork();
            
            IPEndPoint LocalEp = new IPEndPoint(IPAddress.Any, 0x8AE);
            // It's not a problem to do this with more than one remote device,
            // the underlying udp socket is static
            OpENer.Class1Activate(LocalEp);

            // ForwardOpen in P2P, cycle 200 ms
            ForwardOpen_Config conf = new ForwardOpen_Config(Outputs, Inputs, true, 200*1000);
            // here can change conf for exemple to set Exclusive use, change priority or CycleTime not equal in the both direction

            // Attributes order cannot be changed, last optional attribute true
            // will write the config value Config.RawData (modifies it after ReadDataFromNetwork before this call)
            ForwardClose_Packet ClosePacket;
            EnIPNetworkStatus result = OpENer.ForwardOpen(Config, Outputs, Inputs, out ClosePacket, conf, false);

            if (result == EnIPNetworkStatus.OnLine)
            {
                // Register Inputs events to get notified
                Inputs.T2OEvent += new T2OEventHandler(Inputs_T2OEvent);

                Console.WriteLine("Running, hit a key to stop");
                while (!Console.KeyAvailable)
                {
                    Outputs.RawData[0] = (byte)(Outputs.RawData[0] + 1);
                    Outputs.Class1UpdateO2T(); // must be called even if no data changed to maintain the link (Heartbeat)
                    Thread.Sleep(200);
                }
                OpENer.ForwardClose(Inputs, ClosePacket);
            }
            else
                Console.WriteLine("Fail");
        }

        static void Inputs_T2OEvent(EnIPAttribut sender)
        {
            Console.Write('.');
        }
    }
}
