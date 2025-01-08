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
        /// 读取配置的值给UI界面。
        /// </summary>
        private void ReadCfg()
        {
            //txtIP.Text = MyCfg.Cfg.IP;

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
            //MyCfg.Cfg.IP = txtIP.Text;

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
                Console.WriteLine("Main程序应用退出时关闭scrcpy。");
            }

            _prc.Close();
            _prc.Dispose();
            Console.WriteLine("Main程序应用退出。");
        }

        /// <summary>
        /// 关闭其他一些程序使用的资源。
        /// </summary>
        private void CloseOther()
        {
            if (MouseHook.HookID != IntPtr.Zero)
            {
                MouseHook.CancelHook();
            }
            wfh.Dispose();
        }

        /// <summary>
        /// 启动scrcpy并以有线方式连接手机。
        /// </summary>
        private async Task ConnectScrcpy()
        {
            ProcessStartInfo info = new(MV.ScrcpyPath)
            {
                Arguments = $"{GetAllArguments()} --select-usb",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                //RedirectStandardInput = true,
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
                        return;
                    }
                    Console.WriteLine($">>>>>>>>>>>>>>{i++}:查找程序：{_scName}，{_prc.HasExited}。");
                }
                while (!_prc.HasExited);
                MessageBox.Show("未能成功启动程序，请重试。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 获得所有用户选择的命令行参数。
        /// </summary>
        private string GetAllArguments()
        {
            StringBuilder sb = new();

            foreach (PropertyInfo prop in typeof(MyCfg).GetProperties())
            {
                string str = GetArg(prop.Name);
                if (str != null && ConvToBool(prop.GetValue(MyCfg.Cfg)))
                {
                    sb.Append($"{str} ");
                }
                //Console.WriteLine($"属性{prop.Name}：{prop.GetValue(MyCfg.Cfg)}，{GetArg(prop.Name)}。");
            }

            _scName = $"{DateTime.Now:HHmmss}mook";
            sb.Append($"--keyboard=uhid --window-title={_scName} ");
            //Console.WriteLine($"命令行：{sb}。");
            return sb.ToString();
        }

        /// <summary>
        /// 获得指定属性（Property）的特性（Attribute），如果没有特性（Attribute）则返回null。
        /// </summary>
        /// <param name="Prop">属性（Property）的名称。</param>
        private string GetArg(string Prop)
        {
            var info = typeof(MyCfg).GetProperty(Prop).GetCustomAttribute<ArgumentsAttribute>();
            return info?.Arguments ?? null;
        }

        /// <summary>
        /// 获得指定值是否可以转化为bool，不能返回false，如果可以返回其bool值。
        /// </summary>
        private bool ConvToBool(object val)
        {
            return val is bool result && result;
        }

        /// <summary>
        /// 获得scrcpy的句柄，并将scrcpy放到panel中。
        /// </summary>
        /// <returns>true--嵌入成功；false嵌入失败。</returns>
        private bool EmbeddedForm()
        {
            foreach (var p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.Contains(_scName))
                {
                    _scHandle = p.MainWindowHandle;
            
                    HideTitleBar(_scHandle);
                    DLL.SetParent(_scHandle, pnlShow.Handle);
                    DLL.ShowWindow(_scHandle, 3);

                    Console.WriteLine($"程序信息：{_scHandle},{p.MainWindowTitle}。");
                    return true;
                }
            }

            //2024.11.15
            //IntPtr h = FindWindow(null, _scName);
            //if (h != IntPtr.Zero)
            //{
            //    _scHandle = h;
            //
            //    HideTitleBar(h);
            //    await Task.Delay(100);
            //    SetParent(h, pnlShow.Handle);
            //    await Task.Delay(100);
            //    ShowWindow(h, 3);
            //
            //    Console.WriteLine($"找到scrcpy句柄：{h}。");
            //    return true;
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
            style = style & ~WS_CAPTION;

            //删除WS_THICKFRAME样式以隐藏窗体可以调整大小的边框
            style = style & ~WS_THICKFRAME;

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
                btnMaximize.Content = "\ue923";
            }
            else
            {
                SaveTempSize();
                Width = wd; 
                Height = ht;
                Left = Top = 0;
                btnMaximize.Content = "\ue922";
            }
        }

        /// <summary>
        /// 保存当前窗体的尺寸和位置到临时变量中。
        /// </summary>
        private void SaveTempSize()
        {
            _tempSize.Width = Width;
            _tempSize.Height = Height;
            _tempSize.Left = Left;
            _tempSize.Top = Top;
        }

        /// <summary>
        /// 获得临时变量中的窗体尺寸和位置。
        /// </summary>
        private void LoadTempSize()
        {
            Width = _tempSize.Width;
            Height = _tempSize.Height;
            Left = _tempSize.Left;
            Top = _tempSize.Top;
        }

        /// <summary>
        /// 2024.11.15
        /// 改变scrcpy窗口的大小，以完全贴合父级panel，需要先最小化再最大化，防止相对坐标错误。
        /// </summary>
        private async Task ChgScrcpySize()
        {
            //Console.WriteLine($"stp大小变更:{Stp.ActualWidth}，{Stp.ActualHeight}，{Stp.Width}，{Stp.Height}。");
            //gShow.Width = Stp.ActualWidth - 50;
            //gShow.Height = Stp.ActualHeight - 70;
            //Console.WriteLine($"差:{Stp.ActualWidth - gShow.ActualWidth}，{Stp.ActualHeight - gShow.ActualHeight}。");
            Console.WriteLine($"panel的大小：{pnlShow.Width}，{pnlShow.Height}。");

            DLL.ShowWindow(_scHandle, 2);
            await Task.Delay(100);
            DLL.ShowWindow(_scHandle, 3);
        }

        /// <summary>
        /// 重新设置scrcpy窗口的大小，以完全贴合父级panel，需要先最小化再最大化，防止相对坐标错误。
        /// 如果相对坐标仍然错误，两次操作间需要增加延时，给系统足够的处理时间。
        /// </summary>
        private void ResizeSC()
        {
            Console.WriteLine($"重新设置scrcpy大小。");

            DLL.ShowWindow(_scHandle, 1);
            //await Task.Delay(100);
            DLL.ShowWindow(_scHandle, 3);
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
        /// 以有线形式启动Scrcpy。
        /// </summary>
        private async void WiredStart()
        {
            if (_scHandle == IntPtr.Zero)
            {
                btnWired.IsEnabled = false;
                await ConnectScrcpy();
                btnWired.IsEnabled = true;
            }
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

            int scWidth = (int)(ActualWidth - ExtraWidth);
            int scHeight = (int)(ActualHeight - ExtraHeight);
            int max = GetGreatestCommonDivisor(scWidth, scHeight);

            int xProportion = scWidth / max;
            int yProportion = scHeight / max;
            Console.WriteLine($"当前Scrcpy的比例：({scWidth / max}，{scHeight / max})，期望比例:({_horizontalRatio},{_verticalRatio})。");

            //计算通分后的分子，如果标准分子大，应以宽为准，调整高度；反之调整宽度
            int currentNumerator = xProportion * _verticalRatio;
            int standardNumerator = yProportion * _horizontalRatio;
            if (currentNumerator > standardNumerator)
            {
                Width = (ActualHeight - ExtraHeight) / _verticalRatio * _horizontalRatio + ExtraWidth;
            }
            else if (currentNumerator < standardNumerator)
            {
                Height = (ActualWidth - ExtraWidth) / _horizontalRatio * _verticalRatio + ExtraHeight;
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
        }

    }
}
