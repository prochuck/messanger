using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace messanger_ui
{
    class Data_wr
    {
        public static string message_history_name { get;private set; } = "history "+FonWindow.user_name;
        static public bool save_message(Message mess)
        {
            string conv_file_path;
            if (mess.sender == FonWindow.user_name)
            {
                conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name + @"\" + mess.reciever + ".txt";
            }
            else
            {
                conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name + @"\" + mess.sender + ".txt";
            }
            string a = Regex.Unescape(JsonConvert.SerializeObject(mess)) + "\n";
            File.AppendAllText(conv_file_path, a);
            return true;
        }

      static  public string[] Get_All_Contacts()
        {
            string conv_file_path;
            conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name;
            string[] res = Directory.GetFiles(conv_file_path);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = Regex.Match(res[i], @"([^\\ ]+).txt").Groups[1].Value;
            }
            
            return res;
        }

        public class Message_worker
        {
            StreamReader sr;
            string conv_file_path;
            public Message_worker() { }

            public Message_worker(string name1, out FileStream fs)
            {

                fs = null;
                conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name + @"\" + name1 + ".txt";
                fs = new FileStream(conv_file_path, FileMode.OpenOrCreate);
                sr = new StreamReader(fs);
            }
            ~Message_worker()
            {
                sr.Close();
            }
            public Message Next()
            {
                if (sr.EndOfStream)
                {
                    Message message = new Message();
                    message.sender = null;
                    sr.BaseStream.Close();
                    sr.Close();
                    return message;
                }
                string text = sr.ReadLine();
                return JsonConvert.DeserializeObject<Message>(text);
            }
        }

    }
}
