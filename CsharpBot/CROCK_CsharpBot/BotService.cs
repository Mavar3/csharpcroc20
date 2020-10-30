using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CROCK_CsharpBot
{
    partial class BotService : ServiceBase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private Bot bot;

        public BotService()
        {
            InitializeComponent();
            bot = new Bot();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                log.Trace("|<- OnStart");
                bot.Start();
                log.Info("Сервис запущен");

            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
            // TODO: Добавьте код для запуска службы.
        }

        protected override void OnStop()
        {
            // TODO: Добавьте код, выполняющий подготовку к остановке службы.
            try
            {
                log.Trace("|<- OnStop");
                bot.Stop();
                log.Info("Сервис остановлен");

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        protected override void OnPause()
        {
            try
            {
                log.Trace("|<- OnStop");
                bot.Stop();
                log.Info("Сервис приостановлен");
            }
            catch (Exception ex)
            {
                log.Info(ex);
            }
            //base.OnPause();
        }

        protected override void OnContinue()
        {
            try
            {
                log.Trace("|<- OnContinue");
                bot.Start();
                log.Info("Сервис возобновлён");
            }
            catch (Exception ex)
            {
                log.Info(ex);
            }
            //base.OnContinue();
        }
    }
}
