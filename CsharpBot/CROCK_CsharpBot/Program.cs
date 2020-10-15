using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CROCK_CsharpBot
{
    class Program
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try 
            {
                Bot bot;
                bot = new Bot();
                bot.Run();
                log.Info("Запуск бота в консольном режиме.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка");
                // Отображкние сообщения, включая вложенные
                do
                {
                    log.Fatal(ex.Message);
                    ex = ex.InnerException;
                }
                while (ex != null);
            }
            finally
            {
                Console.WriteLine("Нажмите Enter для завершения.");
                Console.ReadLine();
            }
        }
    }
}
