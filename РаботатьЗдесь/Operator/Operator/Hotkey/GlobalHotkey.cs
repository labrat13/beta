using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Operator.Hotkey
{
    /// <summary>
    /// Обертка для глобальных хоткеев.
    /// </summary>
    /// <remarks>
    /// Глобальный хоткей это комбинация клавиш, которая регистрируется 
    /// для существующего окна приложения и позволяет вызвать его код в любой момент времени.
    /// Обычно для показа окна пользователю, но можно и другие действия назначить.
    /// Хоткей можно установить только для собственного окна текущего потока приложения. 
    /// </remarks>
    public class GlobalHotkey: IDisposable
    {

        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32", SetLastError = true)]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);
        /// <summary>
        /// Модификатор клавиша Alt
        /// </summary>
        public const int MOD_ALT = 1;
        /// <summary>
        /// Модификатор клавиша Control
        /// </summary>
        public const int MOD_CONTROL = 2;
        /// <summary>
        /// Модификатор клавиша Shift
        /// </summary>
        public const int MOD_SHIFT = 4;
        /// <summary>
        /// Модификатор клавиша Windows
        /// </summary>
        public const int MOD_WIN = 8;
        /// <summary>
        /// Идентификатор сообщения виндовс
        /// </summary>
        public const int WM_HOTKEY = 0x312;
        /// <summary>
        /// Описатель окна
        /// </summary>
        private IntPtr m_WindowHandle;
        /// <summary>
        /// Идентификатор хоткея в сообщении для окна
        /// </summary>
        private int m_HotkeyId;



        /// <summary>
        /// Default constructor.
        /// Use Register() to register this new hotkey
        /// </summary>
        public GlobalHotkey()
        {
        }

        /// <summary>
        /// Получить идентификатор хоткея в сообщении для окна
        /// </summary>
        public int HotkeyId
        {
            get { return m_HotkeyId; }
            //set { m_HotkeyId = value; }
        }

        /// <summary>
        /// Param constructor
        /// Throw Exception if hotkey registration fail.
        /// </summary>
        /// <param name="form">Current form object</param>
        /// <param name="hotkeyId">Identifier for new hotkey. Must be any value between 0 and 0xBFFF.</param>
        /// <param name="modifiers">Key modifier like MOD_CONTROL | MOD_WIN</param>
        /// <param name="key">Single virtual key code, like Keys.K</param>
        public GlobalHotkey(Form form, int hotkeyId, int modifiers, Keys key)
        {
            if (!this.Register(form, hotkeyId, modifiers, key ))
                throw new Exception(String.Format("Unable to register hotkey {0}. Error: {1}", this.ToString(), Marshal.GetLastWin32Error().ToString()));
        }
        /// <summary>
        /// Register global hotkey
        /// Return False if hotkey registration fail.
        /// </summary>
        /// <param name="form">Current form object</param>
        /// <param name="hotkeyId">Identifier for new hotkey. Must be any value between 0 and 0xBFFF.</param>
        /// <param name="modifiers">Key modifier like MOD_CONTROL | MOD_WIN</param>
        /// <param name="key">Single virtual key code, like Keys.K</param>
        /// <returns>Return False if hotkey registration fail.</returns>
        public bool Register(Form form, int hotkeyId, int modifiers, Keys key )
        {
            bool result = RegisterHotKey(form.Handle, hotkeyId, (uint)modifiers, (uint)key);
            this.m_HotkeyId = hotkeyId;
            this.m_WindowHandle = form.Handle;

            return result;
        }
        /// <summary>
        /// Unregister current hotkey
        /// </summary>
        /// <returns>Returns false if hotkey releasing fail.</returns>
        public bool Unregister()
        {
            int result = 0;
            if (this.m_HotkeyId != 0)
            {
                result = UnregisterHotKey(this.m_WindowHandle, this.m_HotkeyId);
                //If the function succeeds, the return value is nonzero.
                if (result != 0)
                {
                    this.m_HotkeyId = 0;
                }
            }
            //используем m_HotkeyId как признак что хоткей освобожден или и не был установлен.
            return (this.m_HotkeyId == 0);
        }

        public override string ToString()
        {
            return String.Format("HotkeyId={0}", this.m_HotkeyId); 
        }
        /// <summary>
        /// Check that Message is current hotkey message
        /// </summary>
        /// <param name="msg">Window Message object</param>
        /// <returns>Returns True if this hotkey activated. Returns False otherwise/</returns>
        public bool ThisHotkey(Message msg)
        {
            if (msg.Msg != GlobalHotkey.WM_HOTKEY)
                return false;
            if ((short)msg.WParam != this.m_HotkeyId)
                return false;
            return true;
        }
        /// <summary>
        /// Check that Message is current hotkey message
        /// </summary>
        /// <param name="msg">Window Message object</param>
        /// <param name="gh">Global hotkey object</param>
        /// <returns>Returns True if this hotkey activated. Returns False otherwise/</returns>
        public static bool ThisHotkey(Message msg, GlobalHotkey gh)
        {
            if (msg.Msg != GlobalHotkey.WM_HOTKEY)
                return false;
            if ((short)msg.WParam != gh.m_HotkeyId)
                return false;
            return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Unregister();
        }

        #endregion
    }
}

