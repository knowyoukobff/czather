using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;
using System.ComponentModel;

namespace server
{
    /// <summary>
    /// Klasa odpowiadajaca za interakcje po stronie serwera.
    /// </summary>
    class Program
    {
        public static Hashtable clientsList = new Hashtable();
        public static Hashtable roomList = new Hashtable();
        public static bool flag;

        /// <summary>
        /// Metoda Main w ktorej serwer nasluchuje klientow po czym dodaje ich do listy.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(ip, 8880);
            TcpClient clientSocket = default;

            serverSocket.Start();
            Console.WriteLine("Serwer uruchomiony");

            while ((true))
            {
                flag = false;
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();

                byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                byte[] bytesClientName = new byte[clientSocket.ReceiveBufferSize - 1];
                byte[] bytesIdRoom = new byte[1];

                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                bytesIdRoom[0] = bytesFrom[0];

                for (int i = 0; i < bytesClientName.Length; i++)
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
                    Stream takenNickstream = clientSocket.GetStream();
                    Byte[] takenNick = null;
                    takenNick = Encoding.ASCII.GetBytes(" Ten nick jest zajety$");
                    takenNickstream.Write(takenNick, 0, takenNick.Length);
                    takenNickstream.Flush();
                    flag = true;
                }
                if (flag == false)
                {
                    roomList.Add(ClientName, IdRoom);
                    broadcast("[" + ClientName + "] dolaczyl do czatu$", ClientName, IdRoom, false);

                    Console.WriteLine("[" + ClientName + "][Pokoj:" + IdRoom + "] dolaczyl do chatu");

                    HandleClinet client = new HandleClinet();
                    client.StartClient(clientSocket, ClientName, clientsList, IdRoom);
                }
            }

        }

        /// <summary>
        /// Metoda ktora rozsyla wiadomosci do klientow.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="uName"></param>
        /// <param name="IdRoom"></param>
        /// <param name="flag_1"></param>
        public static void broadcast(string msg, string uName, string IdRoom, bool flag_1)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;

                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;
                string time = DateTime.Now.ToString("HH:mm");
                string sendingList = "";

                foreach (DictionaryEntry item in roomList)
                {

                    if ((string)item.Value==IdRoom)
                    {
                        sendingList += item.Key+"\r\n";
                    }
                }

                if (flag_1 == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(IdRoom + "[" + uName + "] " + time + " : " + msg + "$" + sendingList);
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(IdRoom + msg + "$" + sendingList);
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }
    }

    /// <summary>
    /// Klasa odpowiadajaca za odbieranie wiadomosci od klientow.
    /// </summary>
    public class HandleClinet
    {
        TcpClient clientSocket;
        string clientName;
        Hashtable clientsList;
        string IdRm;
        Thread ctThread;

        /// <summary>
        /// Metoda ktora przypisuje argumenty do pol oraz uruchamia watek ctThread.
        /// </summary>
        /// <param name="inClientSocket"></param>
        /// <param name="clineNo"></param>
        /// <param name="cList"></param>
        /// <param name="IdRoom"></param>
        public void StartClient(TcpClient inClientSocket, string clineNo, Hashtable cList, string IdRoom)
        {
            this.clientSocket = inClientSocket;
            this.clientName = clineNo;
            this.clientsList = cList;
            this.IdRm = IdRoom;
            ctThread = new Thread(Chat);
            ctThread.Start();
        }

        /// <summary>
        /// Metoda ktora odbiera wiadomosci od klientow po czym wysyla do metody broadcast.
        /// </summary>
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
                    Console.WriteLine("[" + clientName + "][Pokoj:" + IdRm + "]: " + dataFromClient);

                    Program.broadcast(dataFromClient, clientName, IdRm, true);
                }
                catch 
                {
                    Console.WriteLine("Rozlaczono uzytkownika " + clientName);
                    Program.roomList.Remove(clientName);
                    Program.clientsList.Remove(clientName);
                    Program.broadcast("Uzytkownik [" + clientName + "] opuscil pokoj", clientName, IdRm, false);
                    ctThread.Abort();
                }
            }
        }
    }
}
