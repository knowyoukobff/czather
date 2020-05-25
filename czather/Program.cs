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
        public static Hashtable roomList = new Hashtable();
        public static bool flg;
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(ip, 8880);
            TcpClient clientSocket = default(TcpClient);

            serverSocket.Start();
            Console.WriteLine("Serwer uruchomiony");
            
            while ((true))
            {
                flg = false;
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();
                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                byte[] bytesClientName = new byte[clientSocket.ReceiveBufferSize-1];
                byte[] bytesIdRoom = new byte[1];
                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                bytesIdRoom[0] = bytesFrom[0];
                for(int i=0;i< bytesClientName.Length;i++)
                {
                    bytesClientName[i] = bytesFrom[i + 1];
                }
                string IdRoom = Encoding.ASCII.GetString(bytesIdRoom);
                string ClientName = Encoding.ASCII.GetString(bytesClientName);
                ClientName = ClientName.Substring(0, ClientName.IndexOf("$"));
                try
                {
                    clientsList.Add(ClientName, clientSocket);
                }
                catch
                {
                    Stream str = clientSocket.GetStream();
                    Byte[] takenNick = null;
                    takenNick = Encoding.ASCII.GetBytes(" Ten nick jest zajety$");
                    str.Write(takenNick, 0, takenNick.Length);
                    str.Flush();
                    flg = true;
                }
                if (flg == false)
                {
                    if (roomList.ContainsKey(IdRoom))
                    {
                        roomList[IdRoom] += ClientName + "\r\n";
                    }
                    else
                    {
                        roomList.Add(IdRoom, ClientName + "\r\n");
                    }
                    broadcast("[" + ClientName + "] dolaczyl do czatu$", ClientName, IdRoom, false);
                    Console.WriteLine("[" + ClientName + "][Pokoj:"+ IdRoom + "] dolaczyl do chatu");
                    handleClinet client = new handleClinet();
                    client.startClient(clientSocket, ClientName, clientsList, IdRoom);
                }
            }

        }

        public static void broadcast(string msg, string uName, string IdRoom, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;

                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();

                Byte[] broadcastBytes = null;
                string time = DateTime.Now.ToString("HH:mm");

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(IdRoom+"[" + uName + "] "+time + " : " + msg + "$" + roomList[IdRoom]);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(IdRoom+msg + "$" + roomList[IdRoom]);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }  
    }


    public class handleClinet
    {
        TcpClient clientSocket;
        string clientName;
        Hashtable clientsList;
        string IdRm;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList, string IdRoom)
        {
            this.clientSocket = inClientSocket;
            this.clientName = clineNo;
            this.clientsList = cList;
            this.IdRm = IdRoom;
            Thread ctThread = new Thread(Chat);
            ctThread.Start();
        }

        private void Chat()
        {
            while ((true))
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                    string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine("[" + clientName + "][Pokoj:"+IdRm+"]: " + dataFromClient);

                    Program.broadcast(dataFromClient, clientName, IdRm, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    } 
}
