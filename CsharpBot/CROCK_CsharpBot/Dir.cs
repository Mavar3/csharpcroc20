using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CROCK_CsharpBot
{
    class Dir
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public static void MkDir(string path)
        {
            try
            {
                path = path.Contains(".") ? path.Substring(0, path.LastIndexOf("\\")) : path;
                if (Directory.Exists(path))
                {
                    log.Info($"Сохраняю в: {path}");
                    return;
                }
                DirectoryInfo di = Directory.CreateDirectory(path);
                log.Info($"Путь создан: {path}");
            }
            catch (Exception ex)
            {
                log.Error($"Не удаётся создать данный путь: {ex}");
            }
        }
    }
}
