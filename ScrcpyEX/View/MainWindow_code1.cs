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

            //Console.WriteLine($"程序相对于屏幕%：({pt1.X}, {pt1.Y})，{SystemParameters.WindowNonClientFrameThickness.Left}。");
            //Console.WriteLine($"鼠标相对于屏幕%：({pt2.X}, {pt2.Y})。");
            //Console.WriteLine($"鼠标在程序中坐标%：({x}, {y})；scrcpy尺寸%：({maxW}, {maxH})。");
            
            if (x > 0 && x < maxW && y > 0 && y < maxH)
            {
                //Console.WriteLine($"YES，鼠标在scrcpy中。");
                //Console.WriteLine($"////////////////////。\n");
                return new System.Windows.Point(x, y);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 停止录制操作或停止执行脚本的计时器。
        /// </summary>
        private void RecordStop()
        {
            if (_clickTimer.IsEnabled)
            {
                _clickTimer.Stop();
                _countTimer.Stop();
                bdRec.Visibility = Visibility.Hidden;
                dplTip.Visibility = Visibility.Collapsed;
            }
            else
            {
                StartRecord(false);
            }
        }

        /// <summary>
        /// 注册鼠标钩子进行录制脚本操作，如果已注册鼠标钩子，则注销鼠标钩子并保存录制的脚本。
        /// </summary>
        /// <param name="isCanRec">如果未进行录制，是否可以进行录制操作。</param>
        private void StartRecord(bool isCanRec)
        {
            if (_scHandle == IntPtr.Zero)
            {
                popMid.Show("未进行投屏，不能进行录制。");
                return;
            }
            if (_clickTimer.IsEnabled)
            {
                popMid.Show("正在执行脚本，不能进行录制。");
                return;
            }

            if (MouseHook.HookID == IntPtr.Zero)
            {
                if (isCanRec)
                {
                    if (MouseHook.RegisterHook())
                    {
                        bdRec.Visibility = Visibility.Visible;
                        popMid.Show("已开启录制操作，再次点击结束录制。");
                    }
                    else
                    {
                        popMid.Show("钩子注册失败，未能启动录制。");
                    }
                }
            }
            else
            {
                MouseHook.CancelHook();
                bdRec.Visibility = Visibility.Hidden;
                popMid.Show("已停止录制操作。");
            }
        }

        /// <summary>
        /// 打开录制管理界面，界面返回值为true时执行脚本。
        /// </summary>
        private void OpenRecordManage()
        {
            RecManage rec = new();
            if (rec.ShowDialog() == true)
            {
                if (_scHandle == IntPtr.Zero)
                {
                    popMid.Show("未进行投屏，不能执行脚本。");
                }
                else
                {
                    if (MouseHook.HookID != IntPtr.Zero)
                    {
                        popMid.Show("已开启录制操作，不能执行脚本。");
                        return;
                    }

                    bdRec.Visibility = Visibility.Visible;
                    dplTip.Visibility = Visibility.Visible;
                    _vm.CycleNum = 0;

                    _pts = rec.TempRecInfo.Infos;
                    _clickTimer.Interval = TimeSpan.FromSeconds(rec.TempRecInfo.CycleInterval);
                    _clickTimer.Start();
                    _countTimer.Start();
                    Click_Tick(_pts);//立刻执行一次脚本
                }
            }
        }


    }
}
