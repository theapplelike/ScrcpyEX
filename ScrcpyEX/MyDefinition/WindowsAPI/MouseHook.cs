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
    internal class MouseHook
    {
        // 定义Windows API函数和常量
        private const int WH_KEYBOARD = 2;      //安装用于监视击键消息的hook过程
        private const int WH_KEYBOARD_LL = 13;  //安装监视低级别键盘输入事件的hook过程，获得的消息更全面

        // 定义Windows API函数和常量
        private const int WH_MOUSE = 7;         //安装监视鼠标消息的hook过程
        private const int WH_MOUSE_LL = 14;     //安装监视低级别鼠标输入事件的hook过程，获得的消息更全面

        /// <summary>
        /// 存储每一次有效的坐标点。
        /// </summary>
        private static readonly List<PointInfo> infos = [];

        /// <summary>
        /// 钩子句柄，为IntPtr.Zero时未注册鼠标钩子。
        /// </summary>
        public static IntPtr HookID { get; set; } = IntPtr.Zero;

        /// <summary>
        /// 定义一个委托来表示鼠标钩子回调函数。
        /// </summary>
        /// <param name="nCode">事件状态码，如果小于0则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。</param>
        /// <param name="wParam">鼠标消息的标识符。例如WM_LBUTTONDOWN等常量。</param>
        /// <param name="lParam">指向MSLLHOOKSTRUCT结构的指针。</param>
        /// <returns>0--允许该事件继续传播；大于0--此事件到此为止，不再向下传播。建议使用CallNextHookEx函数的处理值。</returns>
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 安装钩子来监听系统的某些类型的事件。 
        /// </summary>
        /// <param name="idHook">指定要监听的钩子类型。</param>
        /// <param name="lpfn">指向钩子过程的指针，也就是钩子触发时要执行的委托。</param>
        /// <param name="hMod">表示持有钩子函数的进程号。</param>
        /// <param name="dwThreadId">要与之关联的钩子过程的线程的标识符。 </param>
        /// <returns>函数成功，返回钩子过程的句柄；函数失败，返回null。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// 删除SetWindowsHookEx函数安装在挂钩链中的挂钩过程。
        /// </summary>
        /// <param name="hhk">要移除的挂钩的句柄。</param>
        /// <returns>true--函数成功；false--函数失败。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// 将挂钩信息传递给当前挂钩链中的下一个挂钩过程。
        /// </summary>
        /// <param name="hhk">挂钩句柄。</param>
        /// <param name="nCode">传递给当前挂钩过程的事件状态码。</param>
        /// <param name="wParam">传递给当前挂钩过程的wParam值。</param>
        /// <param name="lParam">传递给当前挂钩过程的lParam值。</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 检索指定模块的句柄，该模块必须由调用进程加载。
        /// </summary>
        /// <param name="lpModuleName">加载的模块的名称（.dll 或 .exe 文件）。</param>
        /// <returns>函数成功，返回指定模块的句柄；函数失败，返回null。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

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
                    case MP.WM_LBUTTONDOWN: RecordPosition(); break;
                    //case MP.WM_LBUTTONUP: break;
                    //case MP.WM_RBUTTONDOWN: break;
                    //case MP.WM_RBUTTONUP: break;
                    //case MP.WM_MOUSEMOVE: break;
                }
            }
            return CallNextHookEx(HookID, nCode, wParam, lParam);
        }
        
        /// <summary>
        /// 鼠标位置有效时，记录鼠标当前位置。
        /// </summary>
        private static void RecordPosition()
        {
            Point? point = MV.MainWin.GetMouseInScrcpy();
            if (point is Point pt)
            {
                infos.Add(new PointInfo(pt, 1.5));
                Console.WriteLine($"添加点：{pt.X},{pt.Y}。");
            }
        }

        /// <summary>
        /// 注册监听鼠标消息的句柄。
        /// </summary>
        /// <returns>true--注册成功；false--注册失败。</returns>
        public static bool RegisterHook()
        {
            IntPtr hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            HookID = SetWindowsHookEx(WH_MOUSE_LL, HookCallback, hMod, 0);

            return HookID != IntPtr.Zero;
        }

        /// <summary>
        /// 注销钩子。
        /// </summary>
        public static void CancelHook()
        {
            UnhookWindowsHookEx(HookID);
            SaveRecordInfo();
            HookID = IntPtr.Zero;
            Console.WriteLine($"注销钩子句柄：{HookID}。");
        }

        /// <summary>
        /// 保存录制的信息。
        /// </summary>
        private static void SaveRecordInfo()
        {
            if (infos.Count > 0)
            {
                //infos[0].LastTime = 0;
                MyCfg.Cfg.RecInfos.Add(new RecInfo() { Infos = new List<PointInfo>(infos) });
                infos.Clear();
            }
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


    }
}
