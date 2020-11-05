using Newtonsoft.Json;
using System;
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
                            jMail = sRead_stream(stream);
                        }
                        else if (tmp.EndsWith("\n"))
                        {
                            jMail = tmp;
                        }
                        else
                        {
                            jMail = tmp + sRead_stream(stream);
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

                        window.Dispatcher.Invoke(() =>
                        {
                            ListBox _messages_list;
                            _messages_list = (ListBox)window.FindName("messages_list");
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = mail.sender + ": " + mail.content;
                            _messages_list.Items.Add(textBlock);
                        });
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
