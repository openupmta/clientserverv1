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
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            foreach(Socket item in clientList)
            {
                Send(item);
            }
            AddMessage(txbMessage.Text);
            txbMessage.Clear();
            
        }

        IPEndPoint IP;
        Socket server;

        List<Socket> clientList;
        /// <summary>
        /// ket noi voi server
        /// </summary>
        void Connect()
        {
            clientList = new List<Socket>();
            //IP cua server 
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            server.Bind(IP);

            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receive = new Thread(Recieve);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                    //100 thang trong hang cho
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
               

                
            });
            Listen.IsBackground = true;
            Listen.Start();
        }
        /// <summary>
        /// Dong ket noi 
        /// </summary>
        void Close()
        {
            server.Close();
        }
        /// <summary>
        /// gui tin 
        /// </summary>
        void Send(Socket client)
        {
            if (client != null && txbMessage.Text != string.Empty)
                client.Send(Serialize(txbMessage.Text));
        }
        /// <summary>
        /// nhan tin 
        /// </summary>
        void Recieve(Object obj)
        {
            Socket client = obj as Socket;  
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);

                    foreach(Socket item in clientList)
                    {
                        if(item != null && item != client)
                            item.Send(Serialize(message));
                    }
                    AddMessage(message);
                }

            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }


        }
        /// <summary>
        /// Dua message len khung chat 
        /// </summary>
        /// <param name="s"></param>
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
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
    }
}
