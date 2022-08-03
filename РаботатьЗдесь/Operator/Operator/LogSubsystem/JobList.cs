using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Globalization;

namespace Operator.LogSubsystem
{
    
    public class JobList
    {
        /// <summary>
        /// Константа предельный размер списка команд до выдачи события о переполнеии списка
        /// </summary>
        public const int ПредельныйРазмерОчередиКоманд = 4096;

        ///// <summary>
        ///// Список команд
        ///// </summary>
        private Queue<CommandMessage> m_CommandQueue;

        /// <summary>
        /// сигнал о добавлении сообщения в список сообщений
        /// </summary>
        internal ManualResetEvent m_waitEvent;

        /// <summary>
        /// Событие Высокой заполненности списка команд
        /// </summary>
        public event EventHandler TooManyCommandsEvent;

        /// <summary>
        /// Конструктор
        /// </summary>
        public JobList()
        {
            m_CommandQueue = new Queue<CommandMessage>();
            m_waitEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// NT-Close and free unmanaged resources
        /// </summary>
        public void Close()
        {
            m_waitEvent.Close();//close unmanaged resource
            m_CommandQueue.Clear();

            return;
        }

        /// <summary>
        /// NT-Добавить обычную команду в конец списка
        /// </summary>
        public void AddCommand(CommandMessage cmd)
        {
            ICollection ic = m_CommandQueue;
            lock (ic.SyncRoot)
            {
                m_CommandQueue.Enqueue(cmd);
                //если список переполнен, отправить событие переполнения списка
                if (m_CommandQueue.Count > ПредельныйРазмерОчередиКоманд)
                    OnTooManyCommands(new EventArgs());//Событие, наверно, обрабатывается текущим же потоком на стороне получателя.
                //разрешить исполнение ожидающему потоку 
                m_waitEvent.Set();
            }
            return;
        }


        ///// <summary>
        ///// NR-Добавить первоочередную команду
        ///// </summary>
        //public void Add1Command(CommandMessage cmd)
        //{
        //    //List не содержит функции для вставки в начало списка
        //    //Надо заводить список для первоочередных команд
        //    //Ведь вставки в начало списка не годятся, если надо выстроить последовательность команд. 
            
        //    throw new System.NotImplementedException();
        //}

        /// <summary>
        /// Получить команду для исполнения или ждать
        /// </summary>
        /// <remarks>
        /// Принцип работы очереди потока
        /// Если в очереди есть команды, поток извлекает их одну за другой по алгоритму:
        /// while(true)
        /// {
        ///     //get command
        ///     CommandMessage cmd = joblist.WaitCommand();
        ///     //execute command
        ///     ... do work
        /// }
        /// а если сообщений больше нет, поток засыпает внутри WaitCommand.
        /// </remarks>
        public CommandMessage WaitCommand()
        {
            //Если в списке есть команды, вернуть самую первую
            //а если нет, то ждать до появления команды в списке
            CommandMessage result = null;
            //тут заставить поток ждать, пока в списке что-то появится.
            this.m_waitEvent.WaitOne();//
            //тут поток должен спать, а просыпаться только когда в списке что-то появилось.
            ICollection ic = this.m_CommandQueue;
            lock (ic.SyncRoot)
            {
                //тут будет исключение, если список уже пустой - надо его отследить и отладить алгоритм.

                //извлечь первый элемент списка
                result = this.m_CommandQueue.Dequeue();

                //если список пустой, заставить поток уснуть на следующем вызове этой функции
                if (this.m_CommandQueue.Count == 0)
                    this.m_waitEvent.Reset();
                else
                    this.m_waitEvent.Set();//это необязательно, но для надежности .
            }
            return result;
        }


        /// <summary>
        /// NT-Очистить список команд без их исполнения.
        /// </summary>
        public void Clear()
        {
            ICollection ic = m_CommandQueue;
            lock (ic.SyncRoot)
            {
                m_CommandQueue.Clear();
            }
            return;
        }

        /// <summary>
        /// NT-запуск ивента переполнения списка
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTooManyCommands(EventArgs e)
        {
            if (TooManyCommandsEvent != null)
                TooManyCommandsEvent(this, e);
            return;
        }

        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Commands: {0}", this.m_CommandQueue.Count);
        }


    }
}
