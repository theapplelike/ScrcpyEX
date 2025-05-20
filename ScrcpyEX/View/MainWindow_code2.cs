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
using System.Windows.Shapes;
using ScrcpyEX.Model;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.MyClass;

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
            if (KeyboardHook.HookID != IntPtr.Zero)
            {
                KeyboardHook.CancelHook();
            }

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
        /// 获得分辨率的最小比例值、宽和高的比例。
        /// </summary>
        private string GetRatio(MatchCollection mc)
        {
            int.TryParse(mc[0].Value, out int a);
            int.TryParse(mc[1].Value, out int b);
            int max = GetGreatestCommonDivisor(a, b);
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
        /// 设置计时器。
        /// </summary>
        private void SetTimer()
        {
            _timedHandler = (sender, e) => TimedScript_Tick(_recInfo);
            _imageHandler = (sender, e) => ImageScript_Tick(_recInfo);

            _resizeTimer.Tick += new EventHandler(Resize_Tick);
            _resizeTimer.Interval = TimeSpan.FromSeconds(0.1);//大小调整间隔

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
            //Console.WriteLine("正在timer中修改scrcpy大小。");

            new Thread(ResizeSC).Start();
            _resizeTimer.Stop();
        }

        /// <summary>
        /// 执行计时脚本。
        /// </summary>
        private async void TimedScript_Tick(RecInfo recInfo)
        {
            _vm.CycleNum++;
            _vm.Interval = 0;

            await ExecuteClickScript(recInfo);
            Console.WriteLine("计时脚本执行完毕...");
        }

        /// <summary>
        /// 执行图像脚本。
        /// </summary>
        private async void ImageScript_Tick(RecInfo recInfo)
        {
            BitmapSource area = MyGM.CaptureProcessArea(DpiScaleX, DpiScaleY);
            CroppedBitmap temp = new(area, recInfo.CropRect);
            MyGM.SavaImg("tempPic.png", temp);

            double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", $@"{MV.ScriptPath}\tempPic.png");
            if (ssim > 0.9)
            {
                _vm.CycleNum++;
                _vm.Interval = 0;
                await ExecuteClickScript(recInfo);
            }
            Console.WriteLine($"图像脚本完毕，相似度：{ssim:F8}");
        }

        /// <summary>
        /// 执行脚本时，说明已产生的间隔。
        /// </summary>
        private void Count_Tick() => _vm.Interval++;

        /// <summary>
        /// 执行点击脚本，PostMessage发送鼠标消息时，通过第四个参数指定鼠标在窗口中的位置，高16位是y轴，低16位是x轴。
        /// 点击的实际位置会根据实际大小和scrcpy的初始大小的比值进行调整，但有黑边时仍会偏移。
        /// </summary>
        /// <param name="recInfo">当前要执行的脚本。</param>
        private async Task ExecuteClickScript(RecInfo recInfo)
        {
            foreach (PointInfo info in recInfo.Infos)
            {
                await Task.Delay(TimeSpan.FromSeconds(info.LastTime));
                //Console.WriteLine($"Timer点击：{info.LastTime}-{info.Pt.X},{info.Pt.Y}");

                //保存的坐标数据都是以默认尺寸为准，使用时需要乘以当前尺寸和默认尺寸的比例
                int x = (int)(info.Pt.X * ScRatioX);
                int y = (int)(info.Pt.Y * ScRatioY);
                //Console.WriteLine($"比例：（{ScRatioX},{ScRatioY}），实际点击：（{x},{y}）。");

                IntPtr lp = new((y << 16) + x);
                DLL.PostMessage(_scHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, lp);
                DLL.PostMessage(_scHandle, WM.WM_LBUTTONDOWN, (IntPtr)WM.MK_LBUTTON, lp);
                DLL.PostMessage(_scHandle, WM.WM_LBUTTONUP, IntPtr.Zero, lp);
            }

            //如果为有限次循环，检查是否到循环次数
            if (recInfo.IsLimited && recInfo.CycleNumber <= _vm.CycleNum)
            {
                StopScriptAndRecord();
            }
        }

        public async Task<Bitmap> CaptureScrcpyWindow(IntPtr scHandle, IntPtr mainHandle)
        {
            //Scrcpy嵌入在WPF窗体中，所以要判断主窗体是否最小化
            bool isIcon = DLL.IsIconic(mainHandle);
            Console.WriteLine($"是否最小化:{isIcon}。");
            if (isIcon)
            {
                DLL.ShowWindow(mainHandle, SW.SW_SHOWNOACTIVATE);
                await Task.Delay(50);
            }

            DLL.GetWindowRect(scHandle, out DLL.LpRect rc);
            if (rc.Width <= 0 || rc.Height <= 0)
            {
                return null;
            }
            Console.WriteLine($"Scrcpy窗口尺寸：{rc.Left},{rc.Top},{rc.Right},{rc.Bottom},{rc.Width},{rc.Height}");

            Bitmap bmp = new(rc.Width, rc.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                bool success = DLL.PrintWindow(scHandle, hdc, 0x2);//PW_RENDERFULLCONTENT
                g.ReleaseHdc(hdc);
                if (!success)
                {
                    Console.WriteLine("截图获取失败");
                    return null;
                }
            }
            return bmp;
        }


    }
}
