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
            TcpClient client = new TcpClient();
            client.Connect(server, port);
            StringBuilder response = new StringBuilder();
            NetworkStream stream = client.GetStream();
            
            string LoginR = LoginRegist.Text;
            string ParolR = GetPassword();

            MainWindow taskWindow = new MainWindow();
            this.Content = taskWindow.Content;

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
