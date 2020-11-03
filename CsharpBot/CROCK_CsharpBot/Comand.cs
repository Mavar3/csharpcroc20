using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CROCK_CsharpBot
{
    enum Comand
    {
        [Description("Начало работы с ботом")]
        Start,
        [Description("Помощь в работе с ботом")]
        Help,
        [Description("Регистрация для работы с ботом")]
        Registration,
        [Description("Информация о боте")]
        Info
    }
}
