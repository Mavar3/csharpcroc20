using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace CROCK_CsharpBot
{
    public class Bot
    {
        private TelegramBotClient client;
        public Bot()
        {
            client = new TelegramBotClient("1322961991:AAEqbpx7E4TlC7Wont1yjOhdpveip0PCvO0");
            client.OnMessage += MessagProcessor;
        }

        private void MessagProcessor(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There");
            Console.WriteLine(e.Message.Text);
            // throw new NotImplementedException();
        }

        public void Run()
        {
            client.StartReceiving();
        }
    }
}
