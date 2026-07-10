using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using static ScrcpyEX.MyDefinition.WindowsAPI.VK;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code
    {
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
            switch (msg)
            {
                case WM.ACTIVATE:
                    #region wParam高字节为0X20，低字节为1时，说明窗口被激活，但处于最小化
                    if ((int)wParam == 0X200001)
                    {
                        handled = true;
                        Debug.WriteLine($"拦截消息：{msg:X4}，{(int)wParam:X4}，{(int)lParam:X4}，窗口被激活，但处于最小化");
                    }
                    #endregion
                    break;
                case WM.DPICHANGED:
                    #region DPI 改变时重新获取
                    MV.DpiScaleX = VisualTreeHelper.GetDpi(this).DpiScaleX;
                    MV.DpiScaleY = VisualTreeHelper.GetDpi(this).DpiScaleY;
                    Debug.WriteLine($"DPI 改变了，重新获取当前窗口的 DPI");
                    #endregion
                    break;
                case WM.SYSKEYDOWN:
                    #region 转发SC的快捷键
                    if (wParam.ToInt32() == VK_O && (lParam.ToInt64() & (1L << 29)) != 0)
                    {
                        if ((DLL.GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)//Shift是否按下
                        {
                            PostShortcutsThree(VK_O);
                        }
                        else
                        {
                            PostShortcutsTwo(VK_O);
                        }
                        handled = true;
                    }
                    #endregion
                    break;
                default:
                    //Debug.WriteLine($"MEG：{msg:X4}，{(long)wParam:X4}，{(long)lParam:X4}");
                    break;
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

            bdRec.Visibility = Visibility.Collapsed;
            dplTip.Visibility = Visibility.Collapsed;

            ReadCfg();
            SetTimer();
            ScOptInit();
            SetControlEvent();
            SetUISize(MyCfg.Cfg.IsUseLastSize);

            //添加消息钩子监听程序的所有消息
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            _hwndSource.AddHook(WndProc);
            _mainHandle = _hwndSource.Handle;

            //输入法设置为英文模式，通过焦点按钮实现
            btnFouce.Focus();

            //注册键盘钩子
            KMC_Hook.RegisterHook(_mainHandle);
        }

        /// <summary>
        /// 保存数据、清理使用的系统资源。
        /// </summary>
        private void MyClose()
        {
            _hwndSource.RemoveHook(WndProc);
            SaveCfg();
            
            KMC_Hook.CancelHook();
            MRS_Hook.CancelHook();
            MDC_Hook.CancelHook();

            if (!Debugger.IsAttached) MyGM.StopAdbServer();
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
                Top = MyCfg.Cfg.Size.Top;
                Left = MyCfg.Cfg.Size.Left;
                Width = MyCfg.Cfg.Size.Width;
                Height = MyCfg.Cfg.Size.Height;
            }
            else
            {
                Top = (SystemParameters.WorkArea.Height - ActualHeight) / 2;
                Left = (SystemParameters.WorkArea.Width - ActualWidth) / 2;
                Width = _scWidth + ExtraWidth;
                Height = _scHeight + ExtraHeight;
            }
            //Debug.WriteLine($"窗体尺寸：{Width},{Height}。");
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
                MyCfg.Cfg.Size.Top = Top;
                MyCfg.Cfg.Size.Left = Left;
                MyCfg.Cfg.Size.Width = Width;
                MyCfg.Cfg.Size.Height = Height;
                MyCfg.Cfg.Size.IsRecord = true;
            }
            MyCfg.Save();
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
            Process[] prcs = Process.GetProcessesByName("Scrcpy");
            foreach (Process prc in prcs)
            {
                prc.Kill();
                prc.Dispose();
            }
        }

        /// <summary>
        /// Scrcpy启动完成后，初始化Scrcpy相关的所有功能。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async Task ScrcpyInit(bool isWired, string ip = "", string port = "5555")
        {
            _scHost = new ScrcpyHost();
            bdShow.Child = await _scHost.StartScrcpyAsync(isWired, ip, port);

            if (bdShow.Child != null)
            {
                Activate();
                CreateSavedPop(false, DLL.GetForegroundWindow() == _mainHandle);
                InputModeSwitcher.SwitchToEnglishMode(ScHandle);

                Debug.WriteLine($"-*-激活:{IsActive}，主:{_mainHandle}，SC:{ScHandle}");
            }
        }

        /// <summary>
        /// 最大化或者还原窗口。
        /// </summary>
        private void Maximize()
        {
            double wd = SystemParameters.WorkArea.Width;
            double ht = SystemParameters.WorkArea.Height;
            _isAdjustPop = false;
            HideKeyMapping();

            if (Width >= wd && Height >= ht)
            {
                LoadTempSize(_tempSize);
                btnMaximize.Content = "\ue922";
            }
            else
            {
                SaveTempSize();
                Top = Left = 0;
                Width = wd;
                Height = ht;
                btnMaximize.Content = "\ue923";
            }
        }

        /// <summary>
        /// 保存当前窗体的尺寸和位置到临时变量中。
        /// </summary>
        private void SaveTempSize() => _tempSize = new(Top, Left, Width, Height);

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

            Top = size.Top;
            Left = size.Left;
            Width = size.Width;
            Height = size.Height;
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
                Top = 0;
                Left = 0;
            }
            else if ((bool)rdoLoc2.IsChecked)
            {
                Top = 0;
                Left = SystemParameters.WorkArea.Width - Width - 3;//完全贴合右边会导致截图脚本有偏移
            }
            else if ((bool)rdoLoc3.IsChecked)
            {
                Top = SystemParameters.WorkArea.Height - Height;
                Left = 0 - SystemParameters.WindowNonClientFrameThickness.Left;
            }
            else if ((bool)rdoLoc4.IsChecked)
            {
                Top = SystemParameters.WorkArea.Height - Height;
                Left = SystemParameters.WorkArea.Width - Width;
            }
        }

        /// <summary>
        /// 窗体宽度修改为屏幕的四分之一并移动定位，高度根据短比调整。
        /// </summary>
        private void QuarterMove()
        {
            double ratio;
            if (_verticalReso > _horizontalReso)
            {
                ratio = 1.0 * _horizontalReso / _verticalReso;
            }
            else
            {
                ratio = 1.0 * _verticalReso / _horizontalReso;
            }

            Width = SystemParameters.WorkArea.Width / 2;
            Height = (Width - ExtraWidth) * ratio + ExtraHeight;
            Locate();
        }

        /// <summary>
        /// 关闭Scrcpy，同时回收焦点。
        /// </summary>
        private void CloseScrcpy()
        {
            if (!IsScClosed)
            {
                _scHost.DisposeScrcpy();
                btnFouce.Focus();
                bdShow.Child = null;
                StopScriptAndRecord();
                Debug.WriteLine($"用户结束Scrcpy程序。");
            }
        }

        /// <summary>
        /// 消除Scrcpy的黑边。
        /// </summary>
        private void RemoveScBlack()
        {
            if (IsScClosed)
            {
                popMid.Show("未投屏，无需消除黑边。");
                return;
            }

            if (MyCfg.Cfg.IsKeepLandscape)
            {
                double hope = ScActualHeight * _resoRatio + ExtraWidth;
                if (hope < 0) return;

                Width = hope;
                Debug.WriteLine($"保持横屏，调整宽度，期望：{hope}，实际：{ActualWidth}");
            }
            else if (_horizontalReso < _verticalReso)
            {
                double currResoRatio = ScActualHeight / ScActualWidth;
                if (currResoRatio > _resoRatio)
                {
                    Debug.WriteLine($"竖屏，上下有黑边，{currResoRatio}");
                    AdjustHeight();
                }
                else
                {
                    Debug.WriteLine($"竖屏，左右有黑边，{currResoRatio}");
                    AdjustWidth();
                }
            }
            else
            {
                double currResoRatio = ScActualWidth / ScActualHeight;
                if (currResoRatio < _resoRatio)
                {
                    Debug.WriteLine($"横屏，上下有黑边，{currResoRatio}");
                    AdjustHeight();
                }
                else
                {
                    Debug.WriteLine($"横屏，左右有黑边，{currResoRatio}");
                    AdjustWidth();
                }
            }
        }

        /// <summary>
        /// 黑边在上下，调整高度。
        /// </summary>
        private void AdjustHeight()
        {
            double ratio = _verticalReso * 1.0 / _horizontalReso;
            Height = ScActualWidth * ratio + ExtraHeight;
            Debug.WriteLine($"调整高度：{ratio}，期望：{ScActualWidth * ratio + ExtraHeight}，实际：{ActualHeight}");
        }

        /// <summary>
        /// 黑边在左右，调整宽度。
        /// </summary>
        private void AdjustWidth()
        {
            double ratio = _horizontalReso * 1.0 / _verticalReso;
            Width = ScActualHeight * ratio + ExtraWidth;
            Debug.WriteLine($"调整宽度：{ratio}，期望：{ScActualHeight * ratio + ExtraWidth}，实际：{ActualWidth}");
        }

        /// <summary>
        /// 缩小程序指定步长的尺寸，以 宽边/短边 为步长基准。
        /// </summary>
        /// <param name="step">步长。</param>
        private void ReduceSize(int step = 50)
        {
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }

            double tgtWd = Width - _resoRatio * step;
            double tgtHt = Height - step;
            
            if (tgtWd < 0 || tgtHt < 0)
            {
                return;
            }

            Width = tgtWd;
            Height = tgtHt;
            Debug.WriteLine($"缩小期望宽高：({tgtWd},{tgtHt})，实际：({ActualWidth},{ActualHeight})");
            Locate();
            RemoveScBlack();
        }

        /// <summary>
        /// 放大程序指定步长的尺寸，以 宽边/短边 为步长基准。
        /// </summary>
        /// <param name="step">步长。</param>
        private void AmplifySize(int step = 50)
        {
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }

            if (Width + _resoRatio * step > SystemParameters.WorkArea.Width)
            {
                Width = SystemParameters.WorkArea.Width;
            }
            else
            {
                Width += _resoRatio * step;
            }
            if (Height + step > SystemParameters.WorkArea.Height)
            {
                Height = SystemParameters.WorkArea.Height;
            }
            else
            {
                Height += step;
            }
            Debug.WriteLine($"放大后：{ActualWidth},{ActualHeight}");

            Locate();
            RemoveScBlack();
        }

        /// <summary>
        /// 打开设备的无线调试功能，开启后可以无线连接，需要在同一个网络中。
        /// </summary>
        private async void WirelessDebug()
        {
            popMid.Show("请保持USB连接稳定。");
            await OpenWirelessDebug();
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
                    string ip = (await MyGM.AdbAsync($@"-s {dev} shell ip route | awk '{{print $9}}'")).Trim();
                    if (ip == "")
                    {
                        popMid.Show("未获得设备IP地址，请检查是否在同一网络中。");
                        return false; ;
                    }

                    //将安卓设备上的 ADB 服务从 USB 模式切换为网络模式
                    await MyGM.AdbAsync($"-s {dev} tcpip 5555");

                    //连接指定 IP 地址和端口上的 Android 设备，须在同一局域网中
                    await MyGM.AdbAsync($"connect {ip}:5555");

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
            string output = await MyGM.AdbAsync("devices");
            return [.. output
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !line.StartsWith("List of devices") && !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries))
                .Where(parts => parts.Length >= 2 && parts[1] == "device")
                .Select(parts => parts[0].Trim())];
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

            //计算菜单栏右侧按钮是否需要放入移出溢出菜单
            if (e.NewSize.Width >= 329)
            {
                while (stpRightMenu.Children.Count < 3)
                {
                    MoveOutSysMenu();
                }
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
                while (stpRightMenu.Children.Count > 1)
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
            List<Button> buttons = [.. dcpScMain.Children.OfType<Button>().Where(b => b.Name != mainBtn.Name)];
            foreach (Button btn in buttons)
            {
                popScOpt.Add(btn);
            }
        }

        /// <summary>
        /// 打开指定菜单，在溢出菜单中时，不再依赖按钮，在鼠标位置弹出菜单。
        /// </summary>
        /// <param name="btn">打开菜单的按钮。</param>
        /// <param name="pop">需要打开的菜单。</param>
        private void OpenMenu(Button btn, Popup pop)
        {
            if (pop.IsOpen)
            {
                pop.IsOpen = false;
                return;
            }

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

        /// <summary>
        /// 焦点在SC上时使用Alt+Tab切换窗口，SC会收到Alt按下事件，但不会收到抬起事件，
        /// 导致Alt+Tab回来后SC即使有焦点也不能触发键盘输入，如果此时按下H会触发Alt+H快捷键。
        /// Win+Tab不会导致问题，鼠标切换也不会导致问题。
        /// 所以需要向SC发送Alt键抬起消息。
        /// </summary>
        /// <param name="hWnd">目标窗口句柄。</param>
        /// <param name="isRightAlt">是否为右Alt。</param>
        public void SendAltKeyUp(IntPtr hWnd, bool isRightAlt)
        {
            uint vk = isRightAlt ? VK_RMENU : VK_LMENU;
            uint scanCode = DLL.MapVirtualKey(vk, 0);
            uint extended = isRightAlt ? 1u : 0u;

            IntPtr lParamUp = BuildLParam(1, scanCode, extended, 0, 1, 1);
            DLL.PostMessage(hWnd, WM.SYSKEYUP, (IntPtr)vk, lParamUp);
        }

        /// <summary>
        /// 组装WM_KEYDOWN、WM_KEYUP、WM_SYSKEYDOWN、WM_SYSKEYUP键盘消息的 lParam 参数。
        /// </summary>
        /// <param name="repeatCount">0-15：重复次数。</param>
        /// <param name="scanCode">16-23：OEM 扫描码。</param>
        /// <param name="extendedKey">24：扩展键标志(是扩展键，值为1；否则为0)，左Alt为0，右Alt为1。</param>
        /// <param name="contextCode">29：上下文码（仅当消息为 WM_SYSKEYDOWN 或 WM_SYSKEYUP，并且 Alt 键被按住时，值才为 1）。</param>
        /// <param name="previousState">30：前一个键状态（0=之前是抬起，1=之前是按下）。</param>
        /// <param name="transitionState">31：转换状态（0=本次按下，1=本次抬起）。</param>
        /// <returns>IntPtr类型的 lParam 参数。</returns>
        private IntPtr BuildLParam(uint repeatCount, uint scanCode, uint extendedKey, uint contextCode, uint previousState, uint transitionState)
        {
            uint lParam = (scanCode << 16)
                        | (extendedKey << 24)
                        | (contextCode << 29)
                        | (previousState << 30)
                        | (transitionState << 31)
                        | (repeatCount & 0x0000FFFF);
            return (IntPtr)lParam;
        }

        /// <summary>
        /// 通过ADB打开安卓的Remote NDIS。
        /// </summary>
        private async Task OpenRndis()
        {
            MyGM.POP("正在打开USB网络共享");
            await MyGM.AdbAsync("shell svc usb setFunctions rndis");
        }


    }
}
