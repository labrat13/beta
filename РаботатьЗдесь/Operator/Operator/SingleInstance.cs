using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Operator
{
    /// <summary>
    /// NFT-Контролирует единственный экземпляр приложения в операционной системе
    /// TODO: Я не уверен, что этот класс будет стабильно работать для пользователя, у которого нет прав администратора.
    /// Это из-за именного мутекса - яхз, что там с правами пользователя. Нет сейчас возможности тестировать под разными пользователями. 
    /// </summary>
    /// <example>
    /// Этот код должен располагаться в начале приложения, еще до инициализации формы.
    /// Удобно поместить его в файле Program.cs сразу в начале Main()
    /// SingleInstance si = SingleInstance.Create("Meraman.Test");
    /// if (si == null)
    /// {
    ///     MessageBox.Show("Not single instance", "Check");
    ///     return;
    /// }
    /// //else work next code
    /// </example>
    public class SingleInstance
    {
        //Уникальный идентификатор приложения - строка имени приложения вроде Meraman.Operator
		//Обычно нужно обеспечивать единственность для приложения в любой его версии.
		//Если нужно реагировать только на определенные версии приложения, то надо в строку добавить еще номер версии.
        //Если нужно, чтобы приложение работало для разных пользователей одновременно, надо в строку добавить имя пользователя.
		
		
		/// <summary>
        /// Уникальный идентификатор для приложения
        /// </summary>
        private String m_uid;
		/// <summary>
        /// Уникальный идентификатор для приложения
        /// </summary>
        public String Uid
        {
            get { return m_uid; }
            set { m_uid = value; }
        }

        /// <summary>
        /// Mutex internal object
        /// </summary>
        private Mutex m_Mutex;

        /// <summary>
        /// NT-Конструктор
        /// </summary>
        /// <param name="uid">Уникальный идентификатор для приложения</param>
        public SingleInstance(string uid)
        {
            this.m_uid = uid;
            //if mutex not exists create it
            //else throw exception
            if (this.Exists(uid) == false)
                m_Mutex = new Mutex(false, uid);//мутекс создаем не принадлежащим никакому потоку, так как при закрытии он почему-то не хочет освобождаться и выбрасывает исключение.
            else throw new ApplicationException();
            //тут могут быть и другие исключения, они передаются вызывающему коду, 
            //показывая, что мутекс существует, хотя и недоступен приложению.
        }

        /// <summary>
        /// NR-Деструктор
        /// </summary>
        /// <remarks>
        /// Срабатывает, когда объект уничтожается. 
        /// Благодаря ему, не надо специально вызывать освобождение мутекса в основном коде программы.
        /// </remarks>
        ~SingleInstance()
        {
            if (m_Mutex != null)
            {
                //m_Mutex.ReleaseMutex();//выбрасывает исключение, если мутекс не принадлежит текущему потоку. 
                m_Mutex.Close();
            }
        }
        /// <summary>
        /// NT-Единственная функция проверки единственности 
        /// </summary>
        /// <param name="uid">Уникальный идентификатор для приложения, как имя мутекса</param>
        /// <returns>Если мутекс создан успешно, то возвращается ссылка на объект.
        /// Если мутекс уже существует или не может быть создан, возвращается null.
        /// Тогда приложение нужно закрыть, так как и мутекс не создан, и другая копия уже, вероятно, запущена.
        /// </returns>
        public static SingleInstance Create(String uid)
        {
            SingleInstance inst = null;
            try
            {
                inst = new SingleInstance(uid);
            }
            catch (Exception ex)
            {
                ;
            }
            return inst;
        }

        /// <summary>
        /// NT-Проверить существование мутекса по имени и получить его если существует.
        /// </summary>
        /// <returns>Возвращается true если мутекс существует, иначе false </returns>
        private bool Exists(String mutexName)
        {
            bool result = false;
            try
            {
                Mutex mm = Mutex.OpenExisting(mutexName);
                result = true;
                mm.Close();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                result = false;
            }

            // There are three cases: (1) The mutex does not exist.
            // (2) The mutex exists, but the current user doesn'code 
            // have access. (3) The mutex exists and the user has
            // access.
            //

            return result;
        }

        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string m;
            if(m_Mutex == null) m = "null";
            else m = m_Mutex.Handle.ToString();
            return String.Format("{0}({1})", m_uid, m);
        }

    }
}
