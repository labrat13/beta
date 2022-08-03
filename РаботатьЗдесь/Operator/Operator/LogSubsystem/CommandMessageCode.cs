using System;
using System.Collections.Generic;
using System.Text;

namespace Operator.LogSubsystem
{
    public enum CommandMessageCode
    {
        None = 0,
        /// <summary>
        /// Команда закрыть файлы лога
        /// </summary>
        CloseLogFiles = 1,
        /// <summary>
        /// Команда добавить сообщение лога, указанное в аргументе команды
        /// </summary>
        AddLogMessage = 2,
        /// <summary>
        /// Команда прекратить работу подсистемы
        /// </summary>
        FinishLogLoop = 3,
    }
}
