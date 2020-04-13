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

        List<EndPoint> mListOfClients;

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
            mListOfClients = new List<EndPoint>();
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

            //to add clients into the list
            if (textReceived.Equals("<DISCOVER>"))
            {
                //check if the list already has the clients endpoint 
                if (!mListOfClients.Contains(e.RemoteEndPoint))
                {
                    mListOfClients.Add(e.RemoteEndPoint);
                    Console.WriteLine($"Total Clients : {mListOfClients.Count}");

                }
                //conformation packet for the sender
                SendTextToEndPoint("<CONFIRM>", e.RemoteEndPoint);// so now the sender will bw able to save the serves endpoint and send specific messages here without broadcasting 


            }

            //to receive more data need to call the function again:: 
            startReceivingData();

        }

        private void SendTextToEndPoint(string textToSend, EndPoint remoteEndPoint)
        {

            if (string.IsNullOrEmpty(textToSend) || remoteEndPoint == null)
            {
                return;
            }
            SocketAsyncEventArgs saeaSend = new SocketAsyncEventArgs();
            saeaSend.RemoteEndPoint = remoteEndPoint;

            //convert the string to bytes so that it can be sent through the socket
            var bytesToSend = Encoding.ASCII.GetBytes(textToSend);

            saeaSend.SetBuffer(bytesToSend, 0, bytesToSend.Length);

            //if sending is completed calls this callback methord to inform that the task is compoleted
            saeaSend.Completed += SendTextToEndpointCompleted;

            mSockBroadcastReciver.SendAsync(saeaSend);

        }

        private void SendTextToEndpointCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Completed sending text to {e.RemoteEndPoint}");
        }
    }
}
