using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Reflection;
using ScrcpyEX.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using OpenCvSharp;

namespace ScrcpyEX.View
{
    public partial class MainWindow//test
    {
        /// <summary>
        /// 测试专用。
        /// </summary>
        private void Test_Tick(object sender, EventArgs e)
        {
            IntPtr intPtr = DLL.GetForegroundWindow();
            Console.WriteLine($"窗体/SC：{IsActive}，{intPtr == _scHandle}。");
        }

        private void BtnTest_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MyGM.POP("yesclick");
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            //var ssim = OpenCv.Compare_SSIM($@"{MV.ScriptPath}\test1.png", $@"{MV.ScriptPath}\test2.png");
            //Console.WriteLine($"相似度：{ssim}");
            //await Task.Delay(3000);
            System.Windows.Point pt1 = PointToScreen(new System.Windows.Point(0, 0));
            Console.WriteLine($"程序左上角：{pt1.X},{pt1.Y}。");

            Bitmap image = await CaptureScrcpyWindow(_scHandle, new WindowInteropHelper(this).Handle);
            if (image != null)
            {
                RecInfo recInfo = MyCfg.Cfg.RecInfos[1];
                Int32Rect cropRect = new(
                    recInfo.CropRect.X - 5,//这里要减去：scrcpy左上角减程序左上角的差
                    recInfo.CropRect.Y - 70,
                    recInfo.CropRect.Width,
                    recInfo.CropRect.Height);

                BitmapSource area = MyGM.ConvertToBitmapSource(image);
                Console.WriteLine($"非工作区：{SystemParameters.WindowNonClientFrameThickness.Left},{SystemParameters.WindowNonClientFrameThickness.Right}," +
                $"比例：{DpiScaleX},{DpiScaleY}," +
                $"起点：{cropRect.X}，{cropRect.Y}，" +
                $"宽高：{cropRect.Width},{cropRect.Height}，" +
                $"终点：{cropRect.X + cropRect.Width}，{cropRect.Y + cropRect.Height}，" +
                $"图片大小：{area.Width}，{area.Height}," +
                $"程序大小：{MV.MainWin.Width}，{MV.MainWin.Height}");

                if (cropRect.X + cropRect.Width > (int)area.Width)
                {
                    cropRect.Width -= cropRect.X + cropRect.Width - (int)area.Width;
                }
                if (cropRect.Y + cropRect.Height > (int)area.Height)
                {
                    cropRect.Height -= cropRect.Y + cropRect.Height - (int)area.Height;
                }

                CroppedBitmap temp = new(area, cropRect);
                MyGM.SavaImg("tempPic.png", temp);

                

                double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", $@"{MV.ScriptPath}\tempPic.png");
                if (ssim > 0.9)
                {
                    await ExecuteClickScript(recInfo);
                }
                Console.WriteLine($"图像脚本完毕，相似度：{ssim:F8}");
            }

            image?.Save($@"{MV.ScriptPath}\A.png", ImageFormat.Png);
            Console.WriteLine("完成");
            //MyGM.SavaImg("A.png", image);

            //Point pt1 = PointToScreen(new Point(0, 0));
            //int x = (int)(pt1.X + (_limitPop.HorizontalOffset + ExtraWidthLeft + 50) * DpiScaleX);
            //int y = (int)(pt1.Y + (_limitPop.VerticalOffset + ExtraHeightTop + 50) * DpiScaleY);
            //DLL.SetCursorPos(x, y);
            //await AdbTest1(500, 500);

            //Console.WriteLine($"工作区：{SystemParameters.WorkArea.Width}，{SystemParameters.WorkArea.Height}。");
            //Console.WriteLine($"程序宽高：{ActualWidth}，{ActualHeight}。");

            //if (_testTimer.IsEnabled)
            //{
            //    _testTimer.Stop();
            //}
            //else
            //{
            //    _testTimer.Start();
            //}
            await Task.Delay(1);
        }

        /// <summary>
        /// 使用adb发送点击命令。
        /// </summary>
        /// <param name="centreX">映射控件的圆心X坐标。</param>
        /// <param name="centreY">映射控件的圆心Y坐标。</param>
        private async void ADB_Click(int centreX, int centreY)
        {
            double x = centreX / ScActualWidth * 2312 / DpiScaleX;
            double y = centreY / ScActualHeight * 1080 / DpiScaleY;
            await AdbTest1(x, y);

            Console.WriteLine($"安卓坐标:({x},{y})，程序实际宽高：({ScActualWidth},{ScActualHeight})，");
        }

