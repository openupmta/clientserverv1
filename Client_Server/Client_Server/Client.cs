using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client_Server
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        //private void Form1_Load(object sender, EventArgs e)
        //{

        //}

        private void BtnSend_Click(object sender, EventArgs e)
        {
            Send();
            AddMessage(txbMessage.Text);
        }

        IPEndPoint IP; 
        Socket client; 
        /// <summary>
        /// ket noi voi server
        /// </summary>
        void Connect()
        {
            //IP cua server 
            IP= new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client =  new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.IP);
           
            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Khong the ket noi", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Thread listen = new Thread(Recieve);
            listen.IsBackground = true;
            listen.Start();
        }
        /// <summary>
        /// Dong ket noi 
        /// </summary>
        void Close()
        {
            client.Close();
        }
        /// <summary>
        /// gui tin 
        /// </summary>
        void Send()
        {
            if(txbMessage.Text != string.Empty)
                client.Send(Serialize(txbMessage.Text));
        }
        public string MessageCurrent = "Idle";
        private EndPoint end;

        void Send1(string fName)
        {
            string path = "";
            fName = fName.Replace("\\", "/");
            while(fName.IndexOf("/") > -1)
            {
                path += fName.Substring(0, fName.IndexOf("/") + 1);
                fName = fName.Substring(fName.IndexOf("/") + 1);
            }
            byte[] fNameByte = Encoding.ASCII.GetBytes(fName);
            MessageCurrent = "Buffering .... ";
            byte[] fileData = File.ReadAllBytes(path + fName);
            byte[] clientData = new byte[4 + fNameByte.Length + fileData.Length];
            byte[] fNameLen = BitConverter.GetBytes(fNameByte.Length);
            fNameLen.CopyTo(clientData, 0);
            fNameByte.CopyTo(clientData, 4 + fNameByte.Length);
            MessageCurrent = "Connect to server ... ";
            client.Connect(end);


            
        }
        /// <summary>
        /// nhan tin 
        /// </summary>
        void Recieve()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);
                    AddMessage(message);
                }

            }
            catch
            {
                Close();
            }
            
            
        }
        /// <summary>
        /// Dua message len khung chat 
        /// </summary>
        /// <param name="s"></param>
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
            txbMessage.Clear();
                
        }
        /// <summary>
        /// phan manh 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }
        /// <summary>
        /// Gom manh lai
        /// </summary>
        /// <returns></returns>
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
            
        }
        /// <summary>
        /// Dong ket noi khi dong khung 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void Client_Load(object sender, EventArgs e)
        {

        }

        private void lsvMessage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            //Send1();
        }
    }
}
