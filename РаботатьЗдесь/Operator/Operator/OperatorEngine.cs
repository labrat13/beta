using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Operator.LogSubsystem;

namespace Operator
{
    /// <summary>
    /// Собственно Оператор как движок 
    /// </summary>
    public class OperatorEngine
    {
        #region Fields
        /// <summary>
        /// Объект контроллера лога
        /// </summary>
        private LogSubsystem.LogManager m_Log;
        /// <summary>
        /// Объект настроек Оператора
        /// </summary>
        private OperatorSettings m_settings;


        #endregion

        #region Events
        /// <summary>
        /// NR-команда обратной связи
        /// </summary>
        public event System.EventHandler<Engine.TextEventArgs> BackCommandEvent;

        /// <summary>
        /// NR-Вывод на консоль
        /// </summary>
        public event System.EventHandler<Engine.TextEventArgs> ConsoleOutputEvent;

        /// <summary>
        /// NR-Вывод в строку состояния окна
        /// </summary>
        public event System.EventHandler<Engine.TextEventArgs> StateChangedEvent;

        #endregion

        /// <summary>
        /// NR-Конструктор
        /// </summary>
        public OperatorEngine()
        {
            //TODO: Доделать движок здесь
        }

        #region Properties
        /// <summary>
        /// Объект контроллера лога
        /// </summary>
        public LogSubsystem.LogManager Log
        {
            get { return m_Log; }
            set { m_Log = value; }
        }
        /// <summary>
        /// Объект настроек Оператора
        /// </summary>
        public OperatorSettings Settings
        {
            get { return m_settings; }
            set { m_settings = value; }
        }
        #endregion
        /// <summary>
        /// NR-Initialize engine
        /// </summary>
        /// <param name="settingFilePath">Путь к каталогу Оператора. Обычно это текущий каталог приложения.</param>
        /// <remarks>
        /// Если файл настроек Оператора не найден в каталоге Оператора, он будет создан с настройками по умолчанию.
        /// </remarks>
        public void Open(String operatorFolderPath)
        {
            //TODO: Доделать движок здесь
            //Open or create settings file
            //1 загрузить файл настроек
            String setpath = Path.Combine(operatorFolderPath, OperatorSettings.НазваниеФайлаНастроек);
            //create file if not exists
            if (!File.Exists(setpath))
            {
                this.m_settings = new OperatorSettings();
                this.m_settings.Save(setpath);
                //тут нельзя сразу использовать свежесозданный объект настроек, поскольку его переменная 
                //путь к файлу настроек инициализируется только при сохранении и загрузке файла
                //и используется затем в коде Оператора для пути каталога Оператора 
            }
            //load settings from file
            this.m_settings = OperatorSettings.Load(setpath);


            //Log subsystem init
            //2 создать менеджер лога
            this.m_Log = new LogManager(this);

            //3 открыть менеджер лога
            this.m_Log.Open(this.m_settings);
            //TODO: подключить вывод сообщений лога в окно приложения - здесь некуда, это надо делать в главной форме отдельно.
            //Да и там пока некуда - нужно сделать независимую форму-окно просмотра этих сообщений от лога, и в него выводить.
            //this.m_Log.MessageAdded += new EventHandler<LogMessageAddedEventArgs>(m_logman_m_MessageAdded);



            return;
        }

        /// <summary>
        /// NR-Finalize engine
        /// </summary>
        public void Close()
        {
            //TODO: Доделать движок здесь

            //2 завершить менеджер лога
            this.m_Log.Close();
            //3 обнулить менеджер лога
            this.m_Log = null;


            return;
        }



        /// <summary>
        /// NT-Событие канала времени - секундные импульсы
        /// </summary>
        public void СобытиеСекунд()
        {
            //Этот код сейчас выполняется потоком таймера окна Оператор

            //это таймер для испытания таймаута файлов лога.
            //его надо подключить к функции менеджера лога 
            //включить после инициализации менеджера лога
            //выключить до завершения менеджера лога
            if(this.m_Log != null)
                this.m_Log.timer1Second();//передаем импульсы в лог для механизма таймаута файлов лога
            
            return;
        }

        /// <summary>
        /// NR-Событие канала хоткеев - название сработавшего хоткея
        /// </summary>
        public void СобытиеХоткей(String hotkeyName)
        {
            ; //throw new System.NotImplementedException();//TODO: Доделать канал хоткеев здесь
        }

        /// <summary>
        /// NR-Событие канала ввода пользователя - введенный текст
        /// </summary>
        public void СобытиеВводКонсоли(String text)
        {
            ;//throw new System.NotImplementedException();//TODO: Доделать канал консоли здесь
        }

        /// <summary>
        /// NT-Событие канала лога - сообщение для лога
        /// </summary>
        public void СобытиеЗаписьВЛог(LogSubsystem.LogMessage msg)
        {
            this.m_Log.AddMessage(msg);

            return;
        }

        /// <summary>
        /// NR-Событие управления исполнением процедуры
        /// </summary>
        public void СобытиеУправленияИсполнением(Engine.FlowControlCommandCode cmd)
        {
            //throw new System.NotImplementedException();//TODO: Доделать канал управления здесь
            //TODO: тут надо вызвать соответствующий метод Исполнителя
            //и исполнитель должен внести запись в лог, что поступила соответствующая команда.
            //Можно просто передать cmd команду Исполнителю отсюда.
            //Это первоочередная команда, она не должна ждать.
        }

        /// <summary>
        /// NR-Событие Запуск Оператор
        /// </summary>
        public void СобытиеЗапускаОператор()
        {
            ;//throw new System.NotImplementedException();//TODO: Доделать движок здесь
        }

        /// <summary>
        /// NR-Событие Завершение Оператор
        /// </summary>
        public void СобытиеЗавершенияОператор()
        {
            ;//throw new System.NotImplementedException();//TODO: Доделать движок здесь
        }

        /// <summary>
        /// NR-События питания и сеанса пользователя
        /// </summary>
        /// <remarks>
        /// События сеанса пользователя: 
        /// - событие блокировки сеанса
        /// - событие восстановления сеанса
        /// События питания:
        /// - подключение сети
        /// - отключение сети
        /// - изменение состояния питания
        /// - TODO: тут надо еще проработать значение событий питания
        /// </remarks>
        /// <param name="code">Код события смены состояния системы</param>
        public void СобытиеПитанияИСеанса(Engine.SystemStateCode code)
        {
            ;//throw new System.NotImplementedException();//TODO: Доделать код событий питания и сеанса пользователя здесь
        }
    }
}
