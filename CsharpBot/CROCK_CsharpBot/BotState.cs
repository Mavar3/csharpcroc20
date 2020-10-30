using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CROCK_CsharpBot
{
    // Т.к. основной файл
    // В Namespace - ссылка на ссат или пространство имён
    [XmlRoot(ElementName = "State", Namespace = "")]
    public class BotState
    {
        /// <summary>
        /// Массив пользователей
        /// </summary>
        
        [XmlElement(ElementName = "User")]
        public User[] Users;

        public User this[long id]
        {
            get
            {
                User user = Users.Where(a => a.ID == id).FirstOrDefault();
                return user;
            }
        }

        /// <summary>
        /// Добавление пользователя в массив
        /// </summary>
        /// <param name="user"></param>
        public bool AddUser(User user)
        {
            if (Users == null)
            {
                Users = new User[1] { user };
                return true;
            }
            else if(!Users.Where(a => a.ID == user.ID).Any())
            {
                Array.Resize(ref Users, Users.Length + 1);
                Users[Users.Length - 1] = user;
                return true;
            }
            else
            {
                // Пользователь уже есть в массиве
                return false;
            }
        }

        /// <summary>
        /// Сохранение состояния в виде XML-файла
        /// </summary>
        /// <param name="name">Имя файла</param>
        public void Save(string name)
        {
            // Объект выполняющий сериализацию
            XmlSerializer s = new XmlSerializer(typeof(BotState));
            // Настройка формированя XML-файла
            var setings = new XmlWriterSettings()
            {
                Indent = true //Делаем читабельным
            };
            // Файл для записи данных
            // (i) using - в случае ошибки, всё равно закрывает файл
            using (XmlWriter w = XmlWriter.Create(name))
            {
                // Сериализация
                s.Serialize(w, this);
            }
        }

        public static BotState Load(string name)
        {
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(BotState));
                using (XmlReader r = XmlReader.Create(name))
                {
                    return (BotState)s.Deserialize(r);
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                return new BotState();
            }
        }
    }
}
