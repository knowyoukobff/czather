using System;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;

namespace clienth
{
    /// <summary>
    /// Klasa odpowiadająca za interakcje uzytkownika w oknie klienta.
    /// </summary>
    public partial class Form : System.Windows.Forms.Form
    {
        TcpClient client = new TcpClient();
        NetworkStream stm = default;

        string clntroom;
        string readData = null;
        string readList = null;
        public bool flag=false;

        public Color black = Color.FromArgb(0, 0, 0);
        public Color white = Color.FromArgb(255, 255, 255);
        public Color grey = Color.FromArgb(105, 105, 105);

        /// <summary>
        /// Konstruktor klasy, ktory inicjalizuje komponenty.
        /// </summary>
        public Form()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Metoda ktora uruchamia metode Sendmsg().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_btn_Click(object sender, EventArgs e)
        {
            Sendmsg();
        }

        /// <summary>
        /// Metoda ktora uruchamia metode Sendmsg() za pomoca klawisza Enter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                Sendmsg();
            }       
        }

        /// <summary>
        /// Metoda ktora zmienia wyglad aplikacji na tryb nocny.
        /// </summary>
        public void ChangeToNightmode()
        {
                BackColor = black;
                checkBox1.ForeColor = white;
                label1.ForeColor = white;
                label2.ForeColor = white;
                textBox1.BackColor = grey;
                textBox1.ForeColor = white;
                textBox4.BackColor = grey;
                textBox4.ForeColor = white;
                label3.ForeColor = white;
        }

        /// <summary>
        /// Metoda ktora zmienia wyglad aplikacji na tryb dnia.
        /// </summary>
        public void ChangeToDaymode()
        {
                BackColor = white;
                checkBox1.ForeColor = black;
                label1.ForeColor = black;
                label2.ForeColor = black;
                textBox1.ForeColor = black;
                textBox1.BackColor = white;
                textBox4.BackColor = white;
                textBox4.ForeColor = black;
                label3.ForeColor = black;
        }

        /// <summary>
        /// Metoda ktora dziala jak GetMessage tylko w trybie synchronicznym.
        /// </summary>
        private void GetMessage_sync()
        {
         stm = client.GetStream();
         int buffSize = client.ReceiveBufferSize;

         byte[] inStream = new byte[buffSize];
         byte[] bytesMsg = new byte[client.ReceiveBufferSize - 1];

         stm.Read(inStream, 0, buffSize);

         for (int i = 0; i < bytesMsg.Length; i++)
         {
            bytesMsg[i] = inStream[i + 1];
         }

         string getMsg = Encoding.ASCII.GetString(bytesMsg);   
         string getList = Encoding.ASCII.GetString(bytesMsg);
         getMsg = getMsg.Substring(0, getMsg.IndexOf("$"));
         getList = getList.Substring(getList.LastIndexOf("$"));
         getList = getList.TrimStart('$');

         if (getMsg == "Ten nick jest zajety")
         {
            flag = true;
         }

         else
         {
            textBox1.Text = "Polaczony z serwerem";
         }

         if(readList!= getList)
         {
            readList = getList;
         }

         readData = getMsg;
         Msg();
        }

        /// <summary>
        /// Metoda ktora pobiera wiadomosci z serwera i wysyla do metody Msg().
        /// </summary>
        private void GetMessage()
        {
            while (true)
            {
                stm = client.GetStream();
                int buffSize = client.ReceiveBufferSize;

                byte[] inStream = new byte[buffSize];
                byte[] bytesMsg = new byte[client.ReceiveBufferSize - 1];
                byte[] bytesIdRoom = new byte[1];

                stm.Read(inStream, 0, buffSize);
                bytesIdRoom[0] = inStream[0];

                for (int i = 0; i < bytesMsg.Length; i++)
                {
                    bytesMsg[i] = inStream[i + 1];
                }

                string getMsg = Encoding.ASCII.GetString(bytesMsg);
                string idRoom = Encoding.ASCII.GetString(bytesIdRoom);
                string getList = Encoding.ASCII.GetString(bytesMsg);
                getMsg = getMsg.Substring(0, getMsg.IndexOf("$"));
                getList = getList.Substring(getList.LastIndexOf("$"));
                getList = getList.TrimStart('$');
                if (getMsg == "Ten nick jest zajety")
                {
                    flag = true;
                }
                if (clntroom == idRoom)
                {
                    readData = getMsg;
                    if(readList!=getList)
                    {
                        readList = getList;
                    }
                }                
                else
                {
                    readData = "";
                }
                Msg();            
             }
        }

        /// <summary>
        /// Metoda ktora laczy klienta z serwerem po czym uruchamia watek ctThread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_btn_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                client.Connect("127.0.0.1", 8880);
                stm = client.GetStream();
                this.clntroom = idroom.Text;
                byte[] outStream = Encoding.ASCII.GetBytes(idroom.Text + textBox3.Text + "$");
                stm.Write(outStream, 0, outStream.Length);
                stm.Flush();

                GetMessage_sync();

                if (flag is true)
                {
                    textBox1.Text = "Nick zajety..Sprobuj ponownie";
                    textBox3.Text = "";
                    client.Close();
                    stm.Close();
                    client = new TcpClient();
                    flag = false;
                }
                else
                {
                    Thread ctThread = new Thread(GetMessage);
                    ctThread.Start();
                    ctThread.IsBackground = true;
                    textBox2.ReadOnly = false;
                    textBox3.ReadOnly = true;
                    idroom.ReadOnly = true;
                    label1.Text = "Twoj nick:";
                    Connect_btn.Enabled = false;
                    Send_btn.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Metoda ktora zapisuje wiadomosci odebrane od serwera do textBoxa.
        /// </summary>
        private void Msg()
        {

            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(Msg));
            else
            {
                if (readData == "")
                {
                    textBox1.Text += readData;
                }
                else
                {
                    textBox4.Text = readList;
                    textBox1.Text = textBox1.Text + Environment.NewLine + readData;
                }
            }
        }

        /// <summary>
        /// Metoda ktora wysyla wiadomosci do serwera.
        /// </summary>
        private void Sendmsg()
        {
            if (textBox2.Text != "")
            {
                byte[] outStream = Encoding.ASCII.GetBytes(textBox2.Text + "$");
                stm.Write(outStream, 0, outStream.Length);
                stm.Flush();
                textBox2.Text = "";
            }
        }

        /// <summary>
        /// Metoda ktora przewija textBox na sam dol po kazdej wiadomosci z serwera.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
        }

        /// <summary>
        /// Metoda ktora uruchamia Tryb Nocny.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = sender as CheckBox;
            if(check.Checked)
            {
                ChangeToNightmode();
            }
            else
            {
                ChangeToDaymode();
            }
        }

       
    }
}