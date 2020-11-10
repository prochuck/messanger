using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

// важно: нельзя отправлять на сервер \n


namespace TcpClientApp
{

    [Serializable, XmlRoot("message")]
    public struct message
    {
        public string sender { get; set; }
        public string reciever { get; set; }
        public string content { get; set; }
    }
    [Serializable]
    public struct user_data
    {
        public string name { get; set; }
        public string password { get; set; }
    }
    class Program
    {

        private const int port = 7001;
        //109.95.219.97
        private const string server = "127.0.0.1";
        const string user_data_file_name= "data.json";
        static string name;
        static void Main(string[] args)
        {

            name=null;
            bool isReg; 
            string password;
            if (File.Exists(user_data_file_name))
            {
                string jFileText = File.ReadAllText(user_data_file_name);
                user_data user =  JsonSerializer.Deserialize<user_data>(jFileText);
                name = user.name;
                password = user.password;
                isReg = true;
            }
            else
            {
                isReg = false;
                password = "";
            }
            try
            {

                TcpClient client = new TcpClient();
                client.Connect(server, port);
                StringBuilder response = new StringBuilder();
                NetworkStream stream = client.GetStream();

                

                //переключатель зарегистрированности
                //isReg = false;
                
                //отправка своего имени
                string ans=null;
                if (isReg)
                {
                    stream.Write(Encoding.UTF8.GetBytes("log"));
                    sRead_stream(stream);
                    stream.Write(Encoding.UTF8.GetBytes(name));
                    ans=sRead_stream(stream);
                    if (ans == "пользователь уже в сети") throw new Exception("пользователь уже в сети");
                    else if (ans == "логин не найден") throw new Exception("логин не найден");
                    else ans = "";
                    stream.Write(Encoding.UTF8.GetBytes(password));
                    ans = sRead_stream(stream);
                    if (ans!= "авторизирован") throw new Exception("ошибка авторизации");
                    Console.WriteLine(ans);
                }
                else
                {
                    stream.Write(Encoding.UTF8.GetBytes("reg"));
                    sRead_stream(stream);
                    while (ans!= "логин доступен")
                    {
                        if (ans != null) Console.WriteLine(ans);
                        name = Console.ReadLine();
                        stream.Write(Encoding.UTF8.GetBytes(name));
                        ans = sRead_stream(stream);
                    }
                    Console.WriteLine(ans);
                    ans = null;
                    while (ans != "пароль принят")
                    {
                        if (ans != null) Console.WriteLine(ans);
                        password = Console.ReadLine();
                        stream.Write(Encoding.UTF8.GetBytes(password));
                        ans = sRead_stream(stream);
                    }
                    Console.WriteLine(ans);
                }


                if (!isReg)
                {
                    isReg = true;

                    user_data user = new user_data();
                    user.name = name;
                    user.password = password;
                    string a = JsonSerializer.Serialize<user_data>(user);
                    File.WriteAllText(user_data_file_name, a);


                }

                //создание потока вывода данных на экран
                cw_stream np = new cw_stream(stream);
                Thread potok_vivoda = new Thread(new ThreadStart(np.Vivod));
                potok_vivoda.Start();


                //формат команды для сервера: comand <данные для этой команды>
                //список команд:
                //send <получатель> <сообщение> - отправляет сообщение
                //GetStory <отправитель> - получает всю историю переписки
                //GetMessage <отправитель> - получает все не полученные сообщения (сейчас бесполезна, но в когда будет граф. инт. будет иметь смысл
                
                string input,command="";
                
                

                //отправка данных
                do
                {
                    string comand_pattern = @"^[\w]+";
                    input = Console.ReadLine();
                    command = Regex.Match(input, comand_pattern).Value;
                    GroupCollection groups;
                    switch (command)
                    {

                        case "send":
                            #region
                            string send_pattern = @" ([a-z0-9]+) (.+)";
                            groups = Regex.Match(input, send_pattern).Groups;
                            if (groups.Count==3)
                            {
                                message a = new message();
                                a.reciever = groups[1].Value;
                                a.content = groups[2].Value;
                                a.sender = name;
                                string ms=JsonSerializer.Serialize<message>(a)+"\n";
                                stream.Write(Encoding.UTF8.GetBytes("send " + ms));
                            }
                            else
                            {
                                Console.WriteLine("неправильный формат команды");
                            }
                            #endregion
                            break;
                        case "GetStory":
                            #region
                            string GetStory_pattern = @" ([a-z0-9]+)";
                            groups = Regex.Match(input, GetStory_pattern).Groups;
                            if (groups.Count!=1)
                            {
                                stream.Write(Encoding.UTF8.GetBytes("GetStory "+ groups[0].Value + "\n"));
                            }
                            else
                            {
                                Console.WriteLine("неправильный формат команды");
                            }
                            #endregion
                            break;
                        case "GetMailbox":
                            if (input.Length != 0)
                            {
                                stream.Write(Encoding.UTF8.GetBytes("GetMailbox " + name + "\n"));
                            }
                            else
                            {
                                Console.WriteLine("неправильный формат команды");
                            }
                            break;
                        default:
                            Console.WriteLine("Команда не найдена");
                            break;
                    }
                    command = "";
                    input = "";
                } while (true);


            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }

            Console.WriteLine("Запрос завершен...");
        }
        public class cw_stream
        {
            NetworkStream stream;
            public cw_stream(NetworkStream stream)
            {
                this.stream = stream;
            }
            public void Vivod()
            {
                //Вооообщем, вся эта конструкция нужна на случай, если много сообщение приёдйт одовременно.
                //Теперь любое сообщение кончается на \n.
                //В случае если системаа прочитала сразу 2 сообщения за раз, она сначала обработает 1, а второе сохранит в tmp
                //и прочитает его при следующем проходе. Как-то так. Это лучшее, что я смог придумать.
                string tmp=""; // переносит символы, не относящиеся к текущему сообщение в следующие сообщение.
                message mail;
                string jMail=" ";
                while (true)
                {
                    try
                    {

                        if (tmp.Length == 0)
                        {
                            jMail = sRead_stream(stream);
                        }
                        else if (tmp.EndsWith("\n"))
                        {
                            jMail = tmp;
                        }
                        else
                        {
                            jMail =tmp+ sRead_stream(stream);
                        }
                        if (jMail.Contains("\n"))
                        {
                            tmp = jMail.Substring(jMail.IndexOf("\n")+1);
                            jMail = jMail.Substring(0, jMail.IndexOf("\n"));
                        }

                        mail = JsonSerializer.Deserialize<message>(jMail);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(mail.sender);
                        Console.ResetColor();
                        Console.WriteLine(": " + mail.content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
        static string sRead_stream(NetworkStream stream)
        {
            
            int len;
            byte[] buffer = new byte[64];
            StringBuilder builder = new StringBuilder();
            do
            {
                len = stream.Read(buffer, 0, buffer.Length);
                buffer = crypt.Decrypt(buffer, len);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, len));
            } while (stream.DataAvailable);
            return builder.ToString();
        }
        static public class crypt
        {
            static public byte[] Encrypt(byte[] data, byte[] key) { return data; }
            static public byte[] Decrypt(byte[] data, int len) { return data; }
        }
    }
    
}
