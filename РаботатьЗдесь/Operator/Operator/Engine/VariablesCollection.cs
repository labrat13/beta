using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Xml;

namespace Operator.Engine
{
    //пример применения класса приведен в конце файла
    
    /// <summary>
    /// NT - Кросс-поточная коллекция переменных сеанса 
    /// </summary>
    /// <remarks>
    /// Здесь должны храниться временные переменные сеанса приложения.
    /// Переменные имеют имя и могут иметь значение, кроме null. Так как null не парсится обратно из строкового значения.
    /// Переменные извлекаются из коллекции по их названию - ключу.
    /// Предполагается, что само приложение знает тип каждой переменной. Хотя все значения хранятся как строки и могут использоваться как строки.
    /// Коллекция потокобезопасная.
    /// - Это не проверялось!
    /// Не следует править файл данных вручную.
    /// 
    /// Можно не использовать файл данных, а хранить переменные только на время сеанса.
    /// 
    /// Объект класса содержит объект CultureInfo(ru-RU), который можно использовать для конверсий данных в других частях приложения.
    /// Разумеется, пока объект класса существует. Но поэтому и не стоит этим увлекаться.
    /// 
    /// В ХМЛ обычно проблема с сохранением строки в том же виде, что и исходная.
    /// Это надо тестировать отдельно
    /// -Двойные пробелы сохраняются.
    /// -переводы строк \r\n сохраняются правильно.
    /// -русские буквы сохраняются\
    /// -<>[]". сохраняются
    /// -остальные случаи не проверялись пока.
    /// 
    /// </remarks>
    public class VariablesCollection
    {
        //TODO: документировать выбрасываемые исключения для функций класса

        /// <summary>
        /// Словарь для хранения переменных
        /// </summary>
        private Dictionary<String, String> m_Dictionary;

        /// <summary>
        /// Кэш информации о культуре, только для чтения.
        /// </summary>
        private CultureInfo m_Culture;


        /// <summary>
        /// RT-Конструктор
        /// </summary>
        public VariablesCollection()
        {
            this.m_Dictionary = new Dictionary<string, string>();
            this.m_Culture = new CultureInfo("ru-RU");
        }

        /// <summary>
        /// Получить кэш информации о культуре, только для чтения.
        /// </summary>
        public CultureInfo Culture
        {
            get { return m_Culture; }
        }

        /// <summary>
        /// RT-Загрузить данные из файла
        /// </summary>
        /// <param name="filename">Путь ХМЛ-файла с данными</param>
        public void Load(string filename)
        {
            //тут вручную распарсить хмл-файл и заполнить данными словарь

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;

            //lock dictionary
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                XmlReader reader = XmlReader.Create(filename, settings);
                //read
                reader.Read();
                reader.Read();//doc
                while (reader.Read())
                {
                    //read tag start
                    if (reader.IsStartElement())
                    {
                        String name = reader.LocalName;
                        //if this tag has empty value, add to dictionary
                        if (reader.IsEmptyElement)
                        {
                            this.m_Dictionary.Add(name, String.Empty);
                        }
                        else
                        {
                            //else read tag value string, add to dictionary
                            reader.Read();
                            String val = reader.ReadString();
                            this.m_Dictionary.Add(name, val);
                        }
                    }
                    //end of tags - not used, skipped 
                }
                reader.Close();
            }

            return;
        }

        /// <summary>
        /// RT-Сохранить данные в файл
        /// </summary>
        /// <param name="filename">Путь ХМЛ-файла с данными, будет перезаписан</param>
        public void Save(string filename)
        {
            //тут вручную записать данные в ХМЛ-файл
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.CloseOutput = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.NewLineHandling = NewLineHandling.Entitize;
            settings.OmitXmlDeclaration = false;
            settings.NewLineOnAttributes = false;
            //lock dictionary
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                XmlWriter wr = XmlWriter.Create(filename, settings);
                wr.WriteStartDocument(true);
                //write comment Created (date time)
                wr.WriteComment(String.Format("Created {0}", DateTime.Now.ToString(this.m_Culture)));
                wr.WriteStartElement("doc");
                //write values
                foreach (KeyValuePair<string, string> kvp in this.m_Dictionary)
                {
                    String va = kvp.Value;
                    if (va == null) va = "[null]";
                    wr.WriteStartElement(kvp.Key);
                    wr.WriteValue(va);
                    wr.WriteEndElement();
                    //wr.WriteElementString(kvp.Key, va);
                }
                //close file
                wr.WriteEndElement();
                wr.WriteEndDocument();
                wr.Close();
            }

            return;
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, string val)
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                if (m_Dictionary.ContainsKey(keyname))
                    m_Dictionary[keyname] = val;
                else
                    m_Dictionary.Add(keyname, val);

                return;
            }
        }
        #region Set function overrides
        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, Int32 val)
        {
            String va = val.ToString(this.m_Culture);
            this.Set(keyname, va);
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, Int64 val)
        {
            String va = val.ToString(this.m_Culture);
            this.Set(keyname, va);
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, Double val)
        {
            String va = val.ToString(this.m_Culture);
            this.Set(keyname, va);
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, float val)
        {
            String va = val.ToString(this.m_Culture);
            this.Set(keyname, va);
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, DateTime val)
        {
            String va = val.ToString(this.m_Culture);
            this.Set(keyname, va);
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, TimeSpan val)
        {
            String va = val.ToString();
            this.Set(keyname, va);
        }

        /// <summary>
        /// RT-Создать или изменить переменную
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        /// <param name="val">Новое значение переменной</param>
        public void Set(string keyname, Boolean val)
        {
            String va = val.ToString(this.m_Culture);
            this.Set(keyname, va);
        }
