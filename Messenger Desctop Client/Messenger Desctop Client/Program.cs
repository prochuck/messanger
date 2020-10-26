using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Xml.Serialization;

namespace TcpClientApp
{
    [Serializable, XmlRoot("message")]
    public struct message
    {
        public string addresant { get; set; }
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
        private const string server = "127.0.0.1";
        const string user_data_file_name= "data.json";
        static void Main(string[] args)
        {
            string name=null;
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
                byte[] data = new byte[256];
                StringBuilder response = new StringBuilder();
                NetworkStream stream = client.GetStream();

                XmlSerializer formatter = new XmlSerializer(typeof(message));

                //переключатель зарегистрированности
                //isReg = false;
                

                //отправка своего имени
                string ans=null;
                if (isReg)
                {
                    stream.Write(Encoding.UTF8.GetBytes("log"));
                    sRead_stream(stream);
                    stream.Write(Encoding.UTF8.GetBytes(name));
                    sRead_stream(stream);
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

                //отправка сообщений
                do
                {
                    message a=new message();
                    a.addresant = Console.ReadLine();
                    a.content = Console.ReadLine();
                    MemoryStream ms = new MemoryStream();
                    formatter.Serialize(ms, a);
                    byte[] crypted=crypt.Encrypt(ms.ToArray(), data);
                    stream.Write(crypted);
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
                XmlSerializer formatter = new XmlSerializer(typeof(message));
                while (true)
                {
                    List<byte> some_data = new List<byte>();
                    byte[] buffer = new byte[64];
                    message mail;
                    int count = 0;
                    do
                    {
                        count += stream.Read(buffer);
                        some_data.AddRange(buffer);
                    } while (stream.DataAvailable);
                    if (count % buffer.Length != 0) some_data.RemoveRange(count, some_data.Count - count);
                    if (count == 0) continue;
                    MemoryStream ms = new MemoryStream(crypt.Decrypt(some_data.ToArray(), some_data.Count));
                    mail = (message)formatter.Deserialize(ms);
                    Console.WriteLine(mail.content);
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