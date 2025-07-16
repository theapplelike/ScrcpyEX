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
using WPoint = System.Windows.Point;  //WPF的Point
using DPoint = System.Drawing.Point;  //Windows Forms的Point
using WRectangle = System.Windows.Shapes.Rectangle;
using DRectangle = System.Drawing.Rectangle;
using WBrushes = System.Windows.Media.Brushes;
using DBrushes = System.Drawing.Brushes;
using System.Reflection;

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
        //private void SetProcess()
        //{
        //    _prc.EnableRaisingEvents = true;
        //    _prc.Exited += Prc_Exited;
        //    _prc.ErrorDataReceived += Prc_ErrorDataReceived;
        //    _prc.OutputDataReceived += Prc_OutputDataReceived;
        //}
        //
        ///// <summary>
        ///// 第三方程序退出时，停止读取输出流和错误流，并重置句柄。
        ///// </summary>
        //private void Prc_Exited(object sender, EventArgs e)
        //{
        //    _prc.CancelOutputRead();
        //    _prc.CancelErrorRead();
        //    _scHandle = IntPtr.Zero;
        //    if (KeyboardHook.HookID != IntPtr.Zero)
        //    {
        //        KeyboardHook.CancelHook();
        //    }
        //
        //    Console.WriteLine($"scrcpy程序已关闭。");
        //}
        //
        ///// <summary>
        ///// 读取错误流。
        ///// </summary>
        //private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data == null)
        //    {
        //        return;
        //    }
        //    Console.WriteLine($"[E]{e.Data}");
        //
        //    if (e.Data.Contains("A popup should open on the device to request authorization."))
        //    {
        //        popMid.Show("请在设备上同意授权后再试。");
        //    }
        //}
        //
        ///// <summary>
        ///// 读取输出流。
        ///// </summary>
        //private void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data == null)
        //    {
        //        return;
        //    }
        //    Console.WriteLine($"[I]{e.Data}");
        //
        //    //获得当前分辨率，第一个参数是宽，第二个参数是高
        //    if (e.Data.Contains("Texture"))
        //    {
        //        Regex reg = new("[0-9]+");
        //        MatchCollection mc = reg.Matches(e.Data);
        //
        //        _vm.ResolutionDesc = $"投屏分辨率 {mc[0].Value}*{mc[1].Value}，{GetRatio(mc)}";
        //
        //        //DLL.ShowWindow(_scHandle, SW.SW_SHOWNORMAL);
        //        //await Task.Delay(100);
        //        //DLL.ShowWindow(_scHandle, SW.SW_SHOWMAXIMIZED);
        //        Dispatcher.Invoke(() =>
        //        {
        //            //btnMinimize.Background = WBrushes.Red;
        //            gMain.InvalidateMeasure();
        //            gMain.InvalidateArrange();
        //            gMain.InvalidateVisual();
        //            gMain.UpdateLayout();
        //            //gMain.rende
        //        });
        //        
        //        Console.WriteLine($"分辨率改变，外部");
        //    }
        //}

        /// <summary>
        /// 显示分辨率信息到界面上。
        /// </summary>
        /// <param name="texture">代表分辨率参数的字符串。</param>
        public void ShowResolutionDesc(string texture)
        {
            Regex reg = new("[0-9]+");
            MatchCollection mc = reg.Matches(texture);

            _vm.ResolutionDesc = $"投屏分辨率 {mc[0].Value}*{mc[1].Value}，{GetRatio(mc)}";
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

            //new Thread(ResizeSC).Start();
            //_resizeTimer.Stop();
        }

        /// <summary>
        /// 重新设置scrcpy窗口的大小，以完全贴合父级panel，需要先最小化再最大化，防止相对坐标错误。
        /// 如果相对坐标仍然错误，两次操作间需要增加延时，给系统足够的处理时间。
        /// </summary>
        //private async void ResizeSC()
        //{
        //    Console.WriteLine($"重新设置scrcpy大小。");
        //
        //    DLL.ShowWindow(_scHandle, SW.SW_SHOWNORMAL);
        //    await Task.Delay(100);
        //    DLL.ShowWindow(_scHandle, SW.SW_SHOWMAXIMIZED);
        //
        //    Dispatcher.Invoke(() =>
        //    {
        //        foreach (Popup pop in MV.TempPopups)
        //        {
        //            pop.IsOpen = false;
        //        }
        //        CreateSavedPop();
        //    });
        //}

        /// <summary>
        /// 执行计时脚本。
        /// </summary>
        private async void TimedScript_Tick(RecInfo recInfo)
        {
            _vm.CycleNum++;
            _stopwatch = Stopwatch.StartNew();
            await ExecuteClickScript(recInfo, ScHandle);
            //Console.WriteLine("计时脚本执行完毕...");
        }

        /// <summary>
        /// 执行图像脚本。
        /// </summary>
        private async void ImageScript_Tick(RecInfo recInfo)
        {
            //25.5.22
            //BitmapSource area = MyGM.CaptureProcessArea(DpiScaleX, DpiScaleY);
            //CroppedBitmap temp = new(area, recInfo.CropRect);
            //MyGM.SavaImg("tempPic.png", temp);
            //
            //double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", $@"{MV.ScriptPath}\tempPic.png");
            //if (ssim > 0.9)
            //{
            //    _vm.CycleNum++;
            //    _vm.Interval = 0;
            //    await ExecuteClickScript(recInfo);
            //}
            //Console.WriteLine($"图像脚本完毕，相似度：{ssim:F8}");

            Console.WriteLine($"进入新一轮，当前间隔：{_vm.Running}。");
            _scriptInterval = 5;

            CroppedBitmap scWindow = await GetScriptImg(ScHandle, _mainHandle, recInfo.CropRect);
            if (scWindow == null)
            {
                popMid.Show("获取Scrcpy脚本截图失败。");
                return;
            }

            double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", scWindow);
            if (ssim > 0.9)
            {
                if (recInfo.IsAdaptive)
                {
                    _scriptInterval = (int)(_vm.Running * 0.9);//每次将上一次间隔的90%作为自适应间隔
                    _scriptInterval = _scriptInterval < 5 ? 5 : _scriptInterval;
                }

                _vm.CycleNum++;
                _stopwatch = Stopwatch.StartNew();
                await ExecuteClickScript(recInfo, ScHandle);
            }
            Console.WriteLine($"图像脚本完毕，相似度：{ssim:F8}");

            Console.WriteLine($"自适应间隔:{_scriptInterval}秒。");
            _scriptTimer.Interval = TimeSpan.FromSeconds(_scriptInterval);
        }

        /// <summary>
        /// 执行脚本时，说明已产生的间隔。
        /// </summary>
        private void Count_Tick() => _vm.Running = (int)_stopwatch.Elapsed.TotalSeconds;

        /// <summary>
        /// 执行点击脚本，PostMessage发送鼠标消息时，通过第四个参数指定鼠标在窗口中的位置，高16位是y轴，低16位是x轴。
        /// 点击的实际位置会根据实际大小和scrcpy的初始大小的比值进行调整，但有黑边时仍会偏移。
        /// </summary>
        /// <param name="recInfo">当前要执行的脚本。</param>
        private async Task ExecuteClickScript(RecInfo recInfo, IntPtr scHandle)
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
                DLL.PostMessage(scHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, lp);
                DLL.PostMessage(scHandle, WM.WM_LBUTTONDOWN, (IntPtr)WM.MK_LBUTTON, lp);
                DLL.PostMessage(scHandle, WM.WM_LBUTTONUP, IntPtr.Zero, lp);
            }

            //如果为有限次循环，检查是否到循环次数
            if (recInfo.IsLimited && recInfo.CycleNumber <= _vm.CycleNum)
            {
                StopScriptAndRecord();
            }
        }

        /// <summary>
        /// 获得脚本的截图，Scrcpy嵌入在WPF窗体中，所以要判断主窗体是否最小化，
        /// 最小化时要显示窗口但不激活，否则最小化时窗口不会渲染。
        /// </summary>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        /// <param name="mainHandle">承载Scrcpy程序的句柄。</param>
        /// <param name="rect">脚本图片的裁剪区域。</param>
        /// <returns>返回System.Windows.Media.Imaging.CroppedBitmap类型的裁剪图。</returns>
        private async Task<CroppedBitmap> GetScriptImg(IntPtr scHandle, IntPtr mainHandle, Int32Rect rect)
        {
            //判断主窗口是否最小化
            bool isIcon = DLL.IsIconic(mainHandle);
            if (isIcon)
            {
                DLL.ShowWindow(mainHandle, SW.SW_SHOWNOACTIVATE);
                await Task.Delay(50);
            }
            //Console.WriteLine($"是否最小化:{isIcon}。");

            //获得scrcpy窗口的矩形大小
            DLL.GetWindowRect(scHandle, out DLL.LpRect rc);
            if (rc.Width <= 0 || rc.Height <= 0)
            {
                return null;
            }
            //Console.WriteLine($"Scrcpy窗口尺寸：{rc.Left},{rc.Top},{rc.Right},{rc.Bottom},{rc.Width},{rc.Height}");

            //将scrcpy的窗口复制到Bitmap中
            Bitmap bmp = new(rc.Width, rc.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                bool success = DLL.PrintWindow(scHandle, hdc, 0x2);//PW_RENDERFULLCONTENT
                g.ReleaseHdc(hdc);
                if (!success)
                {
                    return null;
                }
            }

            //计算程序左上角和Scrcpy左上角坐标的差，
            //脚本截图时以主程序为底图，以主程序的左上角为准；
            //脚本比对时以Scrcpy窗口为底图，以Scrcpy窗口的左上角为准，所以计算最后的截取区域时要加上两者的差
            BitmapSource scWindow = MyGM.ConvertToBitmapSource(bmp);
            WPoint main = PointToScreen(new WPoint(0, 0));
            WPoint diff = new(main.X - rc.Left, main.Y - rc.Top);
            //Console.WriteLine($"左上角的差：{diff.X},{diff.Y}");

            //计算对scrcpy窗口截取的区域
            Int32Rect cropRect = new(
                (int)(rect.X + diff.X),
                (int)(rect.Y + diff.Y), rect.Width, rect.Height);

            //Console.WriteLine($"非工作区：{SystemParameters.WindowNonClientFrameThickness.Left},{SystemParameters.WindowNonClientFrameThickness.Right}，" +
            //    $"比例：{DpiScaleX},{DpiScaleY}，" +
            //    $"起点：{cropRect.X},{cropRect.Y}，" +
            //    $"宽高：{cropRect.Width},{cropRect.Height}，" +
            //    $"终点：{cropRect.X + cropRect.Width},{cropRect.Y + cropRect.Height}，" +
            //    $"图片大小：{scWindow.Width},{scWindow.Height}，" +
            //    $"程序大小：{MV.MainWin.Width},{MV.MainWin.Height}");

            //检查范围，获得最终的截图区域
            CheckRect(ref cropRect, (int)scWindow.Width, (int)scWindow.Height);
            CroppedBitmap temp = new(scWindow, cropRect);
            //if (Debugger.IsAttached)
            //{
            //    MyGM.SavaImg("scWindow.png", scWindow);
            //}

            return temp;
        }

        //25.6.5
        /// <summary>
        /// 对比脚本图像和Scrcpy指定区域的图像。
        /// </summary>
        /// <param name="scWindow">整个Scrcpy窗口的截图。</param>
        /// <param name="diff">程序左上角和Scrcpy左上角坐标的差。</param>
        /// <param name="recInfo">当前要执行的脚本。</param>
        /// <returns>返回相似度，0-1之间，越高说明越相似。</returns>
        //private double CompareImage(BitmapSource scWindow, WPoint diff, RecInfo recInfo)
        //{
        //    //计算要对Scrcpy截取的区域，X和Y要加上程序左上角和Scrcpy左上角坐标的差
        //    Int32Rect rect= recInfo.CropRect;
        //    Int32Rect cropRect = new(
        //        (int)(rect.X + diff.X), 
        //        (int)(rect.Y + diff.Y), rect.Width, rect.Height);
        //
        //    //Console.WriteLine($"非工作区：{SystemParameters.WindowNonClientFrameThickness.Left},{SystemParameters.WindowNonClientFrameThickness.Right}，" +
        //    //    $"比例：{DpiScaleX},{DpiScaleY}，" +
        //    //    $"起点：{cropRect.X},{cropRect.Y}，" +
        //    //    $"宽高：{cropRect.Width},{cropRect.Height}，" +
        //    //    $"终点：{cropRect.X + cropRect.Width},{cropRect.Y + cropRect.Height}，" +
        //    //    $"图片大小：{sc.Width},{sc.Height}，" +
        //    //    $"程序大小：{MV.MainWin.Width},{MV.MainWin.Height}");
        //
        //    CheckRect(ref cropRect, (int)scWindow.Width, (int)scWindow.Height);
        //    CroppedBitmap temp = new(scWindow, cropRect);
        //    if (Debugger.IsAttached)
        //    {
        //        MyGM.SavaImg("tempPic.png", temp);
        //    }
        //
        //    return OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", temp);
        //}

        /// <summary>
        /// 如果要截取的区域超出sc图片的范围，则将值限定在范围内。
        /// 用户的截图不能超出Scrcpy的范围，因为脚本临时图片始终会被限制在Scrcpy的窗口当中。
        /// </summary>
        /// <param name="rect">说明截图范围的矩形。</param>
        /// <param name="maxWidth">最大宽度。</param>
        /// <param name="maxHeight">最大高度。</param>
        private void CheckRect(ref Int32Rect rect, int maxWidth, int maxHeight)
        {
            if (rect.X + rect.Width > maxWidth)
            {
                rect.Width -= rect.X + rect.Width - maxWidth;
            }
            if (rect.Y + rect.Height > maxHeight)
            {
                rect.Height -= rect.Y + rect.Height - maxHeight;
            }
            if (rect.X < 0)
            {
                rect.X = 0;
            }
            if (rect.Y < 0)
            {
                rect.Y = 0;
            }
        }

        /// <summary>
        /// 在一个窗口中显示脚本和对比图像。
        /// </summary>
        /// <param name="recInfo">当前执行的脚本。</param>
        private async void ImgCompare(RecInfo recInfo)
        {
            if (recInfo.ImgName == "")
            {
                popMid.Show("此脚本未截取图片，不能进行比对。");
                return;
            }

            CroppedBitmap scWindow = await GetScriptImg(ScHandle, _mainHandle, _recInfo.CropRect);
            if (scWindow == null)
            {
                popMid.Show("获取Scrcpy截图失败。");
                return;
            }

            ShowImg showImg = new(recInfo, scWindow);
            showImg.ShowDialog();
        }


    }
}
