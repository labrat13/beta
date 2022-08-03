using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using Operator.Hotkey;
using Operator.Engine;
using Operator.LogSubsystem;

namespace Operator
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// CultureInfo объект как кеш для быстрых функций.
        /// </summary>
        private CultureInfo m_myCultureInfoRU;

        /// <summary>
        /// Счетчик секунд для функции таймера
        /// </summary>
        private int m_seconds;
        /// <summary>
        /// Принимает и перерабатывает системные события для канала питания и сеанса пользователя, для движка.
        /// </summary>
        /// <remarks>
        /// Содержит часть функционала главного окна, разгружает его от функций и полей 
        /// Должен создаваться после создания объекта движка.
        /// Должен инициализироваться после инициализации объекта движка, но до начала его работы.
        /// Должен завершаться перед завершением объекта движка.
        /// </remarks>
        private SystemEventCanalizator m_sysEventCanalizator;
        /// <summary>
        /// Объект движка Оператора
        /// </summary>
        private OperatorEngine m_engine;
        
        public Form1()
        {
            InitializeComponent();
            this.m_myCultureInfoRU = new CultureInfo("ru-RU", true);
            this.m_seconds = 0;
            this.m_engine = new OperatorEngine();
            this.m_sysEventCanalizator = new SystemEventCanalizator(this.m_engine);//должен создаваться после создания объекта движка
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //показать дату в строке состояния до таймера, чтобы она сразу показывалась, а не только в начале новой минуты.
            showDateTimeInStatusLine(DateTime.Now);

            //показать стартовый текст в строке состояния
            setStatusText("Загрузка...",true);
            
            //Тут создать и инициализировать объект Движка
            String appfolder = Application.StartupPath;
            m_engine.Open(appfolder);
            //и сообщить пользователю о запуске лога
            setStatusText("Лог начал работу", true);

            //записать в лог сообщение о функции окна
            this.m_engine.СобытиеЗаписьВЛог(new LogMessage(LogMessageCode.ТребуетсяОпределить, "On MainForm_Load()"));
            //установить ловушки системных событий
            m_sysEventCanalizator.initSystemEvents();//инициализировать после инициализации Движка.

            m_engine.СобытиеЗапускаОператор();// Тут вызвать канал ЗапускОператор

            //Зарегистрировать глобальные хоткеи 12шт
            //это надо делать после инициализации системы каналов сообщений
            setStatusText("Регистрация хоткеев...", true);
            this.initHotKeyCollection();
            
            
            //timer enable
            //это обновляет дату в строке состояния каждую минуту.
            //и таймаут файлов подсистемы лога
            setStatusText("Запуск таймера...", true);
            this.timer1.Enabled = true;

            //записать в лог сообщение о функции окна
            this.m_engine.СобытиеЗаписьВЛог(new LogMessage(LogMessageCode.ТребуетсяОпределить, "Leave MainForm_Load()"));
            
            //инициализация закончена успешно, ожидаем реакции пользователя
            //показать стартовый текст в строке состояния
            setStatusText("Приказывай, Повелитель...", false);//тут функция заканчивается, и все события будут обработаны формой.

            return;
        }
        /// <summary>
        /// NR-Окно собирается закрыться
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //вывести сообщение пользователю, если он успеет прочитать это.
            setStatusText("Operator завершает работу..", true);
            //записать в лог сообщение о функции окна
            this.m_engine.СобытиеЗаписьВЛог(new LogMessage(LogMessageCode.ТребуетсяОпределить, "On MainForm_FormClosing()"));
            //вызвать канал событий ЗавершениеОператора
            this.m_engine.СобытиеЗавершенияОператор();
            //закрыть ловушки системных событий
            this.m_sysEventCanalizator.purgeSystemEvents();//вызвать до завершения Движка
            //Остановить таймер тут?
            this.timer1.Enabled = false;
            //Закрыть Движок
            this.m_engine.Close();
            this.m_engine = null;
            //вывести сообщение пользователю, если он успеет прочитать это.
            setStatusText("Operator завершил работу", false);

            return;
        }
        /// <summary>
        /// NR-Окно точно закроется
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //удалить хоткеи правильно
            this.purgeHotKeyCollection();

            return;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //Свернуть окно в трей
            if (this.WindowState == FormWindowState.Minimized)
            {
                NotifyFormCollapse();
            }
        }

        /// <summary>
        /// NT-перегруженная оконная процедура формы
        /// </summary>
        /// <param name="m">Сообщение окну</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            //обработать хоткеи:
            //разные сообщения окну приходят раньше, чем создаются объекты хоткеев.
            //Поэтому тут надо отфильтровывать сообщения хоткеев, которые приходят уже после создания окна.
            processHotKeyMsg(ref m);



            return;
        }

