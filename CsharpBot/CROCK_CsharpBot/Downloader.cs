using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.IO;


namespace CROCK_CsharpBot
{
    public class Downloader
    {
        private TelegramBotClient client;
        public Downloader(TelegramBotClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Загрузка фотографий либо документа из телеграм
        /// </summary>
        /// <param name="fileId">Id файла</param>
        public async Task DownloadPhotoOrDocument(string fileId)
        {
            try
            {
                var file = await client.GetFileAsync(fileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                Console.WriteLine($"File name 4 d: {filename}\n" +
                    $"-----------------------------------");
                using (var saveImageOrDocStream = System.IO.File.Open(filename, FileMode.Create))
                {
                    await client.DownloadFileAsync(file.FilePath, saveImageOrDocStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }
        }

    }
}
