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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        /// <summary>
        /// 获得鼠标在scrcpy中的位坐标，计算需要额外减去Point(左额外距离*Dpi比例-左非工作区距离*Dpi比例，上额外距离*Dpi比例)。
        /// 计算最大宽度和高度时需要乘以Dpi比例。
        /// </summary>
        /// <returns>如果鼠标在scrcpy中，返回鼠标在scrcpy中的坐标，否则返回null。</returns>
        public System.Windows.Point? GetMouseInScrcpy()
        {
            System.Windows.Point pt1 = PointToScreen(new Point(0, 0));//获得程序左上角相对于屏幕的位置
            System.Drawing.Point pt2 = System.Windows.Forms.Control.MousePosition;//获取鼠标相对于屏幕的位置

            int x = (int)(pt2.X - pt1.X - ExtraWidthLeft * ScaleX + SystemParameters.WindowNonClientFrameThickness.Left * ScaleX);
            int y = (int)(pt2.Y - pt1.Y - ExtraHeightTop * ScaleY);
            double maxW = (ActualWidth - ExtraWidth) * ScaleX;
            double maxH = (ActualHeight - ExtraHeight) * ScaleY;

            Console.WriteLine($"程序相对于屏幕%：({pt1.X}, {pt1.Y})，{SystemParameters.WindowNonClientFrameThickness.Left}。");
            Console.WriteLine($"鼠标相对于屏幕%：({pt2.X}, {pt2.Y})。");
            Console.WriteLine($"鼠标在程序中坐标%：({x}, {y})；scrcpy尺寸%：({maxW}, {maxH})。");
            
            if (x > 0 && x < maxW && y > 0 && y < maxH)
            {
                Console.WriteLine($"YES，鼠标在scrcpy中。");
                Console.WriteLine($"////////////////////。\n");
                return new System.Windows.Point(x, y);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 注册鼠标钩子进行录制脚本操作。
        /// </summary>
        private async void StartRecord()
        {
            //if (_scHandle == IntPtr.Zero)
            //{
            //    await popMid.SHW("未进行投屏，不能进行录制。");
            //    return;
            //}

            if (MouseHook.HookID == IntPtr.Zero)
            {
                if (MouseHook.RegisterHook())
                {
                    //btnRecManage.Content = "\ueb4a";
                    await popMid.SHW("已开启录制操作，再次点击结束录制。");
                }
                else
                {
                    await popMid.SHW("钩子注册失败，未能启动录制。");
                }
            }
            else
            {
                MouseHook.CancelHook();
                //btnRecManage.Content = "\uea40";
                await popMid.SHW("已停止录制操作。");
            }
        }

        /// <summary>
        /// 打开录制管理界面，界面返回值为true时执行脚本。
        /// </summary>
        private async void OpenRecordManage()
        {
            RecManage rec = new();
            if (rec.ShowDialog() == true)
            {
                if (_scHandle == IntPtr.Zero)
                {
                    await popMid.SHW("未进行投屏，不能执行脚本。");
                }
                else
                {
                    _pts = rec.TempRecInfo.Infos;
                    _clickTimer.Interval = TimeSpan.FromSeconds(rec.TempRecInfo.CycleInterval);
                    _clickTimer.Start();
                    Click_Tick(_pts);
                }
            }
        }


    }
}
