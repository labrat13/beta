using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Operator.Utility;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Operator
{
    /// <summary>
    /// Настройки приложения Оператор
    /// </summary>
    public class OperatorSettings
    {
        /// <summary>
        /// Стандартное название файла настроек Оператора
        /// </summary>
        public const String НазваниеФайлаНастроек = "settings.xml";
        /// <summary>
        /// Дефолтовое значение таймаута бездействия лога, в секундах, до закрытия файлов лога.
        /// </summary>
        public const int LogТаймаутФайлов = 60;
        /// <summary>
        /// Дефолтовое значение максимального количества файлов общего лога в сеансе
        /// </summary>
        public const int LogПределКоличестваФайловЛогаВСеансе = 999;
        /// <summary>
        /// Дефолтовое значение максимального размера файла общего лога в сеансе, в мегабайтах.
        /// </summary>
        public const int LogПределРазмераФайлаЛогаМб = 1;
        /// <summary>
        /// Дефолтовое значение максимального размера всех файлов лога на диске, в мегабайтах.
        /// </summary>
        public const int LogПределРазмераКаталогаЛогаМб = 1024;//1024Мб = 1Гб

        /// <summary>
        /// Дефолтовое значение названия файла лога консоли
        /// </summary>
        public const string LogConsoleFileName = "Console";
        /// <summary>
        /// Дефолтовое значение названия файла лога нераспознанных команд
        /// </summary>
        public const string LogUnknownCmdFileName = "UnknownCmd";
        /// <summary>
        /// Дефолтовое значение названия файла лога всех видов, с точкой в начале
        /// </summary>
        public const string LogРасширениеФайлаЛога = ".xmlog";
        /// <summary>
        /// Дефолтовое значение пути каталога лога
        /// </summary>
        public const string LogПутьКаталогаЛога = "\\Log";

        #region fields
        /// <summary>
        /// Путь к файлу настроек для автоматического сохранения итп
        /// </summary>
        private string m_SettingFilePath;

        /// <summary>
        /// Путь к папке лога относительно каталога с приложением Оператор, или абсолютный путь файла.
        /// </summary>
        /// <remarks>
        /// Абсолютный путь содержит : .
        /// Относительный путь удобен для переноски Оператора - где распаковал, оттуда и работает.
        /// Относительный путь начинается с /
        /// </remarks>
        private string m_logFolderPath;

        /// <summary>
        /// таймаут бездействия лога, в секундах, до закрытия файлов лога.
        /// </summary>
        /// <remarks>
        /// В миллисекундах нельзя - код считает секунды от таймера.
        /// </remarks>
        private int m_logTimeoff;

        /// <summary>
        /// Версия Оператор, в которой был создан лог.
        /// </summary>
        private OperatorVersion m_Version;

        /// <summary>
        /// Версия подсистемы лога, для оценки совместимости.
        /// </summary>
        private OperatorVersion m_LogVersion;
        /// <summary>
        /// Расширение файла лога, с точкой в начале
        /// </summary>
        private string m_РасширениеФайлаЛога;

        /// <summary>
        /// Максимальный размер файла общего лога в сеансе, в мегабайтах
        /// </summary>
        private int m_ПределРазмераФайлаЛогаМб;

        /// <summary>
        /// Максимальное количество файлов общего лога в сеансе.
        /// </summary>
        private int m_ПределКоличестваФайловЛогаВСеансе;
        /// <summary>
        /// Максимальный размер всех файлов лога на диске, в мегабайтах.
        /// </summary>
        private int m_ПределРазмераКаталогаЛогаМб;


        #endregion
        /// <summary>
        /// Конструктор
        /// </summary>
        public OperatorSettings()
        {
            this.SetDefault();
        }

        #region Properties

        /// <summary>
        /// Получить абсолютный путь к каталогу Оператора
        /// </summary>
        [Category("General"), Description("Путь к каталогу Оператора")]
        [XmlIgnore]
        public string OperatorFolderPath
        {
            get { return Path.GetDirectoryName(this.m_SettingFilePath); }//Здесь вернуть абсолютный путь к текущему каталогу
        }

        /// <summary>
        /// Путь к файлу настроек для автоматического сохранения итп
        /// </summary>
        [Category("General"), Description("Путь к файлу настроек Оператора")]
        public string SettingFilePath
        {
            get { return m_SettingFilePath; }
            set { m_SettingFilePath = value; }
        }
        /// <summary>
        /// Версия Оператор, в которой был создан файл настроек.
        /// </summary>
        [Category("General"), Description("Версия Оператора")]
        public OperatorVersion Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        /// <summary>
        /// Путь к папке лога относительно каталога с приложением Оператор либо абсолютный путь к папке лога.
        /// </summary>
        /// <remarks>
        /// Абсолютный путь содержит : 
        /// Относительный путь удобен для переноски Оператора - где распаковал, оттуда и работает.
        /// Относительный путь начинается с \
        /// </remarks>
        [Category("Log"), Description("Путь к каталогу лога Оператора")]
        public string LogFolderPath
        {
            get { return m_logFolderPath; }
            set { m_logFolderPath = value; }
        }

        /// <summary>
        /// таймаут бездействия лога, в секундах, до закрытия файлов лога.
        /// </summary>
        [Category("Log"), Description("Время бездействия, в секундах, до закрытия файлов лога")]
        public int LogTimeoff
        {
            get { return m_logTimeoff; }
            set { m_logTimeoff = value; }
        }
        /// <summary>
        /// Версия подсистемы лога, для оценки совместимости.
        /// </summary>
        [Category("Log"), Description("Версия Лога Оператора")]
        public OperatorVersion LogVersion
        {
            get { return m_LogVersion; }
            set { m_LogVersion = value; }
        }
        /// <summary>
        /// Расширение файла лога, с точкой в начале
        /// </summary>
        [Category("Log"), Description("Расширение файла лога, с точкой в начале")]
        public string РасширениеФайлаЛога
        {
            get { return m_РасширениеФайлаЛога; }
            set { m_РасширениеФайлаЛога = value; }
        }
        /// <summary>
        /// Максимальный размер всех файлов лога на диске, в мегабайтах.
        /// </summary>
        [Category("Log"), Description("Максимальный размер всех файлов лога на диске, в мегабайтах")]
        public int ПределРазмераКаталогаЛогаМб
        {
            get { return m_ПределРазмераКаталогаЛогаМб; }
            set { m_ПределРазмераКаталогаЛогаМб = value; }
        }
        /// <summary>
        /// Максимальный размер одного файла общего лога на диске, в мегабайтах.
        /// </summary>
        [Category("Log"), Description("Максимальный размер файла лога, в мегабайтах")]
        public int ПределРазмераФайлаЛогаМб
        {
            get { return m_ПределРазмераФайлаЛогаМб; }
            set { m_ПределРазмераФайлаЛогаМб = value; }
        }
        [Category("Log"), Description("Максимальное количество файлов лога в сеансе")]
        public int ПределКоличестваФайловЛогаВСеансе
        {
            get { return m_ПределКоличестваФайловЛогаВСеансе; }
            set { m_ПределКоличестваФайловЛогаВСеансе = value; }
        }

        #endregion



        /// <summary>
        /// NT-Загрузить настройки из файла
        /// </summary>
        /// <param name="filename">Путь к файлу настроек</param>
        public static OperatorSettings Load(String filename)
        {

            //load file
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(OperatorSettings));
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            OperatorSettings result = (OperatorSettings)reader.Deserialize(file);
            file.Close();
            //вписать путь к файлу настроек в свойства Оператора
            result.m_SettingFilePath = filename;
            //проверить  настройки  и исправить некритические ошибки
            result.CheckValues();

            return result;
        }

        /// <summary>
        /// NT-Сохранить настройки в файл
        /// </summary>
        public void Save()
        {
            //сохраним по старому пути
            this.Save(this.m_SettingFilePath);
        }

        /// <summary>
        /// NT-Сохранить настройки в файл
        /// </summary>
        /// <param name="filePath">Путь к файлу настроек</param>
        public void Save(String filePath)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(OperatorSettings));
            System.IO.StreamWriter file = new System.IO.StreamWriter(filePath);
            writer.Serialize(file, this);
            file.Close();

            return;
        }

        /// <summary>
        /// NT-Проверить и исправить значения параметров в допустимые пределы
        /// </summary>
        public void CheckValues()
        {
            //TODO: проверить версию Оператора и подсистемы лога.
            //Если не совпадает, выдать исключение.

            if (!Directory.Exists(this.OperatorFolderPath))
                throw new Exception("Путь OperatorFolderPath не существует");

            if (String.IsNullOrEmpty(this.LogFolderPath))
                throw new Exception("Путь LogFolderPath не указан");

            if (String.IsNullOrEmpty(this.m_РасширениеФайлаЛога))
                throw new Exception("Значение РасширениеФайлаЛога не указано");

            if ((this.LogTimeoff < 10) || (this.LogTimeoff > 3600))
                throw new Exception("Значение LogTimeoff не находится в допустимых пределах (10..3600)");

            //предел размера файла лога: от 1 Мб до 2047Мб, чтобы не превысить ограничение FAT 2Гб на файл.
            if ((this.ПределРазмераФайлаЛогаМб < 1) || (this.ПределРазмераФайлаЛогаМб > 2047))
                throw new Exception("Значение ПределРазмераФайлаЛога не находится в допустимых пределах (1Мб..2047Мб)");

            if ((this.ПределКоличестваФайловЛогаВСеансе < 1) || (this.ПределКоличестваФайловЛогаВСеансе > 8192))
                throw new Exception("Значение ПределКоличестваФайловЛогаВСеансе не находится в допустимых пределах (1..8192)");

            //предел размера каталога лога: от 100 Мб до 2Тб.
            if ((this.m_ПределРазмераКаталогаЛогаМб < 100) || (this.m_ПределРазмераКаталогаЛогаМб > 2097152))
                throw new Exception("Значение ПределРазмераКаталогаЛога не находится в допустимых пределах (100Мб..2097152Мб)");

            return;
        }

        /// <summary>
        /// NT-Установить значения по умолчанию
        /// </summary>
        public void SetDefault()
        {
            this.m_logFolderPath = OperatorSettings.LogПутьКаталогаЛога;//относительно каталога Оператор
            this.m_logTimeoff = OperatorSettings.LogТаймаутФайлов; //60 секунд таймаут
            this.m_Version = new OperatorVersion(Assembly.GetExecutingAssembly().GetName().Version);
            this.m_LogVersion = new OperatorVersion(LogSubsystem.LogManager.СтрокаВерсииПодсистемыЛога);
            //this.m_SettingFilePath
            this.m_ПределКоличестваФайловЛогаВСеансе = OperatorSettings.LogПределКоличестваФайловЛогаВСеансе;
            this.m_ПределРазмераФайлаЛогаМб = OperatorSettings.LogПределРазмераФайлаЛогаМб;
            this.m_ПределРазмераКаталогаЛогаМб = OperatorSettings.LogПределРазмераКаталогаЛогаМб;
            this.m_РасширениеФайлаЛога = OperatorSettings.LogРасширениеФайлаЛога;

            return;
        }

        public override string ToString()
        {
            return base.ToString();//TODO: add code here - я не знаю, что тут показывать пользователю.
        }



    }
}