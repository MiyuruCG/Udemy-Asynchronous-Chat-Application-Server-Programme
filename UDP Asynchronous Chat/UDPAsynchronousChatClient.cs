using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Asynchronous_Chat
{
    public class UDPAsynchronousChatClient
    {
        // to send broadcast messages need to create the following objects
        Socket mSocketBroadcastSender;
        IPEndPoint mIPEBroadcast;
        IPEndPoint mIPEPLocal; // to represent the local machine
        
        public UDPAsynchronousChatClient(int _localPort, int _remotePort)
        {
            // para 1 : used to receive data
            // para 2 : port no of the endpoint wich will receive the packet that this programme is sending 

            mIPEBroadcast = new IPEndPoint(IPAddress.Broadcast, _remotePort);
            //IPAddress.Broadcast :: use to broadcast messages to the given port 
            
            mIPEPLocal = new IPEndPoint(IPAddress.Any, _localPort);

            mSocketBroadcastSender = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
                );

            mSocketBroadcastSender.EnableBroadcast = true;
        }

        public void SendBroadcast(string strDataForBroadcast)
        {
            if (string.IsNullOrEmpty(strDataForBroadcast))
            {
                return;
            }
            try
            {
                if (!mSocketBroadcastSender.IsBound)
                {
                    mSocketBroadcastSender.Bind(mIPEPLocal);
                }
                var dataBytes = Encoding.ASCII.GetBytes(strDataForBroadcast); // coverting string to byte[]

                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();

                //need to convert string into byte[] to broadcast it
                saea.SetBuffer(dataBytes, 0, dataBytes.Length);
                saea.RemoteEndPoint = mIPEBroadcast;

                saea.Completed += SenndCompletedCallBack;

                mSocketBroadcastSender.SendToAsync(saea);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private void SenndCompletedCallBack(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Data sent succesfully to : {e.RemoteEndPoint}");
        }
    }
}
