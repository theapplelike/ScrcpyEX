using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System.Windows.Controls.Primitives;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code
    {
        /// <summary>
        /// 自定义初始化。
        /// </summary>
        private void MyInit()
        {
            MV.MainWin = this;
            MV.Pop = pop;
            _mainHandle = new WindowInteropHelper(this).Handle;
            bdRec.Visibility = Visibility.Hidden;
            dplTip.Visibility = Visibility.Collapsed;

            SetControlEvent();
            SetProcess();
            SetTimer();
            ReadCfg();
            SetUISize(true);
        }

        /// <summary>
        /// 如果不使用上一次的尺寸位置，则设置为默认初始大小，
        /// 宽=scrcpy宽+左右边距，高=scrcpy高+上下边距+标题栏高。
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
        /// 检查scrcpy是否已经运行，如果运行则停止。
        /// </summary>
        private void CheckScrcpy()
        {
            // 查询指定名称的进程（无需包含.exe扩展名）
            Process[] processes = Process.GetProcessesByName("scrcpy");

            if (processes.Length > 0)
            {
                //Console.WriteLine($"进程ID: {processes[0].Id}");
                //Console.WriteLine($"主模块路径: {processes[0].MainModule?.FileName}");
                processes[0].Kill();
            }
        }

        /// <summary>
        /// 读取配置的值给UI界面。
        /// </summary>
        private void ReadCfg()
        {
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
            }
            MyCfg.Save();
        }

        /// <summary>
        /// 保存数据、清理使用的系统资源。
        /// </summary>
        private void MyClose()
        {
            _resizeTimer.Stop();

            SaveCfg();
            ClosePrc();
            CloseOther();
        }

        /// <summary>
        /// 外部程序未关闭时需要关闭，并且在关闭前要一直等待，防止Exited事件未执行完就执行Close()和Dispose()，
        /// 这样会导致非托管资源不能被释放。
        /// </summary>
        private void ClosePrc()
        {
            if (_scHandle != IntPtr.Zero)
            {
                _prc.Kill();
                _prc.WaitForExit();
                Console.WriteLine("Main程序应用退出时关闭Scrcpy。");
            }

            _prc.Close();
            _prc.Dispose();
            Console.WriteLine("Scrcpy已清理完毕。");
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
            wfh.Dispose();
        }

        /// <summary>
        /// 启动Scrcpy。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async void StartScrcpy(bool isWired)
        {
            if (_scHandle == IntPtr.Zero)
            {
                btnWired.IsEnabled = btnWirel.IsEnabled = false;
                CheckScrcpy();
                await ConnectScrcpy(isWired);
                btnWired.IsEnabled = btnWirel.IsEnabled = true;
            }
        }

        /// <summary>
        /// 启动Scrcpy并以指定方式连接手机，并不能使用WaitForInputIdle()等待Scrcpy启动完成，因为Process启动的是命令行，
        /// 图像界面是由命令行启动的。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async Task ConnectScrcpy(bool isWired)
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
                    if (EmbeddedForm())
                    {
                        KeyboardHook.RegisterHook();
                        CreateSavedPop();
                        return;
                    }
                    Console.WriteLine($">>>>>>>>>>>>>>{i++}:查找程序：{_scName}，程序是否被关闭：{_prc.HasExited}。");
                }
                while (!_prc.HasExited || i < 30);
                MessageBox.Show("未能成功启动程序，请重试。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
            _scName = $"{DateTime.Now:HHmmss}mook";
            sb.Append($"--keyboard=uhid --window-title={_scName} ");

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
        /// 获得scrcpy的句柄，并将scrcpy放到panel中。
        /// </summary>
        /// <returns>true--嵌入成功；false嵌入失败。</returns>
        private bool EmbeddedForm()
        {
            // 查询指定名称的进程（无需包含.exe扩展名）
            Process[] processes = Process.GetProcessesByName("scrcpy");
            //Console.WriteLine($"进程的数量：{processes.Length}。");
            if (processes.Length > 0 && processes[0].MainWindowHandle != IntPtr.Zero)
            {
                _scHandle = processes[0].MainWindowHandle;

                HideTitleBar(_scHandle);
                IntPtr parent = DLL.SetParent(_scHandle, pnlShow.Handle);
                DLL.ShowWindow(_scHandle, SW.SW_SHOWMAXIMIZED);

                Console.WriteLine($"Scrcpy句柄：{_scHandle}，名称：{processes[0].MainWindowTitle}，父级句柄：{parent}。");
                return parent != IntPtr.Zero;
            }

            //23.6.17
            //foreach (var p in Process.GetProcesses())
            //{
            //    if (p.MainWindowTitle.Contains(_scName))
            //    {
            //        _scHandle = p.MainWindowHandle;
            //
            //        HideTitleBar(_scHandle);
            //        DLL.SetParent(_scHandle, pnlShow.Handle);
            //        DLL.ShowWindow(_scHandle, SW.SW_SHOWMAXIMIZED);
            //
            //        Console.WriteLine($"Scrcpy句柄：{_scHandle}，名称：{p.MainWindowTitle}。");
            //        return true;
            //    }
            //}

            return false;
        }

        /// <summary>
        /// 隐藏指定窗口的标题栏。通过位运算移除WS_CAPTION样式，从而隐藏标题栏。然后设置为新的窗口样式。
        /// </summary>
        /// <param name="handle">窗口的句柄。</param>
        private void HideTitleBar(IntPtr handle)
        {
            int GWL_STYLE = -16;//检索窗口的样式
            int WS_CAPTION = 0xC00000;//是窗口标题栏的样式标志
            int WS_THICKFRAME = 0x00040000;//允许通过拖动边缘来调整窗口大小
            int style = DLL.GetWindowLong(handle, GWL_STYLE);

            //删除WS_CAPTION样式以隐藏标题栏  
            style &= ~WS_CAPTION;

            //删除WS_THICKFRAME样式以隐藏窗体可以调整大小的边框
            style &= ~WS_THICKFRAME;

            //设置为新的窗口样式
            DLL.SetWindowLong(handle, GWL_STYLE, style);
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
                LoadTempSize();
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
        /// 获得临时变量中的窗体尺寸和位置。
        /// </summary>
        private void LoadTempSize()
        {
            if (_tempSize == null)
            {
                SetUISize();
            }
            else
            {
                Width = _tempSize.Width;
                Height = _tempSize.Height;
                Left = _tempSize.Left;
                Top = _tempSize.Top;
            }
        }

        /// <summary>
        /// 重新设置scrcpy窗口的大小，以完全贴合父级panel，需要先最小化再最大化，防止相对坐标错误。
        /// 如果相对坐标仍然错误，两次操作间需要增加延时，给系统足够的处理时间。
        /// </summary>
        private async void ResizeSC()
        {
            Console.WriteLine($"重新设置scrcpy大小。");

            DLL.ShowWindow(_scHandle, SW.SW_SHOWNORMAL);
            await Task.Delay(100);
            DLL.ShowWindow(_scHandle, SW.SW_SHOWMAXIMIZED);

            Dispatcher.Invoke(() =>
            {
                foreach (Popup pop in MV.TempPopups)
                {
                    pop.IsOpen = false;
                }
                CreateSavedPop();
            });
        }

        /// <summary>
        /// 将窗体定位到指定位置，当把Left设置为0时，左侧可能不会完全贴合，会留下一点空白，
        /// 这是窗口边框，其大小取决于当前的Windows主题，左侧要贴合要减去非工作区的左侧宽度。
        /// </summary>
        private void Locate()
        {
            if ((bool)rdoLoc1.IsChecked)
            {
                Left = 0 - SystemParameters.WindowNonClientFrameThickness.Left;
                Top = 0;
            }
            else if ((bool)rdoLoc2.IsChecked)
            {
                Left = SystemParameters.WorkArea.Width - Width;
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
            Console.WriteLine($"宽高：{Width},{Height}。");
        }

        /// <summary>
        /// 外部程序被Kill()后，会执行Exited事件，前提是EnableRaisingEvents要设置为true。
        /// </summary>
        private void HandStopScrcpy()
        {
            if (_scHandle != IntPtr.Zero)
            {
                _prc.Kill();
                StopScriptAndRecord();
                Console.WriteLine("手动结束scrcpy程序。");
            }
        }

        /// <summary>
        /// 消除投屏的黑边。
        /// </summary>
        private void RemoveBlack()
        {
            if (_scHandle == IntPtr.Zero)
            {
                popMid.Show("未进行投屏，不能进行调整。");
                return;
            }

            int scWidth = (int)ScActualWidth;
            int scHeight = (int)ScActualHeight;
            int max = GetGreatestCommonDivisor(scWidth, scHeight);

            int xProportion = scWidth / max;
            int yProportion = scHeight / max;
            Console.WriteLine($"当前Scrcpy的比例：({scWidth / max}，{scHeight / max})，期望比例:({_horizontalRatio},{_verticalRatio})。");

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
            if (_scHandle == IntPtr.Zero)
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
            RemoveBlack();
        }

        /// <summary>
        /// 放大指定步长的尺寸，以15:7为标准。
        /// </summary>
        /// <param name="step">步长，默认为8。</param>
        private void AmplifySize(int step = 8)
        {
            if (_scHandle == IntPtr.Zero)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }
            if (ScRatioX >= 1.5)
            {
                //X轴不能再放大
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
            RemoveBlack();
        }

    }
}
