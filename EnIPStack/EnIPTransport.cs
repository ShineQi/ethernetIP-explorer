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
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace System.Net.EnIPStack
{
    public delegate void MessageReceivedHandler(object sender, byte[] packet, EncapsulationPacket EncapPacket, int offset, int msg_length, IPEndPoint remote_address);

    // Could be used for client & server implementation
    public class EnIPUDPTransport
    {
        public event MessageReceivedHandler MessageReceived;

        private UdpClient m_exclusive_conn;
        String m_local_endpoint;
        private bool m_is_server;

        public EnIPUDPTransport(String Local_endpoint, bool IsServer)
        {
            m_local_endpoint = Local_endpoint;
            m_is_server = IsServer;
        }

        private void Open()
        {
            if (m_is_server)
            {
                System.Net.EndPoint ep = new IPEndPoint(System.Net.IPAddress.Any, 0xAF12);
                if (!string.IsNullOrEmpty(m_local_endpoint)) ep = new IPEndPoint(IPAddress.Parse(m_local_endpoint), 0xAF12);
                m_exclusive_conn = new UdpClient();
                m_exclusive_conn.ExclusiveAddressUse = true;
                m_exclusive_conn.Client.Bind((IPEndPoint)ep);
                m_exclusive_conn.EnableBroadcast = true;
            }
            else
            {
                System.Net.EndPoint ep = new IPEndPoint(System.Net.IPAddress.Any, 0);
                if (!string.IsNullOrEmpty(m_local_endpoint)) ep = new IPEndPoint(IPAddress.Parse(m_local_endpoint), 0);
                m_exclusive_conn = new UdpClient((IPEndPoint)ep);
                m_exclusive_conn.EnableBroadcast = true;
            }

        }

        public void Start()
        {
            Open();
            m_exclusive_conn.BeginReceive(OnReceiveData, m_exclusive_conn);
        }

        private void OnReceiveData(IAsyncResult asyncResult)
        {
            System.Net.Sockets.UdpClient conn = (System.Net.Sockets.UdpClient)asyncResult.AsyncState;
            try
            {
                System.Net.IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Any, 0);
                byte[] local_buffer;
                int rx = 0;

                try
                {
                    local_buffer = conn.EndReceive(asyncResult, ref ep);
                    rx = local_buffer.Length;
                }
                catch (Exception) // ICMP port unreachable
                {
                    //restart data receive
                    conn.BeginReceive(OnReceiveData, conn);
                    return;
                }

                if (rx <24)    // Too small
                {
                    //restart data receive
                    conn.BeginReceive(OnReceiveData, conn);
                    return;
                }

                try
                {
                    int Offset = 0;
                    EncapsulationPacket Encapacket = new EncapsulationPacket(local_buffer, ref Offset);
                    //verify message
                    if (MessageReceived != null)
                        MessageReceived(this, local_buffer, Encapacket, Offset, local_buffer.Length, ep);                   
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Exception in udp recieve: " + ex.Message);
                }
                finally
                {
                    //restart data receive
                    conn.BeginReceive(OnReceiveData, conn);
                }
            }
            catch (Exception ex)
            {
                //restart data receive
                if (conn.Client != null)
                {
                    Trace.TraceError("Exception in Ip OnRecieveData: " + ex.Message);
                    conn.BeginReceive(OnReceiveData, conn);
                }
            }
        }

        public void Send(EncapsulationPacket Packet, IPEndPoint ep)
        {
            byte[] b=Packet.toByteArray();
            m_exclusive_conn.Send(b, b.Length, ep);
        }

        // A lot of problems on Mono (Raspberry) to get the correct broadcast @
        // so this method is overridable (this allows the implementation of operating system specific code)
        // Marc solution http://stackoverflow.com/questions/8119414/how-to-query-the-subnet-masks-using-mono-on-linux for instance
        //
        protected virtual IPEndPoint _GetBroadcastAddress()
        {
            // general broadcast
            System.Net.IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse("255.255.255.255"), 0xAF12);
            // restricted local broadcast (directed ... routable)
            foreach (System.Net.NetworkInformation.NetworkInterface adapter in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                foreach (System.Net.NetworkInformation.UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                    if (LocalEndPoint.Address.Equals(ip.Address))
                    {
                        try
                        {
                            string[] strCurrentIP = ip.Address.ToString().Split('.');
                            string[] strIPNetMask = ip.IPv4Mask.ToString().Split('.');
                            StringBuilder BroadcastStr = new StringBuilder();
                            for (int i = 0; i < 4; i++)
                            {
                                BroadcastStr.Append(((byte)(int.Parse(strCurrentIP[i]) | ~int.Parse(strIPNetMask[i]))).ToString());
                                if (i != 3) BroadcastStr.Append('.');
                            }
                            ep = new IPEndPoint(System.Net.IPAddress.Parse(BroadcastStr.ToString()), 0xAF12);
                        }
                        catch { }  //On mono IPv4Mask feature not implemented
                    }

            return ep;
        }

        IPEndPoint BroadcastAddress = null;
        public IPEndPoint GetBroadcastAddress()
        {
            if (BroadcastAddress == null) BroadcastAddress = _GetBroadcastAddress();
            return BroadcastAddress;
        }

        // Give 0.0.0.0:xxxx if the socket is open with System.Net.IPAddress.Any
        // Today only used by _GetBroadcastAddress method & the bvlc layer class in BBMD mode
        // Some more complex solutions could avoid this, that's why this property is virtual
        public virtual IPEndPoint LocalEndPoint
        {
            get
            {
                return (IPEndPoint)m_exclusive_conn.Client.LocalEndPoint;
            }
        }

    }
}