#region *** Notify icon functions and handlers ***

        // Одиночный клик правой кнопкой мыши по иконке трея - показ контекстного меню иконки трея

        //TODO: Тут надо придумать целую систему из очереди сообщений для показа через балун.
        //Даже если их много, каждая должна ждать, пока предыдущая или истечет или будет кликнута пользователем.
        //Это асинхронный процесс, и я пока не могу предложить код для этого.
        //Но это явно должно быть выделено в отдельный класс, поскольку код сложный.
        
        /// <summary>
        /// Отрицательная реакция на сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_BalloonTipClosed(object sender, EventArgs e)
        {
            //Это событие генерируется и при закрытии балуна нажатием на крестик, и по истечении таймаута балуна.
            //Тут надо перейти к показу балуна со следующим сообщением, если он есть в очереди.
            //Но я знаю, что код здесь выполняется асинхронно и не блокирует основной процесс.
        }
        /// <summary>
        /// Положительная реакция на сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            //Тут должна быть запущена некоторая процедура, которая зависит от содержания сообщения.
            //А остальные сообщения в это время должны ждать, пока пользователь не закончит эту работу.
            //То есть, это не асинхронные события, а прямой вызов функции-обработчика.
            //А потом надо перейти к показу балуна со следующим сообщением, если он есть в очереди.
            //Но я знаю, что код здесь выполняется асинхронно и не блокирует основной процесс.

            //Сейчас просто восстанавливаем окно из трея, чтобы посмотреть что произошло в окне.
            NotifyFormExpand();
        }
        /// <summary>
        /// Двойной клик любой кнопкой мыши - разворачивание окна из трея
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            NotifyFormExpand();
        }
        /// <summary>
        /// Действия при свертывании окна в трей
        /// </summary>
        private void NotifyFormCollapse()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3000);
            this.ShowInTaskbar = false;
        }
        /// <summary>
        /// Действия при восстановлении окна из трея
        /// </summary>
        private void NotifyFormExpand()
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }
        /// <summary>
        /// RT-Одиночный клик правой кнопкой мыши - показ контекстного меню автоматически
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //Сейчас не вызывается, так как не подключен к notifyIcon1.
            //NotifyIcon криво работает - то показывает меню, то нет.
            //Непредсказуемо работает вот именно здесь.
            //Нельзя получить экранные координаты контрола, нельзя перевести координаты мыши в координаты контрола, чтобы показать меню из кода.
            //Поэтому нельзя сделать так, чтобы одиночный левый клик восстанавливал окно, а правый - показывал контекстное меню.
        }




        /// <summary>
        /// Пункт Показать из контекстного меню трея
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotifyFormExpand();
        }
        /// <summary>
        /// Пункт Закрыть приложение из контекстного меню трея
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowBaloon(String Title, String text, System.Windows.Forms.ToolTipIcon icon, int time)
        {
            //Этот вызов не изменяет установленные ранее тексты и иконку
            //Но он не показывает ничего, если notifyIcon невидим
            notifyIcon1.ShowBalloonTip(time, Title, text, icon);
            return;
        }

