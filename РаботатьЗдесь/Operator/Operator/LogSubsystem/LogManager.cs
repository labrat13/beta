using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Operator.Utility;

namespace Operator.LogSubsystem
{
    //Пример использования класса приведен в конце файла

    /// <summary>
    /// NT-Менеджер лога
    /// </summary>
    /// <remarks>
    /// Для многопоточной схемы этот класс применяет отдельный поток - циклический исполнитель команд.
    /// Команды буферизуются в списке, поток лога их выбирает и исполняет. А если команд нет - спит.
    /// Также, сущесвует канал обратной связи для сообщений о событиях лога и ошибках в логе. 
    /// Если ошибка произойдет в логе, то надо закрывать приложение.
    /// Хотя я не знаю, может ли быть обработано исключение, возникшее в другом потоке, но записать о нем некуда.
    /// </remarks>
    public class LogManager
    {

        #region Fields
        /// <summary>
        /// Константа версия подсистемы лога. 
        /// Хранится здесь как в специальном месте для констант подсистемы лога.
        /// </summary>
        public const string СтрокаВерсииПодсистемыЛога = "1.0.0.0";

        /// <summary>
        /// Абсолютный путь к папке Лога из Настроек Оператора
        /// </summary>
        private string m_КаталогЛогаОператора;

        /// <summary>
        /// Счетчик таймера для файлов лога
        /// </summary>
        private int m_timerCounter;
        /// <summary>
        /// Предел таймера для файлов лога
        /// Быстрый кеш значения из настроек, так как значение востребовано каждую секунду.
        /// </summary>
        private int m_logTimeoutInterval;
        /// <summary>
        /// Флаг что таймаут файлов лога еще не сработал
        /// Нужен чтобы запустить команду закрытия файлов лога однократно до следующего сброса счетчика таймаута
        /// </summary>
        private bool m_logFileTimeoutWaiting;
        /// <summary>
        /// Сеанс лога текущий
        /// </summary>
        private СеансЛога m_Session;
        /// <summary>
        /// NT-Добавлено сообщение основного лога
        /// </summary>
        public event EventHandler<LogMessageAddedEventArgs> MessageAdded;

        /// <summary>
        /// Объект настроек Оператора
        /// </summary>
        private OperatorSettings m_settings;
        /// <summary>
        /// Back reference to Operator engine
        /// </summary>
        private OperatorEngine m_engine;


        #endregion

        /// <summary>
        /// NT-Конструктор
        /// </summary>
        /// <remarks>
        /// Только инициализирует некоторые поля. Для инициализации нужно вызвать Open(..)
        /// </remarks>
        public LogManager(OperatorEngine backref)
        {
            m_engine = backref;
            m_timerCounter = 0;//сбросить счетчик файлов лога
            m_logTimeoutInterval = OperatorSettings.LogТаймаутФайлов;//Типично 1 минута до закрытия файлов лога.
            m_logFileTimeoutWaiting = true;

            return;
        }
        #region Properties
        /// <summary>
        /// Back reference to Operator engine
        /// </summary>
        public OperatorEngine Engine
        {
            get { return m_engine; }
            set { m_engine = value; }
        }
        /// <summary>
        /// Абсолютный путь к папке Лога из Настроек Оператора
        /// </summary>
        /// <remarks>
        /// Если путь в настройках указан относительный, хранить его здесь как абсолютный.
        /// Значение загружается в функции Open(..)
        /// </remarks>
        public string КаталогЛогаОператора
        {
            get { return m_КаталогЛогаОператора; }
            set { m_КаталогЛогаОператора = value; }
        }

        /// <summary>
        /// Объект настроек Оператора
        /// </summary>
        /// <remarks>
        /// Перенести в объект Движка, когда он будет.
        /// </remarks>
        internal OperatorSettings Settings
        {
            get { return m_settings; }
            //set { m_settings = value; }
        }
        #endregion

        /// <summary>
        /// NT-Начать работу и Открыть сеанс лога
        /// </summary>
        /// <param name="sett">настройки</param>
        public void Open(OperatorSettings sett)
        {
            //1 сохранить объект настроек оператора
            m_settings = sett;
            //создать идентификатор сеанса движка и лога - перенесено в объект СеансЛога
            //2 загрузить абсолютный путь к каталогу лога до открытия СеансЛога
            //загрузить сюда абсолютный путь каталога лога, преобразовав из относительного, если нужно
            this.m_КаталогЛогаОператора = Utility.Utilities.ПолучитьАбсолютныйПуть(sett.LogFolderPath, sett.OperatorFolderPath); 
            //3 создать и открыть сеанс лога
            СеансЛога session = new СеансЛога(this);//тут передавать надо объект движка, а менеджер лога через проперти
            session.Open(sett);//открыть сеанс лога
            this.m_Session = session;

            //4 настроить механизм таймаута файлов лога
            this.m_logTimeoutInterval = sett.LogTimeoff;
            this.m_timerCounter = 0;

            return;
        }

