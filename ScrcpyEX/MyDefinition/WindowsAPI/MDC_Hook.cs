using ScrcpyEX.ExtensionMethod;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using static ScrcpyEX.MyDefinition.WindowsAPI.DLL;
using static ScrcpyEX.MyDefinition.WindowsAPI.HookDLL;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Mouse Double Click，检测鼠标双击的钩子。
    /// </summary>
    internal class MDC_Hook
    {
        /// <summary>
        /// 安装监视低级别鼠标输入事件的hook过程，获得的消息更全面。
        /// </summary>
        private const int WH_MOUSE_LL = 14;

        /// <summary>
        /// 钩子句柄。
        /// </summary>
        private static IntPtr _hookId;

        /// <summary>
        /// 检测双击的目标窗口的句柄。
        /// </summary>
        private static IntPtr _targetHwnd;

        /// <summary>
        /// 防止委托被GC回收。
        /// </summary>
        private static HookProc _hookProc;

        /// <summary>
        /// 记录最后一次点击的时间。
        /// </summary>
        private static uint _lastClickTime = 0;

        /// <summary>
        /// 双击的最大时间间隔。
        /// </summary>
        private static readonly uint _dblClickTime = GetDoubleClickTime();

        /// <summary>
        /// 整个屏幕的设备上下文句柄（Device Context）。
        /// </summary>
        private static IntPtr _desktopDC;

        /// <summary>
        /// SC黑边的颜色。
        /// </summary>
        private static readonly Color _bg = Color.FromArgb(34, 34, 34);

        /// <summary>
        /// 双击钩子是否启用。
        /// </summary>
        public static bool Enable { get; set; } = false;

        /// <summary>
        /// 注册检测鼠标双击的钩子。
        /// </summary>
        /// <param name="targetHwnd">需要检测双击的窗口的句柄。</param>
        public static void RegisterHook(IntPtr targetHwnd)
        {
            _targetHwnd = targetHwnd;
            _hookProc = HookCallback;
            _desktopDC = GetDC(IntPtr.Zero);//获取整个屏幕的设备上下文

            IntPtr hMod = Marshal.GetHINSTANCE(typeof(MDC_Hook).Module);
            _hookId = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, hMod, 0);
        }

        /// <summary>
        /// 注销钩子。
        /// </summary>
        public static void CancelHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
            
            _hookProc = null;
            ReleaseDC(IntPtr.Zero, _desktopDC);
        }

        /// <summary>
        /// 鼠标钩子回调函数。
        /// </summary>
        /// <param name="nCode">如果为HC_ACTION(0)，表示 wParam 和 lParam 包含有效信息，否则应返回CallNextHookEx的处理值。</param>
        /// <param name="wParam">鼠标消息的标识符。例如WM_LBUTTONDOWN等常量。</param>
        /// <param name="lParam">指向MSLLHOOKSTRUCT结构的指针。</param>
        /// <returns>CallNextHookEx函数的处理值。</returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (Enable && nCode >= 0 && (uint)wParam == WM.LBUTTONDOWN)
            {
                //通过指针偏移直接读取
                int x = Marshal.ReadInt32(lParam, 0);
                int y = Marshal.ReadInt32(lParam, 4);
                uint currentTime = (uint)Marshal.ReadInt32(lParam, 16);

                // 检测双击，在SC的黑边区域双击会导致SC鼠标点击错位
                if (currentTime - _lastClickTime <= _dblClickTime)
                {
                    // 双击发生时，验证窗口
                    Point pt = new() { X = x, Y = y };
                    IntPtr hwndAtPoint = WindowFromPoint(pt);

                    if (hwndAtPoint == _targetHwnd && IsBackground(x, y))
                    //if (hwndAtPoint == _targetHwnd)
                    {
                        Debug.WriteLine($"目标窗口上检测到双击");

                        //_lastClickTime = 0; // 重置状态，防止连续触发
                        return (IntPtr)1;   // 吞掉双击的第二次点击
                    }
                }

                // 更新上一次点击信息（无论是否双击）
                _lastClickTime = currentTime;
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// 指定的坐标位置颜色是否和SC背景颜色相同。
        /// </summary>
        /// <param name="x">X坐标。</param>
        /// <param name="y">Y坐标。</param>
        /// <returns>true--相同；false--不相同。</returns>
        private static bool IsBackground(int x, int y)
        {
            uint pixel = GetPixel(_desktopDC, x, y);
            Color cl = ColorTranslator.FromWin32((int)pixel);
            
            //Debug.WriteLine($"颜色：{cl.R},{cl.G},{cl.B}");
            return cl == _bg;
        }


    }
}
