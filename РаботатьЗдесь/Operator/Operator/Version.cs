using System;
using System.Collections.Generic;
using System.Text;

namespace Operator
{
    /// <summary>
    /// NT-Версия программы Оператор и ее частей
    /// </summary>
    public class OperatorVersion
    {
        /// <summary>
        /// Номер версии
        /// </summary>
        private int m_MainVersion;
        /// <summary>
        /// Номер версии
        /// </summary>
        public int MainVersion
        {
            get { return m_MainVersion; }
            set { m_MainVersion = value; }
        }
        /// <summary>
        /// Номер подверсии
        /// </summary>
        private int m_SubVersion;
        /// <summary>
        /// Номер подверсии
        /// </summary>
        public int SubVersion
        {
            get { return m_SubVersion; }
            set { m_SubVersion = value; }
        }
        /// <summary>
        /// Номер исправления
        /// </summary>
        private int m_PatchNumber;
        /// <summary>
        /// Номер исправления
        /// </summary>
        public int PatchNumber
        {
            get { return m_PatchNumber; }
            set { m_PatchNumber = value; }
        }
        /// <summary>
        /// Номер сборки
        /// </summary>
        private int m_BuildNumber;
        /// <summary>
        /// Номер сборки
        /// </summary>
        public int BuildNumber
        {
            get { return m_BuildNumber; }
            set { m_BuildNumber = value; }
        }
        

        /// <summary>
        /// NT- Default constructor
        /// </summary>
        public OperatorVersion()
        {
        }

        /// <summary>
        /// NT-Конструктор из объекта версии
        /// </summary>
        public OperatorVersion(Version ver)
        {
            this.m_BuildNumber = ver.Build;
            this.m_MainVersion = ver.Major;
            this.m_PatchNumber = ver.Revision;
            this.m_SubVersion = ver.Minor;
            return;
        }

        /// <summary>
        /// NT-Конструктор из строки версии
        /// </summary>
        /// <param name="versionString">Строка версии, только цифры. Пример: 1.0.24.0546</param>
        public OperatorVersion(String versionString)
        {
            if (!Parse(versionString))
                throw new ArgumentException("Invalid format in: " + versionString, "versionString");
        }

        /// <summary>
        /// NT-Можно ли указанную версию считать эквивалентной текущей версии.
        /// </summary>
        /// <param name="ver"></param>
        /// <remarks>
        /// Сравниваются поля версии и подверсии. Номер патча и билда игнорируются.
        /// </remarks>
        /// <returns></returns>
        public bool IsEquivalentVersion(OperatorVersion ver)
        {
            if (m_MainVersion != ver.m_MainVersion) return false;
            if (m_SubVersion != ver.m_SubVersion) return false;
            return true;
        }
        /// <summary>
        /// NT-Можно ли указанную версию считать допустимой для работы с ней.
        /// </summary>
        /// <param name="ver"></param>
        /// <remarks>
        /// Сравниваются поля версии и подверсии. Номер патча и билда игнорируются.
        /// </remarks>
        /// <returns></returns>
        public bool IsCompatibleVersion(OperatorVersion ver)
        {
            if (m_MainVersion != ver.m_MainVersion) return false;
            if (m_SubVersion != ver.m_SubVersion) return false;
            return true;
        }
        /// <summary>
        /// NT-Парсить строку версии
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        public bool Parse(String versionString)
        {
            bool result = true;
            try
            {
                string[] sar = versionString.Split(new char[] { '.' }, StringSplitOptions.None);
                m_MainVersion = Int32.Parse(sar[0]);
                m_SubVersion = Int32.Parse(sar[1]);
                m_PatchNumber = Int32.Parse(sar[2]);
                m_BuildNumber = Int32.Parse(sar[3]);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m_MainVersion);
            sb.Append('.');
            sb.Append(m_SubVersion);
            sb.Append('.');
            sb.Append(m_PatchNumber);
            sb.Append('.');
            sb.Append(m_BuildNumber);

            return sb.ToString();
        }

    }
}
