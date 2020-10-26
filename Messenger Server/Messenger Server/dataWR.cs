using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;

namespace Messenger_Server_Part
{

    partial class Program
    {
        static public class dataWR
        {
            [Serializable]
            struct user_data
            {

                public string name;
                public string password;
            }
            const int max_name_lenght = 20;
            const int min_name_lenght = 4;
            const int pass_lenght = 20;
            const int min_pas_len = 3;
            const string reg_pattern = @"\b(^[a-z 0-9]*$)";
            const RegexOptions options = RegexOptions.IgnoreCase;
            static public bool is_registred(string name)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream file = null;
                while (file == null)
                {
                    try
                    {
                        file = File.Open(user_data_patch, FileMode.OpenOrCreate);
                    }
                    catch (System.IO.IOException)
                    {
                        Thread.Sleep(10);
                    }
                }
                if (name.Length > max_name_lenght|| name.Length<min_name_lenght)
                {
                    file.Close();
                    return false;
                }
                while (file.Position < file.Length)
                {
                    user_data tmp = (user_data)formatter.Deserialize(file);
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
                bool a = Regex.IsMatch(name, reg_pattern, options);
                if (Regex.IsMatch(name, reg_pattern, options) && !is_registred(name) && name.Length >= min_name_lenght && name.Length <= max_name_lenght) return true;
                return false;
            }
            static public string get_password_by_name(string name)
            {
                BinaryFormatter formatter=new BinaryFormatter();
                FileStream file = null;
                while (file == null)
                {
                    try
                    {
                        file = File.Open(user_data_patch, FileMode.OpenOrCreate);
                    }
                    catch (System.IO.IOException)
                    {
                        Thread.Sleep(10);
                    }
                }
                while (file.Position<file.Length)
                {
                    user_data data = (user_data)formatter.Deserialize(file);
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
                while (file == null)
                {
                    try
                    {
                        file = File.Open(user_data_patch, FileMode.Append);
                    }
                    catch (System.IO.IOException)
                    {
                        Thread.Sleep(10);
                    }
                }
                if (!Regex.IsMatch(name, reg_pattern, options) || !Regex.IsMatch(password, reg_pattern, options) || name.Length > max_name_lenght || name.Length < min_name_lenght || password.Length > pass_lenght || pass_lenght < min_pas_len)
                {
                    file.Close();
                    return false;
                }
                while(file.Position < file.Length)
                {
                    user_data tmp = (user_data)formatter.Deserialize(file);
                    if (name == tmp.name)
                    {
                        file.Close();
                        return false;
                    }
                }
                user_data data;
                data.password = password;
                data.name = name;
                formatter.Serialize(file, data);
                file.Close();
                return true;
            }
        }

    }
}
