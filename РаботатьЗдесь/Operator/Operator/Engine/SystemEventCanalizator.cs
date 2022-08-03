using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace Operator.Engine
{
    /// <summary>
    /// NT-Собирает системные события питания и сеанса пользователя в канал движка.
    /// Выполняет переработку и адаптацию событий.
    /// </summary>
    /// <remarks>
    /// Содержит часть функционала главного окна Оператор, разгружает его от функций и полей 
    /// Должен создаваться после создания объекта движка.
    /// Должен инициализироваться после инициализации объекта движка, но до начала его работы.
    /// Должен завершаться перед завершением объекта движка.
    /// </remarks>
    internal class SystemEventCanalizator
    {
        /// <summary>
        /// Ссылка на объект движка
        /// </summary>
        private OperatorEngine m_engine;
        //event handler's - чтобы их отцепить в конце работы. Обычно их отцепляет ОС как-нибудь, уничтожая все вместе с процессом.
        //Но именно для них указано, чтобы отцеплял клиент сам.
        internal PowerModeChangedEventHandler m_pmceh;
        internal EventHandler m_tceh;
        internal SessionEndingEventHandler m_seeh;
        internal SessionSwitchEventHandler m_sseh;

        private bool m_charging;
        private PowerLineStatus m_wallpowered;
        private BatteryChargeStatus m_batStatus;
        private PowerModes m_powermode;
        ///<summary>
        ///NT-конструктор с параметрами
        ///</summary> 
        /// <param name="engine">Ссылка на объект движка</param>
        public SystemEventCanalizator(OperatorEngine engine)
        {
            this.m_engine = engine;

            //предустановки флагов состояния питания - надо делать правильные, чтобы события обозначали состояния и не пропускались в первый раз.
            this.m_batStatus = BatteryChargeStatus.Unknown;//чтобы статус батареи показывался в первый раз.
            this.m_charging = false;//чтобы если зарядка идет.это событие показывалось в первый раз.
            this.m_powermode = PowerModes.StatusChange;//чтобы и спать и пробуждение показывались в первый раз. 
            this.m_wallpowered = PowerLineStatus.Unknown;//Чтобы и он и офф показывались в первый раз
        }


        /// <summary>
        /// NT-Установить ловушки системных событий
        /// </summary>
        /// <remarks>
        /// Делаем это не в конструкторе, так как события пойдут сразу после создания ловушек.
        /// И надо чтобы код Оператора уже был к ним готов.
        /// </remarks>
        public void initSystemEvents()
        {
            m_pmceh = new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged); 
            SystemEvents.PowerModeChanged += m_pmceh;
            m_tceh = new EventHandler(SystemEvents_TimeChanged);
            SystemEvents.TimeChanged += m_tceh;
            m_seeh = new SessionEndingEventHandler(SystemEvents_SessionEnding);
            SystemEvents.SessionEnding += m_seeh;
            //SystemEvents.SessionEnded - не используется, так как приложение ГУИ уже закрыто в это время.
            m_sseh = new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.SessionSwitch += m_sseh;
            return;
        }

        /// <summary>
        /// NT-Освободить ловушки системных событий
        /// </summary>
        public void purgeSystemEvents()
        {
            //А как тут отцеплять обработчики? в примерах мсдн ничего про это нет
            SystemEvents.PowerModeChanged -= m_pmceh;
            SystemEvents.TimeChanged -= m_tceh;
            SystemEvents.SessionEnding -= m_seeh;
            //SystemEvents.SessionEnded - не используется, так как приложение ГУИ уже закрыто в это время.
            SystemEvents.SessionSwitch -= m_sseh;

            return;
        }
        /// <summary>
        /// NR-Функция-заглушка для канала системных событий движка
        /// </summary>
        /// <param name="code"></param>
        private void sendSystemEvents(SystemStateCode code)
        {
            this.m_engine.СобытиеПитанияИСеанса(code);
        }
        /// <summary>
        /// NT-События блокировки сеанса пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            //TODO: тут не все события заведены в канал - только те что мною наблюдались.
            //остальные события тоже могут возникать и их возможно, полезно обрабатывать.
            //но сейчас их не выявлено
            if (e.Reason == SessionSwitchReason.SessionLock)
                sendSystemEvents(SystemStateCode.UserSessionLocked);
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
                sendSystemEvents(SystemStateCode.UserSessionUnlocked);
            return;
        }
        /// <summary>
        /// NT-событие завершения сеанса пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            //TODO: тут можно отменить завершение сеанса пользователя
            //Но оператор не может этого сделать, так как это можно сделать только отсюда.
            //Хотя, он может вернуть значение после вызова канала. Теоретически, это возможно реализовать.
            //Но практически полезность этого пока неизвестна.
            //- Или можно заранее передать эти параметры(Случаи отмены) в форму, и она будет их отдавать в этих случаях.
            //Но это требует понимания архитектуры и точно не является красивым решением.
            if (e.Reason == SessionEndReasons.Logoff)
                sendSystemEvents(SystemStateCode.UserLogoff);
            else if (e.Reason == SessionEndReasons.SystemShutdown)
                sendSystemEvents(SystemStateCode.SystemShutdown);
            return;
        }
        /// <summary>
        /// NT-отослать события изменения времени
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            sendSystemEvents(SystemStateCode.ClockTimeChanged);
        }


        /// <summary>
        /// NT-распаковать и отослать события питания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            //StatusChange не посылаем в канал событий
            PowerModes pm = e.Mode;
            if (pm != PowerModes.StatusChange)
            {
                //detect changes and send new state
                if (pm != m_powermode)
                {
                    m_powermode = pm;
                    //send new power mode event
                    if (pm == PowerModes.Resume)
                        sendSystemEvents(SystemStateCode.SystemResume);
                    else if (pm == PowerModes.Suspend)
                        sendSystemEvents(SystemStateCode.SystemSuspend);
                }
            }
            //и посылаем изменения других значений, если есть
            PowerStatus ps = SystemInformation.PowerStatus;
            //wall power
            if (ps.PowerLineStatus != m_wallpowered)
            {
                m_wallpowered = ps.PowerLineStatus;
                //send new state event
                if (m_wallpowered == PowerLineStatus.Offline)
                    sendSystemEvents(SystemStateCode.WallPowerOff);
                else if (m_wallpowered == PowerLineStatus.Online)
                    sendSystemEvents(SystemStateCode.WallPowerOn);
            }
            //battery charging status
            bool charging = ((ps.BatteryChargeStatus & BatteryChargeStatus.Charging) > 0);
            if (this.m_charging != charging)
            {
                m_charging = charging;
                //send new state event
                if (charging == true)
                    sendSystemEvents(SystemStateCode.BatteryChargingOn);
                else sendSystemEvents(SystemStateCode.BatteryChargingOff);
            }
            //battery state 
            BatteryChargeStatus bcs = (ps.BatteryChargeStatus & (~BatteryChargeStatus.Charging)); //TODO: как удалить флаг зарядки из состояния батареи?
            if (this.m_batStatus != bcs)
            {
                m_batStatus = bcs;
                //send new state event
                if (bcs == BatteryChargeStatus.High)
                    sendSystemEvents(SystemStateCode.BatteryStatusHigh);
                else if (bcs == BatteryChargeStatus.Low)
                    sendSystemEvents(SystemStateCode.BatteryStatusLow);
                else if (bcs == BatteryChargeStatus.Critical)
                    sendSystemEvents(SystemStateCode.BatteryStatusCritical);
                else if (bcs == BatteryChargeStatus.NoSystemBattery)
                    sendSystemEvents(SystemStateCode.BatteryStatusNoBattery);
                else if (bcs == BatteryChargeStatus.Unknown)
                    sendSystemEvents(SystemStateCode.BatteryStatusUnknown);
            }

            return;
        }


    }
}
