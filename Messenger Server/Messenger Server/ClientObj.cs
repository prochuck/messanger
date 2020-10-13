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
        public class ClientObj
        {
            public TcpClient client;
            string name;
            byte[] key;
            bool is_auth = false;
            NetworkStream stream = null;
            public ClientObj(TcpClient tcpClient)
            {
                client = tcpClient;
            }

            public void tcpConnection()
            {
                XmlSerializer formatter = new XmlSerializer(typeof(message));
                
                List<byte> some_data = new List<byte>();

                
                key = new byte[256];
                try
                {
                    stream = client.GetStream();
                    

                    //приём ключа
                    stream.Read(key, 0, key.Length);
                    stream.Write(serverKey);

                    //регистрация/логининг
                    name = sRead_stream(stream);
                    while (!is_auth)
                    {
                        if (dataWR.is_registred(name))
                        {
                            if (dataWR.get_key_by_name(name) == key) //процесс авторизации
                            {
                                string str = generate_random_str();
                                stream.Write(crypt.Encrypt( Encoding.UTF8.GetBytes(str), serverKey));
                                if (sRead_stream(stream) == str)
                                {
                                    is_auth = true;
                                    stream.Write(Encoding.UTF8.GetBytes("авторизирован"));
                                }
                                else
                                {
                                    stream.Write(Encoding.UTF8.GetBytes("провал авторизации"));
                                    throw new Exception("провал");
                                }
                            }
                        }
                        else
                        {
                            if (dataWR.register_user(name,key))
                            {
                                is_auth = true;
                                stream.Write(Encoding.UTF8.GetBytes("зарегистрирован"));
                            }
                            else
                            {
                                stream.Write(Encoding.UTF8.GetBytes("логин занят"));

                            }
                        }
                    }
                    user ussr;
                    ussr.name = name;
                    ussr.client = client;
                    online_list.Add(ussr);
                    byte[] buffer = new byte[64];
                    int count = 0;
                    while (true)
                    {
                        some_data = new List<byte>();
                        message mail;
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
                            mail = (message)formatter.Deserialize(ms);
                            for (int i = 0; i < online_list.Count; i++)
                            {
                                if (online_list[i].name == mail.addresant)
                                {
                                    ms = new MemoryStream();
                                    formatter.Serialize(ms, mail);
                                    online_list[i].client.GetStream().Write(ms.ToArray());
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
                    if (stream != null)
                        stream.Close();
                    if (client != null)
                        client.Close();
                }
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
