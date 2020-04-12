using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Asynchronous_Chat
{
    public class UDPAshynchronousChatServer
    {
        //
        Socket mSockBroadcastReciver;
        IPEndPoint mIPEPLocal;

        public UDPAshynchronousChatServer()
        {
            mSockBroadcastReciver =
                new Socket(
                    AddressFamily.InterNetwork, //using IPv4
                    SocketType.Dgram,           //Datagram Socket
                    ProtocolType.Udp);          //UDP socket
            

            mIPEPLocal = new IPEndPoint(IPAddress.Any, 23000);
            //any socket object using the same endpoint available to all the ip addresses in this machine 
            //the socket will use port 23000

            mSockBroadcastReciver.EnableBroadcast = true;
        }
    }
}
