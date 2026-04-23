using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ScrcpyEX.MyDefinition.MyClass
{
    internal class ScrcpyHost
    {
        /// <summary>
        /// 对第三方程序的引用。
        /// </summary>
        private readonly Process _prc = new();

        /// <summary>
        /// 真正的HwndHost。
        /// </summary>
        private InternalHwndHost _hwndHost;

        /// <summary>
        /// 是否使用有线形式启动。
        /// </summary>
        private bool _isWired;

        /// <summary>
        /// 有线形式启动时的IP地址。
        /// </summary>
        private string _ip;

        /// <summary>
        /// 有线形式启动启动时的端口号。
        /// </summary>
        private string _port;

        /// <summary>
        /// Scrcpy的句柄，为IntPtr.Zero时代表Scrcpy没有启动。
        /// </summary>
        public IntPtr ScHandle { get; set; } = IntPtr.Zero;

        public ScrcpyHost()
        {
            SetProcess();
        }

        /// <summary>
        /// 异步启动Scrcpy。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        /// <param name="ip">有线形式启动时的IP地址。</param>
        /// <param name="port">有线形式启动启动时的端口号。</param>
        /// <returns>承载Scrcpy的HwndHost。</returns>
        public async Task<HwndHost> StartScrcpyAsync(bool isWired, string ip, string port)
        {
            _isWired = isWired;
            _ip = ip;
            _port = port;

            await StartScrcpy();
            if (ScHandle == IntPtr.Zero)
            {
                MyGM.MidPOP("Scrcpy启动失败。");
                return null;
            }

            // 创建真正的HwndHost
            return _hwndHost = new InternalHwndHost(ScHandle, _prc);
        }

        /// <summary>
        /// 启动Scrcpy并以指定方式连接手机。
        /// </summary>
        private async Task StartScrcpy()
        {
            ProcessStartInfo info = new(MV.ScrcpyPath)
            {
                Arguments = GetCommandLine(),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            _prc.StartInfo = info;
        
            if (_prc.Start())
            {
                _prc.BeginOutputReadLine();
                _prc.BeginErrorReadLine();
                int i = 0;

                do
                {
                    await Task.Delay(250);

                    try
                    {
                        if (_prc.MainWindowHandle != IntPtr.Zero)
                        {
                            ScHandle = _prc.MainWindowHandle;
                            return;
                        }
                        //if (FindScrcpyHandle())//26.4.10
                        //{
                        //    return;
                        //}
                        Debug.WriteLine($">>>>>>>>>>>>>>{++i}：尝试启动Scrcpy。");
                    }
                    catch(InvalidOperationException)
                    {
                        Debug.WriteLine($"第{i}次时启动失败，进程已退出。");
                        break;
                    }
                }
                while (!_prc.HasExited && i < 60);
            }
        }

        /// <summary>
        /// 获得Scrcpy启动时附带的命令行参数。
        /// </summary>
        private string GetCommandLine()
        {
            StringBuilder sb = new();
        
            //根据用户设置获得的命令行参数26.4.10
            //foreach (PropertyInfo prop in typeof(MyCfg).GetProperties())
            //{
            //    string str = GetArg(prop.Name);
            //    if (str != null && ConvToBool(prop.GetValue(MyCfg.Cfg)))
            //    {
            //        sb.Append($"{str} ");
            //    }
            //    Debug.WriteLine($"属性{prop.Name}：{prop.GetValue(MyCfg.Cfg)}，{GetArg(prop.Name)}。");
            //}
            foreach (PropertyInfo prop in typeof(ScrcpySet).GetProperties())
            {
                string arg = prop.GetCustomAttribute<CommandLineAttribute>().Arguments;
                if (arg != null && ConvToBool(prop.GetValue(MyCfg.Cfg.ScrcpySet)))
                {
                    sb.Append($"{arg} ");
                }
                //Debug.WriteLine($"属性{prop.Name}：{prop.GetValue(MyCfg.Cfg.ScrcpySet)}，{arg}。");
            }

            //键盘使用UHID可以输入中文，对安卓输入中文时电脑输入法必须为英文
            //_scName = $"{DateTime.Now:HHmmss}mook";
            //sb.Append($"--keyboard=uhid --window-title={_scName} ");
            sb.Append($"--keyboard=uhid ");
        
            if (_isWired)
            {
                sb.Append("--select-usb");
            }
            else
            {
                sb.Append($"--tcpip={_ip}:{_port}");
            }

            Debug.WriteLine($"ScrcpyHost命令行：{sb}。");
            return sb.ToString();
        }

        /// <summary>
        /// 获得自定义命令行特性。属性（Property）的特性（Attribute），如果没有特性（Attribute）则返回null。
        /// </summary>
        /// <param name="prop">属性（Property）的名称。</param>
        //private string GetArg(string prop)//26.4.10
        //{
        //    var info = typeof(ScrcpySet).GetProperty(prop).GetCustomAttribute<CommandLineAttribute>();
        //    return info?.Arguments ?? null;
        //}

        /// <summary>
        /// 判断传入的对象是否可以转化为bool，不能返回false，如果可以返回其bool值。
        /// </summary>
        private bool ConvToBool(object val) => val is bool result && result;

        /// <summary>
        /// 查找Scrcpy的句柄。
        /// </summary>
        /// <returns>Scrcpy的句柄，未找到返回IntPtr.Zero。</returns>
        //private bool FindScrcpyHandle()//26.4.10
        //{
        //    // 查询指定名称的进程（无需包含.exe扩展名）
        //    Process[] processes = Process.GetProcessesByName("Scrcpy");
        //    //Console.WriteLine($"进程的数量：{processes.Length}。");
        //    if (processes.Length > 0 && processes[0].MainWindowHandle != IntPtr.Zero)
        //    {
        //        ScHandle = processes[0].MainWindowHandle;
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 设置第三方程序的退出、信息读取事件。
        /// </summary>
        private void SetProcess()
        {
            _prc.EnableRaisingEvents = true;
            _prc.Exited += Prc_Exited;
            _prc.ErrorDataReceived += Prc_ErrorDataReceived;
            _prc.OutputDataReceived += Prc_OutputDataReceived;
        }

        /// <summary>
        /// 第三方程序退出时，停止读取输出流和错误流，并重置Scrcpy句柄。
        /// </summary>
        private void Prc_Exited(object sender, EventArgs e)
        {
            _prc.Exited -= Prc_Exited;
            _prc.CancelOutputRead();
            _prc.CancelErrorRead();
            ScHandle = IntPtr.Zero;

            if (KeyboardHook.HookID != IntPtr.Zero)
            {
                KeyboardHook.CancelHook();
            }
            Debug.WriteLine($"ScrcpyHost：Process的Exieted事件执行结束。");
        }

        /// <summary>
        /// 读取错误流。
        /// </summary>
        private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Debug.WriteLine($"[E]{e.Data}");
        
            if (e.Data.Contains("A popup should open on the device to request authorization."))
            {
                MyGM.MidPOP("请在设备上同意授权后再试。");
            }
            else if (e.Data.Contains("INSTALL_FAILED_NO_MATCHING_ABIS: Failed to extract native libraries, res=-113"))
            {
                MyGM.MidPOP("安装的APK文件与设备CPU架构不匹配。");
            }
        }

        /// <summary>
        /// 读取输出信息流。
        /// </summary>
        private void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Debug.WriteLine($"[I]{e.Data}");
        
            //获得当前分辨率，第一个参数是宽，第二个参数是高
            if (e.Data.Contains("Texture"))
            {
                MV.MainWin.ShowResolutionDesc(e.Data);

                //Scrcpy在横竖屏转换的时候会导致坐标错位，需要重绘窗口，并调用基类的方法
                _hwndHost?.RefreshScWindow();
            }
        }

        /// <summary>
        /// 调用后马上重绘Scrcpy窗口。
        /// </summary>
        public void RefreshScrcpy()
        {
            _hwndHost?.RefreshScWindow();
        }

        /// <summary>
        /// 释放Scrcpy的资源，用于用户关闭。
        /// </summary>
        public void DisposeScrcpy()
        {
            _prc.CloseMainWindow();
            _prc.WaitForExit();
            _prc.Dispose();
        }

        /// <summary>
        /// HwndHost中不支持异步，所以外部启动Scrcpy，内部封装Scrcpy。
        /// </summary>
        private class InternalHwndHost(IntPtr scHandle, Process prc) : HwndHost
        {
            /// <summary>
            /// 对第三方程序的引用。
            /// </summary>
            private readonly Process _prc = prc;

            /// <summary>
            /// Scrcpy的句柄，为IntPtr.Zero时代表Scrcpy没有启动。
            /// </summary>
            private readonly IntPtr _scHandle = scHandle;

            /// <summary>
            /// Scrcpy上一次的Rect。
            /// </summary>
            internal Rect _lastRect = new();

            /// <summary>
            /// 当 HwndHost 被添加到可视化树时自动调用，创建并返回要嵌入的 Win32 窗口句柄。
            /// </summary>
            /// <param name="hwndParent">父窗口的窗口句柄。</param>
            /// <returns>要创建的子 Win32 窗口的句柄。</returns>
            protected override HandleRef BuildWindowCore(HandleRef hwndParent)
            {
                // 修改窗口样式
                ModifyWindowStyle();

                // 设置父子关系
                DLL.SetParent(_scHandle, hwndParent.Handle);

                return new HandleRef(this, _scHandle);
            }

            /// <summary>
            /// 当 HwndHost 从可视化树移除或父窗口关闭时，清理嵌入的 Win32 窗口资源。
            /// </summary>
            /// <param name="hwnd">包含窗口句柄的结构。</param>
            protected override void DestroyWindowCore(HandleRef hwnd)
            {
                //Debug.WriteLine($"执行资源清理：{hwnd}，Scrcpy句柄：{_scHandle}");
                if (_scHandle != IntPtr.Zero)
                {
                    _prc.CloseMainWindow(); //进程退出后会调用Exited事件，前提是EnableRaisingEvents属性要设置为true
                    _prc.WaitForExit();     //进程退出后要一直等待，防止Exited事件未执行完就执行Dispose()
                    Debug.WriteLine("InternalHwndHost：Scrcpy未关闭，程序执行关闭操作。");
                }

                _prc.Dispose();
                Debug.WriteLine("InternalHwndHost：Scrcpy已清理完毕，退出整个程序。");
            }

            /// <summary>
            /// 当父容器大小/位置改变时，响应父容器尺寸变化。
            /// </summary>
            /// <param name="rcBoundingBox">窗口的大小位置。</param>
            protected override void OnWindowPositionChanged(Rect rcBoundingBox)
            {
                if (rcBoundingBox == _lastRect)
                {
                    //Console.WriteLine("Scrcpy尺寸未变");
                    return;
                }
                
                _lastRect = rcBoundingBox;
                DLL.MoveWindow(_scHandle, 0, 0, (int)rcBoundingBox.Width, (int)rcBoundingBox.Height, true);
                base.OnWindowPositionChanged(rcBoundingBox);

                Debug.WriteLine("InternalHwndHost：Scrcpy尺寸修改");
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

            /// <summary>
            /// 刷新Scrcpy窗口，重新绘制窗口后通知基类方法。
            /// </summary>
            public void RefreshScWindow()
            {
                DLL.MoveWindow(_scHandle, 0, 0, (int)_lastRect.Width, (int)_lastRect.Height, true);
                base.OnWindowPositionChanged(_lastRect);
                //Console.WriteLine("刷新Scrcpy窗口");
            }
        }


    }
}
