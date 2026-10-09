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
    internal class ScrcpyHost : IDisposable
    {
        /// <summary>
        /// 对第三方程序的引用。
        /// </summary>
        private readonly Process _prc = new();

        /// <summary>
        /// 真正的HwndHost。
        /// </summary>
        private InternalHwndHost _hwndHost = null;

        /// <summary>
        /// 是否使用有线形式启动。
        /// </summary>
        private bool _isWired = true;

        /// <summary>
        /// 默认为空字符串，如果有值：有线形式--【设备序列号】，无线模式--【设备IP地址】。
        /// </summary>
        private string _sign = "";

        /// <summary>
        /// 无线形式启动时的端口号。
        /// </summary>
        private string _port = "";

        /// <summary>
        /// 管理Scrcpy生命周期的作业对象。
        /// </summary>
        private ScrcpyJob _job = null;

        /// <summary>
        /// 是否已释放资源。
        /// </summary>
        private bool _disposed = false;

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
        /// <param name="sign">有线形式--【设备序列号】，无线模式--【设备IP地址】。</param>
        /// <param name="port">无线形式启动启动时的端口号。</param>
        /// <returns>承载Scrcpy的HwndHost。</returns>
        public async Task<HwndHost> StartScrcpyAsync(bool isWired, string sign, string port)
        {
            _isWired = isWired;
            _sign = sign;
            _port = port;

            await StartScrcpy();
            if (ScHandle == IntPtr.Zero)
            {
                MyGM.MidPOP("Scrcpy启动失败。");
                return null;
            }

            // 创建真正的HwndHost
            _hwndHost = new InternalHwndHost(ScHandle, _prc.Id);

            HashSet<int> hs = [.. MyCfg.Cfg.KeyMappings.Select(k => (int)k.VkCode)];
            KMC_Hook.InitUserHotkeys(hs);
            //MDC_Hook.RegisterHook(ScHandle);

            return _hwndHost;
        }

        /// <summary>
        /// 启动Scrcpy并以指定方式连接手机。
        /// </summary>
        private async Task StartScrcpy()
        {
            ProcessStartInfo info = new(MV.ScPath)
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
                    await Task.Delay(500);
                    try
                    {
                        if (_prc.MainWindowHandle != IntPtr.Zero)
                        {
                            ScHandle = _prc.MainWindowHandle;
                            _job?.Dispose();
                            _job = new ScrcpyJob();
                            _job.AddProcess(_prc.Id);
                            return;
                        }
                        Debug.WriteLine($">>>>>>>>>>>>>>{++i}：尝试启动Scrcpy。");
                    }
                    catch(Exception ex)
                    {
                        MyGM.ShwError($"第{i + 1}次启动时发生异常：\n{ex}");
                        break;
                    }
                }
                while (!_prc.HasExited && i < 30);
            }
        }

        /// <summary>
        /// 获得Scrcpy启动时附带的命令行参数。
        /// </summary>
        private string GetCommandLine()
        {
            StringBuilder sb = new();

            //获得自定义命令行特性
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
            sb.Append($"--keyboard=uhid ");
            sb.Append($"--window-x 0 --window-y {SystemParameters.PrimaryScreenHeight * MV.DpiScaleY} --window-width=800 ");

            if (!AudioDeviceDetector.HasActiveRenderDevice()) sb.Append($"--no-audio ");
            if (_isWired)
            {
                if (_sign == "")
                {
                    sb.Append("--select-usb");
                }
                else
                {
                    sb.Append($"--serial={_sign}");
                }
            }
            else
            {
                sb.Append($"--tcpip={_sign}:{_port}");
            }

            Debug.WriteLine($"ScHost命令行：{sb}。");
            return sb.ToString();
        }

        /// <summary>
        /// 判断传入的对象是否可以转化为bool，不能返回false，如果可以返回其bool值。
        /// </summary>
        private bool ConvToBool(object val) => val is bool result && result;

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
            //_prc.CancelOutputRead();
            //_prc.CancelErrorRead();
            _job?.Dispose();
            _job = null;

            ScHandle = IntPtr.Zero;
            KMC_Hook.ClearUserHotkeys();
            //MDC_Hook.CancelHook();
            Debug.WriteLine($"ScHost：Process的Exieted事件执行结束。");
        }

        /// <summary>
        /// 读取错误流。
        /// </summary>
        private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
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
            if (e.Data == null) return;
            Debug.WriteLine($"[I]{e.Data}");

            //获得当前视频帧的宽高，第一个参数是宽，第二个参数是高
            if (e.Data.Contains("Texture"))
            {
                //Scrcpy在横竖屏转换的时候会导致坐标错位，鼠标点击失效，需要重绘窗口
                //_hwndHost?.RefreshScWindow();
                MV.MainWin.ShowTextureInfo(e.Data);
            }
        }

        /// <summary>
        /// 调用后马上重绘Scrcpy窗口。
        /// </summary>
        public void RefreshScrcpy()
        {
            _hwndHost?.RefreshScWindow();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _hwndHost?.Dispose();
                _hwndHost = null;

                _prc.CloseMainWindow(); //进程退出后会调用Exited事件，前提是EnableRaisingEvents属性要设置为true
                _prc.WaitForExit();     //进程退出后要一直等待，防止Exited事件未执行完就执行Dispose()
                _prc.Dispose();
                Debug.WriteLine($"ScHost：所有资源清理完毕。");
            }
        }

        /// <summary>
        /// HwndHost中不支持异步，所以外部启动Scrcpy，内部封装Scrcpy。
        /// </summary>
        private class InternalHwndHost(IntPtr scHandle, int scPid) : HwndHost
        {
            /// <summary>
            /// Scrcpy的句柄，为IntPtr.Zero时代表Scrcpy没有启动。
            /// </summary>
            private readonly IntPtr _scHandle = scHandle;

            /// <summary>
            /// Scrcpy的进程ID。
            /// </summary>
            private readonly int _scPid = scPid;

            /// <summary>
            /// Scrcpy上一次的Rect。
            /// </summary>
            internal Rect _lastRect = new();

            /// <summary>
            /// 监听Scrcpy位置/大小变化的钩子句柄。
            /// </summary>
            private IntPtr _winEventHook;

            /// <summary>
            /// 回调的引用，防止 GC 回收。
            /// </summary>
            private DLL.WinEventDelegate _winEventDelegate;

            /// <summary>
            /// 对象位置/尺寸变化。
            /// </summary>
            private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

            /// <summary>
            /// 回调在当前进程里执行（系统把事件 post 到当前进程的消息队列）。
            /// </summary>
            private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

            /// <summary>
            /// 跳过调用方自己进程产生的事件（防止自己监听自己）。
            /// </summary>
            private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

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
                DLL.ShowWindow(_scHandle, SW.SW_SHOWNOACTIVATE);

                //注册监听SC窗口的回调，在双击黑边窗口消失后将窗口还原
                _winEventDelegate = WinEventCallback;
                _winEventHook = DLL.SetWinEventHook(
                    EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE,
                    IntPtr.Zero, _winEventDelegate, (uint)_scPid, 0,
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

                return new HandleRef(this, _scHandle);
            }

            /// <summary>
            /// 当 HwndHost 从可视化树移除或父窗口关闭时，清理嵌入的 Win32 窗口资源。
            /// </summary>
            /// <param name="hwnd">包含窗口句柄的结构。</param>
            protected override void DestroyWindowCore(HandleRef hwnd)
            {
                Debug.WriteLine("InternalHwndHost：Scrcpy窗口从父级移除。");

                if (_winEventHook != IntPtr.Zero)
                {
                    DLL.UnhookWinEvent(_winEventHook);
                    _winEventHook = IntPtr.Zero;
                }
                _winEventDelegate = null;
            }

            /// <summary>
            /// 当父容器大小/位置改变时，响应父容器尺寸变化。
            /// </summary>
            /// <param name="rcBoundingBox">窗口的大小位置。</param>
            protected override void OnWindowPositionChanged(Rect rcBoundingBox)
            {
                if (rcBoundingBox == _lastRect)
                {
                    return;
                }

                _lastRect = rcBoundingBox;
                DLL.MoveWindow(_scHandle, 0, 0, (int)rcBoundingBox.Width, (int)rcBoundingBox.Height, true);
                base.OnWindowPositionChanged(rcBoundingBox);
            }

            /// <summary>
            /// 监听Scrcpy位置/尺寸改变的回调。
            /// </summary>
            /// <param name="hWinEventHook">钩子句柄。</param>
            /// <param name="eventType">触发的事件类型。</param>
            /// <param name="hwnd">事件关联的窗口句柄，根据句柄筛选指定窗口的事件。</param>
            /// <param name="idObject">触发事件的对象ID。OBJID_WINDOW=0：窗口对象。</param>
            /// <param name="idChild">触发事件的子元素ID。CHILDID_SELF=0：对象自身。</param>
            /// <param name="dwEventThread">事件发生的线程ID。</param>
            /// <param name="dwmsEventTime">事件发生的时间戳（毫秒）。</param>
            private void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
            {
                if (hwnd != _scHandle) return;
                if (idObject != 0) return;
                if (idChild != 0) return;

                DLL.GetWindowRect(_scHandle, out DLL.LpRect r);
                int w = r.Width;
                int h = r.Height;

                if (Math.Abs(w - _lastRect.Width) > 1 || Math.Abs(h - _lastRect.Height) > 1)
                {
                    Debug.WriteLine($"尺寸偏离：实际 {w}×{h}，期望 {_lastRect.Width}×{_lastRect.Height}，触发重绘");
                    RefreshScWindow();
                }
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
            /// 刷新Scrcpy窗口，需要重新指定位置和尺寸，重绘前窗口后通知基类方法。
            /// </summary>
            public void RefreshScWindow()
            {
                DLL.SendMessage(_scHandle, WM.SETREDRAW, (IntPtr)0, IntPtr.Zero);
                DLL.MoveWindow(_scHandle, 0, 0, (int)_lastRect.Width, (int)_lastRect.Height, true);
                base.OnWindowPositionChanged(_lastRect);

                DLL.SendMessage(_scHandle, WM.SETREDRAW, (IntPtr)1, IntPtr.Zero);
            }
        }


    }
}
