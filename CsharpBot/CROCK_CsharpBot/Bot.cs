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
using System.Dynamic;

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

        private async void MessagProcessor(object sender, Telegram.Bot.Args.MessageEventArgs e)
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
                    await client.SendTextMessageAsync(e.Message.Chat.Id, "Hi There\n" +
                             $"Ты прислал мне: {e.Message.Type}");
                    log.Trace(e.Message.Type);
                    return;
                }
                Task invokeTask = (Task)info.Invoke(this, new object[] { e });
                await invokeTask;

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
                //Выходит раньше сохранения и отправки. В Invoke нельзя использовать await
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
            log.Info("\n-------------------------------------\n" + 
                "Начало сохранения и отправки сохранённого фото.\n");
            string dir = Properties.Settings.Default.SavedPhotoDir;
            dir = dir.Substring(0, 1).ToUpper() + dir.Substring(1).ToLower() + "//";
            if (await DownloadPhotoOrDocument(e.Message.Photo.LastOrDefault().FileId, dir))
            {
                await SendPhoto(e.Message.Photo.LastOrDefault().FileId, e.Message.Chat.Id, dir);
            }
            else
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Упс, что-то пошло не так!");
            }
        }

        public async Task DocumentMessage(MessageEventArgs e)
        {
            string dir = Properties.Settings.Default.SavedDocumentDir;
            dir = dir.Substring(0, 1).ToUpper() + dir.Substring(1).ToLower() + "//";
            log.Info("\n-------------------------------------\n" +
                "Начало сохранения и отправки сохранённого документа.\n");
            if (await DownloadPhotoOrDocument(e.Message.Document.FileId, dir))
            {
                await SendDocument(e.Message.Document.FileId, e.Message.Chat.Id, dir);
            }
            else
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Упс, что-то пошло не так!");
            }
        }

        private void MkDir(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    log.Info($"Я попытался создать путь, но он уже существует: {path}");
                    return;
                }
                DirectoryInfo di = Directory.CreateDirectory(path);
                log.Info($"Путь создан: {path}");
            }
            catch(Exception ex)
            {
                log.Error($"Не удаётся создать данный путь: {ex}");
            }
        }

        public async Task<bool> DownloadPhotoOrDocument(string fileId, string dir = "SavedInformation//")
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                MkDir(dir);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageOrDocStream = System.IO.File.Open(dir + filename, FileMode.Create))
                {
                    await client.DownloadFileAsync(file.FilePath, saveImageOrDocStream);
                }
                log.Trace($"File name 4 d: {filename}\n" +
                    $"-----------------------------------");
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error downloading: " + ex.Message);
                return false;
            }
        }

        public async Task SendDocument(string fileId, long chatId, string dir = "SavedInformation//")
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                log.Trace($"File name 4 s: {filename}\n" +
                    $"-------------------------------------");
                using (var sendImageStream = System.IO.File.OpenRead(dir + filename))
                {
                    await client.SendDocumentAsync(chatId, sendImageStream, "That's your document. I saved it on server and than resend!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error downloading: " + ex.Message);
            }
        }

        public async Task SendPhoto(string fileId, long chatId, string dir = "SavedInformation//")
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                log.Trace($"File name 4 s: {filename}\n" +
                    $"-------------------------------------");
                using (var sendImageStream = System.IO.File.OpenRead(dir + filename))
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
                log.Trace($"|{ method }|");

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
            //User user = state[message.Chat.Id];
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