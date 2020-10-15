using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.IO;

namespace CROCK_CsharpBot
{
    public class Sender
    {
        private NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private TelegramBotClient client;
        public Sender(TelegramBotClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Отправка сохранённого документа на клиент
        /// </summary>
        /// <param name="fileId">Id фотографии</param>
        /// <param name="chatId">Id чата</param>
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

        /// <summary>
        /// Отправка сохранённой фотографии на клиент
        /// </summary>
        /// <param name="fileId">Id фотографии</param>
        /// <param name="chatId">Id чата</param>
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
    }
}
