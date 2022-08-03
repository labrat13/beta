using System;
using System.Collections.Generic;
using System.Text;

namespace Operator.Engine
{
    /// <summary>
    /// Коды событий для канала событий питания и сеанса пользователя, Движка.
    /// </summary>
    public enum SystemStateCode
    {
        /// <summary>
        /// Значение ошибки
        /// </summary>
        None = 0,
        /// <summary>
        /// Подключено сетевое питание
        /// </summary>
        WallPowerOn,
        /// <summary>
        /// Отключено сетевое питание
        /// </summary>
        WallPowerOff,
        /// <summary>
        /// Батарея начала заряжаться
        /// </summary>
        BatteryChargingOn,
        /// <summary>
        /// Батарея перестала заряжаться
        /// </summary>
        BatteryChargingOff,
        /// <summary>
        /// Пользователь завершает работу выходом из сеанса, но компьютер не выключен.
        /// </summary>
        UserLogoff,
        /// <summary>
        /// Пользователь желает выключить или перезагрузить компьютер.
        /// </summary>
        SystemShutdown,
        /// <summary>
        /// Пользователь временно вышел, сеанс заблокирован окном приветствия.
        /// </summary>
        UserSessionLocked,
        /// <summary>
        /// Пользователь вернулся, сеанс разблокирован
        /// </summary>
        UserSessionUnlocked,
        /// <summary>
        /// Часы операционной системы изменены.
        /// </summary>
        /// <remarks>
        /// Это событие возникает тогда, когда:
        /// - пользователь изменяет системное время вручную.
        /// - компьютер вышел из спящего режима и обновляет системное время из данных БИОС.
        /// </remarks>
        ClockTimeChanged,
        /// <summary>
        /// Компьютер готовится заснуть
        /// </summary>
        SystemSuspend,
        /// <summary>
        /// Компьютер пробудился после засыпания
        /// </summary>
        SystemResume,
        /// <summary>
        /// Высокий заряд батареи
        /// </summary>
        BatteryStatusHigh,
        /// <summary>
        /// Низкий заряд батареи
        /// </summary>
        BatteryStatusLow,
        /// <summary>
        /// Критически низкий заряд батареи
        /// </summary>
        BatteryStatusCritical,
        /// <summary>
        /// Состояние батареи неизвестно
        /// </summary>
        BatteryStatusUnknown,
        /// <summary>
        /// Компьютер не имеет батареи
        /// </summary>
        /// <remarks>TODO: надо придумать, что делать с этим значением</remarks>
        BatteryStatusNoBattery,
    }
}
