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
        public bool flag = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void getMessage()
        {

            while (true)
            {
                flag = false;
                if (flag == false)
                {
                    stm = client.GetStream();
                    int buffSize = client.ReceiveBufferSize;
                    byte[] inStream = new byte[buffSize];
                    stm.Read(inStream, 0, buffSize);
                    string returndata = Encoding.ASCII.GetString(inStream);
                    returndata = returndata.Substring(0, returndata.IndexOf("$"));
                    readData = returndata;
                    if (returndata == "Ten nick jest zajety")
                    {
                        flag = true;
                    }
                    msg();
                }

            }
        }
        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void msg()
        {

            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
                textBox1.Text = textBox1.Text + Environment.NewLine + readData;
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}