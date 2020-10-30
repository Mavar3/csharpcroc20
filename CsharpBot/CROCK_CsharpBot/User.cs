using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CROCK_CsharpBot
{
    public class User
    {
        public UserState userState;
        /// <summary>
        /// ID пользователя
        /// </summary>
        
        [XmlAttribute()]
        public long ID;
        /// <summary>
        /// Имя пользователя
        /// </summary>
        
        [XmlElement(ElementName = "Name")]
        public string FirstName;
        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string LastName;
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName;
        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber;

        [XmlText]
        public string Discription;
    }
}