        /// <summary>
        /// NT-Завершить работу подсистемы лога
        /// </summary>
        public void Close()
        {
            //1 закрыть текущий объект сеанса лога
            this.m_Session.Close();

            return;
        }

        /// <summary>
        /// NT-Добавить сообщение в лог
        /// Эту команду нельзя использовать из самой подсистемы лога, так как она сбрасывает счетчик таймаута файлов лога.
        /// Вместо этого надо отправить команду CommandMessageCode.AddLogMessage в очередь команд потока-исполнителя лога.
        /// </summary>
        /// <param name="msg">Объект сообщения лога</param>
        public void AddMessage(LogMessage msg)
        {
            //добавить сообщение в лог
            this.m_Session.AddMessage(msg);
            //сбросить счетчик таймаута закрытия файлов
            this.resetTimeCounter();

            return;
        }

        /// <summary>
        /// NT-Закрыть файлы лога временно перед переходом в спящий режим и подобных случаях.
        /// </summary>
        public void CloseLogFilesTemporary()
        {
            if (m_Session != null)
                m_Session.CloseLogFilesByTimeout();//Закрыть все файлы лога, как по таймауту

            return;
        }

        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //тут пока я не знаю чего выводить для отладки
            return base.ToString();
        }

        #region Output event functions
        /// <summary>
        /// NT-Отправить сообщение в окно лога пользователя через обратный канал сообщений
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnMessageAdded(LogMessage msg)
        { 
            if (MessageAdded != null)          
            MessageAdded(this, new LogMessageAddedEventArgs(msg)); 
        }
        #endregion

        #region File Timeout functions
        /// <summary>
        /// NT-Сбросить счетчик таймера файлов лога
        /// </summary>
        private void resetTimeCounter()
        {
            //Interlocked. - нет функции для обнуления счетчика
            this.m_timerCounter = 0;
            return;
        }


        /// <summary>
        /// NT-Вход событий от таймера по 1с для закрытия файлов лога по таймауту
        /// </summary>
        public void timer1Second()
        {
            //Этот код сейчас выполняется потоком таймера окна Оператор

            //тут надо буферизовать команду закрытия файлов для разделения потоков.
            //поток, обслуживающий лог, должен закрыть файлы лога.
            //Но эту команду надо передавать асинхронно, чтобы поток лога мог спать на WaitHandle.

            m_timerCounter++;
            if (m_timerCounter >= m_logTimeoutInterval)
            {
                //Закрыть файлы лога, поскольку таймаут вышел

                //Этот код сейчас выполняется потоком таймера окна Оператор
                //тут надо сначала заблокировать все объекты файлов лога на монопольный доступ.
                //а то пока я их закрываю, другой поток может пытаться писать в них что-то.
                //А лучше - закрывать их тем же потоком, что и обслуживает сам лог.
                //Для этого нужна очередь сообщений для потока лога.
                //в нее и писать сообщение-команду, что нужно закрыть файлы лога.
                //тогда поток, обслуживающий лог, сам их закроет. А поток таймера окна - ни при чем.

                //таймаут должен запустить команду закрытия файлов, но только один раз
                if (m_logFileTimeoutWaiting == true)//таймаут не отработал команду
                {
                    if (m_Session != null)
                        m_Session.CloseLogFilesByTimeout();//Закрыть все файлы лога по таймауту
                    m_logFileTimeoutWaiting = false;//таймаут уже отработал команду
                }
                //Счетчик не сбрасываем, пусть считает дальше.
            }
            else
            {
                //таймаут был запущен, но еще не сработал - это надо подтвердить тут
                m_logFileTimeoutWaiting = true;
            }
            return;
        }

