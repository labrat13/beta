using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Operator.LogSubsystem
{
    /// <summary>
    /// NT-Класс представляет объект команды потоку
    /// </summary>
    public class CommandMessage
    {
        /// <summary>
        /// код команды
        /// </summary>
        private CommandMessageCode m_Command;

        /// <summary>
        /// аргумент команды или нуль
        /// </summary>
        private Object m_Argument;

        ///<summary>
        /// Конструктор
        ///</summary>
        public CommandMessage()
        {
            this.m_Command = CommandMessageCode.None;
            //this.m_Argument = null;//done automatically
        }

        ///<summary>
        /// Конструктор для команды без аргументов
        ///</summary>
        /// <param name="cmd">код команды</param>
        public CommandMessage(CommandMessageCode cmd)
        {
            this.m_Command = cmd;
            //this.m_Argument = null;//done automatically
        }
        ///<summary>
        /// Конструктор
        ///</summary>
        /// <param name="cmd">код команды</param>
        /// <param name="arg">аргумент команды или нуль</param>
        public CommandMessage(CommandMessageCode cmd, object arg)
        {
            this.m_Command = cmd;
            this.m_Argument = arg;
        }


        /// <summary>
        /// код команды
        /// </summary>
        public CommandMessageCode Command
        {
            get { return m_Command; }
            set { m_Command = value; }
        }
        /// <summary>
        /// аргумент команды или нуль
        /// </summary>
        public Object Argument
        {
            get { return m_Argument; }
            set { m_Argument = value; }
        }
        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String t = "null";
            if (m_Argument != null)
                t = m_Argument.ToString();
            return String.Format(CultureInfo.CurrentCulture, "{0};{1}", m_Command, t);
        }

    }
}
