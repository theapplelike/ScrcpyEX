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
        /// 获得鼠标在scrcpy中的位置，需要额外减去Point(左额外距离*Dpi比例-左非工作区距离*Dpi比例，上额外距离*Dpi比例)。
        /// </summary>
        public System.Windows.Point GetMouseInScrcpy()
        {
            System.Windows.Point pt1 = PointToScreen(new Point(0, 0));//获得程序左上角相对于屏幕的位置
            System.Drawing.Point pt2 = System.Windows.Forms.Control.MousePosition;//获取鼠标相对于屏幕的位置
            //DpiScale dpi = VisualTreeHelper.GetDpi(this);

            int x = (int)(pt2.X - pt1.X - ExtraWidthLeft * ScaleX + SystemParameters.WindowNonClientFrameThickness.Left * ScaleX);
            int y = (int)(pt2.Y - pt1.Y - ExtraHeightTop * ScaleY);

            Console.WriteLine($"程序相对于屏幕：({pt1.X}, {pt1.Y})，{SystemParameters.WindowNonClientFrameThickness.Left}。");
            Console.WriteLine($"鼠标相对于屏幕：({pt2.X}, {pt2.Y})。");
            Console.WriteLine($"鼠标在程序中坐标：({x}, {y})。\n");
            return new System.Windows.Point(x, y);
        }

        private async void CenterPop(string str)
        {
            await popMid.SHW(str);
        }

        /// <summary>
        /// 注册鼠标钩子进行录制操作。
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
                    await popMid.SHW("已开启录制操作。");
                }
                else
                {
                    await popMid.SHW("钩子注册失败，未能启动录制。");
                }
            }
        }


    }
}