        #endregion

    }

    #region Пример использования менеджера лога
    //тут приводится целиком класс формы приложения, так как он достаточно минималистичен и иллюстрирует использование менеджера лога.

    //public partial class Form1 : Form
    //{
    //    /// <summary>
    //    /// Менеджер лога
    //    /// </summary>
    //    private LogManager m_logman;

    //    public Form1()
    //    {
    //        InitializeComponent();
    //    }
    //    /// <summary>
    //    /// Таймер 1 секундный для закрытия файлов лога по таймауту
    //    /// Если таймер не подключен, закрытия лога по таймауту не будет.
    //    /// Но все остальное вроде должно работать нормально.
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="e"></param>
    //    private void timer1_Tick(object sender, EventArgs e)
    //    {
    //        //это таймер для испытания таймаута файлов лога.
    //        //его надо подключить к функции менеджера лога 
    //        //включить после инициализации менеджера лога
    //        //выключить до завершения менеджера лога
    //        if (m_logman != null)
    //            m_logman.timer1Second();
    //    }

    //    /// <summary>
    //    /// Начать работу подсистемы лога
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="e"></param>
    //    private void startToolStripMenuItem_Click(object sender, EventArgs e)
    //    {
    //        string setpath = "C:\\Temp\\Operator\\settings.xml";

    //        //1 загрузить файл настроек
    //        OperatorSettings set;
    //        //create file if not exists
    //        if (!File.Exists(setpath))
    //        {
    //            set = new OperatorSettings();
    //            set.Save(setpath);
    //        }
    //        //load settings from file
    //        set = OperatorSettings.Load(setpath);


    //        //2 создать менеджер лога
    //        this.m_logman = new LogManager();

    //        //3 открыть менеджер лога
    //        this.m_logman.Open(set);
    //        this.m_logman.MessageAdded += new EventHandler<LogMessageAddedEventArgs>(m_logman_m_MessageAdded);
    //        //4 запустить таймер таймаута
    //        this.timer1.Enabled = true;
    //        //5 сообщить пользователю о готовности
    //        this.toolStripStatusLabel1.Text = "Log started";
    //    }


    //    /// <summary>
    //    /// Завершить работу подсистемы лога
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="e"></param>
    //    private void stopToolStripMenuItem_Click(object sender, EventArgs e)
    //    {
    //        //1 остановить таймер таймаута
    //        this.timer1.Enabled = false;

    //        //2 завершить менеджер лога
    //        this.m_logman.Close();
    //        //3 обнулить менеджер лога
    //        this.m_logman = null;
    //        //4 сообщить пользователю об остановке
    //        this.toolStripStatusLabel1.Text = "Log finished";
    //    }

    //    #region Async adding listbox item functions
    //    /// <summary>
    //    /// Добавить сообщение из лога в листбокс для обратного контроля
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="e"></param>
    //    internal void m_logman_m_MessageAdded(object sender, LogMessageAddedEventArgs e)
    //    {
    //        //Это выполняется в потоке-исполнителе лога
    //        //Тут надо добавить сообщение в листбокс из другого потока.
    //        //Для этого я нагородил отдельный предикат и функцию, все по МСДН и пимеру из интернета.
    //        //Хотя там пишут, что можно через EventHandler и проще.

    //        AddListBoxItem(e.Msg.GetAsString());
    //        return;
    //    }
    //    /// <summary>
    //    /// делегат для передачи данных между потоками
    //    /// </summary>
    //    /// <param name="message"></param>
    //    private delegate void AddListBoxItemDelegate(string message);

    //    private void AddListBoxItem(string message)
    //    {
    //        if (this.listBox1.InvokeRequired)
    //        {
    //            //если это не правильный поток-владелец листбокса, то создать делегат и отправить листбоксу в правильный поток
    //            this.listBox1.BeginInvoke(new AddListBoxItemDelegate(AddListBoxItem), message);//вызывает асинхронно и сразу возвращается.
    //            //не асинхронно - нельзя, так как тогда поток-исполнитель будет ждать исполнения этого коллбека
    //            //а коллбек не исполняется, пока поток формы ждет в Thread.Join() завершения потока-исполнителя.
    //            //получается взаимоблокировка потоков, разрываемая таймаутом в Join().
    //            //и результат коллбека мне не нужен, поэтому все хорошо.
    //        }
    //        else
    //        {
    //            //а сейчас это правильный поток, и тут мы выполняем нужный код
    //            this.listBox1.Items.Add(message);
    //        }
    //    }

    //    #endregion

    //    /// <summary>
    //    /// Отправить одно сообщение лога в лог
    //    /// </summary>
    //    /// <param name="sender"></param>
    //    /// <param name="e"></param>
    //    private void sendToolStripMenuItem_Click(object sender, EventArgs e)
    //    {
    //        //создаем объект сообщения лога
    //        LogMessage msg = new LogMessage(LogMessageCode.ВводКонсолиНеизвестнаяКоманда, LogMessage.Agent_User, "Команда пользователя");
    //        //отправляем сообщение в лог
    //        this.m_logman.AddGeneralMessage(msg);
    //        return;
    //    }



    //}

    //и еще есть функции:
    //CloseLogFilesTemporary() - закрыть файлы лога перед переходом в спящий режим, итп. 
    //Файлы снова откроются автомаически при следующей записи в файл.

    #endregion
}
