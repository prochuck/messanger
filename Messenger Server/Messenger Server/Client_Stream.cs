using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace Messenger_Server_Part
{
    partial class Program
    {


        static object locker_online_list = new object();
        public class Client_Stream
        {
            public TcpClient client;
            string name;
            string password;
            bool is_auth = false;
            NetworkStream stream = null;
            public int idList=-1;
            XmlSerializer formatter = new XmlSerializer(typeof(Message));
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


                    

                    //регистрация/логининг
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
                    }
                    else if (targ == "log")
                    {

                        
                        name = sRead_stream(stream);
                        if (!DataWR.is_registred(name))
                        {
                            stream.Write(Encoding.UTF8.GetBytes("логин не найден"));
                            throw new Exception("провал");
                        }
                        lock (locker_online_list)
                        {
                            foreach (Client_Stream client in online_list)
                            {
                                if (client.name == name)
                                {
                                    stream.Write(Encoding.UTF8.GetBytes("пользователь уже в сети"));
                                    throw new Exception("провал");
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
                            throw new Exception("провал");
                        }
                    }
                    else throw new Exception("no target");
                    lock (locker_online_list)
                    {
                        idList = online_list.Count;
                        online_list.Add(this);
                    }
                    byte[] buffer = new byte[64];
                    int count = 0;
                    while (true)
                    {
                        some_data = new List<byte>();
                        Message mail;
                        count = 0;
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
                            mail = (Message)formatter.Deserialize(ms);
                            lock (locker_online_list)
                            {
                                //отправка всем пользователям
                                if (mail.reciever=="@all")
                                {
                                    foreach (Client_Stream user in online_list)
                                    {
                                        Send_message(mail, user);
                                    }
                                }

                                    
                                //отправка сообщения конкретному пользователю 
                                for (int i = 0; i < online_list.Count; i++)
                                {
                                    if (online_list[i].name == mail.reciever)
                                    {
                                        Send_message(mail, online_list[i]);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (IOException exp)
                {
                    Console.WriteLine("подключение закрыто");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    File.AppendAllText(log_patch, exception.ToString());
                }
                finally
                {
                    if (idList >= 0) {
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

            private void Send_message(Message mail, Client_Stream user)
            {
                MemoryStream ms1 = new MemoryStream();
                formatter.Serialize(ms1, mail);
                user.client.GetStream().Write(ms1.ToArray());
                DataWR.save_message(mail);
            }

            string sRead_stream(NetworkStream stream) {
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
                string letters = "qwertyuiopasdfghjklzxcvbnm";
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
