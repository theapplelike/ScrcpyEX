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
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using ScrcpyEX.MyDefinition.MyClass;
using System.IO;

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
            if (recInfo.Limited && recInfo.CycleNumber <= _vm.CycleNum)
            {
                StopScriptAndRecord();
            }
        }

        /// <summary>
        /// 执行脚本时，说明已产生的间隔。
        /// </summary>
        private void Count_Tick() => _vm.Interval++;

        /// <summary>
        /// 测试专用。
        /// </summary>
        private void Test_Tick(object sender, EventArgs e)
        {
            IntPtr intPtr = DLL.GetForegroundWindow();
            Console.WriteLine($"窗体/SC：{IsActive}，{intPtr == _scHandle}。");
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            //Point pt1 = PointToScreen(new Point(0, 0));
            //int x = (int)(pt1.X + (_limitPop.HorizontalOffset + ExtraWidthLeft + 50) * DpiScaleX);
            //int y = (int)(pt1.Y + (_limitPop.VerticalOffset + ExtraHeightTop + 50) * DpiScaleY);
            //DLL.SetCursorPos(x, y);
            await AdbTest4(500, 500);

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

        private void BtnTest_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MyGM.POP("yesclick");
        }

        /// <summary>
        /// 启动adb发送命令。
        /// </summary>
        private async Task AdbTest1(double x, double y)
        {
            //string adbClickCommand = $"shell input tap {x} {y}";
            string adbClickCommand = $"shell input tap";

            // 创建一个新的进程启动信息
            ProcessStartInfo processStartInfo = new()
            {
                FileName = MV.ADB_Path,         //adb的路径
                Arguments = adbClickCommand,    //执行完命令后adb会直接关闭
                UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
                CreateNoWindow = true,          // 不创建窗口
            };
            Process.Start(processStartInfo);
            Console.WriteLine($"点击命令已发送。");
            await Task.Delay(1);
        }

        static Process _adbPrc = null;
        static StreamWriter sw = null;
        /// <summary>
        /// 启动cmd再启动adb发送命令，只有第一次会相应，之后不能响应可能是写入流第一次后就关闭了。
        /// </summary>
        private async Task AdbTest2(double x, double y)
        {
            //string adbClickCommand = $"shell input tap {x} {y}";
            string adbClickCommand = $"{MV.ADB_Path} shell input tap {x} {y}";
            //string command = $"{MV.ADB_Path} shell input touchscreen swipe 100 101 100 100 10";

            if (_adbPrc == null)
            {
                // 创建一个新的进程启动信息
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = "cmd.exe",         //使用命令提示符来执行ADB命令
                    Arguments = $"/c {adbClickCommand}",    //(/c)表示执行完命令后关闭命令行窗口
                    RedirectStandardOutput = true, // 重定向输出
                    RedirectStandardError = true,  // 重定向错误
                    RedirectStandardInput = true,  //重定向输入
                    UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
                    CreateNoWindow = true,          // 不创建窗口
                };
                _adbPrc = Process.Start(processStartInfo);

                sw = _adbPrc.StandardInput;
                sw.Write(adbClickCommand);
                Console.WriteLine($"创建adb,{sw.BaseStream.CanWrite}.");
            }
            else
            {
                sw = _adbPrc.StandardInput;
                sw.Write(adbClickCommand);
                Console.WriteLine($"对adb发送命令,{sw.BaseStream.CanWrite}.");
            }

            string output = _adbPrc.StandardOutput.ReadToEnd(); // 读取标准输出（可选
            string Error = _adbPrc.StandardError.ReadToEnd();
            await Task.Delay(1);
            Console.WriteLine($"输出：{output}。  错误：{Error}。{_adbPrc.HasExited}。");
        }

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
            _adbPrc = Process.Start(processStartInfo);
            Console.WriteLine($"创建adb.");

            string output = _adbPrc.StandardOutput.ReadToEnd(); // 读取标准输出（可选
            string Error = _adbPrc.StandardError.ReadToEnd();
            await Task.Delay(1);
            Console.WriteLine($"输出：{output}。  错误：{Error}。{_adbPrc.HasExited}。");
        }


    }
}
