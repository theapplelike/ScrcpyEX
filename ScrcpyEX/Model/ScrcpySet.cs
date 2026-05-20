using ScrcpyEX.ExtensionMethod.MyAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// Scrcpy的设置类。
    /// </summary>
    internal class ScrcpySet
    {
        /// <summary>
        /// 保持清醒，防止设备进入休眠状态（无线连接时无效）。
        /// </summary>
        //[CommandLine("--stay-awake")]
        //public bool StayAwake { get; set; } = true;

        /// <summary>
        /// 保持活跃，为防止设备因长时间未活动而自动关闭，会定期向系统发送用户活动信号。
        /// </summary>
        [CommandLine("--keep-active")]
        public bool KeepActive { get; set; } = true;

        /// <summary>
        /// 关闭屏幕，镜像时关闭设备屏幕，但不会进入休眠。
        /// </summary>
        [CommandLine("--turn-screen-off")]
        public bool TurnScreenOff { get; set; } = true;

        /// <summary>
        /// 显示触控，镜像时手指在触碰设备的地方显示接触点。
        /// </summary>
        [CommandLine("--show-touches")]
        public bool ShowTouches { get; set; } = false;

        /// <summary>
        /// 停止镜像时关闭设备屏幕，让设备进入休眠状态。
        /// </summary>
        [CommandLine("--power-off-on-close")]
        public bool PowerOffOnClose { get; set; } = false;

        /// <summary>
        /// Scrcpy运行时禁用电脑屏幕保护。
        /// </summary>
        [CommandLine("--disable-screensaver")]
        public bool DisablScreensaver { get; set; } = true;

    }
}
