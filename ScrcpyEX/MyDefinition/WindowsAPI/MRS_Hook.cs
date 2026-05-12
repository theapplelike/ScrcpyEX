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
using static ScrcpyEX.MyDefinition.WindowsAPI.HookDLL;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Mouse Record Script，录制鼠标点击操作的钩子。
    /// </summary>
    internal class MRS_Hook
    {
        // 安装监视低级别鼠标输入事件的hook过程，获得的消息更全面
        private const int WH_MOUSE_LL = 14;

        /// <summary>
        /// 存储每一次有效的坐标点。
        /// </summary>
        private static readonly List<PointInfo> _points = [];

        /// <summary>
        /// 钩子句柄，为IntPtr.Zero时未注册鼠标钩子。
        /// </summary>
        public static IntPtr HookID { get; set; } = IntPtr.Zero;

        /// <summary>
        /// 注册监听鼠标消息的句柄。
        /// </summary>
        /// <returns>true--注册成功；false--注册失败。</returns>
        public static bool RegisterHook()
        {
            //IntPtr hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            IntPtr hMod = Marshal.GetHINSTANCE(typeof(MRS_Hook).Module);
            HookID = SetWindowsHookEx(WH_MOUSE_LL, HookCallback, hMod, 0);

            return HookID != IntPtr.Zero;
        }

        /// <summary>
        /// 注销钩子。
        /// </summary>
        public static void CancelHook()
        {
            if (HookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(HookID);
                HookID = IntPtr.Zero;
            }
            SaveRecordInfo();
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
                    case WM.LBUTTONDOWN: RecordPosition(); break;
                    //case MP.WM_LBUTTONUP: break;
                }
            }
            return CallNextHookEx(HookID, nCode, wParam, lParam);
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


    }
}
