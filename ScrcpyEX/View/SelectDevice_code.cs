using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.View
{
    public partial class SelectDevice//code
    {
        /// <summary>
        /// 获得设备信息。
        /// </summary>
        private async void GetDeviceInfo()
        {
            _vm.Infos.Clear();
            foreach (string device in _devices)
            {
                if (device.Contains(":") && IPAddress.TryParse(device.Split(':')[0], out _))
                {
                    //设备需要线连接上才可以获得其型号信息
                    //Android 11以下版本可以不带端口号，Android 11及以上可能需要修改这段代码
                    await MyGM.AdbAsync($"connect {device}");
                }
            }

            foreach (string device in _devices)
            {
                //await MyGM.Adb($@"-s {device} shell getprop | grep -E 'net.hostname|ro.product.name|ro.product.device|ro.product.manufacturer|ro.product.brand|ro.product.model'");
                //net.hostname有些设备没有，所以最后只保留了设备的制造商和型号

                string output = await MyGM.AdbAsync($@"-s {device} shell getprop | grep -E 'ro.product.manufacturer|ro.product.model'");
                _vm.Infos.Add(ParseDeviceInfo(output));
            }
        }

        /// <summary>
        /// 解析设备型号信息。
        /// </summary>
        /// <param name="output">从adb获得的返回内容。</param>
        /// <returns>以特定格式返回设备型号信息。</returns>
        private string ParseDeviceInfo(string output)
        {
            List<string> infos = [];
            foreach (string str in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                string[] lines = str.Split([':'], StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                {
                    infos.Add(lines[1].Replace("[", "").Replace("]", "").Trim());
                }
            }

            Debug.WriteLine($"格式化：" + string.Join("，", infos));
            return string.Join(" ", infos);
        }
    }
}
