using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义的枚举。
    /// </summary>
    internal class MyEnum
    {
        /// <summary>
        /// ADB设备的类型。
        /// </summary>
        public enum AdbDeviceType
        {
            /// <summary>
            /// 返回所有设备。
            /// </summary>
            All,

            /// <summary>
            /// 仅 USB 有线连接。
            /// </summary>
            Usb,

            /// <summary>
            /// 仅 TCP/IP 无线连接。
            /// </summary>
            Tcp,

            /// <summary>
            /// 仅 Android 模拟器。
            /// </summary>
            Emulator
        }


    }
}
