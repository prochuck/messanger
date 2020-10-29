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
    public struct Message
    {
        public string sender { get; set; }
        public string reciever { get; set; }
        public string content { get; set; }
    }

    partial class Program
    {
        
        const string message_history_name = @"message history";
        const string user_data_patch = "user_data.bin";
        const string log_patch = "log.txt";
        const int port = 7001;
        static byte[] serverKey = new byte[256];
        static List<Client_Stream> online_list = new List<Client_Stream>();
        static void Main(string[] args)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Message));
            serverKey[5] = 1;
            TcpClient tcpClient;
            TcpListener server = null;
            if (!File.Exists(user_data_patch))
            {
                FileStream user_data = File.Create(user_data_patch);
                user_data.Close();
            }

            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                while (true)
                {
                    tcpClient = server.AcceptTcpClient();
                    Client_Stream client = new Client_Stream(tcpClient);
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
