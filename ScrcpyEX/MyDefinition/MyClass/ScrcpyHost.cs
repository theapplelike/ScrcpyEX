using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ScrcpyEX.MyDefinition.MyClass
{
    internal class ScrcpyHost : HwndHost
    {
        /// <summary>
        /// 对第三方程序的引用。
        /// </summary>
        private readonly Process _prc = new();

        /// <summary>
        /// 是否使用有线形式启动Scrcpy。
        /// </summary>
        private readonly bool _isWired;

        /// <summary>
        /// scrcpy的句柄，为IntPtr.Zero时代表scrcpy没有启动。
        /// </summary>
        private IntPtr _scHandle = IntPtr.Zero;

        public ScrcpyHost(bool isWired)
        {
            _isWired = isWired;
        }

        /// <summary>
        /// 当 HwndHost 被添加到可视化树时自动调用，创建并返回要嵌入的 Win32 窗口句柄。
        /// </summary>
        /// <param name="hwndParent">父窗口的句柄。</param>
        /// <returns>要创建的子 Win32 窗口的句柄。</returns>
        /// <exception cref="InvalidOperationException">Scrcpy没有启动。</exception>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _scHandle = StartScrcpy(_isWired).GetAwaiter().GetResult();
            if (_scHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Scrcpy没有启动。");
            }

            // 修改窗口样式
            ModifyWindowStyle();

            // 设置父子关系
            DLL.SetParent(_scHandle, hwndParent.Handle);

            return new HandleRef(this, _scHandle);
        }

        /// <summary>
        /// 启动Scrcpy并以指定方式连接手机。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async Task<IntPtr> StartScrcpy(bool isWired)
        {
            ProcessStartInfo info = new(MV.ScrcpyPath)
            {
                Arguments = GetCommandLine(isWired),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            _prc.StartInfo = info;

            if (_prc.Start())
            {
                _prc.BeginOutputReadLine();
                _prc.BeginErrorReadLine();
                int i = 0;

                do
                {
                    await Task.Delay(500);
                    IntPtr found = FindScrcpyHandle();
                    if (found != IntPtr.Zero)
                    {
                        return found;
                    }
                    //Console.WriteLine($">>>>>>>>>>>>>>{i++}:查找程序：{_scName}，程序是否被关闭：{_prc.HasExited}。");
                    Console.WriteLine($">>>>>>>>>>>>>>{i++}:程序是否被关闭：{_prc.HasExited}。");
                }
                while (!_prc.HasExited || i < 30);
                MessageBox.Show("未能成功启动程序，请重试。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 获得Scrcpy启动时附带的命令行参数。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private string GetCommandLine(bool isWired)
        {
            StringBuilder sb = new();

            //根据用户设置获得的命令行参数
            foreach (PropertyInfo prop in typeof(MyCfg).GetProperties())
            {
                string str = GetArg(prop.Name);
                if (str != null && ConvToBool(prop.GetValue(MyCfg.Cfg)))
                {
                    sb.Append($"{str} ");
                }
                //Console.WriteLine($"属性{prop.Name}：{prop.GetValue(MyCfg.Cfg)}，{GetArg(prop.Name)}。");
            }

            //键盘使用UHID可以输入中文，对设备输入中文时电脑输入法必须为英文
            //_scName = $"{DateTime.Now:HHmmss}mook";
            //sb.Append($"--keyboard=uhid --window-title={_scName} ");
            sb.Append($"--keyboard=uhid ");

            if (isWired)
            {
                sb.Append("--select-usb");
            }
            else
            {
                sb.Append($"--tcpip={MyCfg.Cfg.IP}:{MyCfg.Cfg.Port}");
            }

            //Console.WriteLine($"命令行：{sb}。");
            return sb.ToString();
        }

        /// <summary>
        /// 获得指定属性（Property）的特性（Attribute），如果没有特性（Attribute）则返回null。
        /// </summary>
        /// <param name="Prop">属性（Property）的名称。</param>
        private string GetArg(string Prop)
        {
            var info = typeof(MyCfg).GetProperty(Prop).GetCustomAttribute<CommandLineAttribute>();
            return info?.Arguments ?? null;
        }

        /// <summary>
        /// 判断传入的对象是否可以转化为bool，不能返回false，如果可以返回其bool值。
        /// </summary>
        private bool ConvToBool(object val) => val is bool result && result;

        /// <summary>
        /// 查找scrcpy的句柄。
        /// </summary>
        /// <returns>scrcpy的句柄，未找到返回IntPtr.Zero。</returns>
        private IntPtr FindScrcpyHandle()
        {
            // 查询指定名称的进程（无需包含.exe扩展名）
            Process[] processes = Process.GetProcessesByName("scrcpy");
            //Console.WriteLine($"进程的数量：{processes.Length}。");
            if (processes.Length > 0 && processes[0].MainWindowHandle != IntPtr.Zero)
            {
                return processes[0].MainWindowHandle;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 修改Scrcpy窗体样式。
        /// </summary>
        private void ModifyWindowStyle()
        {
            int GWL_STYLE = -16;//检索窗口的样式
            int WS_VISIBLE = 0x10000000;//表示窗口可见的标志
            int WS_CHILD = 0x40000000;//表示子窗口的标志，使窗口成为父窗口的子项、移除窗口的标题栏和边框、禁止窗口独立存在

            // 修改窗口样式
            DLL.SetWindowLong(_scHandle, GWL_STYLE, WS_VISIBLE | WS_CHILD);
        }

    }
}
