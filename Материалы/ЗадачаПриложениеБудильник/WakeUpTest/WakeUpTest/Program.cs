using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;
using System.Media;

namespace WakeUpTest
{
    //Тест успешно работает, как и ожидалось, в качестве будильника.
    //Но при пробуждении приложения перекрыты экраном логина (авторизации)
    //Хотя пароля и нет, но кликнуть по аватару придется.
    //Приложения работают, но яхз насколько это стабильно.

    //Код работает в качестве будильника, но таймер не сбрасывается при пробуждении вручную до истечения интервала.
    //Поэтому программа работает даже так: Вкл - установка таймера - засыпание - пробуждение пользователем - ждущий режим пользователем - пробуждение по таймеру - звуковой сигнал.
    //Код просто ожидает завершения таймера, неважно, включен комп или спит в это время.
 
    //Надо еще выяснить, что если два таймера выставлены одновременно - они оба обслуживаются?
    //И даже с пробуждением-засыпанием между ними?
    //А они оба срабатывают одновременно. Только не знаю - по раннему или по позднему таймеру.
    //По мсдн - по позднему таймеру. В таймере есть возможность поставить обработчик завершения, но он в Шарпе не получится.
    //Это именной таймер, вот по имени они и различаются. По имени таймер можно найти и остановить или уничтожить или переустановить.
    //Имя таймера должно быть уникальным среди всех объектов ядра:  мутексов, пип итд.

    //И как тогда сделать нормальный режим работы?
    //Это просто поток засыпает на таймере, хоть в работе, хоть во сне.
    //А что же делать? Если поток пробуждается по таймеру, то будильник играет звуковой сигнал и работает дальше.
    //А если комп проснулся раньше таймера, то что? Можно убить ожидающий поток и таймер тоже, и перед засыпанием компа создать новый.
    //Но как отследить что комп проснулся после засыпания? Нужно получить это событие пробуждения, чтобы выключить ждущий таймер и его поток.
    //Можно по переменной-флагу - установить перед засыпанием компа. 
    //А от вызова функции до собственно засыпания компа проходит какие-то время, 
    //и вот тут надо либо ждать время, либо ловить событие пробуждения компа и по нему мочить таймер будильника.
    //И это все рассчитано на то, что не будет двух программ одновременно, или же это не существенно?
    class Program
    {

        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        static void Main(string[] args)
        {
            SetWaitForWakeUpTime();
        }

        static void SetWaitForWakeUpTime()
        {
            Console.WriteLine("Current time is {0}", DateTime.Now);
            DateTime utc = DateTime.Now.AddMinutes(5);

            long duetime = utc.ToFileTime();

            using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer"))
            {
                if (SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero, IntPtr.Zero, true))
                {
                    //sleep - test it
                    System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, false, false);
                    //wait for wakeup
                    using (EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset))
                    {
                        wh.SafeWaitHandle = handle;
                        wh.WaitOne();
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            // You could make it a recursive call here, setting it to 1 hours time or similar 
            Console.WriteLine("Wake up call");
            //Пробуем будильник из компа сделать - играем звуковой файл в каталоге рядом с приложением
            SoundPlayer player = new SoundPlayer("ST.WAV");
            player.PlayLooping();
            //тут пользователь нажимает энтер в консоли и это закрывает приложение и прекращает звуковой сигнал.
            Console.ReadLine();
        } 

        



    }
}
