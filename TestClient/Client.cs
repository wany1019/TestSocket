using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace TestClient
{
    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousClient
    {
        private static ManualResetEvent sendDone =
       new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private const int port = 11000;

        public static Socket client;

        private static void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                client.Connect(remoteEP);

                Console.WriteLine("선수 입장");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    
                    int bytesRec = client.Receive(bytes);


                    NetworkStream networkStream = new NetworkStream(client);


                    string strJson = Encoding.Unicode.GetString(bytes, 0, bytesRec);


                    Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private static void Send()
        {
            string data = Console.ReadLine() + "<EOF>";

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);

            sendDone.WaitOne();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void EndClinet()
        {
            // Release the socket.  
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void Update()
        {
            
        }

        public static int Main(String[] args)
        {
            StartClient();

            Thread t = new Thread(new ThreadStart(Receive));
            t.IsBackground = true;
            t.Start();

            while (true)
            {
                Send();
            }

            EndClinet();
            return 0;
        }
    }

    public class TestInfo
    {
        public string Id { get; set; }
        public List<string> tList = new List<string>();
    }
}