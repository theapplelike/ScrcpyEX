using Newtonsoft.Json;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.MyClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ExtensionMethod
{
    /// <summary>
    /// 自定义的配置参数类。
    /// </summary>
    internal class MyCfg
    {
        /// <summary>
        /// 全局配置类在程序中的静态实例。
        /// </summary>
        public static MyCfg Cfg { get; set; } = null;

        //全局配置保存路径
        private static readonly string _cfgPath = @".\main\Local\UserData\config.json";

        static MyCfg()
        {
            Cfg = ReadCfg();
        }

        /// <summary>
        /// 获得自定义配置，配置不存在时会创建目录和配置文件，并写入新的配置实例。
        /// </summary>
        private static MyCfg ReadCfg()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_cfgPath));
            if (!File.Exists(_cfgPath))
            {
                File.WriteAllText(_cfgPath, JsonConvert.SerializeObject(new MyCfg()));
            }

            using StreamReader reader = File.OpenText(_cfgPath);
            return JsonConvert.DeserializeObject<MyCfg>(reader.ReadToEnd());
        }

        /// <summary>
        /// 保存自定义配置，配置不存在时会创建目录和配置文件，并写入当前配置实例。
        /// </summary>
        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_cfgPath));
            File.WriteAllText(_cfgPath, JsonConvert.SerializeObject(Cfg));
        }

        /// <summary>
        /// 无线连接的IP地址。
        /// </summary>
        //public string IP { get; set; }

        /// <summary>
        /// 无线连接的端口号。
        /// </summary>
        //public string Port { get; set; } = "5555";

        /// <summary>
        /// 定位到左上角。
        /// </summary>
        public bool Locate1 { get; set; } = false;

        /// <summary>
        /// 定位到右上角。
        /// </summary>
        public bool Locate2 { get; set; } = true;

        /// <summary>
        /// 定位到左下角。
        /// </summary>
        public bool Locate3 { get; set; } = false;

        /// <summary>
        /// 定位到右下角。
        /// </summary>
        public bool Locate4 { get; set; } = false;

        /// <summary>
        /// 记录窗体尺寸、位置信息。
        /// </summary>
        public FormSize Size { get; set; } = new();

        /// <summary>
        /// 录制操作信息的集合。
        /// </summary>
        public List<RecInfo> RecInfos { get; set; } = [];

        /// <summary>
        /// 键盘映射信息类列表，用于保存键盘映射信息。
        /// </summary>
        public List<KeyMapping> KeyMappings { get; set; } = [];

        /// <summary>
        /// 记录Scrcpy相关的设置。
        /// </summary>
        public ScrcpySet ScrcpySet { get; set; } = new();

        /// <summary>
        /// 最后一次连接Scrcpy是否使用有线连接。
        /// </summary>
        public bool IsUseWiredInLast { get; set; } = true;

        /// <summary>
        /// 是否使用上一次退出时的位置和大小。
        /// </summary>
        public bool IsUseLastSize { get; set; } = true;

        /// <summary>
        /// 是否保持横屏。
        /// </summary>
        public bool IsKeepLandscape { get; set; } = false;

        /// <summary>
        /// 保持清醒，防止设备进入休眠状态（无线连接时无效）。
        /// </summary>
        //[CommandLine("--stay-awake")]
        //public bool StayAwake { get; set; }
        //
        ///// <summary>
        ///// 关闭屏幕，镜像时关闭设备屏幕，但不会进入休眠。
        ///// </summary>
        //[CommandLine("--turn-screen-off")]
        //public bool OffScreen { get; set; }
        //
        ///// <summary>
        ///// 显示触控，镜像时手指在触碰设备的地方显示接触点。
        ///// </summary>
        //[CommandLine("--show-touches")]
        //public bool ShowTouches { get; set; }
        //
        ///// <summary>
        ///// 停止镜像时关闭设备屏幕，让设备进入休眠状态。
        ///// </summary>
        //[CommandLine("--power-off-on-close")]
        //public bool DeviceSleep { get; set; }
        //
        ///// <summary>
        ///// Scrcpy运行时禁用屏幕保护。
        ///// </summary>
        //[CommandLine("--disable-screensaver")]
        //public bool Disablecreensaver { get; set; }


    }
}
