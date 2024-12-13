using Microsoft.Win32;
using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    internal class MouseHook
    {
        // 定义一个委托来表示鼠标钩子回调函数
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        // 定义Windows API函数和常量
        private const int WH_KEYBOARD = 2;      //安装用于监视击键消息的hook过程
        private const int WH_KEYBOARD_LL = 13;  //安装监视低级别键盘输入事件的hook过程，获得的消息更全面

        // 定义Windows API函数和常量
        private const int WH_MOUSE = 7;         //安装监视鼠标消息的hook过程
        private const int WH_MOUSE_LL = 14;     //安装监视低级别鼠标输入事件的hook过程，获得的消息更全面

        /// <summary>
        /// 钩子句柄。
        /// </summary>
        public static IntPtr HookID { get; set; } = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// POINT 结构定义点的 x 坐标和 y 坐标。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 定义MOUSEHOOKSTRUCT结构体，用于接收鼠标事件数据。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            /// <summary>
            /// 坐标值结构体。
            /// </summary>
            public POINT pt;

            /// <summary>
            /// 如果消息是WM_MOUSEWHEEL，则该成员的高16位字节是滚动方向及距离，低16位字节被保留。
            /// 正值表示轮子向前滚动，远离用户；负值表示轮子向后滚动，朝向用户。
            /// 一次滚轮点击被定义为WHEEL_DELTA，即120。
            /// </summary>
            public uint mouseData;

            /// <summary>
            /// 事件注入标志。
            /// </summary>
            public uint flags;

            /// <summary>
            /// 此消息的时间戳，单位ms，自系统启动以来的相对时间值。
            /// </summary>
            public uint time;

            /// <summary>
            /// 与消息关联的其他信息。
            /// </summary>
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// 鼠标钩子回调函数。
        /// </summary>
        /// <param name="nCode">如果nCode小于零，则钩子过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。</param>
        /// <param name="wParam">鼠标消息的标识符。例如WM_LBUTTONDOWN等常量。</param>
        /// <param name="lParam">指向MSLLHOOKSTRUCT结构的指针。</param>
        /// <returns>CallNextHookEx函数的处理值。</returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // 检查鼠标事件类型
                switch ((uint)wParam)
                {
                    case MP.WM_LBUTTONDOWN: RecordPosition(lParam); break;
                    //case MP.WM_LBUTTONUP: break;
                    //case MP.WM_RBUTTONDOWN: break;
                    //case MP.WM_RBUTTONUP: break;
                    //case MP.WM_MOUSEMOVE: break;
                }
            }
            return CallNextHookEx(HookID, nCode, wParam, lParam);
        }

        private static void RecordPosition(IntPtr lParam)
        {
            Console.WriteLine("鼠标左键按下。");

            Point pt = MV.MainWin.GetMouseInScrcpy();
        }

        /// <summary>
        /// 获取鼠标位置。
        /// </summary>
        /// <param name="lParam">指向MSLLHOOKSTRUCT结构的指针。</param>
        private void GetMouseLocation(IntPtr lParam)
        {
            MSLLHOOKSTRUCT mouseData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            Point mPt = new(mouseData.pt.x, mouseData.pt.y);

            Console.WriteLine($"鼠标坐标1：{mPt.X},{mPt.Y}。");
        }

        /// <summary>
        /// 注册监听鼠标消息的句柄。
        /// </summary>
        /// <returns>是否注册成功。</returns>
        public static bool RegisterHook()
        {
            // 获取当前模块的句柄
            IntPtr hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            // 设置鼠标钩子
            HookID = SetWindowsHookEx(WH_MOUSE_LL, HookCallback, hMod, 0);

            return HookID != IntPtr.Zero;
        }

        /// <summary>
        /// 注销钩子。
        /// </summary>
        protected static void CancelHook()
        {
            UnhookWindowsHookEx(HookID);
        }


    }
}
