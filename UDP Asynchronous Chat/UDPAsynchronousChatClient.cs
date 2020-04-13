using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private EndPoint mChatServerEP;

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

                saea.Completed += SendCompletedCallBack;

                mSocketBroadcastSender.SendToAsync(saea);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private void SendCompletedCallBack(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Data sent succesfully to : {e.RemoteEndPoint}");

            if (Encoding.ASCII.GetString(e.Buffer).Equals("<DISCOVER>"))
            {
                ReceiveTextFromServer(expectedValue: "<CONFIRM>", IPEPReceiverLocal: mIPEPLocal);
            }
            
        }

        private void ReceiveTextFromServer(string expectedValue, IPEndPoint IPEPReceiverLocal)
        {
            if (IPEPReceiverLocal == null)
            {
                Console.WriteLine("No IPEndpoint specified");
                return;
            }

            SocketAsyncEventArgs saeaSendConfirmation = new SocketAsyncEventArgs();

            //buffer to get the received data
            saeaSendConfirmation.SetBuffer(new byte[1024], 0, 1024);
            saeaSendConfirmation.RemoteEndPoint = IPEPReceiverLocal;

            //specifi the value that we want to gret from the server
            saeaSendConfirmation.UserToken = expectedValue;

            saeaSendConfirmation.Completed += ReceiveConfirmationCompoleted;

            mSocketBroadcastSender.ReceiveFromAsync(saeaSendConfirmation);

        }

        private void ReceiveConfirmationCompoleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                Debug.WriteLine($"Zero bytes transferred, socket error: {e.SocketError}");
                return; 
            }

            var receivedText = Encoding.ASCII.GetString(e.Buffer, 0, e.BytesTransferred);
            //e.Buffer = contains the received data
            //e.BytesTransferred = contains the size
            var expectedText = Convert.ToString(e.UserToken);

            if (receivedText.Equals(expectedText))
            {
                Console.WriteLine($"Received confirmation from server : {e.RemoteEndPoint}");

                //remember the server endpoint thsat we recived the message
                mChatServerEP = e.RemoteEndPoint;

                //now we are connected to the server and need to receive more data from the server...
                ReceiveTextFromServer(string.Empty, mChatServerEP as IPEndPoint); 
            }
            else if(string.IsNullOrEmpty(expectedText) && 
                !string.IsNullOrEmpty(receivedText))//userToken empty : server is sending a messagge from another client
            {
                Console.WriteLine($"text received: {receivedText}");
                ReceiveTextFromServer(string.Empty, mChatServerEP as IPEndPoint);
            }
            else if( !string.IsNullOrEmpty(expectedText) &&
                !receivedText.Equals(expectedText))//a server receives the <DISCOVER> message but sends a message that client didnt expected ~~ this means that the server is not what we wanted
            {
                Console.WriteLine($"Expected token not returned by the server.");
            }

        }

        public void SendMessageToKnownServer(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    Console.WriteLine("String is empty....");
                    return;
                }

                //byte array to store the coverted string
                var bytesToSend = Encoding.ASCII.GetBytes(message);

                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                saea.SetBuffer(bytesToSend, 0, bytesToSend.Length);

                //mChatServerEP = when we send a broadcast the recived server's Endpoint. This will be sent through the callback function
                saea.RemoteEndPoint = mChatServerEP;

                saea.UserToken = message;

                saea.Completed += SendMessageToKnownServerCompoletedCallback;

                mSocketBroadcastSender.SendToAsync(saea);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private void SendMessageToKnownServerCompoletedCallback(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Sent: {e.UserToken}{Environment.NewLine}Server: {e.RemoteEndPoint}");
        }
    }
}
