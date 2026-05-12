using ScrcpyEX.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ScrcpyEX.MyDefinition.WindowsAPI.HookDLL;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Mouse Double Click，检测鼠标双击的钩子。
    /// </summary>
    internal class MDC_Hook
    {
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

        // 用于双击检测的状态
        private static uint _lastClickTime = 0;
        private static int _lastClickX = 0;
        private static int _lastClickY = 0;

        /// <summary>
        /// 安装监视低级别鼠标输入事件的hook过程，获得的消息更全面。
        /// </summary>
        private const int WH_MOUSE_LL = 14;

        /// <summary>
        /// 双击时，鼠标两次点击在 X 方向允许的最大偏移范围（像素）。
        /// </summary>
        private const int SM_CXDOUBLECLK = 36;

        /// <summary>
        /// 双击时，鼠标两次点击在 Y 方向允许的最大偏移范围（像素）。
        /// </summary>
        private const int SM_CYDOUBLECLK = 37;

        /// <summary>
        /// 注册检测鼠标双击的钩子。
        /// </summary>
        /// <param name="targetHwnd">需要检测双击的窗口的句柄。</param>
        public static void RegisterHook(IntPtr targetHwnd)
        {
            _targetHwnd = targetHwnd;
            _hookProc = HookCallback;

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
            if (nCode >= 0 && (uint)wParam == WM.LBUTTONDOWN)
            {
                // 解析低级鼠标钩子结构体
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                uint currentTime = hookStruct.time;
                int x = hookStruct.pt.x;
                int y = hookStruct.pt.y;

                // 获取系统双击参数
                uint dblClickTime = DLL.GetDoubleClickTime();
                int dblClickX = DLL.GetSystemMetrics(SM_CXDOUBLECLK);
                int dblClickY = DLL.GetSystemMetrics(SM_CYDOUBLECLK);

                // 检测双击，在SC的黑边区域双击会导致SC鼠标点击错位
                if ((currentTime - _lastClickTime <= dblClickTime) &&
                    (Math.Abs(x - _lastClickX) <= dblClickX / 2) &&
                    (Math.Abs(y - _lastClickY) <= dblClickY / 2))
                {
                    // 双击发生时，验证窗口
                    System.Drawing.Point pt = new() { X = x, Y = y };
                    IntPtr hwndAtPoint = DLL.WindowFromPoint(pt);

                    if (hwndAtPoint == _targetHwnd)
                    {
                        Debug.WriteLine("目标窗口上检测到双击");
                        
                        _lastClickTime = 0; // 重置状态，防止连续触发
                        return (IntPtr)1;   // 吞掉双击的第二次点击
                    }
                }

                // 更新上一次点击信息（无论是否双击）
                _lastClickTime = currentTime;
                _lastClickX = x;
                _lastClickY = y;
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }


    }
}
