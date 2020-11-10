using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace messanger_ui
{
    public partial class MainWindow
    {
        static public object saved_list_locker=new object();
        static int user_id = -1;//пользователь, чатс с которым сейчас открыт
        static List<string>  saved_users_list = new List<string> ();//пользователь, чатс с которым сейчас открыт
        public class cw_stream
        {
            Window window;
            NetworkStream stream;
            public cw_stream(NetworkStream stream)
            {
                this.stream = stream;
            }
            public cw_stream(NetworkStream stream,Window window)
            {
                this.stream = stream;
                this.window = window;
            }
            public void Vivod()
            {
                //Вооообщем, вся эта конструкция нужна на случай, если много сообщение приёдйт одовременно.
                //Теперь любое сообщение кончается на \n.
                //В случае если системаа прочитала сразу 2 сообщения за раз, она сначала обработает 1, а второе сохранит в tmp
                //и прочитает его при следующем проходе. Как-то так. Это лучшее, что я смог придумать.
                string tmp = ""; // переносит символы, не относящиеся к текущему сообщение в следующие сообщение.
                Message mail;
                string jMail = " ";

                
                while (true)
                {
                    try
                    {

                        if (tmp.Length == 0)
                        {
                            jMail = FonWindow.sRead_stream(stream);
                        }
                        else if (tmp.EndsWith("\n"))
                        {
                            jMail = tmp;
                        }
                        else
                        {
                            jMail = tmp + FonWindow.sRead_stream(stream);
                        }
                        if (jMail.Contains("\n"))
                        {
                            tmp = jMail.Substring(jMail.IndexOf("\n") + 1);
                            jMail = jMail.Substring(0, jMail.IndexOf("\n"));
                        }

                        mail = JsonConvert.DeserializeObject<Message>(jMail);

                        if (mail.sender == "@server")
                        {
                            string command_pattern = @"(^[A-z0-9]+ )";
                            string command = Regex.Match(mail.content, command_pattern).Value.Trim();
                            GroupCollection groups;


                            //любые команды от   !!!сервера!!!
                            switch (command)
                            {


                                case "alive":
                                    stream.Write(Encoding.UTF8.GetBytes("alive"+"\n"), 0, Encoding.UTF8.GetBytes("alive"+"\n").Length);
                                    break;
                                case "is_registred":
                                    #region
                                    string is_registred_pattern = @" ([a-z0-9]+) (.+)";
                                    groups = Regex.Match(mail.content, is_registred_pattern).Groups;
                                    if (groups.Count == 3)
                                    {
                                        string ans = groups[1].Value;
                                        string ans_name = groups[2].Value;
                                        if (ans == "yes")
                                        {
                                            lock (saved_list_locker)
                                            {
                                                if (!saved_users_list.Contains(ans_name))
                                                {
                                                    if (saved_users_list.Count == 0)
                                                    {
                                                        user_id = 0;
                                                    }
                                                    saved_users_list.Add(groups[2].Value);
                                                    window.Dispatcher.Invoke(() =>
                                                    {
                                                        ListBox _user_list = (ListBox)window.FindName("user_list");
                                                        TextBlock textBlock = new TextBlock();
                                                        textBlock.Text = ans_name;
                                                        _user_list.Items.Add(textBlock);
                                                    });
                                                    //проверка на онлайн
                                                    Message message = new Message();
                                                    message.sender = FonWindow.user_name;
                                                    message.reciever = "@server";
                                                    message.content = ans_name;
                                                    string jMessage = "is_online " + JsonConvert.SerializeObject(message);
                                                    FonWindow.stream.Write(Encoding.UTF8.GetBytes(jMessage + "\n"), 0, Encoding.UTF8.GetBytes(jMessage + "\n").Length);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            window.Dispatcher.Invoke(() =>
                                            {
                                                TextBlock name_input_block = (TextBlock)window.FindName("name_input_block");
                                                name_input_block.Visibility = Visibility.Visible;
                                                name_input_block.Text = "пользователь не найден";
                                            });
                                        }
                                       
                                    }
                                    #endregion
                                    break;
                                case "is_online":
                                    #region
                                    string is_online_pattern = @" ([a-z0-9]+) (.+)";
                                    groups = Regex.Match(mail.content, is_online_pattern).Groups;
                                    if (groups.Count == 3)
                                    {
                                        string ans = groups[1].Value;
                                        string ans_name = groups[2].Value;
                                        if (saved_users_list.Contains(ans_name))
                                        {
                                            if (ans == "yes")
                                            {
                                                lock (saved_list_locker)
                                                {

                                                    window.Dispatcher.Invoke(() =>
                                                    {
                                                        ListBox _user_list = (ListBox)window.FindName("user_list");
                                                        foreach (TextBlock block in _user_list.Items)
                                                        {
                                                            if (block.Text == ans_name)
                                                            {
                                                                block.Foreground = Brushes.Green;
                                                            }
                                                        }
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                lock (saved_list_locker)
                                                {

                                                    window.Dispatcher.Invoke(() =>
                                                    {
                                                        ListBox _user_list = (ListBox)window.FindName("user_list");
                                                        foreach (TextBlock block in _user_list.Items)
                                                        {
                                                            if (block.Text == ans_name)
                                                            {
                                                                block.Foreground = Brushes.Red;
                                                            }
                                                        }
                                                    });
                                                }
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            continue;
                        }
                    
                        Data_wr.save_message(mail);
                        lock (saved_list_locker)
                        {
                            if (!saved_users_list.Contains(mail.sender))
                            {
                                if (saved_users_list.Count == 0)
                                {
                                    user_id = 0;
                                }
                                saved_users_list.Add(mail.sender);
                                window.Dispatcher.Invoke(() =>
                                {
                                    ListBox _user_list = (ListBox)window.FindName("user_list");
                                    TextBlock textBlock = new TextBlock();
                                    textBlock.Text = mail.sender;
                                    _user_list.Items.Add(textBlock);
                                });
                                //проверка на онлайн
                                Message message = new Message();
                                message.sender = FonWindow.user_name;
                                message.reciever = "@server";
                                message.content = mail.sender;
                                string jMessage = "is_online " + JsonConvert.SerializeObject(message);
                                FonWindow.stream.Write(Encoding.UTF8.GetBytes(jMessage + "\n"), 0, Encoding.UTF8.GetBytes(jMessage + "\n").Length);
                            }
                            if (saved_users_list[user_id] == mail.sender)
                            {
                                window.Dispatcher.Invoke(() =>
                                {
                                    ListBox _messages_list = (ListBox)window.FindName("messages_list");
                                    TextBlock textBlock = new TextBlock();
                                    textBlock.Text = mail.sender + ": " + mail.content;
                                    _messages_list.Items.Add(textBlock);
                                });
                                //проверка на онлайн
                                Message message = new Message();
                                message.sender = FonWindow.user_name;
                                message.reciever = "@server";
                                message.content = mail.sender;
                                string jMessage = "is_online " + JsonConvert.SerializeObject(message);
                                FonWindow.stream.Write(Encoding.UTF8.GetBytes(jMessage + "\n"), 0, Encoding.UTF8.GetBytes(jMessage + "\n").Length);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }


            }
        }
    }
}
