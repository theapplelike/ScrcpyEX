using ScrcpyEX.MyDefinition.WindowsAPI;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using ScrcpyEX.ExtensionMethod;
using System.Windows.Shapes;
using ScrcpyEX.Model;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code2
    {
        /// <summary>
        /// 代码设置界面控件的事件。
        /// </summary>
        private void SetControlEvent()
        {
            rdoLoc1.Click += (s, e) => Locate();
            rdoLoc2.Click += (s, e) => Locate();
            rdoLoc3.Click += (s, e) => Locate();
            rdoLoc4.Click += (s, e) => Locate();
        }

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

            //获得当前分辨率，第一个参数是宽，第二个参数是高
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
            _isLandscape = a > b;
            _horizontalRatio = a / max;
            _verticalRatio = b / max;

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

            _clickTimer.Tick += (sender, e) => Click_Tick(_recInfo);
            _clickTimer.Interval = TimeSpan.FromSeconds(1);

            _countTimer.Tick += (sender, e) => Count_Tick();
            _countTimer.Interval = TimeSpan.FromSeconds(1);

            _testTimer.Tick += new EventHandler(Test_Tick);
            _testTimer.Interval = TimeSpan.FromSeconds(1);
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
        /// 点击的实际位置会根据实际大小和scrcpy的初始大小的比值进行调整，但有黑边时仍会偏移。
        /// </summary>
        private async void Click_Tick(RecInfo recInfo)
        {
            _vm.CycleNum++;
            _vm.Interval = 0;

            //如果为有限次循环，检查是否到循环次数
            if (recInfo.Limited && recInfo.CycleNumber <= _vm.CycleNum)
            {
                StopScriptAndRecord();
            }

            foreach (PointInfo info in recInfo.Infos)
            {
                await Task.Delay(TimeSpan.FromSeconds(info.LastTime));
                //Console.WriteLine($"Timer点击：{info.LastTime}-{info.Pt.X},{info.Pt.Y}");

                double xRatio = (ActualWidth - ExtraWidth) / _scWidth;
                double yRatio = (ActualHeight - ExtraHeight) / _scHeight;
                int x = (int)(info.Pt.X * xRatio);
                int y = (int)(info.Pt.Y * yRatio);
                //Console.WriteLine($"比例：（{xRatio},{yRatio}），实际点击：（{x},{y}）。");

                IntPtr lp = new((y << 16) + x);
                DLL.PostMessage(_scHandle, MP.WM_MOUSEMOVE, IntPtr.Zero, lp);
                DLL.PostMessage(_scHandle, MP.WM_LBUTTONDOWN, (IntPtr)MP.MK_LBUTTON, lp);
                DLL.PostMessage(_scHandle, MP.WM_LBUTTONUP, IntPtr.Zero, lp);
            }
        }

        /// <summary>
        /// 执行脚本时，说明已产生的间隔。
        /// </summary>
        private void Count_Tick()
        {
            _vm.Interval++;
        }

        /// <summary>
        /// 测试专用。
        /// </summary>
        private void Test_Tick(object sender, EventArgs e)
        {
            //Point point = Mouse.GetPosition(wfh);
            //Console.WriteLine($"wfh相对于与鼠标：({point.X}, {point.Y})");
            //
            //Window window = Window.GetWindow(wfh);
            //Point point1 = wfh.TransformToAncestor(window).Transform(new Point(0, 0));
            //Console.WriteLine($"相对于屏幕：({point1.X}, {point1.Y})");

            Point point2 = PointToScreen(new Point(0, 0));
            Console.WriteLine($"程序相对于屏幕：({point2.X}, {point2.Y})");

            //Point point3 = PointFromScreen(new Point(0, 0));
            //Console.WriteLine($"：({point3.X}, {point3.Y})");

            // 获取鼠标相对于屏幕的位置
            System.Drawing.Point point4 = System.Windows.Forms.Control.MousePosition;
            Console.WriteLine($"鼠标相对于屏幕：({point4.X}, {point4.Y})");

            Console.WriteLine($"鼠标在程序中坐标：({point4.X - point2.X}, {point4.Y - point2.Y})\n");
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine($"gMain的大小:{gMain.ActualWidth}，{gMain.ActualHeight}，{gMain.Width}，{gMain.Height}。");
            Console.WriteLine($"pnlShow的大小:{pnlShow.Width}，{pnlShow.Height}。");

            //IntPtr intPtr = IntPtr.Zero;

            //DLL.SwitchToThisWindow(_scHandle, true);
            //IntPtr intPtr = DLL.SetFocus(_scHandle);

            //Console.WriteLine($"焦点：{Keyboard.FocusedElement}，{_scHandle}，{intPtr}。");

            //System.Windows.Forms.SendKeys.SendWait("+K");
            //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            //Console.WriteLine($"scrcpy句柄：{btnTest.Cursor}。");

            //await Task.Delay(3000);

            //_isRecord = true;

            //if (_clickTimer.IsEnabled)
            //{
            //    _clickTimer.Stop();
            //}
            //else
            //{
            //    _clickTimer.Start();
            //}
            //
            //System.Windows.Point pt1 = PointToScreen(new Point(0, 0));//获得程序左上角相对于屏幕的位置
            //DLL.SetCursorPos((int)(pt1.X + 5),(int)(pt1.Y + 70));
            //MyGM.POP("yesclick");
            //if (MouseHook.RegisterHook())
            //{
            //    Console.WriteLine("yes");
            //}
            //else
            //{
            //    Console.WriteLine("no");
            //}

            //将窗口水平居中点转换为屏幕坐标，PointToScreen()转化出的结果已经乘以了缩放比例
            //Point screenCenter = PointToScreen(new Point(ActualWidth / 2, 35));
            //Console.WriteLine($"{screenCenter.X},{screenCenter.Y},{ActualWidth / 2},{ActualHeight / 2}");

            // 将弹出窗口的位置设置为窗口的中心
            //pop.HorizontalOffset = (screenCenter.X / ScaleX - pop.Child.RenderSize.Width / 2) - ExtraWidthLeft;
            //pop.VerticalOffset = (screenCenter.Y / ScaleY - pop.Child.RenderSize.Height / 2);

            //Console.WriteLine($"{pop.HorizontalOffset},{pop.VerticalOffset}");
            //Console.WriteLine($"{pop.Child.RenderSize.Width},{pop.Child.RenderSize.Height}");
            //pop.HorizontalOffset = 100;
            //pop.VerticalOffset = ExtraHeightTop - (ActualHeight - ExtraHeightTop) / 2 + 5;

            //Console.WriteLine($"{_scHeight},{ActualHeight}");
            await Task.Delay(1);
        }

        private void BtnTest_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MyGM.POP("yesclick");
        }
    }
}
