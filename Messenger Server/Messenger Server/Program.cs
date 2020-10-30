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
        static List<Client_Stream> online_list = new List<Client_Stream>();
        static void Main(string[] args)
        {
            if (!File.Exists(user_data_patch))
            {
                FileStream user_data = File.Create(user_data_patch);
                user_data.Close();
            }
            Thread sThread = new Thread(Server_Thread);
            sThread.Name = "Server Thread";
            sThread.IsBackground = true;
            sThread.Start();


            bool isend = false;
            string command;
            while (!isend)
            {
                command=Console.ReadLine();
                switch (command)
                {
                    //останавливает сервер
                    case "stop":
                        isend = true;
                        break;
                    default:
                        break;
                }
                command = "";
            }
            
        }

        private static void Server_Thread()
        {
            TcpClient tcpClient;
            TcpListener server = null;
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                while (true)
                {
                    tcpClient = server.AcceptTcpClient();
                    Client_Stream client = new Client_Stream(tcpClient);
                    Thread tcpClientThread = new Thread(new ThreadStart(client.tcpConnection));
                    tcpClientThread.IsBackground = true;
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
