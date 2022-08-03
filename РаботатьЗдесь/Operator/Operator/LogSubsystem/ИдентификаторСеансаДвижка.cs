using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;

namespace Operator.LogSubsystem
{
    public class ИдентификаторСеансаДвижка
    {
        private const string formatString = "yyyyMMdd-HHmmss";
        
        private Int64 m_КодСеансаДвижка;

        public ИдентификаторСеансаДвижка()
        {
            m_КодСеансаДвижка = 0;
        }

        public ИдентификаторСеансаДвижка(Int64 val)
        {
            m_КодСеансаДвижка = val;
        }

        public Int64 КодСеансаДвижка
        {
            get { return m_КодСеансаДвижка; }
            set { m_КодСеансаДвижка = value; }
        }

        /// <summary>
        /// RT-Создать новый идентификатор
        /// </summary>
        /// <returns>Возвращает новый идентификатор</returns>
        public static ИдентификаторСеансаДвижка СоздатьНовыйИдентификатор()
        {
            //производится из текущего времени системных часов.
            //вообще, в таких случаях ставят задержку на х миллисекунд, чтобы следующий вызов не вернул 
            //это же значение, так как миллисекунда еще не прошла. (Или сколько там гранулярность счетчика?)
            //Но в данном случае вряд ли я буду запускать несколько раз в секунду сеанс движка.
            //А если буду, то это станет проблемой.
            
            long time = DateTime.Now.ToBinary();
            return new ИдентификаторСеансаДвижка(time);
        }

        /// <summary>
        /// RT-Получить строковое значение идентификатора
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            DateTime dt = DateTime.FromBinary(m_КодСеансаДвижка);
            return dt.ToString(formatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// RT-Распарсить строку идентификатора
        /// </summary>
        /// <param name="val">Строка Идентификатора</param>
        /// <returns>Возвращает True при успехе, False при неудаче.</returns>
        /// <remarks>
        /// Это нельзя использовать для восстановления идентификатора из строки, 
        /// поскольку исходный идентификатор еще содержит таймзону и миллисекунды,
        /// а в строке идентификатора их нет. Поэтому и результаты будут различаться.
        /// Если надо полностью восстанавливать идентификатор из строки, то надо огрубить 
        /// функцию создания идентификатора, явно передавая ей значения составляющих. 
        /// Но сейчас предполагается, что эта функция только будет проверять имя папки сеанса лога.
        /// Что она соответствует формату и это папка сеанса лога.
        /// Поэтому она сделана максимально просто.
        /// </remarks>
        public bool Parse(string val)
        {
            bool flag = false;
            try
            {
                String line = val.Trim();
                //checking
                if ((line.Length != 15)  || (line[8] != '-')) 
                    throw new Exception();

                //get year
                Int32 y = Int32.Parse(line.Substring(0, 4), CultureInfo.InvariantCulture);
                Int32 m = Int32.Parse(line.Substring(4, 2), CultureInfo.InvariantCulture);
                Int32 d = Int32.Parse(line.Substring(6, 2), CultureInfo.InvariantCulture);
                Int32 h = Int32.Parse(line.Substring(9, 2), CultureInfo.InvariantCulture);
                Int32 mm = Int32.Parse(line.Substring(11, 2), CultureInfo.InvariantCulture);
                Int32 s = Int32.Parse(line.Substring(13, 2), CultureInfo.InvariantCulture);
                DateTime result = new DateTime(y, m, d, h, mm, s);
                //
                this.m_КодСеансаДвижка = result.ToBinary();
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
            }
            return flag;
        }

    }
}
