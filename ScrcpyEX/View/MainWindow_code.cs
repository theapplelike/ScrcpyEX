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

namespace ScrcpyEX.View
{
    public partial class MainWindow//code
    {
        /// <summary>
        /// 自定义初始化。
        /// </summary>
        private void MyInit()
        {
            MV.Pop = pop;
            MV.MainWin = this;

            SetControlEvent();
            SetProcess();
            SetTimer();
            ReadCfg();
            SetUISize();
        }

        /// <summary>
        /// 设置整体的窗口大小并居中，宽=scrcpy宽+左右边距，高=scrcpy高+上下边距+标题栏高。
        /// </summary>
        private void SetUISize()
        {
            Width = _scWidth + ExtraWidth;
            Height = _scHeight + ExtraHeight;

            Left = (SystemParameters.WorkArea.Width - ActualWidth) / 2;
            Top = (SystemParameters.WorkArea.Height - ActualHeight) / 2;
            //Console.WriteLine($"窗体尺寸：{Width},{Height}。");
        }

        /// <summary>
        /// 读取配置的值给UI界面。
        /// </summary>
        private void ReadCfg()
        {
            //txtIP.Text = MyCfg.Cfg.IP;
            //chkStayAwake.Checked = MyCfg.Cfg.StayAwake;
            //chkOffScreen.Checked = MyCfg.Cfg.OffScreen;
            //chkShowTouches.Checked = MyCfg.Cfg.ShowTouches;
            //chkDeviceSleep.Checked = MyCfg.Cfg.DeviceSleep;

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
            //MyCfg.Cfg.StayAwake = chkStayAwake.Checked;
            //MyCfg.Cfg.OffScreen = chkOffScreen.Checked;
            //MyCfg.Cfg.ShowTouches = chkShowTouches.Checked;
            //MyCfg.Cfg.DeviceSleep = chkDeviceSleep.Checked;

            //MyCfg.Cfg.IP = txtIP.Text;
            MyCfg.Cfg.StayAwake = true;
            MyCfg.Cfg.OffScreen = true;
            MyCfg.Cfg.ShowTouches = false;
            MyCfg.Cfg.DeviceSleep = false;

            MyCfg.Cfg.Locate1 = (bool)rdoLoc1.IsChecked;
            MyCfg.Cfg.Locate2 = (bool)rdoLoc2.IsChecked;
            MyCfg.Cfg.Locate3 = (bool)rdoLoc3.IsChecked;
            MyCfg.Cfg.Locate4 = (bool)rdoLoc4.IsChecked;
            MyCfg.Save();
        }

        private void MyClose()
        {
            _resizeTimer.Stop();

            SaveCfg();
            ClosePrc();
            wfh.Dispose();
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
            SaveCfg();
            StringBuilder sb = new();

            foreach (PropertyInfo prop in typeof(MyCfg).GetProperties())
            {
                string str = GetArg(prop.Name);
                if (str != null && GetVal(prop.GetValue(MyCfg.Cfg)))
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
        private bool GetVal(object val)
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
                    //ShowWindow(p.MainWindowHandle, 0);
                    //IntPtr pnl = pnlShow.Handle;
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
            int GWL_STYLE = -16;//检索窗口样式
            int WS_CAPTION = 0xC00000;//是窗口标题栏的样式标志
            int WS_THICKFRAME = 0x00040000;//允许通过拖动边缘来调整窗口大小
            //在窗口的标题栏中添加“最大化”按钮
            //int WS_MAXIMIZEBOX = 0x00010000;

            int style = DLL.GetWindowLong(handle, GWL_STYLE);

            //删除WS_CAPTION样式以隐藏标题栏  
            style = style & ~WS_CAPTION;

            //删除WS_THICKFRAME样式以隐藏窗体可以调整大小的边框
            style = style & ~WS_THICKFRAME;

            //设置为新的窗口样式
            DLL.SetWindowLong(handle, GWL_STYLE, style);
        }

        /// <summary>
        /// 最大化或者还原窗口，最大化时增加6的边距。
        /// </summary>
        private void Maximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                btnMaximize.Content = "\ue923";
                gMain.Margin = new Thickness(0);
            }
            else
            {
                WindowState = WindowState.Maximized;
                btnMaximize.Content = "\ue922";
                gMain.Margin = new Thickness(6);
            }
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


    }
}
