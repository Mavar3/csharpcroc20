using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

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
            switch (e.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Contact:
                    string phone = e.Message.Contact.PhoneNumber;
                    client.SendTextMessageAsync(e.Message.Chat.Id, $"Твой телефон: {phone}");
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    if (e.Message.Text.Substring(0, 1) == "/")
                    {
                        CommadProcessor(e.Message);
                    }
                    else
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There");
                        client.SendTextMessageAsync(e.Message.Chat.Id, $"Ты сказал мне: {e.Message.Text}");
                        Console.WriteLine(e.Message.Text);
                    }
                    break;
                default:
                    client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There");
                    client.SendTextMessageAsync(e.Message.Chat.Id, $"Ты прислал мне: {e.Message.Type}");
                    Console.WriteLine(e.Message.Type);
                    break;

            }

            // throw new NotImplementedException();
        }
        private void CommadProcessor(Telegram.Bot.Types.Message message)
        {
            string comand = message.Text.Substring(1).ToLower();
            switch (comand)
            {
                case "start":
                    var button = new KeyboardButton("Поделимь телефоном");
                    button.RequestContact = true;
                    var array = new KeyboardButton[] { button };
                    var reply = new ReplyKeyboardMarkup(array, true, true );
                    client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName} скажи мне свой телефон: ");
                    break;
                default:
                    client.SendTextMessageAsync(message.Chat.Id, $"Я пока не понимаю команду: {comand}");
                    break;
            }
        }

        public void Run()
        {
            client.StartReceiving();
        }
    }
}
