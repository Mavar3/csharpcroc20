using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CROCK_CsharpBot
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Запуск бота в консольном режиме.\nНажмите Enter для завершения работы.");
            Bot bot;
            bot = new Bot();
            bot.Run();
            Console.ReadLine();
        }
    }
}
