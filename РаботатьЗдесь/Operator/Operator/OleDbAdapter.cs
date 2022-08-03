using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Globalization;


namespace Operator
{
    /*  Состояние шаблона:
	30 сентября 2017 г - начальная версия. 
	Компилируется, но не знаю, правильно ли работает.
	
	
	
	*/
	
	/* Зарезервированные слова, которые надо заключать в квадратные скобки в запросах 
	   нежелательные имена для столбцов и таблиц, в любом регистре:
		text module session 
	*/
	
	/// <summary>
    /// Класс адаптера БД
    /// </summary>
    public class DbAdapter
    {
        #region Fields
        /// <summary>
        /// Имя файла базы данных
        /// </summary>
        public const string DatabaseFileName = "db.mdb";
        /// <summary>
        /// database connection string
        /// </summary>
        public static String ConnectionString;
        /// <summary>
        /// database connection
        /// </summary>
        private System.Data.OleDb.OleDbConnection m_connection;
        /// <summary>
        /// Transaction for current connection
        /// </summary>
        private OleDbTransaction m_transaction;
        /// <summary>
        /// Database is read-only
        /// </summary>
        private bool dbReadOnly;

        //TODO: Укажите здесь все таблицы БД как строковые константы. Это упростит работу с общими функциями таблиц.
        ///// <summary>
        ///// Константа название таблицы БД - для функций адаптера
        ///// </summary>
        //internal const string TableName1 = "TableName1";

        //все объекты команд сбрасываются в нуль при отключении соединения с БД
        //TODO: Новые команды внести в ClearCommands()
        /// <summary>
        /// Команда без параметров, используемая во множестве функций
        /// </summary>
        private OleDbCommand m_cmdWithoutArguments;
        //private OleDbCommand m_cmd1;
        //private OleDbCommand m_cmd2;
        //private OleDbCommand m_cmd3;
        //private OleDbCommand m_cmd4;
        //private OleDbCommand m_cmd5;
        //private OleDbCommand m_cmd6;

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        public DbAdapter()
        {
            
        }
        /// <summary>
        /// Database is read-only
        /// </summary>
        public bool ReadOnly
        {
            get { return dbReadOnly; }
            //set { dbReadOnly = value; }
        }

        #region Service functions

        /// <summary>
        /// NT-все объекты команд класса сбросить в нуль
        /// </summary>
        protected void ClearCommands()
        {
            m_cmdWithoutArguments = null;
            //m_cmd1 = null;
            //m_cmd2 = null;
            //m_cmd3 = null;
            //m_cmd4 = null;
            //m_cmd5 = null;
            //m_cmd6 = null;
            return;
        }

        /// <summary>
        /// NT-Создать и инициализировать объект адаптера БД
        /// </summary>
        /// <param name="dbFile">Путь к файлу БД</param>
        /// <param name="ReadOnly">Флаг Открыть БД только для чтения</param>
        /// <returns>Возвращает инициализированный объект адаптера БД</returns>
        public static DbAdapter SetupDbAdapter( String dbFile, bool ReadOnly)
        {
            //теперь создать новый интерфейс БД
            DbAdapter dblayer = new DbAdapter();
            dblayer.dbReadOnly = ReadOnly;
            String constr = createConnectionString(dbFile, ReadOnly);
            //connect to database
            dblayer.Connect(constr);
            return dblayer;
        }


        /// <summary>
        /// NT-Открыть соединение с БД
        /// </summary>
        /// <param name="connectionString">Строка соединения с БД</param>
        public void Connect(String connectionString)
        {
            //create connection
            OleDbConnection con = new OleDbConnection(connectionString);
            //try open connection
            con.Open();
            con.Close();
            //close existing connection
            Disconnect();
            //open new connection and set as primary
            this.m_connection = con;
            ConnectionString = connectionString;
            this.m_connection.Open();

            return;
        }


        /// <summary>
        /// NT-Закрыть соединение с БД
        /// </summary>
        public void Disconnect()
        {
            if (m_connection != null)
            {
                if (m_connection.State == System.Data.ConnectionState.Open)
                    m_connection.Close();
                m_connection = null;
            }

            //все объекты команд сбросить в нуль при отключении соединения с БД, чтобы ссылка на объект соединения при следующем подключении не оказалась устаревшей
            ClearCommands();

            return;
        }



        /// <summary>
        /// NT-Начать транзакцию. 
        /// </summary>
        public void TransactionBegin()
        {
            m_transaction = m_connection.BeginTransaction();
            //сбросить в нуль все объекты команд, чтобы они были пересозданы для новой транзакции
            ClearCommands();
        }
        //NT-Подтвердить транзакцию Нужно закрыть соединение после этого!
        public void TransactionCommit()
        {
            m_transaction.Commit();
            //сбросить в нуль все объекты команд, чтобы они были пересозданы для новой транзакции
            ClearCommands(); //надо ли сбросить m_transactions = null?
            m_transaction = null;

        }
        //NT-Отменить транзакцию. Нужно закрыть соединение после этого!
        public void TransactionRollback()
        {
            m_transaction.Rollback();
            //сбросить в нуль все объекты команд, чтобы они были пересозданы для новой транзакции
            ClearCommands();
            m_transaction = null;
        }

