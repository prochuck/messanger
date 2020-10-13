using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Serialization;

namespace Messenger_Server_Part
{
    [Serializable, XmlRoot("message")]
    public struct message
    {
        public string addresant;
        public string content;
    }

    partial class Program
    {
        public struct user
        {
            public string name;
            public TcpClient client;
        }
        const string user_data_patch = "user_data.bin";
        const string log_patch = "log.txt";
        const int port = 8888;
        static byte[] serverKey = new byte[256];
        static List<user> online_list = new List<user>();
        static void Main(string[] args)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(message));
            serverKey[5] = 1;
            TcpClient tcpClient;
            TcpListener server = null;
            if (!File.Exists(user_data_patch))
            {
                FileStream user_data = File.Create(user_data_patch);
            }

            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();
                while (true)
                {
                    tcpClient = server.AcceptTcpClient();
                    var a= tcpClient.GetStream();
                    var b= tcpClient.GetStream();
                    ClientObj client = new ClientObj(tcpClient);
                    Thread tcpClientThread = new Thread(new ThreadStart(client.tcpConnection));
                    
                    tcpClientThread.Start();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                File.AppendAllText(log_patch, exception.ToString());
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
            }

        }

    }
}
