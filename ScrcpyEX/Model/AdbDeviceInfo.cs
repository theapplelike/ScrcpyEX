using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// ADB连接设备的信息。
    /// </summary>
    internal class AdbDeviceInfo
    {
        /// <summary>
        /// 设备的制造商。
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 设备的型号。
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 设备的 序列号 或 IP地址:端口号。
        /// </summary>
        public string Sign { get; set; }


    }
}