        /// <summary>
        /// NT-Создать строку соединения с БД
        /// </summary>
        /// <param name="dbFile">Путь к файлу БД</param>
        public static string createConnectionString(string dbFile, bool ReadOnly)
        {
            //Provider=Microsoft.Jet.OLEDB.4.0;Data Source="C:\Documents and Settings\salomatin\Мои документы\Visual Studio 2008\Projects\RadioBase\радиодетали.mdb"
            OleDbConnectionStringBuilder b = new OleDbConnectionStringBuilder();
            b.Provider = "Microsoft.Jet.OLEDB.4.0";
            b.DataSource = dbFile;
            //это только для БД на незаписываемых дисках
            if (ReadOnly)
            {
                b.Add("Mode", "Share Deny Write");
            }
            //user id and password can specify here
            return b.ConnectionString;
        }

        /// <summary>
        /// NT-Извлечь файл шаблона базы данных из ресурсов сборки
        /// </summary>
        /// <remarks>
        /// Файл БД должен быть помещен в ресурсы сборки в VisualStudio2008.
        /// Окно Свойства проекта - вкладка Ресурсы - кнопка-комбо Добавить ресурс - Добавить существующий файл. Там выбрать файл БД.
        /// При этом он помещается также в дерево файлов проекта, и при компиляции берется оттуда и помещается в сборку как двоичный массив байт.
        /// После этого можно изменять этот файл проекта, изменения в ресурс сборки будут внесены после компиляции
        /// Эта функция извлекает файл БД в указанный путь файла.
        /// </remarks>
        /// <param name="filepath">Путь к итоговому файлу *.mdb</param>
        public static void extractDbFile(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Create);
            byte[] content = Properties.Resources.db;//Укажите здесь имя ресурса - шаблона БД
            fs.Write(content, 0, content.Length);
            fs.Close();
        }

        #endregion


        #region *** Для всех таблиц ***
        /// <summary>
        /// NT- Получить максимальное значение ИД для столбца таблицы
        /// Обычно применяется для столбца первичного ключа, но можно и для других целочисленных столбцов.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <returns>Returns max value or -1 if no results</returns>
        public int getTableMaxId(string table, string column)
        {
            //SELECT MAX(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MAX({0}) FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }
        /// <summary>
        /// NT-Получить минимальное значение ИД для столбца таблицы
        /// Обычно применяется для столбца первичного ключа, но можно и для других целочисленных столбцов.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <returns>Returns min value or -1 if no results</returns>
        public int getTableMinId(string table, string column)
        {
            //SELECT MIN(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MIN({0}) FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }

        /// <summary>
        /// NT-Получить число записей в таблице
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <returns>Returns row count or -1 if no results</returns>
        public int GetRowCount(string table, string column)
        {
            //SELECT COUNT(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }

        /// <summary>
        /// NT-Получить число записей в таблице, с указанным числовым значением.
        /// </summary>
        /// <remarks>Применяется для столбца первичного ключа, проверяет что запись с этим ключом существует.
        /// Но может применяться и в других случаях.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца</param>
        /// <param name="val">Числовое значение в столбце</param>
        /// <returns>Возвращает число записей с таким значением этого столбца, или -1 при ошибке.</returns>
        public int GetRowCount(string table, string column, int val)
        {
            //SELECT column FROM table WHERE (column = value);
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 120;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) FROM {1} WHERE ({0} = {2});", column, table, val);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }

		/// <summary>
        /// NT-Удалить запись(и) из таблицы по значению поля в столбце
        /// </summary>
        /// <remarks>Удаляет все строки с указанным значением параметра.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца</param>
        /// <param name="val">Значение столбца</param>
        /// <returns>Возвращает число затронутых (удаленных) строк таблицы.</returns>
        private int DeleteRow(string table, string column, int val)
        {
            //DELETE FROM table WHERE (column = value);
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 120;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE ({0}.{1} = {2});", table, column, val);
            m_cmdWithoutArguments.CommandText = query;
            return m_cmdWithoutArguments.ExecuteNonQuery(); //Тут могут быть исключения из-за другого типа данных
        }
		
		/// <summary>
        /// NT-получить значение автоинкремента для последней измененной таблицы в текущем сеансе БД
        /// </summary>
        /// <returns></returns>
        internal int GetLastAutonumber()
        {
            //SELECT COUNT(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            m_cmdWithoutArguments.CommandText = "SELECT @@IDENTITY;";
            return (int)m_cmdWithoutArguments.ExecuteScalar();
        }
		
        //TODO: Если нужна функция очистки всей БД, раскомментируйте код и измените его, вписав правильные имена таблиц.
        ///// <summary>
        ///// NFT-Очистить БД Хранилища
        ///// </summary>
        ///// <returns>True if Success, False otherwise</returns>
        //internal bool ClearDb()
        //{
        //    bool result = false;
        //    try
        //    {
        //        this.TransactionBegin();
        //        this.ClearTable(DbAdapter.ContentTableName);
        //        this.ClearTable(DbAdapter.DocumentTableName);
        //        this.ClearTable(DbAdapter.PictureTableName);
        //        this.TransactionCommit();
        //        result = true;
        //    }
        //    catch (Exception)
        //    {
        //        this.TransactionRollback();
        //        result = false;
        //    }
        //    return result;
        //}

        /// <summary>
        /// RT-Удалить все строки из указанной таблицы.
        /// Счетчик первичного ключа не сбрасывается - его отдельно надо сбрасывать.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        public void ClearTable(string table)
        {
            //DELETE FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 600;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0};", table);
            m_cmdWithoutArguments.CommandText = query;
            m_cmdWithoutArguments.ExecuteNonQuery();

            return;
        }

        #endregion

        #region *** Для таблицы свойств ключ-значение ***
        /// <summary>
        /// NT-Получить значения свойств из таблицы БД
        /// </summary>
        /// <remarks>
        /// Это функция для таблицы Ключ-Значения. 
        /// Структура таблицы:
        /// - id counter, primary key - первичный ключ, не читается.
        /// - p text - название параметра, ключ (строка), должно быть уникальным.
        /// - d text - значение параметра, значение (строка), допускаются повторы и пустые строки.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <returns>Словарь, содержащий все пары ключ-значение из таблицы БД</returns>
        public Dictionary<String, String> ReadKeyValueDictionary(String table)
        {
            Dictionary<String, string> dict = new Dictionary<string, string>();
            //create command
            String query = String.Format("SELECT * FROM {0};", table);
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection);
            cmd.CommandTimeout = 120;
            //execute command
            OleDbDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    //int id = rdr.GetInt32(0); //id not used
                    String param = rdr.GetString(1);
                    String val = rdr.GetString(2);
                    //store to dictionary 
                    dict.Add(param, val);
                }
            }
            //close reader
            rdr.Close();

            return dict;
        }



