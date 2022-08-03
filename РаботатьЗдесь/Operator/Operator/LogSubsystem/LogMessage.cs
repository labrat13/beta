using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Operator.LogSubsystem
{
    /// <summary>
    /// NR-Текстовое собщение лога
    /// </summary>
    public class LogMessage
    {
        /// <summary>
        /// Константа названия источника сообщения - подсистем Оператора
        /// Для свойства LogMessage.Agent
        /// </summary>
        public const string Agent_System = "Operator";
        /// <summary>
        /// Константа названия источника сообщения - команды пользователя  Записать в лог сообщение.
        /// Для свойства LogMessage.Agent
        /// </summary>
        public const string Agent_User = "User";
        
        #region fields

        /// <summary>
        /// Таймштамп сообщения
        /// </summary>
        private DateTime m_MsgTime;
        /// <summary>
        /// Код - аббревиатура сообщения
        /// </summary>
        private LogMessageCode m_MsgCode;
        /// <summary>
        /// Название источника сообщения
        /// </summary>
        private String m_Agent;
        /// <summary>
        /// Текст сообщения
        /// </summary>
        private string m_MsgText;

#endregion
        /// <summary>
        /// NT-Конструктор по умолчанию
        /// </summary>
        public LogMessage()
        {
            m_MsgCode = LogMessageCode.Unknown;
            m_MsgText = String.Empty;
            m_MsgTime = DateTime.Now;
            m_Agent = LogMessage.Agent_System;
        }

        /// <summary>
        /// NT-Конструктор  с параметрами. Таймштамп создается автоматически. Агент="Operator".
        /// </summary>
        /// <param name="code">Код - аббревиатура сообщения</param>
        /// <param name="text">Текст сообщения</param>
        public LogMessage(LogMessageCode code, string text)
        {
            m_MsgCode = code;
            m_MsgText = text;
            m_MsgTime = DateTime.Now;
            m_Agent = LogMessage.Agent_System;
        }
        /// <summary>
        /// NT-Конструктор  с параметрами. Таймштамп создается автоматически.
        /// </summary>
        /// <param name="code">Код - аббревиатура сообщения</param>
        /// <param name="text">Текст сообщения</param>
        /// <param name="agent">Название агента - источника сообщения.</param>
        public LogMessage(LogMessageCode code, string agent, string text)
        {
            m_MsgCode = code;
            m_MsgText = text;
            m_MsgTime = DateTime.Now;
            m_Agent = agent;
        }
        /// <summary>
        /// NR-Конструктор - парсер строки
        /// </summary>
        public LogMessage(string textline)
        {
            this.parse(textline);
        }

        #region properties
        /// <summary>
        /// Таймштамп сообщения
        /// </summary>
        public DateTime MsgTime
        {
            get { return m_MsgTime; }
            set { m_MsgTime = value; }
        }
        /// <summary>
        /// Код - аббревиатура сообщения
        /// </summary>
        public LogMessageCode MsgCode
        {
            get { return m_MsgCode; }
            set { m_MsgCode = value; }
        }
        /// <summary>
        /// Название источника сообщения:
        /// Operator - для всех подсистем Оператора
        /// User - для команды пользователя Записать в лог сообщение.
        /// </summary>
        public String Agent
        {
            get { return m_Agent; }
            set { m_Agent = value; }
        }
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string MsgText
        {
            get { return m_MsgText; }
            set { m_MsgText = value; }
        }

        #endregion

        /// <summary>
        /// NT-Собрать строку сообщения для показа пользователю
        /// </summary>
        /// <remarks>
        /// Эта функция для превращения сообщения в строку для вывода в текстовый список на экран.
        /// CSV формат из ToString не самый удачный, тут можно сделатьлучше.
        /// </remarks>
        public String GetAsString()
        {
            //адаптировано для вывода в листбокс как строки
            return String.Format(CultureInfo.CurrentCulture, "{0};{1};{2};{3}", this.m_MsgTime.ToString("dd.MM.yyyy-HH:mm:ss.fff", CultureInfo.CurrentCulture), this.m_MsgCode.ToString(), this.m_Agent, this.m_MsgText);
        }

        /// <summary>
        /// NR-Распарсить сообщение лога из текста
        /// </summary>
        /// <param name="text">Текст сообщения</param>
        private void parse(string text)
        {
            throw new System.NotImplementedException();//TODO: добавить код здесь, когда он потребуется, в последующих релизах.
        }

        /// <summary>
        /// NT-Текст для отладки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0};{1};{2};{3}", this.m_MsgCode.ToString(), this.m_Agent, this.m_MsgText, this.m_MsgTime.ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture));
        }
    }
}
