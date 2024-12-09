using Newtonsoft.Json.Linq;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using static ScrcpyEX.MyDefinition.DLL;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        /// <summary>
        /// 设置第三方程序的绑定事件，且触发退出事件。
        /// </summary>
        private void SetProcess()
        {
            _prc.EnableRaisingEvents = true;
            _prc.Exited += Prc_Exited;
            _prc.ErrorDataReceived += Prc_ErrorDataReceived;
            _prc.OutputDataReceived += Prc_OutputDataReceived;
        }

        /// <summary>
        /// 第三方程序退出时，停止读取输出流和错误流，并重置句柄。
        /// </summary>
        private void Prc_Exited(object sender, EventArgs e)
        {
            _prc.CancelOutputRead();
            _prc.CancelErrorRead();
            _scHandle = IntPtr.Zero;
            Console.WriteLine($"scrcpy程序已关闭。");
        }

        /// <summary>
        /// 读取错误流。
        /// </summary>
        private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"[E]{e.Data}。");
        }

        /// <summary>
        /// 读取输出流。
        /// </summary>
        private void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Console.WriteLine($"[I]{e.Data}。");

            //获得当前分辨率
            if (e.Data.Contains("Texture"))
            {
                Regex reg = new("[0-9]+");
                MatchCollection mc = reg.Matches(e.Data);

                _vm.ResolutionDesc = $"投屏分辨率 {mc[0].Value}*{mc[1].Value}，{GetRatio(mc)}";
            }
        }

        /// <summary>
        /// 获得分辨率的最小比例值。
        /// </summary>
        private string GetRatio(MatchCollection mc)
        {
            int.TryParse(mc[0].Value, out int a);
            int.TryParse(mc[1].Value, out int b);
            int max = GetGreatestCommonDivisor(a, b);
            return $"比例 {a / max}:{b / max}";
        }

        /// <summary>
        /// 获得最大公约数。
        /// </summary>
        private int GetGreatestCommonDivisor(int a, int b)
        {
            if (a < b)
            {
                a += b;
                b = a - b;
                a -= b;
            }
            return (a % b == 0) ? b : GetGreatestCommonDivisor(a % b, b);
        }

        /// <summary>
        /// 设置计时器，在其中进行第三方程序的大小调整。
        /// </summary>
        private void SetTimer()
        {
            _resizeTimer.Tick += new EventHandler(Resize_Tick);
            _resizeTimer.Interval = TimeSpan.FromSeconds(0.1);//大小调整间隔

            _t.Tick += new EventHandler(t_Tick);
            _t.Interval = TimeSpan.FromSeconds(1);//大小调整间隔
        }

        /// <summary>
        /// 要使用Thread在线程中调整第三方程序大小，这样不会改变焦点，导致用户调整大小操作被打断。
        /// </summary>
        private void Resize_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("正在timer中修改scrcpy大小。");

            new Thread(ResizeSC).Start();
            _resizeTimer.Stop();
        }

        /// <summary>
        /// PostMessage发送鼠标消息时，通过第四个参数指定鼠标在窗口中的位置，高16位是y轴，低16位是x轴。
        /// </summary>
        private void t_Tick(object sender, EventArgs e)
        {
            IntPtr lp = new((1500 << 16) + 700);
            DLL.PostMessage(_scHandle, MP.WM_MOUSEMOVE, IntPtr.Zero, lp);
            DLL.PostMessage(_scHandle, MP.WM_LBUTTONDOWN, (IntPtr)MP.MK_LBUTTON, lp);
            DLL.PostMessage(_scHandle, MP.WM_LBUTTONUP, IntPtr.Zero, lp);
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine($"gMain的大小:{gMain.ActualWidth}，{gMain.ActualHeight}，{gMain.Width}，{gMain.Height}。");

            //IntPtr intPtr = IntPtr.Zero;

            //DLL.SwitchToThisWindow(_scHandle, true);
            //IntPtr intPtr = DLL.SetFocus(_scHandle);

            //Console.WriteLine($"焦点：{Keyboard.FocusedElement}，{_scHandle}，{intPtr}。");

            //System.Windows.Forms.SendKeys.SendWait("+K");
            //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            //Console.WriteLine($"scrcpy句柄：{btnTest.Cursor}。");

            //Point point2 = Mouse.GetPosition(btnTest);
            //Console.WriteLine("(" + point2.X + ", " + point2.Y + ")");
            //await Task.Delay(3000);

            //Console.WriteLine($"值：{p}，{(long)p:X8}。");

            _t.Start();
        }

        private void BtnTest_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MyGM.POP("yesclick");
        }


    }
}
