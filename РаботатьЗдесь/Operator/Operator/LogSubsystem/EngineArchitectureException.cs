using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Operator.LogSubsystem
{
    /// <summary>
    /// Исключение архитектуры движка
    /// Обычно сообщает о превышении установленных ограничений.
    /// Но пока используется везде где попало.
    /// </summary>
    [SerializableAttribute]
    public class EngineArchitectureException : Exception
    {

        public EngineArchitectureException() : base()
        {
        }

        public EngineArchitectureException(String message) : base(message)
        {
        }

        public EngineArchitectureException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EngineArchitectureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


    }
}