        /// <summary>
        /// 启动adb发送命令。
        /// </summary>
        private async Task AdbTest1(double x, double y)
        {
            //string adbClickCommand = $"shell input tap {x} {y}";
            string adbClickCommand = $"shell dumpsys power | grep \"mStayOn\"";

            // 创建一个新的进程启动信息
            ProcessStartInfo processStartInfo = new()
            {
                FileName = MV.ADB_Path,         //adb的路径
                Arguments = adbClickCommand,    //执行完命令后adb会直接关闭
                RedirectStandardOutput = true, // 重定向输出
                UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
                CreateNoWindow = true,          // 不创建窗口
            };
            Process adbPrc = Process.Start(processStartInfo);
            string output = adbPrc.StandardOutput.ReadToEnd();

            Console.WriteLine($"{adbClickCommand}，命令已发送，命令结果：{output}。");
            await Task.Delay(1);
        }

        //static Process _adbPrc = null;
        //static StreamWriter sw = null;
        /// <summary>
        /// 启动cmd再启动adb发送命令，只有第一次会相应，之后不能响应可能是写入流第一次后就关闭了。
        /// </summary>
        //private async Task AdbTest2(double x, double y)
        //{
        //    //string adbClickCommand = $"shell input tap {x} {y}";
        //    string adbClickCommand = $"{MV.ADB_Path} shell input tap {x} {y}";
        //    //string command = $"{MV.ADB_Path} shell input touchscreen swipe 100 101 100 100 10";
        //
        //    if (_adbPrc == null)
        //    {
        //        // 创建一个新的进程启动信息
        //        ProcessStartInfo processStartInfo = new()
        //        {
        //            FileName = "cmd.exe",         //使用命令提示符来执行ADB命令
        //            Arguments = $"/c {adbClickCommand}",    //(/c)表示执行完命令后关闭命令行窗口
        //            RedirectStandardOutput = true, // 重定向输出
        //            RedirectStandardError = true,  // 重定向错误
        //            RedirectStandardInput = true,  //重定向输入
        //            UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
        //            CreateNoWindow = true,          // 不创建窗口
        //        };
        //        _adbPrc = Process.Start(processStartInfo);
        //
        //        sw = _adbPrc.StandardInput;
        //        sw.Write(adbClickCommand);
        //        Console.WriteLine($"创建adb,{sw.BaseStream.CanWrite}.");
        //    }
        //    else
        //    {
        //        sw = _adbPrc.StandardInput;
        //        sw.Write(adbClickCommand);
        //        Console.WriteLine($"对adb发送命令,{sw.BaseStream.CanWrite}.");
        //    }
        //
        //    string output = _adbPrc.StandardOutput.ReadToEnd(); // 读取标准输出（可选
        //    string Error = _adbPrc.StandardError.ReadToEnd();
        //    await Task.Delay(1);
        //    Console.WriteLine($"输出：{output}。  错误：{Error}。{_adbPrc.HasExited}。");
        //}

        /// <summary>
        /// 通过端口发送命令。
        /// </summary>
        private async Task AdbTest3(double x, double y)
        {
            string adbClickCommand = $"shell input tap {x} {y}";

            ADBSocketCommunicator communicator = new ADBSocketCommunicator();
            await communicator.ConnectAsync();

            // 发送"help"命令以获取支持的ADB命令列表
            string helpResponse = await communicator.SendCommandAsync(adbClickCommand);
            Console.WriteLine("重定向输出:");
            Console.WriteLine(helpResponse);

            communicator.Disconnect();
        }

        /// <summary>
        /// 据说可以降低延迟，但权限被拒绝：sendevent: /dev/input/event2: Permission denied。
        /// </summary>
        private async Task AdbTest4(double x, double y)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 1 $((0x14a)) 1");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 3 $((0x30)) $((0x15))");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 3 $((0x39)) 0");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 3 $((0x35)) {x}");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 3 $((0x36)) {y}");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 0 2 0");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 0 0 0");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 1 $((0x14a)) 0");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 0 2 0");
            sb.AppendLine($"{MV.ADB_Path} shell sendevent /dev/input/event2 0 0 0");

            // 创建一个新的进程启动信息
            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",         //使用命令提示符来执行ADB命令
                Arguments = $"/c {sb}",    //(/c)表示执行完命令后关闭命令行窗口
                RedirectStandardOutput = true, // 重定向输出
                RedirectStandardError = true,  // 重定向错误
                RedirectStandardInput = true,  //重定向输入
                UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
                CreateNoWindow = true,          // 不创建窗口
            };
            Process adbPrc = Process.Start(processStartInfo);
            Console.WriteLine($"创建adb.");

            string output = adbPrc.StandardOutput.ReadToEnd(); // 读取标准输出（可选
            string Error = adbPrc.StandardError.ReadToEnd();
            await Task.Delay(1);
            Console.WriteLine($"输出：{output}。  错误：{Error}。{adbPrc.HasExited}。");
        }
    }
}
