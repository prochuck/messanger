using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Messenger_Server_Part
{

    partial class Program
    {
        static object user_data_locker = new object();
        static public class DataWR
        {
            [Serializable]
            struct User_data
            {
                public string name;
                public string password;
            }
            const int max_name_lenght = 20;
            const int min_name_lenght = 4;
            const int pass_lenght = 20;
            const int min_pas_len = 3;
            const string reg_name_pattern = @"\b(^[a-z0-9]*$)";
            const RegexOptions options = RegexOptions.IgnoreCase;
            static public bool is_registred(string name)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream file = null;
                lock (user_data_locker)
                {
                    file = File.Open(user_data_patch, FileMode.OpenOrCreate);
                }
                if (name.Length > max_name_lenght || name.Length < min_name_lenght)
                {
                    file.Close();
                    return false;
                }
                while (file.Position < file.Length)
                {
                    User_data tmp = (User_data)formatter.Deserialize(file);
                    if (name == tmp.name)
                    {
                        file.Close();
                        return true;
                    }
                }
                file.Close();
                return false;
            }
            static public bool can_register(string name)
            {
                bool a = Regex.IsMatch(name, reg_name_pattern, options);
                if (Regex.IsMatch(name, reg_name_pattern, options) && !is_registred(name) && name.Length >= min_name_lenght && name.Length <= max_name_lenght) return true;
                return false;
            }
            static public string get_password_by_name(string name)
            {
                BinaryFormatter formatter=new BinaryFormatter();
                FileStream file = null;
                lock (user_data_locker)
                {
                    file = File.Open(user_data_patch, FileMode.OpenOrCreate);
                }
                while (file.Position<file.Length)
                {
                    User_data data = (User_data)formatter.Deserialize(file);
                    if (name != data.name) continue;
                    else
                    {
                        file.Close();
                        return data.password;
                    }
                }

                return null;
            }
            static public bool register_user(string name,string password)
            {
                
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream file = null;
                lock (user_data_locker)
                {
                    file = File.Open(user_data_patch, FileMode.Append);
                }
                if (!Regex.IsMatch(name, reg_name_pattern, options) || !Regex.IsMatch(password, reg_name_pattern, options) || name.Length > max_name_lenght || name.Length < min_name_lenght || password.Length > pass_lenght || pass_lenght < min_pas_len)
                {
                    file.Close();
                    return false;
                }
                while(file.Position < file.Length)
                {
                    User_data tmp = (User_data)formatter.Deserialize(file);
                    if (name == tmp.name)
                    {
                        file.Close();
                        return false;
                    }
                }
                User_data data;
                data.password = password;
                data.name = name;
                formatter.Serialize(file, data);
                file.Close();
                return true;
            }

            static public bool save_message_to_mailbox(Message mess)
            {

                //string pattern = "(^" + mess.sender + "@" + mess.reciever + "$)|(^" + mess.reciever + "@" + mess.sender + "$)";
                string conv_file_path;


                conv_file_path = Directory.GetCurrentDirectory() + @"\" + mailbox_name + @"\" + mess.reciever + ".txt";
                bool is_readed = false;
                string a = Regex.Unescape(JsonSerializer.Serialize<Message>(mess)) + "\n";
                do
                {
                    try
                    {
                        File.AppendAllText(conv_file_path, a);
                        is_readed = true;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(200);
                    }
                } while (!is_readed);
                return true;
            }

            static public bool save_message(Message mess)
            {
               
                //string pattern = "(^" + mess.sender + "@" + mess.reciever + "$)|(^" + mess.reciever + "@" + mess.sender + "$)";
                string conv_file_path;
                if (string.Compare(mess.sender, mess.reciever) == -1)
                {

                    conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name + @"\" + mess.sender + "@" + mess.reciever + ".txt";
                }
                else
                {
                    conv_file_path = Directory.GetCurrentDirectory()+ @"\" +message_history_name + @"\" + mess.reciever + "@" +mess.sender  + ".txt";
                }
                bool is_readed = false;
                string a = Regex.Unescape(JsonSerializer.Serialize<Message>(mess)) + "\n";
                do
                {
                    try
                    {
                        File.AppendAllText(conv_file_path, a);
                        is_readed=true;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(200);
                    }
                } while (!is_readed);
                return true;
            }



            //штука которая имеет всю историю сообщений и отдаёт по 1 по запросу
            public class Message_worker
            {
                string conv_file_path;
                StreamReader sr;
                bool is_mailbox;
                public Message_worker() { }
                /// <summary>
                /// Перегружен для работы с mailbox. Читает все сообщения, отправленные пользователю, пока тот был не в сети.
                /// </summary>
                /// <param name="name1"></param>
                /// <param name="fs"></param>
                public Message_worker(string name1, out FileStream fs)     
                {
                    is_mailbox = true;
                    fs = null;
                    conv_file_path = Directory.GetCurrentDirectory() + @"\" + mailbox_name + @"\" + name1 + ".txt";
                    bool is_readed = false;
                    do
                    {
                        try
                        {
                            fs = new FileStream(conv_file_path, FileMode.OpenOrCreate);
                            sr = new StreamReader(fs);
                            is_readed = true;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(200);
                        }
                    } while (!is_readed);
                }
                public Message_worker(string name1, string name2,out FileStream fs)
                {
                    is_mailbox = false;
                    fs = null;
                    if (string.Compare(name1, name2) == -1)
                    {
                        conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name + @"\" + name1 + "@" + name2 + ".txt";
                    }
                    else
                    {
                        conv_file_path = Directory.GetCurrentDirectory() + @"\" + message_history_name + @"\" + name2 + "@" + name1 + ".txt";
                    }
                    bool is_readed = false;
                    do
                    {
                        try
                        {
                            fs = new FileStream(conv_file_path,FileMode.OpenOrCreate);
                            sr = new StreamReader(fs);
                            is_readed = true;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(200);
                        }
                    } while (!is_readed);
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
                        sr.Close();
                        sr.BaseStream.Close();
                        if (is_mailbox)
                        {
                            File.Delete(conv_file_path);
                        }
                        return message;
                    }
                    string text = sr.ReadLine();
                    return JsonSerializer.Deserialize<Message>(text);
                }
            }
        }

    }
}
