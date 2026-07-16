using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Shapes;
using static ScrcpyEX.MyDefinition.WindowsAPI.VK;
using DPoint = System.Drawing.Point;  //Windows Forms的Point
using WPoint = System.Windows.Point;  //WPF的Point

namespace ScrcpyEX.View
{
    public partial class MainWindow//test
    {
        /// <summary>
        /// 实现模拟器一样的方向控制
        /// *minitouch在华为nova 4e上没有权限运行，minitouch服务运行不起来
        /// *STFService.apk可以运行，但是发送指令没有反应
        /// 
        /// 双击黑边SC会消失但不会关闭--目前SC消失时不能获得消息，使用双击拦截临时解决
        /// 横屏时拦截双击体验差，能否只拦截黑色区域的双击--会有20ms左右的延迟-y
        /// border和scrcpy相同大小，监听SDL的消息-x
        /// 虚拟屏幕-y
        /// com.bilibili.azurlane
        /// 标题栏每个按钮快捷键-y
        /// adb开启USB网络-y
        /// sc突然断线要清理资源-y
        /// sc有焦点时显示图标-y
        /// 缩放在竖屏下有问题-y
        /// 缩放和位置变更都会重置pop，在缩放和位置同时改变时，只执行其中一个重置-y
        /// 宽度很宽但高度很小时，pop垂直方向会在界面外，原因是偏移都用宽的比例-y
        /// 按钮实现Alt+O快捷键-y
        /// 虚拟屏幕能否呈现为宽屏-京东等应用不支持分屏，在虚拟屏幕上运行会强制回到主屏幕
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

        /// <summary>
        /// 测试用的小屏无黑边尺寸205,136
        /// await MyGM.AdbAsync("devices");
        /// </summary>
        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);
            //await MyGM.AdbMoreAsync($"shell sm list-volumes public");
            //await MyGM.AdbMoreAsync($"shell ls /storage/");
            //await MyGM.AdbMoreAsync($"shell echo $SECONDARY_STORAGE");
            new DataSync().ShowDialog();
        }

        //private void AdbPersistent()
        //{
        //    if (_persistentShell == null)
        //    {
        //        InitPersistentShell();
        //    }
        //
        //    MV.St = Stopwatch.StartNew();
        //    CheckInputShown();
        //    Debug.WriteLine($"持久化：{MV.St.ElapsedMilliseconds}ms");
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
