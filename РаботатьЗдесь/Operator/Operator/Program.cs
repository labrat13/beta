using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Operator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SingleInstance si = SingleInstance.Create("Meraman.Operator");
            if (si == null)
            {
                MessageBox.Show("Обнаружено, что приложение Оператор уже работает.\nНовый экземпляр приложения Оператор будет закрыт.", "Оператор", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //TODO: тут можно вставить код, показывающий на экран первое приложение. 
                //Чтобы пользователь сразу же получал доступ к окну Оператора, раз уж он ему потребовался, а не разворачивал его вручную. 
                return;
            }
            //else work next code
            //показывать пользователю ошибки из других потоков
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //собственно показать окно приложения
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            return;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), "Operator - Unhandled domain exception", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            Application.Exit();
            return;
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Operator - Unhandled thread exception", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            Application.Exit();
            return;
        }

        
    }
}
