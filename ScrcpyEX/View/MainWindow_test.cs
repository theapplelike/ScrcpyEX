using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
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
        /// 横屏时拦截双击体验差，能否只拦截黑色区域的双击--会有20ms左右的延迟
        /// 双击黑边SC会消失但不会关闭--目前SC消失时不能获得消息，使用双击拦截临时解决
        /// border和scrcpy相同大小，监听SDL的消息-x
        /// 虚拟屏幕
        /// com.bilibili.azurlane
        /// 标题栏每个按钮快捷键-y
        /// adb开启USB网络-y
        /// sc突然断线要清理资源-y
        /// 添加新的按键映射后更新到KMC_Hook的热键列表-y
        /// F11会触发安卓的截图，可以考虑拦截-y
        /// sc有焦点时显示图标-y
        /// 缩放在竖屏下有问题
        /// 组图像脚本在手动比较的时候现在只传入了第一个-y
        /// 截图时放大镜在125%缩放的电脑偏移很大-y
        /// 图像脚本和自己组成组脚本，删除这个组脚本时会报错-y
        /// 缩放和位置变更都会重置pop，在缩放和位置同时改变时，只执行其中一个重置
        /// 宽度很宽但高度很小时，pop垂直方向会在界面外，原因是偏移都用宽的比例
        /// </summary>
        private void Test_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            //IntPtr hwnd = DLL.GetForegroundWindow();
            Dispatcher.BeginInvoke(() =>
            {
                //Debug.WriteLine($"窗体/SC：{IsActive}，{hwnd}，{hwnd == ScHandle}。");
            });
        }

        private void BtnTest_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MyGM.POP("yesclick");
        }

        //测试用的小屏无黑边尺寸205,136
        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);

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
        //    Debug.WriteLine($"安卓坐标:({x},{y})，程序实际宽高：({ScActualWidth},{ScActualHeight})，");
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
        //    Debug.WriteLine($"{adbClickCommand}，命令已发送，命令结果：\n{output}");
        //    await Task.Delay(1);
        //}

        /// <summary>
        /// 通过端口发送命令。
        /// </summary>
        //private async Task AdbTest3(double x, double y)
        //{
        //    string adbClickCommand = $"shell input tap {x} {y}";
        //
        //    ADBSocketCommunicator communicator = new();
        //    await communicator.ConnectAsync();
        //
        //    // 发送"help"命令以获取支持的ADB命令列表
        //    string helpResponse = await communicator.SendCommandAsync(adbClickCommand);
        //    Debug.WriteLine("重定向输出:");
        //    Debug.WriteLine(helpResponse);
        //
        //    communicator.Disconnect();
        //}
        
        //private void AdbPersistent()
        //{
        //    if (_persistentShell == null)
        //    {
        //        InitPersistentShell();
        //    }
        //    CheckInputShown();
        //}
        //
        //private static Process _persistentShell = null;
        //private static StreamWriter _shellWriter;
        //private static StreamReader _shellReader;
        //
        ///// <summary>
        ///// 初始化持久化 shell。
        ///// </summary>
        //private void InitPersistentShell()
        //{
        //    _persistentShell = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = MV.ADB_Path,
        //            Arguments = "shell",//让adb启动一个持久的远程shell会话
        //            UseShellExecute = false,
        //            RedirectStandardInput = true,
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            CreateNoWindow = true
        //        }
        //    };
        //    _persistentShell.Start();
        //    _shellWriter = _persistentShell.StandardInput;
        //    _shellReader = _persistentShell.StandardOutput;
        //}
        //
        ///// <summary>
        ///// 发送命令并读取直到特定结束标记
        ///// </summary>
        ///// <returns></returns>
        //private bool CheckInputShown()
        //{
        //    // 发送命令，末尾加一个唯一 echo 标记来定位结束行
        //    string marker = Guid.NewGuid().ToString();
        //    _shellWriter.WriteLine($"dumpsys input_method | grep mInputShown; echo {marker}");
        //    _shellWriter.Flush();
        //
        //    string line;
        //    while ((line = _shellReader.ReadLine()) != null)
        //    {
        //        if (line.Contains(marker))
        //        {
        //            Debug.WriteLine($"跳出循环，{line}");
        //            break;
        //        } 
        //        if (line.Contains("mInputShown=true"))
        //        {
        //            Debug.WriteLine($"输出行：{line}");
        //            return true;
        //        }
        //    }
        //    
        //    return false;
        //}

    }
}
