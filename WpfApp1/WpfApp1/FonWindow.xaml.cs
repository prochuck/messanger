using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
using static messanger_ui.MainWindow;

namespace messanger_ui
{
    /// <summary>
    /// Логика взаимодействия для FonWindow.xaml
    /// </summary>


    public partial class FonWindow : Window
    {
        static public NetworkStream stream;
        static public string user_name;
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


        public FonWindow()
        {

            InitializeComponent();
            //OpenTest();

        }
        /*public void FileCreat()
        {
            if (!File.Exists(user_data_file_name))
                File.Create(user_data_file_name);
        }
        */
        public enum pages
        {
            login
        }
        private void ButLogin_Click(object sender, RoutedEventArgs e)
        {
            Podskazka1.Text = "";
            Podskazka2.Text = "";
            TcpClient client = new TcpClient();  // подключаемся к серверу
            try
            {
                client.Connect(server, port);

                stream = client.GetStream();

                string sLogin = loginAvto.Text;
                string sParol = ParolAvto.Password;
                string sServer_response;

                byte[] outLoginR = System.Text.Encoding.UTF8.GetBytes(sLogin);
                byte[] outParolR = System.Text.Encoding.UTF8.GetBytes(sParol);
                byte[] OtvetLR = new byte[256];

                stream.Write(Encoding.UTF8.GetBytes("log"), 0, Encoding.UTF8.GetBytes("log").Length);
                sRead_stream(stream);



                if (!String.IsNullOrEmpty(sLogin))   // проверяем заполнение логина
                {
                    Podskazka1.Visibility = Visibility.Collapsed;
                    stream.Write(outLoginR, 0, outLoginR.Length);        // отправляем логин на сервер
                    sServer_response = sRead_stream(stream); // получаем ответ о возможности такого логина


                    if (sServer_response != "доступен")
                    {
                        Podskazka1.Text = sServer_response;
                        Podskazka1.Foreground = Brushes.Red;
                        Podskazka1.Visibility = Visibility.Visible;
                        throw new Exception(sServer_response);
                    }

                }
                else
                {
                    Podskazka1.Text = "Введите логин";
                    Podskazka1.Foreground = Brushes.Red;
                    Podskazka1.Visibility = Visibility.Visible;
                    throw new Exception("нет логина");
                }
                if (!String.IsNullOrEmpty(sParol))   // проверяем заполнение пароля
                {
                    Podskazka2.Visibility = Visibility.Collapsed;
                    stream.Write(outParolR, 0, outParolR.Length);    // отправляем пароль на сервер
                    byte[] OtvetPR = new byte[256];
                    sServer_response = sRead_stream(stream); // получаем ответ о возможности такого пароля
                    if (sServer_response != "авторизирован") // если пароль проходит открываем окно чата
                    {
                        Podskazka2.Text = sServer_response;
                        Podskazka2.Foreground = Brushes.Red;
                        Podskazka2.Visibility = Visibility.Visible;
                        throw new Exception(sServer_response);
                    }
                }
                else
                {
                    Podskazka2.Text = "введите пароль";
                    Podskazka2.Foreground = Brushes.Red;
                    Podskazka2.Visibility = Visibility.Visible;
                    throw new Exception("нет пароля");
                }

                Start_io_stream(stream, sLogin);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                if (stream != null)
                {
                    stream.Close();
                }
            }
            #region

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

            #endregion
        }

        

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Podskazka1.Text = "";
            Podskazka2.Text = "";
            TcpClient client = new TcpClient();  // подключаемся к серверу
            try
            {
                client.Connect(server, port);

                stream = client.GetStream();

                string sLogin = LoginRegist.Text;
                string sParol = ParolRegist.Password;
                string sServer_response;

                byte[] outLoginR = System.Text.Encoding.UTF8.GetBytes(sLogin);
                byte[] outParolR = System.Text.Encoding.UTF8.GetBytes(sParol);
                byte[] OtvetLR = new byte[256];

                stream.Write(Encoding.UTF8.GetBytes("reg"), 0, Encoding.UTF8.GetBytes("reg").Length);
                sRead_stream(stream);



                if (!String.IsNullOrEmpty(sLogin))   // проверяем заполнение логина
                {
                    Podskazka1.Visibility = Visibility.Collapsed;
                    stream.Write(outLoginR, 0, outLoginR.Length);        // отправляем логин на сервер
                    sServer_response = sRead_stream(stream); // получаем ответ о возможности такого логина


                    if (sServer_response != "логин доступен")
                    {
                        Podskazka1.Text = sServer_response;
                        Podskazka1.Foreground = Brushes.Red;
                        Podskazka1.Visibility = Visibility.Visible;
                        throw new Exception(sServer_response);
                    }

                }
                else
                {
                    Podskazka1.Text = "Введите логин";
                    Podskazka1.Foreground = Brushes.Red;
                    Podskazka1.Visibility = Visibility.Visible;
                    throw new Exception("нет логина");
                }
                if (!String.IsNullOrEmpty(sParol))   // проверяем заполнение пароля
                {
                    Podskazka2.Visibility = Visibility.Collapsed;
                    stream.Write(outParolR, 0, outParolR.Length);    // отправляем пароль на сервер
                    byte[] OtvetPR = new byte[256];
                    sServer_response = sRead_stream(stream); // получаем ответ о возможности такого пароля
                    if (sServer_response != "пароль принят") // если пароль проходит открываем окно чата
                    {
                        Podskazka2.Text = sServer_response;
                        Podskazka2.Foreground = Brushes.Red;
                        Podskazka2.Visibility = Visibility.Visible;
                        throw new Exception(sServer_response);
                    }
                }
                else
                {
                    Podskazka2.Text = "введите пароль";
                    Podskazka2.Foreground = Brushes.Red;
                    Podskazka2.Visibility = Visibility.Visible;
                    throw new Exception("нет пароля");
                }






                Start_io_stream(stream, sLogin);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
                if (stream != null)
                {
                    stream.Close();
                }
            }
            #region

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

            #endregion
        }

        private void Start_io_stream(NetworkStream stream, string sLogin)
        {
            user_name = sLogin;
            if (!Directory.Exists(Data_wr.message_history_name))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + Data_wr.message_history_name);
            }


            MainWindow taskWindow = new MainWindow();

            cw_stream np = new cw_stream(stream, taskWindow);
            Thread potok_vivoda = new Thread(new ThreadStart(np.Vivod));

            byte[] data = Encoding.UTF8.GetBytes("GetMailbox " + user_name+"\n");
            FonWindow.stream.Write(data, 0, data.Length);
            potok_vivoda.IsBackground = true;
            potok_vivoda.Name = "Input_Thread";
            potok_vivoda.Start();

            this.Content = taskWindow.Content;
        }



        private void ButRegist_Click(object sender, RoutedEventArgs e)
        {

            loginAvto.Visibility = Visibility.Collapsed;
            ParolAvto.Visibility = Visibility.Collapsed;
            logo.Visibility = Visibility.Collapsed;
            butLogin.Visibility = Visibility.Collapsed;
            LoginRegist.Visibility = Visibility.Visible;
            ParolRegist.Visibility = Visibility.Visible;
            butAfterRegist.Visibility = Visibility.Visible;
            ButRegist.Visibility = Visibility.Collapsed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        static public string sRead_stream(NetworkStream stream)
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

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
