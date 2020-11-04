using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace Messenger_Server_Part
{
    partial class Program
    {
        static object locker_online_list = new object();
        public class Client_Stream
        {
            public TcpClient client;
            public string name{get;private set;}
            string password;
            bool is_auth = false;
            NetworkStream stream = null;
            public int idList=-1;
            public Client_Stream(TcpClient tcpClient)
            {
                client = tcpClient;
            }

            public void tcpConnection()
            {
                
                List<byte> some_data = new List<byte>();


                password = "";
                try
                {
                    stream = client.GetStream();

                    //регистрация/аутентификация
                    string targ = sRead_stream(stream);
                    stream.Write(Encoding.UTF8.GetBytes("get"));
                    if (targ == "reg")
                    {
                        while (!is_auth)
                        {
                            name = sRead_stream(stream);
                            if (DataWR.can_register(name))
                            {
                                is_auth = true;
                                stream.Write(Encoding.UTF8.GetBytes("логин доступен"));
                            }
                            else
                            {
                                stream.Write(Encoding.UTF8.GetBytes("логин занят"));
                            }
                        }
                        is_auth = false;
                        while (!is_auth)
                        {
                            password = sRead_stream(stream);
                            if (DataWR.register_user(name, password))
                            {
                                is_auth = true;
                                stream.Write(Encoding.UTF8.GetBytes("пароль принят"));
                            }
                            else
                            {
                                stream.Write(Encoding.UTF8.GetBytes("плохой пароль"));
                            }
                        }

                        Console.WriteLine("Пользователь " + name + "  зарегистрирован");
                    }
                    else if (targ == "log")
                    {
                        name = sRead_stream(stream);
                        if (!DataWR.is_registred(name))
                        {
                            stream.Write(Encoding.UTF8.GetBytes("логин не найден"));
                            throw new Exception("ошибка авторизации: не найден логин для " + name);
                        }
                        lock (locker_online_list)
                        {
                            foreach (Client_Stream client in online_list)
                            {
                                if (client.name == name)
                                {
                                    stream.Write(Encoding.UTF8.GetBytes("пользователь уже в сети"));
                                    throw new Exception("!!! ошибка авторизации: пользователь " + name + " уже в сети");
                                }
                            }
                        }
                        stream.Write(Encoding.UTF8.GetBytes("доступен"));
                        password = sRead_stream(stream);

                        if (System.Linq.Enumerable.SequenceEqual(DataWR.get_password_by_name(name), password)) //процесс авторизации
                        {
                            is_auth = true;
                            stream.Write(Encoding.UTF8.GetBytes("авторизирован"));
                        }
                        else
                        {
                            stream.Write(Encoding.UTF8.GetBytes("неверный пароль"));
                            throw new Exception("ошибка авторизации: неверный пароль для " + name);
                        }
                        Console.WriteLine("пользователь " + name + " вошёл в сеть");
                    }
                    else throw new Exception("ошибка подключения: не указанна цель");



                    lock (locker_online_list)
                    {
                        idList = online_list.Count;
                        online_list.Add(this);
                    }
                    byte[] buffer = new byte[64];
                    int count = 0;
                    string command,input = "";

                    while (true)
                    {

                        string command_pattern = @"(^[A-z0-9]+ )";
                        command = "";
                        input = "";
                        Message mail;
                        count = 0;
                        // чтение команды
                        input = sRead_stream(stream);
                        if (input.Length==0)
                        {
                            Message message = new Message();
                            message.content = "alive";
                            message.sender = "@server";
                            message.reciever = name;
                            Send_message(message, this, false);
                            if (!(sRead_stream(stream) == "alive"))
                            {
                                throw new Exception("разыв соедениения для пользователя " + name);
                            }
                            continue;
                        }
                        command = Regex.Match(input, command_pattern).Value.Trim();
                        if (command.Length+1<input.Length)
                        {
                            input = input.Substring(command.Length+1);
                        }

                        switch (command)
                        {
                            case "send":
                                #region 
                                //функция для чтения mail
                                bool is_sended = false;
                                mail = JsonSerializer.Deserialize<Message>(input);
                                lock (locker_online_list)
                                {
                                    //отправка всем пользователям
                                    if (mail.reciever == "@all")
                                    {
                                        is_sended = true;
                                        foreach (Client_Stream user in online_list)
                                        {
                                            Send_message(mail, user,false);
                                        }
                                        break;
                                    }
                                    //отправка сообщения конкретному пользователю 
                                    for (int i = 0; i < online_list.Count; i++)
                                    {
                                        
                                        if (online_list[i].name == mail.reciever)
                                        {
                                            is_sended = true;
                                            Send_message(mail, online_list[i],true);
                                            break;
                                        }
                                    }
                                }

                                if (!is_sended&&DataWR.is_registred(mail.reciever))
                                {
                                    DataWR.save_message_to_mailbox(mail);
                                }
                                #endregion 
                                break;
                            case "GetStory":
                                #region
                                input = input.Trim();

                                //наверное это хороший способ? надо спросить кого0нибудь кто гарит...
                                FileStream fileStream=null;
                                DataWR.Message_worker message_Worker = new DataWR.Message_worker(name,input, out fileStream);
                                Message ms = message_Worker.Next();
                                while (ms.sender!=null)
                                {
                                    Send_message(ms, this,false);
                                    ms = message_Worker.Next();
                                }
                                fileStream.Close();
                                #endregion
                                break;
                            case "GetMailbox":
                                #region
                                input = input.Trim();
                                fileStream = null;
                                message_Worker = new DataWR.Message_worker(input, out fileStream);
                                ms = message_Worker.Next();
                                while (ms.sender != null)
                                {
                                    Send_message(ms, this, true);
                                    ms = message_Worker.Next();
                                }
                                
                                fileStream.Close();
                                #endregion
                                break;
                            default:
                                break;
                        }

                    }

                }
                catch (IOException exp)
                {
                    
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    File.AppendAllText(log_patch, exception.ToString());
                }
                finally
                {
                    if (name != "") Console.WriteLine("подключение закрыто для " + name);
                    if (idList >= 0)
                    {
                        lock (locker_online_list)
                        {
                            for (int i = idList + 1; i < online_list.Count; i++)
                            {
                                online_list[i].idList--;
                            }
                            online_list.RemoveAt(idList);
                        }
                    }
                    if (stream != null)
                        stream.Close();
                    if (client != null)
                        client.Close();
                }
            }

            static public void Send_message(Message mail, Client_Stream user,bool bSave_to_story)
            {
                
                string mess=JsonSerializer.Serialize<Message>(mail)+"\n";
                user.stream.Write(Encoding.UTF8.GetBytes(mess));
                if (bSave_to_story)  DataWR.save_message(mail);
            }

            static string sRead_stream(NetworkStream stream) {
                int len;
                byte[] buffer = new byte[64];
                StringBuilder builder = new StringBuilder();
                do
                {
                    len = stream.Read(buffer, 0, buffer.Length);
                    buffer=crypt.Decrypt(buffer, len);
                    builder.Append(Encoding.UTF8.GetString(buffer, 0, len));
                } while (stream.DataAvailable);
                return builder.ToString();
            }

            string generate_random_str()
            {
                string letters = "qwertyudfghdghjklzxcjdsbnm";
                Random random = new Random();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < random.Next(6,18); i++)
                {
                    sb.Append(letters[random.Next(0, letters.Length - 1)]);
                }
                return sb.ToString();
            }
        }

    }
}