        /// <summary>
        /// NT - Перезаписать значения свойств в таблице БД.
        /// Все записи из таблицы будут удалены и заново вставлены.
        /// </summary>
        /// <remarks>
        /// Это функция для таблицы Ключ-Значения. 
        /// Структура таблицы:
        /// - id counter, primary key - первичный ключ, не читается.
        /// - p text - название параметра, ключ (строка), должно быть уникальным.
        /// - d text - значение параметра, значение (строка), допускаются повторы и пустые строки.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="dic">Словарь, содержащий пары ключ-значение</param>
        public void StoreKeyValueDictionary(String table, Dictionary<string, string> dic)
        {
            //1 - очистить таблицу
            String query = String.Format("DELETE * FROM {0};", table);
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection);
            cmd.CommandTimeout = 120;
            cmd.ExecuteNonQuery();
            //2 - записать новые значения
            query = String.Format("INSERT INTO {0} (param, val) VALUES (?, ?);", table);
            cmd.CommandText = query;
            cmd.Parameters.Add(new OleDbParameter("@p0", OleDbType.VarWChar));
            cmd.Parameters.Add(new OleDbParameter("@p1", OleDbType.VarWChar));
            //execute commands
            foreach (KeyValuePair<String, String> kvp in dic)
            {
                cmd.Parameters[0].Value = kvp.Key;
                cmd.Parameters[1].Value = kvp.Value;
                cmd.ExecuteNonQuery();
            }

            return;
        }

        /// <summary>
        /// NT-Получить один из параметров, не загружая весь набор
        /// </summary>
        /// <remarks>
        /// Это функция для таблицы Ключ-Значения. Ыункция универсальная, поэтому надо указывать имена таблиц и столбцов. 
        /// Структура таблицы:
        /// - id counter, primary key - первичный ключ, не читается.
        /// - p text - название параметра, ключ (строка), должно быть уникальным.
        /// - d text - значение параметра, значение (строка), допускаются повторы и пустые строки.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="columnName">Название столбца ключа</param>
        /// <param name="paramName">Название параметра (ключ)</param>
        /// <returns>Возвращает строку значения параметра</returns>
        public string GetKeyValueParameter(String table, String columnName, String paramName)
        {
            //create command
            String query = String.Format("SELECT * FROM {0} WHERE ({1} = '{2}' );", table, columnName, paramName);
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection);
            cmd.CommandTimeout = 120;
            //execute command
            OleDbDataReader rdr = cmd.ExecuteReader();
            String result = String.Empty;
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    //int id = rdr.GetInt32(0); //id not used
                    //String param = rdr.GetString(1);//param not used
                    result = rdr.GetString(2);
                }
            }
            //close reader
            rdr.Close();
            return result;
        }

        #endregion


        //TODO: Добавить код новых функций здесь, каждый комплект функций для таблицы поместить в отдельный region
        //новые команды для них обязательно внести в ClearCommands(), иначе транзакции будут работать неправильно.
    }
}
