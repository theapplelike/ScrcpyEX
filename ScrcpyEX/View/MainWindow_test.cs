using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.Quality;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static ScrcpyEX.MyDefinition.WindowsAPI.VK;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
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
        /// adb开启USB网络-y
        /// 虚拟屏幕能否呈现为宽屏-京东等应用不支持分屏，在虚拟屏幕上运行会强制回到主屏幕
        /// 启动时如果程序在屏幕外，定位回屏幕内
        /// </summary>
        //private void Test_Tick(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    //IntPtr hwnd = DLL.GetForegroundWindow();
        //    Dispatcher.BeginInvoke(() =>
        //    {
        //        //Debug.WriteLine($"窗体/SC：{IsActive}，{hwnd}，{hwnd == ScHandle}。");
        //    });
        //}

        /// <summary>
        /// 测试用的小屏无黑边尺寸205,136
        /// await MyGM.AdbAsync("devices -l");
        /// </summary>
        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);
            await MyGM.AdbMoreAsync($"devices -l");
            //await MyGM.AdbMoreAsync($"shell ls -l /sdcard/");
            //await MyGM.AdbMoreAsync($"shell ls -la /sdcard/");
            //await MyGM.AdbMoreAsync($"shell echo $SECONDARY_STORAGE");
            //await MyGM.AdbMoreAsync($"shell ls -la storage/A409-11E9/");
            //await MyGM.AdbMoreAsync($"shell setprop service.adb.tcp.port -1");
            //(string otp, string _, int exc) = await MyGM.AdbMoreAsync($"shell getprop service.adb.tcp.port");
            //MyGM.POP($"service.adb.tcp.port：{otp}，终止值：{exc}");

            //PicTest(@"F:\new\Pic\测试");
            //PicTest(@"F:\new\Pic\截图5");
            //PicTest(@"F:\new\Pic\截图6");
            //PicTest(@"F:\new\Pic\fox");
            //PicTest(@"F:\new\Pic\拍照");
            //PicTest(@"F:\new\Pic\按钮");
        }

        private void PicTest(string root)
        {
            string[] files = Directory.GetFiles(root);
            //int i = 0;

            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                string name = Path.GetFileName(file);
                Debug.WriteLine($"- - - - -{name}");
            
                //OpenCv.CompareSSIM(@$"{root}\原始{ext}", file);
                //OpenCv.CalculateRgbHist(@$"{root}\原始{ext}", file);
                //OpenCv.CalculateHsvHist(@$"{root}\原始{ext}", file);
                //OpenCv.CalculateHuMoment(@$"{root}\原始{ext}", file);
                OpenCv.CalculateSIFT(@$"{root}\原始{ext}", file);
                OpenCv.CalculateORB(@$"{root}\原始{ext}", file);
            
                //MatchUITemplate(@$"{root}\原始{ext}", file, @$"{root}\result{++i}-{name}");
            }
        }

        public double MatchUITemplate(string targetPath, string templatePath, string savePath)
        {
            if (targetPath == templatePath) return 1;

            using Mat target = Cv2.ImRead(targetPath, ImreadModes.Grayscale);
            using Mat template = Cv2.ImRead(templatePath, ImreadModes.Grayscale);

            // 如果截图（目标）比模板还小，绝对匹配不到（直接返回0）
            if (target.Width < template.Width || target.Height < template.Height)
            {
                Debug.WriteLine("目标图像太小，无法包含完整的模板图标。");
                return 0.0;
            }

            using Mat result = new();

            // 模板匹配算法：使用归一化相关系数匹配法 (0~1)，最适合UI识别
            Cv2.MatchTemplate(target, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

            // 截取匹配的框坐标
            OpenCvSharp.Rect matchRect = new(maxLoc.X, maxLoc.Y, template.Width, template.Height);

            if (maxVal > 0.85)
            {
                // 把识别出来的框画在图上
                using Mat colorTarget = Cv2.ImRead(targetPath, ImreadModes.Color);
                Cv2.Rectangle(colorTarget, matchRect, Scalar.Red, 2);
                Cv2.ImWrite(savePath, colorTarget);
                Debug.WriteLine($"模板匹配相似度: {maxVal:F4}，匹配位置: {maxLoc}");
            }
            else
            {
                Debug.WriteLine($"匹配失败: {maxVal:F4}，匹配位置: {maxLoc}");
            }

            return maxVal;
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
