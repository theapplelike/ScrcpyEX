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
using System.Windows.Input;
using System.Windows.Media;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        /// <summary>
        /// 获得鼠标在scrcpy中的位置，需要额外减去坐标
        /// （左额外距离*Dpi比例-左非工作区距离*Dpi比例，上额外距离*Dpi比例）。
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

        private void GetDpi()
        {
            // 获取与当前窗口关联的 DPI 设置
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            Console.WriteLine($"DPI X: {dpi.PixelsPerInchX}, DPI Y: {dpi.PixelsPerInchY}");
            Console.WriteLine($"缩放比例: {dpi.DpiScaleX}，{dpi.DpiScaleY}");
        }

        private async void CenterPop(string str)
        {
            popMid.VerticalOffset = ExtraHeightTop - (ActualHeight - ExtraHeightTop) / 2 + 5 * ScaleX;
            await popMid.SHW(str);
        }

        private void StartRecord()
        {
            if (MouseHook.HookID == IntPtr.Zero)
            {
                if (!MouseHook.RegisterHook())
                {

                }
            }

            if (MouseHook.RegisterHook())
            {
                Console.WriteLine("yes");
            }
            else
            {
                Console.WriteLine("no");
            }
        }


    }
}
