using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

namespace messanger_ui
{
    /// <summary>
    /// Логика взаимодействия для FonWindow.xaml
    /// </summary>

    public partial class FonWindow : Window
    {

        private const int port = 7001;
        private const string server = "127.0.0.1";


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
            string Login = loginAvto.Text;

            MainWindow taskWindow = new MainWindow();
            this.Content = taskWindow.Content;
            ButRegist.Visibility = Visibility.Collapsed;

        }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            TcpClient client = new TcpClient();  // подключаемся к серверу
            client.Connect(server, port);
            
            NetworkStream stream = client.GetStream();
            
            string LoginR = LoginRegist.Text;
            string ParolR = GetPassword();

            String rLoginR = LoginR;
            String rParolR = ParolR;

            byte[] outLoginR = System.Text.Encoding.UTF8.GetBytes(rLoginR);
            byte[] outParolR = System.Text.Encoding.UTF8.GetBytes(rParolR);
            stream.Write(outLoginR, 0, outLoginR.Length);        // отправляем логин на сервер
            byte[] OtvetLR= new byte[256];
            int bytes = stream.Read(OtvetLR, 0, OtvetLR.Length); // получаем ответ о возможности такого логина
            string mOtvetLR = Encoding.UTF8.GetString(OtvetLR, 0, bytes);
            if (!String.IsNullOrEmpty(rLoginR))   // проверяем заполнение логина
            {
                Podskazka1.Visibility = Visibility.Collapsed;

                if (!String.IsNullOrEmpty(rParolR))   // проверяем заполнение пароля
                {
                    Podskazka2.Visibility = Visibility.Collapsed;

                    if (mOtvetLR == "1")
                    {
                        stream.Write(outParolR, 0, outLoginR.Length);    // отправляем пароль на сервер
                        byte[] OtvetPR = new byte[256];
                        bytes = stream.Read(OtvetLR, 0, OtvetLR.Length); // получаем ответ о возможности такого логина
                        string mOtvetPR = Encoding.UTF8.GetString(OtvetLR, 0, bytes);
                        if (mOtvetPR == "1") // если пароль проходит открываем окно чата
                        {
                            MainWindow task1Window = new MainWindow();
                            this.Content = task1Window.Content;
                        }
                        else if (mOtvetPR == "2") // если нет - выволдим сообщение об ошибки
                        {
                            TextBlock Podskazka2 = new TextBlock();
                            Podskazka2.Text = "Пароль уже занят, попробуйте другой";
                            Podskazka2.Foreground = Brushes.Red;
                            Podskazka2.Visibility = Visibility.Visible;

                        }                     
                    }
                    else 
                    {
                        TextBlock Podskazka2 = new TextBlock();
                        Podskazka2.Text = "Введите пароль";
                        Podskazka2.Foreground = Brushes.Red;
                        Podskazka2.Visibility = Visibility.Visible;
                    }
                }
                else if (mOtvetLR == "2")
                {
                    TextBlock Podskazka1 = new TextBlock();
                    Podskazka1.Text = "Логин уже занят, попробуйте другой";
                    Podskazka1.Foreground = Brushes.Red;
                    Podskazka1.Visibility = Visibility.Visible;
                }
                
                
            }
            else
            {
                TextBlock Podskazka1 = new TextBlock();
                Podskazka1.Text = "Введите логин";
                Podskazka1.Foreground = Brushes.Red;
                Podskazka1.Visibility = Visibility.Visible;
            }

            Console.WriteLine(GetPassword());

        }
        public string GetPassword()
        {
            return ParolRegist.Password;

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
    }
}
