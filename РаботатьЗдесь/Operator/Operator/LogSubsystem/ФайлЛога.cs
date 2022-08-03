using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Operator.Utility;
using System.Globalization;
using System.Xml;

namespace Operator.LogSubsystem
{
    
    /// <summary>
    /// Представляет файл лога для всех видов файлов лога
    /// </summary>
    public class ФайлЛога
    {
        /// <summary>
        /// Имя файла без пути
        /// Сейчас не используется, но инициализируется
        /// </summary>
        private string m_filename;
        /// <summary>
        /// Путь и имя файла
        /// </summary>
        private string m_filepath;

        /// <summary>
        /// НомерФайлаЛога
        /// Сейчас не используется, но инициализируется вне класса
        /// </summary>
        private int m_номерФайла;

        /// <summary>
        /// Объект для синхронизации потоков
        /// </summary>
        private Object m_SyncroObject;
        /// <summary>
        /// XML writer for file
        /// </summary>
        private XmlWriter m_writer;
        /// <summary>
        /// Stream for xml writer
        /// </summary>
        private Stream m_stream;

        public ФайлЛога()
        {
            this.m_writer = null;
            this.m_SyncroObject = new object();
        }

        /// <summary>
        /// Имя файла без пути
        /// </summary>
        public string FileName
        {
            get { return m_filename; }
            //set { m_filename = value; }
        }
        /// <summary>
        /// Путь и имя файла
        /// </summary>
        public string FilePathName
        {
            get { return m_filepath; }
            set { m_filepath = value; }
        }

        /// <summary>
        /// НомерФайлаЛога
        /// </summary>
        /// <remarks>
        /// Присваивается, но не используется сейчас нигде.
        /// </remarks>
        public int НомерФайла
        {
            get { return m_номерФайла; }
            set { m_номерФайла = value; }
        }


        /// <summary>
        /// NT-Создать имя файла лога в формате ГГГГММДД-ЧЧММСС-ННН.ррр 
        /// </summary>
        /// <param name="sessionId">Строка идентификатора сеанса Движка</param>
        /// <param name="fileId">номер файла лога от 0 до 999</param>
        /// <param name="fileext">расширение файла лога с точкой</param>
        /// <returns>Возвращает имя файла лога в виде строки вида: 20171101-221741-001.log</returns>
        internal static string СоздатьИмяФайла(string sessionId, int fileId, string fileext)
        {
            StringBuilder sb = new StringBuilder(sessionId);
            sb.Append('-');
            sb.Append(fileId.ToString("D3", CultureInfo.InvariantCulture));
            sb.Append(fileext);

            return sb.ToString();
        }

        /// <summary>
        /// NT-Разделить имя файла на части, не выбрасывая исключений.
        /// </summary>
        /// <param name="filename">Имя файла с расширением, но без пути</param>
        /// <param name="sessionId">Ссылка на строку, принимающую  идентификатор сеанса Движка</param>
        /// <param name="fileId">Ссылка на переменную, принимающую номер файла лога</param>
        /// <param name="fileextension">Ссылка на строку, принимающую расширение файла лога</param>
        /// <returns>Возвращает True, если формат правильный и разделение прошло успешно, False в противном случае</returns>
        internal static bool TryParseИмяФайла(string filename, out string sessionId, out int fileId, out string fileextension)
        {
            //функция не используется здесь, но оставлю, может, пригодится где-то. 
            
            //нельзя точно сказать, какой длины будет расширения файла - оно определяется пользователем.
            if ((filename.Length >= 20) && (filename[8] == '-') && (filename[15] == '-') && (filename[19] == '.'))
            {
                //разделяем на части
                sessionId = filename.Substring(0, 15);
                fileextension = filename.Substring(19);//расширение файла с точкой
                return Int32.TryParse(filename.Substring(16, 3), out fileId);
            }
            else
            {
                sessionId = "";
                fileId = 0;
                fileextension = "";
                return false;
            }
        }

        /// <summary>
        /// NT-Открыть ранее созданный объект 
        /// </summary>
        public void Open()
        {
            lock (this.m_SyncroObject)
            {
                if (!isClosed()) return;
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.CloseOutput = true;
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                settings.NewLineHandling = NewLineHandling.Entitize;
                settings.OmitXmlDeclaration = true;
                settings.NewLineOnAttributes = false;

                //open underline file stream
                Stream fs = new FileStream(this.m_filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 16*1024, FileOptions.WriteThrough);
                fs.Seek(0, SeekOrigin.End);//в конец файла
                this.m_stream = fs;
                //create xml writer
                this.m_writer = XmlWriter.Create(fs, settings);
                //вписать открывающий элемент сеанса записи в лог
                this.m_writer.WriteStartElement("s");
                this.m_writer.Flush();

                return;
            }
        }

        /// <summary>
        /// NT-Закрыть файл по таймауту или совсем
        /// </summary>
        internal void Close()
        {
            //используется
            //файл может уже быть закрыт, надо проверять это.
            lock (this.m_SyncroObject)
            {
                if (!isClosed())
                {
                    //вписать закрывающий элемент сеанса записи в лог
                    this.m_writer.WriteEndElement();
                    this.m_writer.Flush();
                    //close file
                    this.m_writer.Close();
                    this.m_writer = null;
                    //поток уже должен быть закрыт
                    this.m_stream = null;
                }
                return;
            }
        }
        /// <summary>
        /// NT-Создать файл лога, если не существует, но не открывать его. Потом пользователь сам откроет его как ему надо
        /// </summary>
        internal static ФайлЛога Create(string folderPath, string filename)
        {
            ФайлЛога result = new ФайлЛога();
            result.m_filepath = Path.Combine(folderPath, filename);
            result.m_filename = filename;
            //если файл уже существует, не создавать его
            if(!File.Exists(result.m_filepath))
                Utilities.СоздатьФайлБезИндексирования(result.m_filepath);//Создать новый файл или перезаписать существующий

            return result;
        }
        /// <summary>
        /// NT-проверить что файл закрыт
        /// </summary>
        /// <returns></returns>
        public bool isClosed()
        {
            return ((this.m_stream == null) && (this.m_writer == null));
        }

        /// <summary>
        /// NT-Функция возвращает Int64 позицию записи в файл лога.
        /// </summary>
        /// <returns></returns>
        internal long ПолучитьРазмерФайла()
        {
            // Используется
            lock (this.m_SyncroObject)
            {
                Int64 result = 0;
                if (isClosed())
                {
                    FileInfo fi = new FileInfo(this.m_filepath);
                    result = fi.Length;
                }
                else
                {
                    result = m_stream.Position;//Length
                }
                return result;
            }
        }

        /// <summary>
        /// NT-Добавить сообщение лога
        /// </summary>
        /// <param name="z"></param>
        internal void ДобавитьСообщениеЛога(LogMessage msg)
        {
            //вписать запись в файл лога
            //используется
            //Файл лога надо заново открыть, если он был закрыт по таймауту
            lock (this.m_SyncroObject)
            {
                //check file closed
                if (isClosed())
                    this.Open();
                //write file
                this.m_writer.WriteStartElement("row");
                this.m_writer.WriteAttributeString("time", msg.MsgTime.ToString("dd.MM.yyyy-HH:mm:ss.fff", CultureInfo.CurrentCulture));
                this.m_writer.WriteAttributeString("code", msg.MsgCode.ToString());
                this.m_writer.WriteAttributeString("agent", msg.Agent);
                this.m_writer.WriteString(msg.MsgText);//пишем длинную строку как тело тега
                this.m_writer.WriteEndElement();
                this.m_writer.Flush();

                return;
            }
        }



    }
}
