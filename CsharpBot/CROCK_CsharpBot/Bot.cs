using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;

namespace CROCK_CsharpBot
{
    public class Bot
    {
        private TelegramBotClient client;
        private Downloader server_downloader;
        private Sender server_sender;
        private NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public Bot()
        {
            string token = Properties.Settings.Default.Token;
            client = new TelegramBotClient(token);
            var user = client.GetMeAsync();
            Console.WriteLine(user.Result.Username);
            server_downloader = new Downloader(client);
            server_sender = new Sender(client);
            client.OnMessage += MessagProcessor;
        }

        private async void MessagProcessor(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                log.Trace("|<- MessagProcessor");
                switch (e.Message.Type)
                {
                    case Telegram.Bot.Types.Enums.MessageType.Document:
                        log.Info("\nНачало сохранения и отправки сохранённого документа.\n");
                        await server_downloader.DownloadPhotoOrDocument(e.Message.Document.FileId);
                        await server_sender.SendDocument(e.Message.Document.FileId, e.Message.Chat.Id);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Photo:
                        log.Info("\nНачало сохранения и отправки сохранённого фото.\n");
                        await server_downloader.DownloadPhotoOrDocument(e.Message.Photo.LastOrDefault().FileId);
                        await server_sender.SendPhoto(e.Message.Photo.LastOrDefault().FileId, e.Message.Chat.Id);
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Location:
                        await client.SendTextMessageAsync(e.Message.Chat.Id, $"Пока я не умею работать с таким типом данных, но я учусь.");
                        log.Info("Пользователь запросил геолокацию");
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Contact:
                        string phone = e.Message.Contact.PhoneNumber;
                        if (e.Message.Chat.Id == e.Message.Contact.UserId)
                        {
                            await client.SendTextMessageAsync(e.Message.Chat.Id, $"Твой телефон: {phone}");
                            log.Info($"/n Телефон пользователя: {e.Message.Chat.FirstName} ({e.Message.Contact.UserId}): {e.Message.Contact.PhoneNumber} /n");
                        }
                        else
                        {
                            await client.SendTextMessageAsync(e.Message.Chat.Id, $"Это не твой телефон(!): {phone}");
                            log.Info($"/nПользователя: {e.Message.Contact.UserId} {e.Message.Chat.FirstName} - обманщик/n");
                        }
                        break;
                    case Telegram.Bot.Types.Enums.MessageType.Text:
                        if (e.Message.Text.Substring(0, 1) == "/")
                        {
                            CommadProcessor(e.Message);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There\n" +
                                $"Ты сказал мне: {e.Message.Text}");
                            log.Trace(e.Message.Text);
                        }
                        break;
                    default:
                        await client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There\n" +
                            $"Ты прислал мне: {e.Message.Type}");
                        log.Trace(e.Message.Type);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            // throw new NotImplementedException();
            finally
            {
                log.Trace("|-> MessagProcessor");
            }
        }

        private void CommadProcessor(Telegram.Bot.Types.Message message)
        {
            string comand = message.Text.Substring(1).ToLower();
            switch (comand)
            {
                case "start":
                    var button = new KeyboardButton("Поделиcь телефоном")
                    {
                        RequestContact = true
                    };
                    var array = new KeyboardButton[] { button };
                    var reply = new ReplyKeyboardMarkup(array, true, true );
                    client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName}, скажи мне свой телефон: ", replyMarkup: reply);
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