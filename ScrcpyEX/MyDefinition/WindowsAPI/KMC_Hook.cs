using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.MyClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static ScrcpyEX.MyDefinition.WindowsAPI.HookDLL;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Keyboard Mapping Click，按键输入映射为鼠标点击的钩子。
    /// </summary>
    internal class KMC_Hook
    {
        // 定义Windows API函数和常量
        private const int WH_KEYBOARD = 2;      //安装用于监视击键消息的hook过程
        private const int WH_KEYBOARD_LL = 13;  //安装监视低级别键盘输入事件的hook过程，获得的消息更全面

        /// <summary>
        /// 钩子句柄，为IntPtr.Zero时未注册鼠标钩子。
        /// </summary>
        private static IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// 注册监听键盘消息的句柄。
        /// </summary>
        /// <returns>true--注册成功；false--注册失败。</returns>
        public static bool RegisterHook()
        {
            //IntPtr hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            IntPtr hMod = Marshal.GetHINSTANCE(typeof(KMC_Hook).Module);
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, hMod, 0);

            return _hookId != IntPtr.Zero;
        }

        /// <summary>
        /// 注销键盘钩子。
        /// </summary>
        public static void CancelHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 键盘钩子回调函数。
        /// </summary>
        /// <param name="nCode">挂钩过程用于确定如何处理消息的代码。</param>
        /// <param name="wParam">键盘消息的标识符。</param>
        /// <param name="lParam">指向KBDLLHOOKSTRUCT结构的指针。</param>
        /// <returns>CallNextHookEx函数的处理值。</returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (int)wParam == WM.KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                //Debug.WriteLine($"Key:{(Keys)vkCode}/{vkCode},wParam:{(int)wParam:X}。");

                MV.MainWin.KeyboardMapping((Keys)vkCode);
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }


    }
}