#endregion
        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public String Get(string keyname)
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                return m_Dictionary[keyname];
            }
        }
        #region Get function overrides
        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public Boolean GetAsBoolean(string keyname)
        {
            String s = Get(keyname);
            Boolean result = Boolean.Parse(s);
            return result;
        }

        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public Int32 GetAsInt32(string keyname)
        {
            String s = Get(keyname);
            Int32 result = Int32.Parse(s, this.m_Culture);
            return result;
        }

        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public Int64 GetAsInt64(string keyname)
        {
            String s = Get(keyname);
            Int64 result = Int64.Parse(s, m_Culture);
            return result;
        }

        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public Double GetAsDouble(string keyname)
        {
            String s = Get(keyname);
            Double result = Double.Parse(s, m_Culture);
            return result;
        }

        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public Single GetAsSingle(string keyname)
        {
            String s = Get(keyname);
            Single result = Single.Parse(s, m_Culture);
            return result;
        }

        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public DateTime GetAsDateTime(string keyname)
        {
            String s = Get(keyname);
            DateTime result = DateTime.Parse(s, m_Culture);
            return result;
        }

        /// <summary>
        /// RT-Получить значение переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public TimeSpan GetAsTimeSpan(string keyname)
        {
            String s = Get(keyname);
            TimeSpan result = TimeSpan.Parse(s);
            return result;
        }

        #endregion
        /// <summary>
        /// RT-Проверить существование переменной
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public bool Exists(string keyname)
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                return m_Dictionary.ContainsKey(keyname);
            }
        }

        /// <summary>
        /// RT-Удалить переменную из коллекции
        /// </summary>
        /// <param name="keyname">Название переменной</param>
        public void Remove(string keyname)
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                m_Dictionary.Remove(keyname);
            }
        }

        /// <summary>
        /// RT-Очистить коллекцию переменных
        /// </summary>
        public void Clear()
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                m_Dictionary.Clear();
            }
        }

        /// <summary>
        /// RT-Получить список переменных коллекции
        /// В формате "имя" = "значение"
        /// </summary>
        /// <returns></returns>
        public List<String> ListAllItems()
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                List<String> result = new List<string>();
                foreach (KeyValuePair<string, string> kvp in this.m_Dictionary)
                {
                    String va = kvp.Value;
                    if (va == null) va = "[null]";
                    result.Add(String.Format("\"{0}\" = \"{1}\"", kvp.Key, va));
                }
                return result;
            }
        }
        /// <summary>
        /// RT-Получить описание объекта для отладки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            lock (((ICollection)this.m_Dictionary).SyncRoot)
            {
                return String.Format("Count: {0}", this.m_Dictionary.Count);
            }
        }
    }
}
//    //пример применения класса

//public partial class Form1 : Form
//{
//    private VariablesCollection m_vars;

//    public Form1()
//    {
//        InitializeComponent();
//        m_vars = new VariablesCollection();
//    }

//    private void Form1_Load(object sender, EventArgs e)
//    {
//        //fill values
//        m_vars.Set("var1", true);
//        m_vars.Set("var2", DateTime.Now);
//        m_vars.Set("var3", 0.3456789);
//        m_vars.Set("var4", 0.777f);
//        m_vars.Set("var5", 4);
//        m_vars.Set("var6", ((Int64)4));
//        m_vars.Set("var7", "Мы веселые  ребята, <Мы> ребята -[\"акробаты\"].");
//        m_vars.Set("var8", TimeSpan.FromSeconds(30.0));
//        m_vars.Set("var1", false);

//        //store and load file here
//        String filenametestPath = "C:\\Temp\\colVar.xml";
//        m_vars.Save(filenametestPath);
//        m_vars.Load(filenametestPath);
//        //read values
//        bool a1 = m_vars.GetAsBoolean("var1");
//        DateTime a2 = m_vars.GetAsDateTime("var2");
//        Double a3 = m_vars.GetAsDouble("var3");
//        Single a4 = m_vars.GetAsSingle("var4");
//        Int32 a5 = m_vars.GetAsInt32("var5");
//        Int64 a6 = m_vars.GetAsInt64("var6");
//        String a7 = m_vars.Get("var7");
//        TimeSpan a8 = m_vars.GetAsTimeSpan("var8");

//    }

//    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
//    {
//        //store and load file here
//        String filenametestPath = "C:\\Temp\\colVar.xml";
//        m_vars.Save(filenametestPath);

//    }

//    private void Form1_Shown(object sender, EventArgs e)
//    {
//        //fill listbox1 with collection items
//        this.listBox1.Items.AddRange(this.m_vars.ListAllItems().ToArray());

//        return;
//    }


//}