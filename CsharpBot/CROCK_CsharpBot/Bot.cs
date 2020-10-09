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
        public Bot()
        {
            client = new TelegramBotClient("1322961991:AAEqbpx7E4TlC7Wont1yjOhdpveip0PCvO0");
            client.OnMessage += MessagProcessor;
        }

        private void MessagProcessor(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            switch (e.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    Console.WriteLine("\nНачало сохранения и отправки сохранённого.\n");
                    DownloadPhoto(e.Message.Photo.LastOrDefault().FileId);
                    Thread.Sleep(1000);
                    SendPhoto(e.Message.Photo.LastOrDefault().FileId, e.Message.Chat.Id);
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Location:
                    client.SendTextMessageAsync(e.Message.Chat.Id, $"Пока я не умею работать с таким типом данных, но я учусь.");
                    Console.WriteLine("Пользователь запросил геолокацию");
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Contact:
                    string phone = e.Message.Contact.PhoneNumber;
                    if (e.Message.Chat.Id == e.Message.Contact.UserId)
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, $"Твой телефон: {phone}");
                        Console.WriteLine($"/n Телефон пользователя: {e.Message.Chat.FirstName} ({e.Message.Contact.UserId}): {e.Message.Contact.PhoneNumber} /n");
                    }
                    else
                    {
                        client.SendTextMessageAsync(e.Message.Chat.Id, $"Это не твой телефон(!): {phone}");
                        Console.WriteLine($"/nПользователя: {e.Message.Contact.UserId} {e.Message.Chat.FirstName} - обманщик/n");
                    }
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

        /// <summary>
        /// Загрузка фотографий из телеграм
        /// </summary>
        /// <param name="fileId">Id файла</param>
        private async void DownloadPhoto(string fileId)
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                Console.WriteLine($"File name 4 d: {filename}\n" +
                    $"-----------------------------------");
                using (var saveImageStream = System.IO.File.Open(filename, FileMode.Create))
                {
                    await client.DownloadFileAsync(file.FilePath, saveImageStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }


        /// <summary>
        /// Отправка сохранённой фотографии на клиент
        /// </summary>
        /// <param name="fileId">Id фотографии</param>
        /// <param name="chatId">Id чата</param>
        private async void SendPhoto(string fileId, long chatId)
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                Console.WriteLine($"File name 4 s: {filename}\n");
                using (var sendImageStream = System.IO.File.OpenRead(filename))
                {
                    // await client.SendMediaGroupAsync(chatId, sendImageStream, false);
                    await client.SendPhotoAsync(chatId, sendImageStream, "That's your photo. I saved it on server and than resend!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }


        private void CommadProcessor(Telegram.Bot.Types.Message message)
        {
            string comand = message.Text.Substring(1).ToLower();
            switch (comand)
            {
                case "start":
                    var button = new KeyboardButton("Поделиcь телефоном");
                    button.RequestContact = true;
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
