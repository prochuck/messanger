using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        NetworkStream stream;
        public MainWindow()
        {
            InitializeComponent();

        }
        [Serializable]
        public struct Message
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
        private const int port = 7001;
        //109.95.219.97
        private const string server = "127.0.0.1";
        const string user_data_file_name = "data.json";
        static string name;
        private void Window_Loaded(object sender, EventArgs ev)
        {
            name = null;
            bool isReg;
            string password;
            if (File.Exists(user_data_file_name))
            {
                string jFileText = File.ReadAllText(user_data_file_name);
                user_data user = JsonConvert.DeserializeObject<user_data>(jFileText);
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
                stream = client.GetStream();



                //переключатель зарегистрированности
                //isReg = false;

                //отправка своего имени
                string ans = null;
                if (isReg)
                {
                    stream.Write(Encoding.UTF8.GetBytes("log"), 0, Encoding.UTF8.GetBytes("log").Length);
                    sRead_stream(stream);
                    stream.Write(Encoding.UTF8.GetBytes(name), 0, Encoding.UTF8.GetBytes(name).Length);
                    ans = sRead_stream(stream);
                    if (ans == "пользователь уже в сети") throw new Exception("пользователь уже в сети");
                    else if (ans == "логин не найден") throw new Exception("логин не найден");
                    else ans = "";
                    stream.Write(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetBytes(password).Length);
                    ans = sRead_stream(stream);
                    if (ans != "авторизирован") throw new Exception("ошибка авторизации");
                    Console.WriteLine(ans);
                }
                else
                {
                    stream.Write(Encoding.UTF8.GetBytes("reg"), 0, Encoding.UTF8.GetBytes("reg").Length);
                    sRead_stream(stream);
                    while (ans != "логин доступен")
                    {
                        if (ans != null) Console.WriteLine(ans);
                        name = Console.ReadLine();
                        stream.Write(Encoding.UTF8.GetBytes(name), 0, Encoding.UTF8.GetBytes("reg").Length);
                        ans = sRead_stream(stream);
                    }
                    Console.WriteLine(ans);
                    ans = null;
                    while (ans != "пароль принят")
                    {
                        if (ans != null) Console.WriteLine(ans);
                        password = Console.ReadLine();
                        stream.Write(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetBytes(password).Length);
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
                    string a = JsonConvert.SerializeObject(user);
                    File.WriteAllText(user_data_file_name, a);
                }

                //создание потока вывода данных на экран
                cw_stream np = new cw_stream(stream, this);
                Thread potok_vivoda = new Thread(new ThreadStart(np.Vivod));
                potok_vivoda.IsBackground = true;
                potok_vivoda.Name = "Input_Thread";
                potok_vivoda.Start();


                //формат команды для сервера: comand <данные для этой команды>
                //список команд:
                //send <получатель> <сообщение> - отправляет сообщение
                //GetStory <отправитель> - получает всю историю переписки
                //GetMessage <отправитель> - получает все не полученные сообщения (сейчас бесполезна, но в когда будет граф. инт. будет иметь смысл


                //отправка данных
                /*
                do
                {
                    string comand_pattern = @"^[\w]+";
                    input = Console.ReadLine();
                    command = Regex.Match(input, comand_pattern).Value;
                    GroupCollection groups;
                    byte[] data;
                    switch (command)
                    {
                        case "send":
                            #region
                            string send_pattern = @" ([a-z0-9]+) (.+)";
                            groups = Regex.Match(input, send_pattern).Groups;
                            if (groups.Count == 3)
                            {
                                message a = new message();
                                a.reciever = groups[1].Value;
                                a.content = groups[2].Value;
                                a.sender = name;
                                string ms = JsonConvert.SerializeObject(a);
                                data = Encoding.UTF8.GetBytes("send " + ms);
                                stream.Write(data, 0, data.Length);
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
                            if (groups.Count != 1)
                            {
                                data = Encoding.UTF8.GetBytes("GetStory " + groups[0].Value);
                                stream.Write(data, 0, data.Length);
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
                                data = Encoding.UTF8.GetBytes("GetMailbox " + "name");
                                stream.Write(data, 0, data.Length);
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
                */


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        private void input_box_TextInput(object sender, TextCompositionEventArgs e)
        {

            string comand_pattern = @"^[\w]+";
            string input = input_box.Text;
            input_box.Text = "";
            string command = Regex.Match(input, comand_pattern).Value;
            GroupCollection groups;
            byte[] data;
            switch (command)
            {
                case "send":
                    #region
                    string send_pattern = @" ([a-z0-9]+) (.+)";
                    groups = Regex.Match(input, send_pattern).Groups;
                    if (groups.Count == 3)
                    {
                        Message a = new Message();
                        a.reciever = groups[1].Value;
                        a.content = groups[2].Value;
                        a.sender = name;
                        string ms = JsonConvert.SerializeObject(a);
                        data = Encoding.UTF8.GetBytes("send " + ms);
                        stream.Write(data, 0, data.Length);
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
                    if (groups.Count != 1)
                    {
                        data = Encoding.UTF8.GetBytes("GetStory " + groups[0].Value);
                        stream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        Console.WriteLine("неправильный формат команды");
                    }
                    #endregion
                    break;
                case "GetMailbox":
                    data = Encoding.UTF8.GetBytes("GetMailbox " + name);
                    stream.Write(data, 0, data.Length);
                    break;
                default:
                    Console.WriteLine("Команда не найдена");
                    break;
            }
        }
    }
}
