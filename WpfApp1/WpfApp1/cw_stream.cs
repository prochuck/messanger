using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace messanger_ui
{
    public partial class MainWindow
    {
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

                        if (mail.sender=="@server")
                        {
                            //любые команды от сервера
                            if (mail.content == "alive")
                            {
                                stream.Write(Encoding.UTF8.GetBytes("alive"), 0, Encoding.UTF8.GetBytes("alive").Length);
                            }
                        }
                        Data_wr.save_message(mail);
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
                        }
                        if (saved_users_list[user_id]==mail.sender)
                        {
                            window.Dispatcher.Invoke(() =>
                            {
                                ListBox _messages_list = (ListBox)window.FindName("messages_list");
                                TextBlock textBlock = new TextBlock();
                                textBlock.Text = mail.sender + ": " + mail.content;
                                _messages_list.Items.Add(textBlock);
                            });
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
