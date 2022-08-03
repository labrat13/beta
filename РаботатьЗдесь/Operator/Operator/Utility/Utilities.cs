using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Operator.Utility
{
    /// <summary>
    /// Содержит утилитарные функции
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// RT-Создать каталог по существующему пути и без флага индексирования 
        /// </summary>
        /// <param name="folder">Путь нового каталога</param>
        internal static void СоздатьКаталогБезИндексирования(string folder)
        {
            DirectoryInfo di = Directory.CreateDirectory(folder);
            di.Attributes = FileAttributes.Directory | FileAttributes.NotContentIndexed;
            return;
        }
        /// <summary>
        /// RT-Создать новый файл или перезаписать существующий
        /// </summary>
        /// <param name="p">Путь файла</param>
        internal static void СоздатьФайлБезИндексирования(string p)
        {
            FileStream fs = File.Create(p, 16*1024, FileOptions.WriteThrough);
            fs.Close();
            FileInfo fi = new FileInfo(p);
            fi.Attributes = FileAttributes.Normal | FileAttributes.NotContentIndexed;

            return;
        }
        /// <summary>
        /// RT-Сделать абсолютный путь из относительного, если путь не абсолютный.
        /// </summary>
        /// <param name="refPath">Обрабатываемый путь, абсолютный или относительный</param>
        /// <param name="basePath">Базовый абсолютный путь</param>
        /// <returns>Возвращает абсолютный путь</returns>
        internal static string ПолучитьАбсолютныйПуть(string refPath, string basePath)
        {
            if (IsAbsolutePath(refPath))
                return refPath;//если уже абсолютный, просто вернуть его.
            //иначе
            //1 проверить что базовый путь абсолютный
            if (!IsAbsolutePath(basePath))
                throw new Exception(String.Format(CultureInfo.CurrentCulture, "Путь {0} не является абсолютным", basePath));
            //2 собрать абсолютный путь
            //если первый символ \, то удалить его, иначе пути не склеятся этой функцией
            if(refPath[0] == '\\')
                refPath = refPath.Substring(1);
            String result = Path.Combine(basePath, refPath);

            return result;
        }
        /// <summary>
        /// RT-Убедиться что файловый путь является абсолютным
        /// </summary>
        /// <param name="p">Проверяемый файловый путь, не сетевой.</param>
        /// <returns></returns>
        private static bool IsAbsolutePath(string p)
        {
            String vol = Path.GetPathRoot(p);
            //returns "" or "\" for relative path, and "C:\" for absolute path
            if (vol.Length != 3)
                return false;
            else return true;
        }
        /// <summary>
        /// RT-получить размер свободного места на диске.
        /// С учетом дисковой квоты пользователя, итп.
        /// </summary>
        /// <param name="volume">Буква тома</param>
        /// <returns>Размер свободного места на томе</returns>
        internal static long ПолучитьРазмерСвободногоМестаНаТоме(string volume)
        {
            DriveInfo di = new DriveInfo(volume);
            return di.AvailableFreeSpace;
        }

        /// <summary>
        /// NT-Получить размер указанного каталога
        /// </summary>
        /// <param name="dirpath">Путь к каталогу</param>
        /// <returns>Возвращает размер указанного каталога в байтах</returns>
        public static long GetDirectorySize(string dirpath)
        {
            DirectoryInfo d = new DirectoryInfo(dirpath);
            return getDirectorySizeRecursive(d); 
        }
        /// <summary>
        /// NT-Получить размер указанного каталога рекурсивно
        /// </summary>
        /// <param name="d">Объект каталога</param>
        /// <returns>Возвращает размер указанного каталога в байтах</returns>
        private static long getDirectorySizeRecursive(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += getDirectorySizeRecursive(di);
            }
            return (size);
        }


    }
}
