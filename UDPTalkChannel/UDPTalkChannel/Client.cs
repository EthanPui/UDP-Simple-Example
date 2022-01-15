using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace UDPTalkChannel
{
    public class Client
    {
        #region Private Fields

        private string serverIP;
        private int outgoingport;
        private int incomingport;
        private Socket socket;
        private List<byte> data;
        private byte[] buffer;
        private int bufferSize;

        private ReceivedEventArgs receivedEventArgs;
        private SentEventArgs sentEventArgs;
        #endregion

        #region Public Methods
        public Client(String ServerIP, int OutgoingPort, int IncomingPort, int _bufferSize)
        {
            this.serverIP = ServerIP;
            this.outgoingport = OutgoingPort;
            this.incomingport=IncomingPort;
            this.bufferSize = _bufferSize;

            data = new List<byte>();
            buffer = new byte[bufferSize];

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint localendpoint = new IPEndPoint(IPAddress.Any, incomingport);
            socket.Bind(localendpoint);
            //socket.Connect(localendpoint);

            getMessage();

        }


        public void Send(byte[] message)
        {
            try
            {
                if (socket != null)
                {

                        IPAddress serverIPAddress = IPAddress.Parse(serverIP);
                        IPEndPoint endPoint = new IPEndPoint(serverIPAddress, outgoingport);

                        socket.BeginSendTo(message, 0, message.Length, SocketFlags.None, endPoint, new AsyncCallback(sent), null);


                }
                else
                {
                    handleError(new Exception("Can't sent to a null socket."));
                }
            }
            catch (ObjectDisposedException exx)
            {
                raiseError(new ErrorEventArgs(exx));
            }
            catch (Exception ex)
            {
                handleError(new Exception("Error while trying Send. Look at inner exception for more details.", ex));
            }
        }

        #endregion 

        #region Private Methods
        private void getMessage()
        {
            try
            {
                if (socket != null)
                {
                    IPEndPoint localendpoint = new IPEndPoint(IPAddress.Any, incomingport);
                    EndPoint epClient = (EndPoint)localendpoint;

                    socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epClient, new AsyncCallback(messageReceived), null);
                }
                else
                {
                    handleError(new Exception("Socket is null in getHeader"));
                }
            }
            catch (ObjectDisposedException ex)
            {
                handleError(new Exception("Socket is Disposed " + ex.Message));
                try
                {
                    socket.Close();
                }
                catch (Exception) { }
                socket = null;
            }
            catch (Exception ex)
            {
                handleError(ex);
            }
        }

        private void messageReceived(IAsyncResult result)
        {
            EndPoint remoteEnd = new IPEndPoint(IPAddress.Any, 0);


            socket.EndReceiveFrom(result, ref remoteEnd);
           
            int headerLength = BitConverter.ToInt32(buffer, 0);
            if (headerLength > buffer.Length)
            {
                handleError(new Exception("header length must be smaller euqual or smaller than buffer size"));
            }
            else
            {
                byte[] receivedData = new byte[headerLength + 4];
                Buffer.BlockCopy(buffer, 0, receivedData, 0, receivedData.Length);
                data.AddRange(receivedData);
                receivedEventArgs = new ReceivedEventArgs(data.ToArray(), remoteEnd);
                raiseReceived(receivedEventArgs);
            }

            buffer = new byte[bufferSize];
            data.Clear();
            getMessage();
        }

     
        private void sent(IAsyncResult result)
        {
            socket.EndSend(result);
            
            if (sentEventArgs == null)
                sentEventArgs = new SentEventArgs();
            raiseSent(sentEventArgs);
        }

        private void handleError(Exception ex)
        {
            raiseError(new ErrorEventArgs(ex));
        }

        #endregion


        #region Events
       
        public class ReceivedEventArgs : EventArgs
        {
            public byte[] MessageByte;
            public EndPoint senderClient;

            public ReceivedEventArgs(byte[] messageByte, EndPoint _senderClient)
            {
                MessageByte = messageByte; senderClient = _senderClient;
            }
        }
        public delegate void ReceivedEventHandler(object sender, ReceivedEventArgs e);
        public event ReceivedEventHandler onReceived;
        private void raiseReceived(ReceivedEventArgs e)
        {
            if (onReceived != null) onReceived(this, e);
        }

        public class SentEventArgs : EventArgs
        {

            public SentEventArgs()
            {

            }
        }

        public delegate void SentEventHandler(object sender, SentEventArgs e);
        public event SentEventHandler onSent;
        private void raiseSent(SentEventArgs e)
        {
            if (onSent != null) onSent(this, e);
        }

        public class ErrorEventArgs : EventArgs
        {
            public Exception Ex;
            public ErrorEventArgs(Exception ex)
            {
                Ex = ex;
            }
        }
        public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
        public event ErrorEventHandler onError;
        private void raiseError(ErrorEventArgs e)
        {
            if (onError != null) onError(this, e);
        }

        #endregion
    }
}
