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
        private int retryCount;

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

        //
        public void startReceivingData()
        {
            try
            {
                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                saea.SetBuffer(new byte[1024], 0, 1024);  // memory allocated in this metord will contain the data when the callback function is called
                //1: allocate memory by passing a byte array
                //2: statring point of the data allocation in the array 
                //3: length of available space of the buffer (cannot be > allocated memory of the buffer) (65,507 max val)

                //remote endpoint property
                saea.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                //bind the socket to the local endpoint
                if (!mSockBroadcastReciver.IsBound)  //check if it is already bound or not
                {
                    mSockBroadcastReciver.Bind(mIPEPLocal);
                }

                //populate the completed property of this socket object
                //create a callback methord
                // ' += ' assign a callback methord
                saea.Completed += ReceiveCompletedCallBack;

                //check if the return value is fales 
                if (!mSockBroadcastReciver.ReceiveFromAsync(saea))
                {
                    Console.WriteLine($"Failed to receive data - sock error: {saea.SocketError}");
                    // ' $ ' string interpolation:   new way to concatenate the string ( '+' = '$' )

                    //re run the methor for 10 times
                    if (retryCount++ >= 10)
                    {
                        return;
                    }
                    else
                    {
                        startReceivingData();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }


        private void ReceiveCompletedCallBack(object sender, SocketAsyncEventArgs e)
        {
            //  SocketAsyncEventArgs e :: contains the data someone sends to the socket (stores inside the buffer)
            // e.bytetransferd  :: how many bytes were transfered

            string textReceived = Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred); // convert the bytes into string 
            Console.WriteLine(
                $"Text Received {textReceived}{Environment.NewLine}" + 
                $"Number of bytes Received : {e.BytesTransferred}{Environment.NewLine}" + 
                $"Received data from endpoint : {e.RemoteEndPoint}{Environment.NewLine}"
                );

            //to receive more data need to call the function again:: 
            startReceivingData();

        }
    }
}
