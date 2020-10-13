using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
                public byte[] key;
            }
            const int max_name_lenght = 20;
            const int key_lenght = 256;
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
                if (name.Length > max_name_lenght)
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
            static public byte[] get_key_by_name(string name)
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
                        return data.key;
                    }
                }

                return null;
            }
            static public bool register_user(string name, byte[] key)
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
                if (name.Length > max_name_lenght || key.Length != key_lenght)
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
                data.key = key;
                data.name = name;
                formatter.Serialize(file, data);
                file.Close();
                return true;
            }
        }

    }
}
