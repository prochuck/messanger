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
            string command = "", commtext;
            while (!isend)
            {
                commtext = Console.ReadLine();
                int b = commtext.Length;
                for (int i = 0; i <= b - 1; i++)
                {
                    if (commtext[i] == ' ') break;
                    command = command + commtext[i];
                }
                string com = commtext;
                commtext = "";
                bool f = false;
                for (int i = 0; i <= b - 1; i++)
                {
                    if (com[i] == ' ') f = true;
                    if (f) commtext = commtext + com[i];
                }

                switch (command)
                {
                    case "stop": //Останавливает сервер
                        isend = true;
                        break;
                    case "help": //Выводит доступные команды для сервера
                        Console.WriteLine("Список доступных команд: " +
                                          "\n1)stop - остановка сервера" +
                                          "\n2)help - выводит все доступные команды для сервера" +
                                          "\n3)list - показывает список подключенных пользователей" +
                                          "\n4)mail - <mail имя_пользователя сообщение> отправка сообщения одному пользователю" +
                                          "\n5)say - <say сообщение> отправка сообщения всем пользователям от имени сервера" +
                                          "\n6)admin - <admin имя_пользователя> выдача прав администратора\n" +
                                          "\n7)mute <user name> - блокировка отправки сообщения для определённого пользователя");
                        break;
                    case "list": //Список подключенных пользователей    
                        if (1 > online_list.Count) { Console.WriteLine("В настоящий момент онлайна нет =(\n"); break; }
                        int i = 1;
                        foreach (Client_Stream client in online_list)
                        {
                            Console.WriteLine(i++ + ")" + client.name + "\n"); ;
                        }
                        Console.WriteLine("\n");
                        break;
                    case "mail": //Отправка сообщения одному пользователю   

                        break;
                    case "say": //Отправка сообщения всем пользователям от имени сервера
                        foreach (Client_Stream user in online_list)
                        {
                            /*   Messenger_Server_Part.Program.Client_Stream. some_data;
                               some_data = new List<byte>();
                               Message mail = new Message(); ;
                               int count = 1;
                             Messenger_Server_Part.Program.Client_Stream stream;
                               // чтение сообщений
                               do
                               {
                                   count += stream.Read(buffer);
                                   some_data.AddRange(buffer);
                               } while (stream.DataAvailable);
                               if (count % buffer.Length != 0) some_data.RemoveRange(count, some_data.Count - count);
                               if (count != 0)
                               {
                                   MemoryStream ms = new MemoryStream(crypt.Decrypt(some_data.ToArray(), some_data.Count));
                                 Messenger_Server_Part.Program.Client_Stream.Send_message(mail, user);
                                 */
                            Message a = new Message();
                            a.reciever = "talik";
                            a.content = commtext;
                            a.sender = "server";
                            Messenger_Server_Part.Program.Client_Stream.Send_message(a, user);

                            // }
                        }
                        break;
                    case "admin": //Присвоение прав администратора
                        break;
                    case "mute": //выдача мута пользователю
                        break;
                    default:
                        break;
                }
                commtext = "";
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
