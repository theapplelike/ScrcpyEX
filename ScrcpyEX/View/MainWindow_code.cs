using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code
    {
        /// <summary>
        /// 添加消息钩子监听程序的所有消息。
        /// </summary>
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            _hwndSource?.AddHook(WndProc);
        }

        /// <summary>
        /// 拦截和处理发送到窗口的所有Windows消息。
        /// </summary>
        /// <param name="hwnd">接收消息的窗口的句柄。</param>
        /// <param name="msg">消息标识符，这是一个整数，表示特定的Windows消息。</param>
        /// <param name="wParam">附加消息参数。</param>
        /// <param name="lParam">长消息参数。</param>
        /// <param name="handled">消息处理状态标志，如果设置为true则表示消息已被处理，不再传递给默认的消息处理程序。</param>
        /// <returns>消息处理结果，大多数情况下返回 IntPtr.Zero（表示使用默认处理）。</returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //wParam高字节为0X20，低字节为1时，说明窗口被激活，但处于最小化
            //if (msg == 0x0006 && (int)wParam == 0X200001)
            //{
            //    handled = true;
            //    Debug.WriteLine($"拦截消息：{msg:X4}，{(int)wParam:X4}，{(int)lParam:X4}，说明窗口被激活，但处于最小化");
            //}
            //窗口的非客户区域激活状态改变，wParam为0时窗口失活，为1时窗口激活，
            //当窗口Z序在后面，但处于激活状态时，Z序到最前面是不会触发Activated事件的，此时强制显示映射按钮
            //else if (msg == 0x0086 && (int)wParam == 0X0001)
            //{
            //    //ShowKeyMapping();
            //    //Debug.WriteLine($"拦截消息：{msg:X4}，{(int)wParam:X4}，{(int)lParam:X4}，" +
            //    //    $"未触发Activated事件，强制显示映射按钮");
            //}
            //else
            //    Debug.WriteLine($"MEG：{msg:X4}，{(long)wParam:X4}，{(long)lParam:X4}");

            switch (msg)
            {
                case WM.ACTIVATE:
                    //wParam高字节为0X20，低字节为1时，说明窗口被激活，但处于最小化
                    if ((int)wParam == 0X200001)
                    {
                        handled = true;
                        Debug.WriteLine($"拦截消息：{msg:X4}，{(int)wParam:X4}，{(int)lParam:X4}，窗口被激活，但处于最小化");
                    }
                    break;
                case WM.DPICHANGED:
                    MV.DpiScaleX = VisualTreeHelper.GetDpi(this).DpiScaleX;
                    MV.DpiScaleY = VisualTreeHelper.GetDpi(this).DpiScaleY;
                    Debug.WriteLine($"DPI 改变了，重新获取当前窗口的 DPI");
                    break;
                //default:
                //    Debug.WriteLine($"MEG：{msg:X4}，{(long)wParam:X4}，{(long)lParam:X4}");
                //    break;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// 自定义初始化。
        /// </summary>
        private void MyInit()
        {
            MV.MainWin = this;
            MV.DpiScaleX = VisualTreeHelper.GetDpi(this).DpiScaleX;
            MV.DpiScaleY = VisualTreeHelper.GetDpi(this).DpiScaleY;

            _mainHandle = new WindowInteropHelper(this).Handle;
            bdRec.Visibility = Visibility.Collapsed;
            dplTip.Visibility = Visibility.Collapsed;

            SetControlEvent();
            SetTimer();
            ReadCfg();
            ScOptInit();
            SetUISize(MyCfg.Cfg.IsUseLastSize);
            InputModeSwitcher.SwitchToEnglishMode(_mainHandle);

            //快捷键
            InputBindings.Add(new KeyBinding(new RelayCommand(ScGotFocus), Key.F1, ModifierKeys.None));
        }

        /// <summary>
        /// 如果不使用上一次的尺寸位置，则设置为默认初始大小，
        /// 宽=Scrcpy宽+左右边距，高=Scrcpy高+上下边距+标题栏高。
        /// </summary>
        /// <param name="isUseLastSize">是否使用上一次的大小和位置。</param>
        private void SetUISize(bool isUseLastSize = false)
        {
            if (isUseLastSize && MyCfg.Cfg.Size.IsRecord)
            {
                Width = MyCfg.Cfg.Size.Width;
                Height = MyCfg.Cfg.Size.Height;
                Left = MyCfg.Cfg.Size.Left;
                Top = MyCfg.Cfg.Size.Top;
            }
            else
            {
                Width = _scWidth + ExtraWidth;
                Height = _scHeight + ExtraHeight;
                Left = (SystemParameters.WorkArea.Width - ActualWidth) / 2;
                Top = (SystemParameters.WorkArea.Height - ActualHeight) / 2;
            }
            //Console.WriteLine($"窗体尺寸：{Width},{Height}。");
        }

        /// <summary>
        /// 读取配置的值给UI界面。
        /// </summary>
        private void ReadCfg()
        {
            if (!MyCfg.Cfg.IsCanResize)
            {
                ResizeMode = ResizeMode.CanMinimize;
            }

            rdoLoc1.IsChecked = MyCfg.Cfg.Locate1;
            rdoLoc2.IsChecked = MyCfg.Cfg.Locate2;
            rdoLoc3.IsChecked = MyCfg.Cfg.Locate3;
            rdoLoc4.IsChecked = MyCfg.Cfg.Locate4;
        }

        /// <summary>
        /// 将UI界面的值保存到配置当中。
        /// </summary>
        private void SaveCfg()
        {
            MyCfg.Cfg.Locate1 = (bool)rdoLoc1.IsChecked;
            MyCfg.Cfg.Locate2 = (bool)rdoLoc2.IsChecked;
            MyCfg.Cfg.Locate3 = (bool)rdoLoc3.IsChecked;
            MyCfg.Cfg.Locate4 = (bool)rdoLoc4.IsChecked;

            //窗体在正常状态下才记录其尺寸位置信息
            if (WindowState == WindowState.Normal)
            {
                MyCfg.Cfg.Size.Width = Width;
                MyCfg.Cfg.Size.Height = Height;
                MyCfg.Cfg.Size.Left = Left;
                MyCfg.Cfg.Size.Top = Top;
                MyCfg.Cfg.Size.IsRecord = true;
            }
            MyCfg.Save();
        }

        /// <summary>
        /// 保存数据、清理使用的系统资源。
        /// </summary>
        private void MyClose()
        {
            SaveCfg();
            CloseOther();
        }

        /// <summary>
        /// 关闭其他一些程序使用的资源。
        /// </summary>
        private void CloseOther()
        {
            if (MouseScript.HookID != IntPtr.Zero)
            {
                MouseScript.CancelHook();
            }
        }

        /// <summary>
        /// 启动Scrcpy。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async void StartScrcpy(bool isWired)
        {
            if (IsScClosed)
            {
                btnWired.IsEnabled = btnWirel.IsEnabled = false;
                CheckScrcpy();

                if (isWired)
                {
                    popScOpt.ExchangeMainBtn(btnWired);
                    await ScrcpyInit(true);
                }
                else
                {
                    popScOpt.ExchangeMainBtn(btnWirel);
                    await PrepareWireless();
                }

                MyCfg.Cfg.IsUseWiredInLast = isWired;
                btnWired.IsEnabled = btnWirel.IsEnabled = true;
            }
            else
            {
                popMid.Show("Scrcpy已经启动");
            }
        }

        /// <summary>
        /// 检查Scrcpy是否已经运行，如果运行则停止。
        /// </summary>
        private void CheckScrcpy()
        {
            // 查询指定名称的进程（无需包含.exe扩展名）
            Process[] prc = Process.GetProcessesByName("Scrcpy");
            if (prc.Length > 0)
            {
                //Debug.WriteLine($"进程ID:{prc[0].Id}，路径:{prc[0].MainModule?.FileName}");
                prc[0].Kill();
            }
        }

        /// <summary>
        /// Scrcpy启动完成后，初始化Scrcpy相关的所有功能。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async Task ScrcpyInit(bool isWired, string ip = "", string port = "5555")
        {
            _scrcpyHost = new ScrcpyHost();
            bdShow.Child = await _scrcpyHost.StartScrcpyAsync(isWired, ip, port);

            if (bdShow.Child != null)
            {
                bool isActivate = Activate();
                Debug.WriteLine($"窗口是否激活：{isActivate}");
                CreateSavedPop(false, DLL.GetForegroundWindow() == _mainHandle);
                KeyboardHook.RegisterHook();

                //Console.WriteLine($"窗体句柄：{_mainHandle},Scrcpy句柄：{_scrcpyHost.ScHandle},前台窗口句柄：{DLL.GetForegroundWindow()}");
                //Console.WriteLine($"前台窗口句柄：{DLL.GetForegroundWindow()}");
            }
        }

        /// <summary>
        /// 最大化或者还原窗口。
        /// </summary>
        private void Maximize()
        {
            double wd = SystemParameters.WorkArea.Width;
            double ht = SystemParameters.WorkArea.Height;

            if (Width >= wd && Height >= ht)
            {
                LoadTempSize(_tempSize);
                btnMaximize.Content = "\ue922";
            }
            else
            {
                SaveTempSize();
                Width = wd; 
                Height = ht;
                Left = Top = 0;
                btnMaximize.Content = "\ue923";
            }
        }

        /// <summary>
        /// 保存当前窗体的尺寸和位置到临时变量中。
        /// </summary>
        private void SaveTempSize() => _tempSize = new(Width, Height, Left, Top);

        /// <summary>
        /// 应用临时变量中的窗体尺寸和位置。
        /// </summary>
        private void LoadTempSize(FormSize size)
        {
            if (size == null)
            {
                SetUISize();
                Locate();
                return;
            }

            Width = size.Width;
            Height = size.Height;
            Left = size.Left;
            Top = size.Top;
        }

        /// <summary>
        /// 将窗体定位到指定位置，当把Left设置为0时，左侧可能不会完全贴合，会留下一点空白，
        /// 这是窗口边框，其大小取决于当前的Windows主题，左侧要贴合要减去非工作区的左侧宽度。
        /// </summary>
        private void Locate()
        {
            if ((bool)rdoLoc1.IsChecked)
            {
                //Left = 0 - SystemParameters.WindowNonClientFrameThickness.Left;
                Left = 0;
                Top = 0;
            }
            else if ((bool)rdoLoc2.IsChecked)
            {
                Left = SystemParameters.WorkArea.Width - Width - 3;//完全贴合右边会导致截图脚本有偏移
                Top = 0;
            }
            else if ((bool)rdoLoc3.IsChecked)
            {
                Left = 0 - SystemParameters.WindowNonClientFrameThickness.Left;
                Top = SystemParameters.WorkArea.Height - Height;
            }
            else if ((bool)rdoLoc4.IsChecked)
            {
                Left = SystemParameters.WorkArea.Width - Width;
                Top = SystemParameters.WorkArea.Height - Height;
            }
        }

        /// <summary>
        /// 窗体宽度修改为屏幕的四分之一并移动定位，高度根据15:7调整。
        /// </summary>
        private void QuarterMove()
        {
            Width = SystemParameters.WorkArea.Width / 2;
            Height = (Width - ExtraWidth) * 1.0 / 15 * 7 + ExtraHeight;
            Locate();
            //Console.WriteLine($"宽高：{Width},{Height}。");
        }

        /// <summary>
        /// 关闭Scrcpy。
        /// </summary>
        private void CloseScrcpy()
        {
            if (!IsScClosed)
            {
                _scrcpyHost.DisposeScrcpy();
                bdShow.Child = null;
                StopScriptAndRecord();
                Debug.WriteLine($"用户结束Scrcpy程序。");
            }
        }

        /// <summary>
        /// 消除Scrcpy的黑边。
        /// </summary>
        private void RemoveScBlack(bool isForcedAdjustment = false)
        {
            if (IsScClosed)
            {
                popMid.Show("未投屏，无需消除黑边。");
                return;
            }
            if (!isForcedAdjustment && MyCfg.Cfg.IsKeepLandscape)
            {
                return;
            }

            int scWidth = (int)ScActualWidth;
            int scHeight = (int)ScActualHeight;
            int max = GetGreatestCommonDivisor(scWidth, scHeight);

            int xProportion = scWidth / max;
            int yProportion = scHeight / max;
            Debug.WriteLine($"消除黑边,当前Scrcpy的比例：({ScActualWidth / max}，{ScActualHeight / max})，期望比例:({_horizontalRatio},{_verticalRatio})。");
            Debug.WriteLine($"当前Scrcpy的宽高：({ScActualWidth},{ScActualHeight})，\n和原始大小的比例：{ScRatioX}，{ScRatioY}");

            //计算通分后的分子，如果标准分子大，应以宽为准，调整高度；反之调整宽度
            int currentNumerator = xProportion * _verticalRatio;
            int standardNumerator = yProportion * _horizontalRatio;
            if (currentNumerator > standardNumerator)
            {
                Width = ScActualHeight / _verticalRatio * _horizontalRatio + ExtraWidth;
            }
            else if (currentNumerator < standardNumerator)
            {
                Height = ScActualWidth / _horizontalRatio * _verticalRatio + ExtraHeight;
            }
        }

        /// <summary>
        /// 缩小指定步长的尺寸，以15:7为标准。
        /// </summary>
        /// <param name="step">步长，默认为8。</param>
        private void ReduceSize(int step = 8)
        {
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }
            if (ScRatioX <= 0.25 || ScRatioY <= 0.25)
            {
                popMid.Show("不能再继续缩小。");
                return;
            }

            Width -= _horizontalRatio * step;
            Height -= _verticalRatio * step;
            Locate();
            RemoveScBlack();
        }

        /// <summary>
        /// 放大指定步长的尺寸，以15:7为标准。
        /// </summary>
        /// <param name="step">步长，默认为8。</param>
        private void AmplifySize(int step = 8)
        {
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }
            if (ScRatioX >= 1.5)
            {
                popMid.Show("不能再继续放大。");
                return;
            }
            else if (Width + _horizontalRatio * step > SystemParameters.WorkArea.Width)
            {
                Width = SystemParameters.WorkArea.Width;
            }
            else
            {
                Width += _horizontalRatio * step;
            }

            if (ScRatioY >= 1.5)
            {
                //Y轴不能再放大
            }
            else if (Height + _verticalRatio * step > SystemParameters.WorkArea.Height)
            {
                Height = SystemParameters.WorkArea.Height;
            }
            else
            {
                Height += _verticalRatio * step;
            }

            Locate();
            RemoveScBlack();
        }

        /// <summary>
        /// 从最小化恢复时可能导致Scrcpy窗口向左偏移，所以恢复时刷新Scrcpy窗口。
        /// </summary>
        private void RefreshScrcpy()
        {
            switch (WindowState)
            {
                case WindowState.Minimized:
                    //Debug.WriteLine("窗口已最小化");
                    break;
                case WindowState.Normal:
                    //Debug.WriteLine("窗口已恢复正常状态");
                    _scrcpyHost?.RefreshScrcpy();
                    break;
                case WindowState.Maximized:
                    //Debug.WriteLine("窗口已最大化");
                    break;
            }
        }

        /// <summary>
        /// 打开设备的无线调试功能，开启后可以无线连接，需要在同一个网络中。
        /// </summary>
        private async void WirelessDebug()
        {
            popMid.Show("请保持USB连接稳定。");
            await OpenWirelessDebug();
            //if (await OpenWirelessDebug())
            //{
            //    popMid.Show("匹配完成，正在启动Scrcpy。");
            //}
            //else
            //{
            //    popMid.Show("没有USB连接的设备或设备未打开USB调试。");
            //}
        }

        /// <summary>
        /// 通过ADB将USB连接的设备打开无线调试，然后重启Scrcpy，打开无线调试的设备可以被Tcpclient访问。
        /// </summary>
        /// <returns>设备无线调试是否打开成功。</returns>
        private async Task<bool> OpenWirelessDebug()
        {
            List<string> devices = await GetAdbDevices();
            foreach (string dev in devices)
            {
                //判断是序列号还是IP，序列号代表是USB连接
                if (!dev.Contains(":") || !IPAddress.TryParse(dev.Split(':')[0], out _))
                {
                    //获得指定设备IP，awk 是一个文本处理工具，按行处理文本
                    //{print $9}表示打印每一行的第9个字段，字段默认以空格或制表符分隔
                    string ip = (await MyGM.Adb($@"-s {dev} shell ip route | awk '{{print $9}}'")).Trim();
                    if (ip == "")
                    {
                        popMid.Show("未获得设备IP地址，请检查是否在同一网络中。");
                        return false; ;
                    }

                    //将安卓设备上的 ADB 服务从 USB 模式切换为网络模式
                    await MyGM.Adb($"-s {dev} tcpip 5555");

                    //连接指定 IP 地址和端口上的 Android 设备，须在同一局域网中
                    await MyGM.Adb($"connect {ip}:5555");

                    CloseScrcpy();
                    await Task.Delay(1000);
                    StartScrcpy(false);

                    popMid.Show("匹配完成，正在启动Scrcpy。");
                    return true;
                }
            }

            popMid.Show("没有设备或未打开USB调试。");
            return false;
        }

        /// <summary>
        /// 所有当前连接到电脑的 Android 设备或模拟器。
        /// USB连接的物理设备，是其序列号；网络连接的设备，格式为 IP地址:端口号；模拟器是 emulator-端口号。
        /// </summary>
        private async Task<List<string>> GetAdbDevices()
        {
            string output = await MyGM.Adb("devices");
            return output
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.StartsWith("List of devices") && !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length >= 2 && parts[1] == "device")
                .Select(parts => parts[0].Trim())
                .ToList();
        }

        /// <summary>
        /// 准备进行无线连接，连接前会查询网络中所有IP和端口开放的设备，如有多个设备则会让用户选择。
        /// </summary>
        private async Task PrepareWireless()
        {
            AdbDeviceScanner scanner = new();
            List<string> devices = await scanner.ScanNetworkForAdbDevicesAsync();

            if (devices.Count <= 0)
            {
                popMid.Show("局域网中没有可以连接的设备。");
            }
            else if (devices.Count == 1)
            {
                await ScrcpyInit(false, devices[0]);
            }
            else
            {
                SelectDevice select = new(devices);
                if (select.ShowDialog() == true)
                {
                    await ScrcpyInit(false, devices[select.Index]);
                }
            }
        }

        /// <summary>
        /// 窗口尺寸改变后要进行的操作。
        /// </summary>
        /// <param name="e">SizeChanged事件相关的数据。</param>
        private void WindowSizeChanged(SizeChangedEventArgs e)
        {
            //Debug.WriteLine($"现在的宽度：{e.NewSize.Width}");

            _vm.Scaling = $"缩放比例 {ScRatioX:P}:{ScRatioY:P}";
            if (!IsScClosed)
            {
                _resizeTimer.Start();
            }

            //计算菜单栏右侧按钮是否需要放入溢出菜单
            if (e.NewSize.Width >= 329)
            {
                MoveOutSysMenu();
            }
            else if (e.NewSize.Width < 329 && e.NewSize.Width >= 223)
            {
                if (stpRightMenu.Children.Count > 2)
                {
                    MoveInSysMenu(false);
                }
                else if (stpRightMenu.Children.Count < 2)
                {
                    MoveOutSysMenu();
                }
            }
            else if (e.NewSize.Width < 223)
            {
                if (stpRightMenu.Children.Count > 1)
                {
                    MoveInSysMenu(true);
                }
            }
        }

        /// <summary>
        /// 将系统按钮加入系统悬停菜单。
        /// </summary>
        /// <param name="isTopmost">加入溢出菜单的按钮是否放到最前面。</param>
        private void MoveInSysMenu(bool isTopmost = true)
        {
            Button btn = stpRightMenu.Children[stpRightMenu.Children.Count - 1] as Button;
            popSysOpt.Add(btn, isTopmost);
        }

        /// <summary>
        /// 将系统按钮移出系统悬停菜单。
        /// </summary>
        private void MoveOutSysMenu() => popSysOpt.MoveOutTo(stpRightMenu);

        /// <summary>
        /// 初始化Scrcpy的主要功能按钮和附属Popup菜单。
        /// </summary>
        private void ScOptInit()
        {
            if (MyCfg.Cfg.IsUseWiredInLast)
            {
                ProcessButton(btnWired);
            }
            else
            {
                ProcessButton(btnWirel);
            }
        }

        /// <summary>
        /// 处理Scrcpy相关的按钮。
        /// </summary>
        /// <param name="mainBtn">作为主要功能的按钮。</param>
        private void ProcessButton(Button mainBtn)
        {
            popScOpt.PlacementTarget = mainBtn;

            //非主要按钮加入Popup菜单
            List<Button> buttons = dcpScMain.Children.OfType<Button>().Where(b => b.Name != mainBtn.Name).ToList();
            foreach (Button b in buttons)
            {
                popScOpt.Add(b);
            }
        }

        /// <summary>
        /// 打开指定菜单，在溢出菜单中时，不再依赖按钮，在鼠标位置弹出菜单。
        /// </summary>
        /// <param name="btn">打开菜单的按钮。</param>
        /// <param name="pop">需要打开的菜单。</param>
        private void OpenMenu(Button btn, Popup pop)
        {
            if (ToolBar.GetIsOverflowItem(btn))
            {
                pop.PlacementTarget = null;
                pop.Placement = PlacementMode.MousePoint;
            }
            else
            {
                pop.PlacementTarget = btn;
                pop.Placement = PlacementMode.Bottom;
            }
            pop.IsOpen = true;
        }

    }
}
