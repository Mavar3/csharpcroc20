using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.ServiceProcess;

namespace CROCK_CsharpBot
{
    class Program
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try 
            {
                string arg1 = args.Count() > 0 ? args[0] : string.Empty;
                arg1 = arg1.ToLower();

                string name = Assembly.GetExecutingAssembly().Location;

                switch(arg1)
                {
                    case "console":
                        Bot bot;
                        bot = new Bot();
                        bot.Start();
                        log.Info("Запуск бота в консольном режиме.");
                        break;
                    case "install":
                        ManagedInstallerClass.InstallHelper(new string[] { name });
                        break;
                    case "uninstall":
                        ManagedInstallerClass.InstallHelper(new string[] { $"/u " + name });
                        break;
                    case "":
                        var svc = new BotService();
                        ServiceBase.Run(svc);
                        break;
                    default:
                        Console.WriteLine($"Неверный параметр: {arg1}");
                        break;
                }

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
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Нажмите Enter для завершения.");
                    Console.ReadLine();
                }
            }
        }
    }
}
