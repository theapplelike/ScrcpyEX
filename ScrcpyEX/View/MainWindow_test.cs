using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DPoint = System.Drawing.Point;  //Windows Forms的Point
using WPoint = System.Windows.Point;  //WPF的Point

namespace ScrcpyEX.View
{
    public partial class MainWindow//test
    {
        /// <summary>
        /// 测试专用。
        /// </summary>
        private void Test_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            IntPtr intPtr = DLL.GetForegroundWindow();
            Dispatcher.BeginInvoke(() =>
            {
                Console.WriteLine($"窗体/SC：{IsActive}，{intPtr == ScHandle}。");
            });
        }

        private void BtnTest_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MyGM.POP("yesclick");
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            //MyGM.GetHandleRect(ScHandle);
            //Console.WriteLine($"SC的实际宽高：{ScActualWidth * DpiScaleX},{ScActualHeight *DpiScaleY},{DpiScaleX}");

            //Console.WriteLine($"比例：{ScRatioX},{ScActualWidth},{ActualWidth - ExtraWidth}");


            //闪烁截屏
            //ReduceSize();
            //RecInfo recInfo = MyCfg.Cfg.RecInfos[2];
            //TestCompareImage(recInfo.CropRect, recInfo.ImgName);
            //AmplifySize();

            //await MatrixComparison(MyCfg.Cfg.RecInfos[2]);
            //for (int i = -1; i <= 1; i++)
            //{
            //    for (int j = -1; j <= 1; j++)
            //    {
            //        Rect rect = MyCfg.Cfg.RecInfos[2].CropRect;
            //        Console.Write($"测试参数：{i},{j}。");
            //        rect.X += i;
            //        rect.Y += j;
            //        TestCompareImage(rect, MyCfg.Cfg.RecInfos[2].ImgName);
            //    }
            //        
            //}
            //Console.WriteLine($"完成//////////////。");

            //int x = (int)(pt1.X + (_limitPop.HorizontalOffset + ExtraWidthLeft + 50) * DpiScaleX);
            //int y = (int)(pt1.Y + (_limitPop.VerticalOffset + ExtraHeightTop + 50) * DpiScaleY);
            //DLL.SetCursorPos(x, y);
            //await AdbTest1();
            //Console.WriteLine($"工作区：{SystemParameters.WorkArea.Width}，{SystemParameters.WorkArea.Height}。");
            //Console.WriteLine($"程序宽高：{ActualWidth}，{ActualHeight}。");
            await Task.Delay(1);
        }

        private async void TestCompareImage(Rect rect, string imgName)
        {
            //WPoint pt1 = PointToScreen(new WPoint(0, 0));
            //Console.WriteLine($"程序左上角：{pt1.X},{pt1.Y}。");

            CroppedBitmap scBitmap = await GetScriptImg(ScHandle, _mainHandle, rect);
            Console.WriteLine($"图像脚本原始参数,起始坐标:({rect.X},{rect.Y}),宽高:({rect.Width},{rect.Height})");
            if (scBitmap != null)
            {
                double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{imgName}", scBitmap);
                Console.WriteLine($"图像脚本完毕，相似度：{ssim:F8}");
            }
            if (Debugger.IsAttached)
            {
                MyGM.SavaImg("tempPic.png", scBitmap);
            }
            //Console.WriteLine($"完成。");
        }

        /// <summary>
        /// 使用adb发送点击命令。
        /// </summary>
        /// <param name="centreX">映射控件的圆心X坐标。</param>
        /// <param name="centreY">映射控件的圆心Y坐标。</param>
        //private async void ADB_Click(int centreX, int centreY)
        //{
        //    double x = centreX / ScActualWidth * 2312 / DpiScaleX;
        //    double y = centreY / ScActualHeight * 1080 / DpiScaleY;
        //    await AdbTest1(x, y);
        //
        //    Console.WriteLine($"安卓坐标:({x},{y})，程序实际宽高：({ScActualWidth},{ScActualHeight})，");
        //}

        /// <summary>
        /// 启动adb发送命令。
        /// </summary>
        //private async Task AdbTest1()
        //{
        //    //string adbClickCommand = $"shell input tap {x} {y}";
        //    //string adbClickCommand = $"shell dumpsys power | grep \"mStayOn\"";
        //    string adbClickCommand = $"shell getprop ro.product.cpu.abi";
        //
        //    // 创建一个新的进程启动信息
        //    ProcessStartInfo processStartInfo = new()
        //    {
        //        FileName = MV.ADB_Path,         //adb的路径
        //        Arguments = adbClickCommand,    //执行完命令后adb会直接关闭
        //        RedirectStandardOutput = true, // 重定向输出
        //        UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
        //        CreateNoWindow = true,          // 不创建窗口
        //    };
        //    Process adbPrc = Process.Start(processStartInfo);
        //    string output = adbPrc.StandardOutput.ReadToEnd();
        //
        //    Console.WriteLine($"{adbClickCommand}，命令已发送，命令结果：\n{output}");
        //    await Task.Delay(1);
        //}

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
