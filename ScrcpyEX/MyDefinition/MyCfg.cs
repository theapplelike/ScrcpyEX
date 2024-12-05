using Newtonsoft.Json;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ExtensionMethod
{
    internal class MyCfg
    {
        //配置在程序中的静态实例
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
        public string IP { get; set; }

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
        /// 保持清醒，防止设备进入休眠状态（无线连接时无效）。
        /// </summary>
        [Arguments("--stay-awake")]
        public bool StayAwake { get; set; }

        /// <summary>
        /// 关闭屏幕，镜像时关闭设备屏幕。
        /// </summary>
        [Arguments("--turn-screen-off")]
        public bool OffScreen { get; set; }

        /// <summary>
        /// 显示触控，镜像时手指在触碰设备的地方显示接触点。
        /// </summary>
        [Arguments("--show-touches")]
        public bool ShowTouches { get; set; }

        /// <summary>
        /// 停止镜像时关闭设备屏幕，让设备进入休眠状态。
        /// </summary>
        [Arguments("--power-off-on-close")]
        public bool DeviceSleep { get; set; }


    }
}
