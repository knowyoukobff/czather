using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

namespace serverh
{
    class Program
    {
        public static Hashtable clientsList = new Hashtable();
        public static bool flg;
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(ip, 8880);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;

            while ((true))
            {
                flg = false;
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();
                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                try
                {
                    clientsList.Add(dataFromClient, clientSocket);
                }
                catch
                {
                    Stream str = clientSocket.GetStream();
                    Byte[] error = null;
                    error = Encoding.ASCII.GetBytes("Ten nick jest zajety$");
                    str.Write(error, 0, error.Length);
                    str.Flush();
                    flg = true;
                }
                if (flg == false)
                {
                    broadcast("[" + dataFromClient + "] dolaczyl do czatu$", dataFromClient, false);
                    Console.WriteLine("[" + dataFromClient + "] dolaczyl do chatu");
                    handleClinet client = new handleClinet();
                    client.startClient(clientSocket, dataFromClient, clientsList);
                }
            }

        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes("[" + uName + "]: " + msg + "$");
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }  //end broadcast function
    }//end Main class


    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                    string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("[" + clNo + "]: " + dataFromClient);
                    string rCount = Convert.ToString(requestCount);

                    Program.broadcast(dataFromClient, clNo, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }//end while
        }//end doChat
    } //end class handleClinet
}//end namespace
