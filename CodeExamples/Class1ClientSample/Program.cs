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
using System.Net;
using System.Net.EnIPStack;
using System.Threading;

namespace Class1ClientSample
{
    // An active UDP connection is required here.
    // Take care to Windows Firewall settings, port 2222 should be open.

    // ForwardOpen and Class1 reception could be made in separate applications
    // but in such a case ConnectionId should exchanged for T2O_ConnectionId
    // member in the corresponding EnIPAttribut
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("172.20.54.58"), 0xAF12);
            EnIPRemoteDevice WagoPlc = new EnIPRemoteDevice(ep);
            WagoPlc.autoConnect = true;
            WagoPlc.autoRegisterSession = true;

            // class 4, instance 107, attribut 3 : Input Data
            EnIPClass Class4 = new EnIPClass(WagoPlc, 4);
            EnIPInstance Instance107 = new EnIPInstance(Class4, 107);
            EnIPAttribut Inputs = new EnIPAttribut(Instance107, 3);
            
            // Read require, it provides the data size in the RawData field
            // If not, one have to make a new on it with the good size before
            // calling ForwardOpen
            Inputs.ReadDataFromNetwork();

            // Open an Udp endpoint, server mode is mandatory : default port 0x8AE
            EnIPUDPTransport ForwardListener = new EnIPUDPTransport("", 0x8AE);
            // Not required in P2P mode
            ForwardListener.JoinMulticastGroup("239.192.72.32");

            // Register UDP callback handler for all Attribut : here just one
            ForwardListener.ItemMessageReceived += new ItemMessageReceivedHandler(Inputs.On_ItemMessageReceived);
            
            // Register me to get notified
            Inputs.T2OEvent += new T2OEventHandler(Inputs_T2OEvent);   
    
            // ForwardOpen in Multicast, T2O, cycle 200 ms, duration infinite (-1)
            Inputs.ForwardOpen(true, true, false, 1000, -1);

            Console.WriteLine("Running, hit a key to stop");

            Console.ReadKey();

            Inputs.ForwardClose();
        }

        static void Inputs_T2OEvent(EnIPAttribut sender)
        {        
            Console.Write('.');
        }

    }
}
