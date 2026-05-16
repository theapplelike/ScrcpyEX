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
using System.Windows.Input;
using static ScrcpyEX.MyDefinition.WindowsAPI.HookDLL;
using Application = System.Windows.Application;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Keyboard Mapping Click，按键输入映射为鼠标点击的钩子。
    /// </summary>
    internal class KMC_Hook
    {
        //安装监视低级别键盘输入事件的hook过程，获得的消息更全面
        private const int WH_KEYBOARD_LL = 13;  

        /// <summary>
        /// 钩子句柄，为IntPtr.Zero时未注册鼠标钩子。
        /// </summary>
        private static IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// 主程序的句柄。
        /// </summary>
        private static IntPtr _mainHandle = IntPtr.Zero;

        /// <summary>
        /// 用户定义的映射键集合。
        /// </summary>
        private static HashSet<int> UserHotkeys;

        /// <summary>
        /// 程序使用的快捷键集合。
        /// </summary>
        private static readonly HashSet<int> SysHotkeys =
        [
            (int)Keys.F1,
            (int)Keys.F2,
            (int)Keys.F3,
            (int)Keys.F4,
            (int)Keys.F5,
            (int)Keys.F6,
            (int)Keys.F7,
            (int)Keys.F8,
            (int)Keys.F11,
            (int)Keys.F12
        ];

        /// <summary>
        /// 是否启用按键映射。
        /// </summary>
        public static bool IsMapping { get; set; } = false;

        /// <summary>
        /// 注册监听键盘消息的句柄。
        /// </summary>
        /// <returns>true--注册成功；false--注册失败。</returns>
        public static bool RegisterHook(IntPtr mainHandle, HashSet<int> hs)
        {
            //IntPtr hMod = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            IntPtr hMod = Marshal.GetHINSTANCE(typeof(KMC_Hook).Module);
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, hMod, 0);
            _mainHandle = mainHandle;
            UserHotkeys = hs;

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
        /// <param name="nCode">如果为HC_ACTION(0)，表示 wParam 和 lParam 包含有效信息，否则应返回CallNextHookEx的处理值。</param>
        /// <param name="wParam">键盘消息的标识符，例如WM_KEYDOWN。</param>
        /// <param name="lParam">指向KBDLLHOOKSTRUCT结构的指针。</param>
        /// <returns>CallNextHookEx函数的处理值；返回值大于0不再向下传播消息。</returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //MV.St = Stopwatch.StartNew();
                int vkCode = Marshal.ReadInt32(lParam);
                bool isSysKey = SysHotkeys.Contains(vkCode);
                bool isUserKey = UserHotkeys.Contains(vkCode);

                if ((isSysKey || isUserKey) && DLL.GetForegroundWindow() == _mainHandle)
                {
                    if (isUserKey && (int)wParam == WM.KEYDOWN && MV.MainWin.ScHandle != IntPtr.Zero)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() => MV.MainWin.KeyboardMapping((Keys)vkCode));
                    }
                    else if (isSysKey && (int)wParam == WM.KEYUP)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() => MV.MainWin.ShortcutKey((Keys)vkCode));
                    }
                    //Debug.WriteLine($"前台：{MV.St.ElapsedMilliseconds}ms");
                    // 拦截热键的按下和抬起，避免SC收到这些键
                    //return (IntPtr)1;
                }
            }
            //Debug.WriteLine($"继续：{MV.St.ElapsedMilliseconds}ms");
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }


    }
}
