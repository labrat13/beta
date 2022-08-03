using System;
using System.Collections.Generic;
using System.Text;

namespace Operator.LogSubsystem
{
    /// <summary>
    /// Событие Новое сообщение лога
    /// </summary>
    public class LogMessageAddedEventArgs : EventArgs
    {
        private readonly LogMessage m_msg;

        
        public LogMessageAddedEventArgs(LogMessage msg)
        {
            this.m_msg = msg;
        }

        /// <summary>
        /// Сообщение лога
        /// </summary>
        public LogMessage Msg
        {
            get { return m_msg; }
            //set { m_msg = value; }
        }

    }
}
