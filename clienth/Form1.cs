using System;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace clienth
{
    public partial class Form1 : Form
    {
        TcpClient client = new TcpClient();
        NetworkStream stm = default(NetworkStream);

        string readData = null;
        public bool flag=false;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendmsg();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                sendmsg();
            }
        }

        private void getMessage_sync()
        {
         stm = client.GetStream();

         int buffSize = client.ReceiveBufferSize;
         byte[] inStream = new byte[buffSize];
         stm.Read(inStream, 0, buffSize);
         string returndata = Encoding.ASCII.GetString(inStream);
         returndata = returndata.Substring(0, returndata.IndexOf("$"));
         if (returndata == "Ten nick jest zajety")
         {
            flag = true;
         }
         else
         {
            textBox1.Text = "Polaczony z serwerem";
         }
         readData = returndata;
         msg();
        }

        private void getMessage()
        {

            while (true)
            {
             stm = client.GetStream();

             int buffSize = client.ReceiveBufferSize;
             byte[] inStream = new byte[buffSize];

             stm.Read(inStream, 0, buffSize);

             string returndata = Encoding.ASCII.GetString(inStream);
             returndata = returndata.Substring(0, returndata.IndexOf("$"));
             if (returndata == "Ten nick jest zajety")
             {
               flag = true;
             }
             readData = returndata;
             msg();            
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {           
            client.Connect("127.0.0.1", 8880);
            stm = client.GetStream();

            byte[] outStream = Encoding.ASCII.GetBytes(textBox3.Text + "$");
            stm.Write(outStream, 0, outStream.Length);
            stm.Flush();

            getMessage_sync();
         
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
                Thread ctThread = new Thread(getMessage);
                ctThread.Start();
                textBox2.ReadOnly = false;
                textBox3.ReadOnly = true;
                label1.Text = "Twoj nick:";
                button2.Enabled = false;
                button1.Enabled = true;
            }


        }

        private void msg()
        {

            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
                textBox1.Text = textBox1.Text + Environment.NewLine + readData;
        }

        private void sendmsg()
        {
            if (textBox2.Text != "")
            {
                byte[] outStream = Encoding.ASCII.GetBytes(textBox2.Text + "$");
                stm.Write(outStream, 0, outStream.Length);
                stm.Flush();
                textBox2.Text = "";
            }
        }
    }
}