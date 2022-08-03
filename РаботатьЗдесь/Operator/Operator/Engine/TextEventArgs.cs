using System;
using System.Collections.Generic;
using System.Text;

namespace Operator.Engine
{
    
    /// <summary>
    /// Аргумент с текстом
    /// </summary>
    public class TextEventArgs : EventArgs
    {
        private string m_text;

        public TextEventArgs(string text)
        {
            this.m_text = text;
        }

        /// <summary>
        /// Text message
        /// </summary>
        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }
    }

}
