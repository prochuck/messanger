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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace messanger_ui
{
    [Serializable]
    public struct Message
    {
        public string sender { get; set; }
        public string reciever { get; set; }
        public string content { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }
        

        

        private void input_box_TextInput(object sender, TextCompositionEventArgs e)
        { 
            
            string input = input_box.Text;
            input_box.Text = "";

            string send_pattern = @"^[А-яA-z0-9 ]+$";

            GroupCollection groups = Regex.Match(input, send_pattern).Groups;
            if (groups.Count != 0)
            {
                Message a = new Message();
                a.reciever = saved_users_list[user_id];
                a.content = groups[0].Value;
                a.sender = FonWindow.user_name;
                Data_wr.save_message(a);
                messages_list.Items.Add(new TextBlock().Text = a.sender + ": " + a.content);
                byte[] data;
                string ms = JsonConvert.SerializeObject(a);
                data = Encoding.UTF8.GetBytes("send " + ms);
                FonWindow.stream.Write(data, 0, data.Length);
            }
            #region
            /*
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
                        a.sender =FonWindow.name;
                        string ms = JsonConvert.SerializeObject(a);
                        data = Encoding.UTF8.GetBytes("send " + ms);
                        FonWindow.stream.Write(data, 0, data.Length);
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
                        FonWindow.stream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        Console.WriteLine("неправильный формат команды");
                    }
                    #endregion
                    break;
                case "GetMailbox":
                    data = Encoding.UTF8.GetBytes("GetMailbox " + FonWindow.name);
                    FonWindow.stream.Write(data, 0, data.Length);
                    break;
                default:
                    Console.WriteLine("Команда не найдена");
                    break;
            }*/
            #endregion
        }


        
        private void user_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            messages_list.Items.Clear();
            TextBlock item = (TextBlock)user_list.SelectedItem;
            string name = item.Text;
            for (int i = 0; i < saved_users_list.Count; i++)
            {
                if (saved_users_list[i]== name)
                {
                    user_id = i;
                    break;
                }
            }

            FileStream fs;
            Data_wr.Message_worker worker = new Data_wr.Message_worker(name, out fs);
            Message ms = worker.Next();
            while (ms.sender!=null)
            {
                TextBlock text_mess = new TextBlock();
                text_mess.Text = ms.sender+": "+ms.content;
                messages_list.Items.Add(text_mess);
                ms = worker.Next();
            }
            messages_list.SelectedItem = null;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
        }

        private void w1_instantiated(object sender, EventArgs e)
        {
            string[] contacts = Data_wr.Get_All_Contacts();
            for (int i = 0; i < contacts.Length; i++)
            {
                saved_users_list.Add(contacts[i]);
                TextBlock textBlock = new TextBlock();
                textBlock.Text = contacts[i];
                user_list.Items.Add(textBlock);
            }
        }
    }
}