//пример использования класса и его функций:
//два хоткея, которые разворачивают и сворачивают форму в трей
//Замечание: два приложения не могут зарегистрировать один и тот же хоткей.
//Поэтому это еще и требует только одной копии запущенного приложения

//using System;
//using System.Windows.Forms;

//namespace WindowsFormsApplication1
//{
//    public partial class Form1 : Form
//    {
//        GlobalHotkey hotkeyCTRL_K;
//        GlobalHotkey hotkeyCTRL_L;
        
//        public Form1()
//        {
//            InitializeComponent();
//            // Assigns the hotkey CTRL+K and CTRL+L
//            // 42 is a random identifier(and can be any thing you wish) of the hot key. 
//            // No other hot key in the calling thread should have the same identifier.
//            // An application must specify a value in the range 0x0000 through 0xBFFF

//            hotkeyCTRL_K = new GlobalHotkey(this, 42, GlobalHotkey.MOD_CONTROL, Keys.K);//создание через конструктор
//            hotkeyCTRL_L = new GlobalHotkey(); //создание и отдельная регистрация хоткея
//            hotkeyCTRL_L.Register(this, 43, GlobalHotkey.MOD_CONTROL, Keys.L);
//            return;
//        }

//        private void Form1_Load(object sender, EventArgs e)
//        {        
//        }

//        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
//        {
//            //unregister hotkeys    
//            this.hotkeyCTRL_K.Unregister();//разрегистрировать хоткей
//            this.hotkeyCTRL_L.Unregister();
//            return;
//        }

//        protected override void WndProc(ref Message m)
//        {
//            base.WndProc(ref m);
//            //разные сообщения окну приходят раньше, чем создаются объекты хоткеев.
//            //Поэтому тут надо отфильтровывать сообщения хоткеев, которые приходят уже после создания окна.
//            //если хоткеев или классов сообщений много, то лучше использовать свитч, чтобы сократить цепочку проверок:
//            //if (m.Msg == GlobalHotkey.WM_HOTKEY)
//            //{
//            //}
//            //а если нет, то так нагляднее:
//            if (GlobalHotkey.ThisHotkey(m, this.hotkeyCTRL_K) == true)
//            {
//                //тут выполняем работу хоткея
//                //обычно надо развернуть и показать окно приложения.
//                //окно может быть свернуто в таскбар или в трей, так что надо его еще и развернуть

//                //если окно было свернуто на таскбар, развернуть его в запомненное состояние
//                //но надо сначала запомнить его было, так что это не та функция.
//                if (this.WindowState == FormWindowState.Minimized)
//                    this.WindowState = FormWindowState.Normal;
//                if (!this.Visible)
//                    this.Visible = true;
//                this.Activate();
//            }
//            else if (GlobalHotkey.ThisHotkey(m, this.hotkeyCTRL_L) == true)
//            {
//                //для CTRL+L развернуть окно на весь экран или наоборот свернуть в таскбар, если оно уже развернуто.
//                if (this.WindowState == FormWindowState.Minimized)
//                {
//                    this.WindowState = FormWindowState.Maximized;
//                    this.Visible = true;
//                    this.Activate();
//                }
//                else
//                    this.WindowState = FormWindowState.Minimized;

//            }
//            return;
//         }

//    }
//}
