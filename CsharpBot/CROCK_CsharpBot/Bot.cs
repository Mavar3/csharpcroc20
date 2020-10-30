using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.ComponentModel.Design;
using CrocCSharpBot;
using Telegram.Bot.Args;
using System.Reflection;

namespace CROCK_CsharpBot
{
    public class Bot
    {
        private TelegramBotClient client;
        // private Downloader server_downloader;
        // private Sender server_sender;
        private NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private BotState state;
        public Bot()
        {

            string token = Properties.Settings.Default.Token;
            client = new TelegramBotClient(token);
            var user = client.GetMeAsync();
            Console.WriteLine(user.Result.Username);
            // server_downloader = new Downloader(client);
            // server_sender = new Sender(client);
            client.OnMessage += MessagProcessor;
            state = BotState.Load(Properties.Settings.Default.FileName); 
        }

        private void MessagProcessor(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                log.Trace("|<- MessagProcessor");

                // Использование метапрограммирования 
                string type = e.Message.Type.ToString();
                string method = type + "Message";
                System.Reflection.MethodInfo info = GetType().GetMethod(method);
                if (info == null)
                {
                    client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There\n" +
                             $"Ты прислал мне: {e.Message.Type}");
                    log.Trace(e.Message.Type);
                    return;
                }
                info.Invoke(this, new object[] { e });

                // switch (e.Message.Type)
                // {
                //     case Telegram.Bot.Types.Enums.MessageType.Document:
                //         await DocumentMessage(e);
                //         break;
                //     case Telegram.Bot.Types.Enums.MessageType.Photo:
                //         await PhotoMessage(e);
                //         break;
                //     case Telegram.Bot.Types.Enums.MessageType.Location:
                //         await LocationMessage(e);
                //         break;
                //     case Telegram.Bot.Types.Enums.MessageType.Contact:
                //         await ContactMessage(e);
                //         break;
                //     case Telegram.Bot.Types.Enums.MessageType.Text:
                //         await TextMessage(e);
                //         break;
                //     default:
                //         await client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There\n" +
                //             $"Ты прислал мне: {e.Message.Type}");
                //         log.Trace(e.Message.Type);
                //         break;
                // }
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

