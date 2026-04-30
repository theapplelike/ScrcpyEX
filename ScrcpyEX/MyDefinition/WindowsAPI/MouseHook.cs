using Microsoft.Win32;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// 鼠标脚本类，实现录制鼠标脚本，使用钩子实现。
    /// </summary>
    internal class MouseScript
    {
        // 定义Windows API函数和常量
        private const int WH_MOUSE = 7;         //安装监视鼠标消息的hook过程
        private const int WH_MOUSE_LL = 14;     //安装监视低级别鼠标输入事件的hook过程，获得的消息更全面

        /// <summary>
        /// 存储每一次有效的坐标点。
        /// </summary>
        private static readonly List<PointInfo> _points = [];

        /// <summary>
        /// 钩子句柄，为IntPtr.Zero时未注册鼠标钩子。
        /// </summary>
        public static IntPtr HookID { get; set; } = IntPtr.Zero;

        /// <summary>
        /// POINT结构定义点的x坐标和y坐标。
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
                    case WM.WM_LBUTTONDOWN: RecordPosition(); break;
                    //case MP.WM_LBUTTONUP: break;
                    //case MP.WM_RBUTTONDOWN: break;
                    //case MP.WM_RBUTTONUP: break;
                    //case MP.WM_MOUSEMOVE: break;
                }
            }
            return HookDLL.CallNextHookEx(HookID, nCode, wParam, lParam);
        }
        
        /// <summary>
        /// 鼠标位置有效时，记录鼠标当前位置。
        /// </summary>
        private static void RecordPosition()
        {
            Point? point = MV.MainWin.GetPointInScrcpy();
            if (point is Point pt)
            {
                _points.Add(new PointInfo(pt, 1.5));
                Debug.WriteLine($"添加点：{pt.X},{pt.Y}。");
            }
        }

        /// <summary>
        /// 注册监听鼠标消息的句柄。
        /// </summary>
        /// <returns>true--注册成功；false--注册失败。</returns>
        public static bool RegisterHook()
        {
            IntPtr hMod = HookDLL.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            HookID = HookDLL.SetMouseHook(WH_MOUSE_LL, HookCallback, hMod, 0);

            return HookID != IntPtr.Zero;
        }

        /// <summary>
        /// 注销钩子。
        /// </summary>
        public static void CancelHook()
        {
            HookDLL.UnhookWindowsHookEx(HookID);
            SaveRecordInfo();
            HookID = IntPtr.Zero;
            //Console.WriteLine($"注销钩子句柄：{HookID}。");
        }

        /// <summary>
        /// 保存录制的信息。
        /// </summary>
        private static void SaveRecordInfo()
        {
            if (_points.Count > 0)
            {
                MyCfg.Cfg.Scripts.Add(new ScriptInfo(_points));
                _points.Clear();
            }
        }

        /// <summary>
        /// 获取鼠标位置。
        /// </summary>
        /// <param name="lParam">指向MSLLHOOKSTRUCT结构的指针。</param>
        //private void GetMouseLocation(IntPtr lParam)
        //{
        //    MSLLHOOKSTRUCT mouseData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
        //    Point mPt = new(mouseData.pt.x, mouseData.pt.y);
        //
        //    Console.WriteLine($"鼠标坐标1：{mPt.X},{mPt.Y}。");
        //}


    }
}