#endregion

        #region *** Timer 480ms functions ***



        /// <summary>
        /// 0.5-second timer procedure handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            
            //генерировать 1-секундные события для лога и канала времени
            int tsec = dt.Second;
            if (m_seconds != tsec)
            {
                //новое значение секунды. Это случается только один раз в секунду.
                //TODO: тут вставить вызов канала событий секунд
                //TODO: Тут вставить вызов 1с для подсистемы лога

                //показать дату и время в строке состояния
                if (tsec == 0) //только в начале каждой минуты, чтобы снизить нагрузку на поток.
                    showDateTimeInStatusLine(dt);
            }

            //обновить окно надо бы, но здесь пока не стоит это вызывать.
            return;
        }


        #endregion


        #region Status line text functions

        /// <summary>
        /// NT-Установить текст строки состояния программы
        /// </summary>
        /// <param name="text">Текст строки состояния</param>
        /// <param name="update">Применить изменения немедленно</param>
        private void setStatusText(string text, bool update)
        {
            this.toolStripStatusLabel_State.Text = text;
            if (update)
                Application.DoEvents();
            return;
        }

        /// <summary>
        /// NR-Показать указанную дату в строке состояния
        /// </summary>
        private void showDateTimeInStatusLine(DateTime dt)
        {
            string text = dt.ToString("HH:mm, dddd, d MMMM yyyyг.", this.m_myCultureInfoRU);
            this.toolStripStatusLabel_Datetime.Text = text;
            return;
        }

        #endregion


        #region HotKey functions

        //TODO: Вынести функции хоткеев в отдельный класс, чтобы разгрузить класс формы. 
        /// <summary>
        /// Словарь глобальных хоткеев Alt+Win+F1 - Alt+Win+F12
        /// Если какой-то хоткей не удалось зарегистрировать, он будет отсутствовать в словаре.
        /// </summary>
        private Dictionary<String, GlobalHotkey> m_HotKeyDictionary;
        /// <summary>
        /// NT-Создать и заполнить хоткеями словарь хоткеев
        /// </summary>
        private void initHotKeyCollection()
        {
            m_HotKeyDictionary = new Dictionary<string, GlobalHotkey>();
            addCreateHotkey("Alt+Win+F1", this, 41, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F1);
            addCreateHotkey("Alt+Win+F2", this, 42, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F2);
            addCreateHotkey("Alt+Win+F3", this, 43, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F3);
            addCreateHotkey("Alt+Win+F4", this, 44, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F4);
            addCreateHotkey("Alt+Win+F5", this, 45, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F5);
            addCreateHotkey("Alt+Win+F6", this, 46, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F6);
            addCreateHotkey("Alt+Win+F7", this, 47, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F7);
            addCreateHotkey("Alt+Win+F8", this, 48, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F8);
            addCreateHotkey("Alt+Win+F9", this, 49, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F9);
            addCreateHotkey("Alt+Win+F10", this, 50, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F10);
            addCreateHotkey("Alt+Win+F11", this, 51, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F11);
            addCreateHotkey("Alt+Win+F12", this, 52, GlobalHotkey.MOD_ALT | GlobalHotkey.MOD_WIN, Keys.F12);
            return;
        }
        /// <summary>
        /// NT-Добавить хоткей в словарь хоткеев
        /// Если такое имя уже есть в словаре, словарь выбросит исключение.
        /// </summary>
        /// <param name="title">Название хоткея, например, Alt+Win+F2.</param>
        /// <param name="form">Объект окна</param>
        /// <param name="hotkeyId">Identifier for new hotkey. Must be any value between 0 and 0xBFFF.</param>
        /// <param name="modifiers">Key modifier like MOD_CONTROL | MOD_WIN</param>
        /// <param name="key">Single virtual key code, like Keys.K</param>
        private void addCreateHotkey(string title, Form form, int hotkeyId, int modifiers, Keys key)
        {
            try
            {
                GlobalHotkey gh = new GlobalHotkey(form, hotkeyId, modifiers, key);
                //add to dictionary if success
                m_HotKeyDictionary.Add(title, gh);
            }
            catch (Exception ex)
            {
                //TODO: вывести тут пользователю, и в лог, сообщение о неудаче регистрации хоткея
            }
            //или можно тут записать в словарь с таким ключом null, но тогда использование словаря везде будет сложнее.
            //зато будет ясно каких хоткеев нет в словаре.
            return;
        }

        /// <summary>
        /// NT-Разрегистрировать хоткеи и очистить их словарь
        /// </summary>
        private void purgeHotKeyCollection()
        {
            //Разрегистрировать хоткеи
            foreach (KeyValuePair<String, GlobalHotkey> kvp in m_HotKeyDictionary)
            {
                kvp.Value.Unregister();
            }
            //удалить все объекты из словаря
            m_HotKeyDictionary.Clear();
        }

        /// <summary>
        /// NT-Обработать сообщение из оконной процедуры и найти в нем хоткей
        /// </summary>
        /// <param name="msg"></param>
        private void processHotKeyMsg(ref Message msg)
        {
            //быстро выйти, если не наше сообщение
            if (msg.Msg != GlobalHotkey.WM_HOTKEY)
                return;
            //искать хоткей по коду идентификатора хоткея
            short id = (short)msg.WParam; 
            foreach (KeyValuePair<String, GlobalHotkey> kvp in m_HotKeyDictionary)
            {
                if(kvp.Value.HotkeyId == id)
                {
                    //соответствующий хоткей найден
                    onHotkeyEvent(kvp.Key);
                    break;//больше искать в словаре незачем
                }
            }
            
            return;
        }
        /// <summary>
        /// NR-Тут отослать событие хоткея в канал сообщений для дальнейшей обработки
        /// </summary>
        /// <param name="hotkeyTitle">Название хоткея, например, Alt+Win+F2.</param>
        private void onHotkeyEvent(string hotkeyTitle)
        {
            //TODO: Тут отослать событие хоткея в канал сообщений для дальнейшей обработки
            //Пока выведем событие хоткея в строку состояния для отладки
            setStatusText("Хоткей " + hotkeyTitle, false);
        }

        #endregion


        #region Функции канала управления исполнением
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

            //Перехват Pause|Break не получается легко.
            //Вместо четких кодов клавиш выводится хрен пойми что.
            //for Pause key:
            //LButton|RButton|ShiftKey
            //KeyValue=19

            //for Shift+Pause key:
            //Key=LButton|RButton|ShiftKey
            //KeyValue=19
            //KeyModifier = Shift
            
            //TODO: Тут надо здесь обрабатывать состояние исполнителя, чтобы изменять иконку в трее
            //хотя, состояние исполнителя можно передавать в окно через канал обратных команд
            //и уже тогда изменять иконку на Оператор в паузе.
            //Надо определиться, где выявлять и обрабатывать команды Pause|Resume|Break: В форме или в исполнителе.

            //Кнопка паузы неправильная: Должно быть: Пауза это Pause, а Останов это Shift+Pause=Break. 
            //Надписи на кнопке неправильные.

            //Сейчас все работает как надо.

            if (e.KeyValue == 19)//Это значение для кнопки Pause/Break
            {
                if (e.Modifiers == Keys.Shift)
                {
                    //send Operator.Engine.FlowControlCommandCode.Break;
                }
                else if(e.Modifiers == Keys.None)
                {
                    //send Operator.Engine.FlowControlCommandCode.Pause;
                }
            }



            return;
        }
        #endregion







//куски-примеры кода - удалить после релиза проекта
       
       ////записать в лог сообщение о функции окна
       //this.m_engine.СобытиеЗаписьВЛог(new LogMessage(LogMessageCode.ТребуетсяОпределить, "On MainForm_Load()"));

    }
}
