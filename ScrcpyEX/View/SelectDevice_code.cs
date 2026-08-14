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
        private async Task LoadDeviceInfo()
        {
            _vm.Infos.Clear();
            if (_isWired)
            {
                await LoadWired();
            }
            else
            {
                await LoadWireless();
            }
        }

        /// <summary>
        /// 通过 序列号 获取设备的制造商和型号。
        /// </summary>
        private async Task LoadWired()
        {
            foreach (string dev in _devices)
            {
                string output = await MyGM.AdbAsync($@"-s {dev} shell getprop | grep -E 'ro.product.manufacturer|ro.product.model'");
                _vm.Infos.Add(ParseDeviceInfo(output));
            }
        }

        /// <summary>
        /// 通过 IP地址:端口号 连接设备获取其制造商和型号。
        /// </summary>
        private async Task LoadWireless()
        {
            foreach (string dev in _devices)
            {
                if (IPAddress.TryParse(dev, out _))
                {
                    //设备需要无线连接上才可以获得其型号信息
                    await MyGM.AdbAsync($"connect {dev}:5555");

                    //await MyGM.Adb($@"-s {device} shell getprop | grep -E 'net.hostname|ro.product.name|ro.product.device|ro.product.manufacturer|ro.product.brand|ro.product.model'");
                    //net.hostname有些设备没有，所以最后只保留了设备的制造商和型号

                    string output = await MyGM.AdbAsync($@"-s {dev} shell getprop | grep -E 'ro.product.manufacturer|ro.product.model'");
                    _vm.Infos.Add(ParseDeviceInfo(output));
                }
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
            foreach (string str in output.SplitReal(['\r', '\n']))
            {
                string[] lines = str.SplitReal([':']);
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