        public async Task ContactMessage(MessageEventArgs e)
        {
            string phone = e.Message.Contact.PhoneNumber;
            if (e.Message.Chat.Id != e.Message.Contact.UserId)
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, $"Это не твой телефон(!): {phone}");
                log.Info($"/nПользователя: {e.Message.Contact.UserId} {e.Message.Chat.FirstName} - обманщик/n");
                return;
            }
            var keyboardRemove = new ReplyKeyboardRemove();
            await client.SendTextMessageAsync(e.Message.Chat.Id, $"Твой телефон: {phone}", replyMarkup: keyboardRemove); // , replyMarkup: null);
            log.Info($"/n Телефон пользователя: {e.Message.Chat.FirstName} ({e.Message.Contact.UserId}): {e.Message.Contact.PhoneNumber} /n");
            // Регистрация пользователя
            // (i) Использование инициализатора
            var user = new User()
            {
                ID = e.Message.Contact.UserId,
                FirstName = e.Message.Contact.FirstName,
                LastName = e.Message.Contact.LastName,
                UserName = e.Message.Chat.Username,
                PhoneNumber = phone,
                // Discription = "Пользователь бота"
            };
            if (state.AddUser(user))
            {
                state.Save(Properties.Settings.Default.FileName);
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Ты отправил номер");
            }
            else
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Ты уже есть");
            }
        }

        public async Task TextMessage(MessageEventArgs e)
        {
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
        }

        public async Task LocationMessage(MessageEventArgs e)
        {
            await client.SendTextMessageAsync(e.Message.Chat.Id, $"Пока я не умею работать с таким типом данных, но я учусь.");
            log.Info("Пользователь запросил геолокацию");
        }

        public async Task PhotoMessage(MessageEventArgs e)
        {
            log.Info("\nНачало сохранения и отправки сохранённого фото.\n");
            await DownloadPhotoOrDocument(e.Message.Photo.LastOrDefault().FileId);
            await SendPhoto(e.Message.Photo.LastOrDefault().FileId, e.Message.Chat.Id);
        }

        public async Task DocumentMessage(MessageEventArgs e)
        {
            log.Info("\nНачало сохранения и отправки сохранённого документа.\n");
            await DownloadPhotoOrDocument(e.Message.Document.FileId);
            await SendDocument(e.Message.Document.FileId, e.Message.Chat.Id);
        }

        public async Task DownloadPhotoOrDocument(string fileId)
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                log.Trace($"File name 4 d: {filename}\n" +
                    $"-----------------------------------");
                using (var saveImageOrDocStream = System.IO.File.Open(filename, FileMode.Create))
                {
                    await client.DownloadFileAsync(file.FilePath, saveImageOrDocStream);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error downloading: " + ex.Message);
            }
        }

        public async Task SendDocument(string fileId, long chatId)
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                log.Trace($"File name 4 s: {filename}\n");
                using (var sendImageStream = System.IO.File.OpenRead(filename))
                {
                    await client.SendDocumentAsync(chatId, sendImageStream, "That's your document. I saved it on server and than resend!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error downloading: " + ex.Message);
            }
        }

        public async Task SendPhoto(string fileId, long chatId)
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                log.Trace($"File name 4 s: {filename}\n");
                using (var sendImageStream = System.IO.File.OpenRead(filename))
                {
                    // await client.SendMediaGroupAsync(chatId, sendImageStream, false);
                    await client.SendPhotoAsync(chatId, sendImageStream, "That's your photo. I saved it on server and than resend!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error downloading: " + ex.Message);
            }
        }

        private void CommadProcessor(Telegram.Bot.Types.Message message)
        {
            try
            {
                log.Trace("|<- ComandProcessor");
                string command = message.Text.Substring(1).ToLower();

                string method = command.Substring(0, 1).ToUpper() + command.Substring(1) + "Command";
                System.Reflection.MethodInfo info = GetType().GetMethod(method);
                if (info == null)
                {
                    client.SendTextMessageAsync(message.Chat.Id, "Я пока не понимаю команду");
                    return;
                }
                info.Invoke(this, new object[] { message });

                // switch (command)
                // {
                //     case "start":
                //         StartCommand(message);
                //         break;
                //     case "info":
                //         InfoCommand(message);
                //         break;
                //     case "help":
                //         HelpCommand(message);
                //         break;
                //     default:
                //         client.SendTextMessageAsync(message.Chat.Id, $"Я пока не понимаю команду: {command}");
                //         break;
                // }
            }
            finally
            {
                log.Trace("|-> ComandProcessor");
            }
        }

        public void InfoCommand(Telegram.Bot.Types.Message message)
        {
            client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName}, этот бот сделан на основе авторского курса от Крок!");
        }

        public void HelpCommand(Telegram.Bot.Types.Message message)
        {
            string m = "Список команд: \n";
            foreach (Comand s in Enum.GetValues(typeof(Comand)))
            {
                string cmd = s.ToString().ToLower();
                string descr = s.ToDescription();
                m += $"/{cmd} - {descr}\n";
            }
            client.SendTextMessageAsync(message.Chat.Id, m, replyMarkup: null);
        }

        public void StartCommand(Telegram.Bot.Types.Message message)
        {
            client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName}, зарегистрируйся /registration");
        }

        public void RegistrationCommand(Telegram.Bot.Types.Message message)
        {
            User user = state[message.Chat.Id];
            var button = new KeyboardButton("Поделиcь телефоном")
            {
                RequestContact = true
            };
            var array = new KeyboardButton[] { button };
            var reply = new ReplyKeyboardMarkup(array, true, true);
            client.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName}, скажи мне свой телефон: ", replyMarkup: reply);
            //user.userState = UserState.Reg;
        }

        public void Start()
        {
            client.StartReceiving();
        }

        public void Stop()
        {
            client.StopReceiving();
        }
    }
}