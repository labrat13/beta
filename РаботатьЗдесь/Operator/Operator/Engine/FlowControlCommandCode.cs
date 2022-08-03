using System;
using System.Collections.Generic;
using System.Text;

namespace Operator.Engine
{
    /// <summary>
    /// Коды событий канала управления потоком исполнения процедуры
    /// </summary>
    public enum FlowControlCommandCode
    {
        /// <summary>
        /// Значение ошибки
        /// </summary>
        None = 0,
        /// <summary>
        /// Команда приостановить исполнение
        /// </summary>
        Pause = 1,
        /// <summary>
        /// Команда прервать/отменить исполнение
        /// </summary>
        Break = 2,
        /// <summary>
        /// Команда придолжить приостановленное исполнение
        /// </summary>
        Resume = 3,
    }
}
