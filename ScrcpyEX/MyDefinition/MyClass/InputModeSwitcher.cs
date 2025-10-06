using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 输入模式切换。
    /// </summary>
    internal class InputModeSwitcher
    {
        /// <summary>
        /// 检索IME类的默认窗口句柄。
        /// </summary>
        /// <param name="hWnd">需要获得IME类的窗口的句柄。</param>
        /// <returns>如果成功，则返回IME类的默认窗口句柄，否则返回NULL。</returns>
        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        private const uint WM_IME_CONTROL = 0x0283;
        private const uint IMC_SETCONVERSIONMODE = 0x0002;
        private const uint IME_CMODE_NATIVE = 0x0001;       //中文模式
        private const uint IME_CMODE_ALPHANUMERIC = 0x0000; //英文模式

        /// <summary>
        /// 切换微软拼音到英文模式
        /// </summary>
        /// <param name="hWnd">需要需要切换模式窗口的句柄。</param>
        public static void SwitchToEnglishMode(IntPtr hWnd)
        {
            IntPtr hIMEWnd = ImmGetDefaultIMEWnd(hWnd);

            if (hIMEWnd != IntPtr.Zero)
            {
                // 设置输入法为英文模式
                DLL.SendMessage(hIMEWnd, WM_IME_CONTROL, (IntPtr)IMC_SETCONVERSIONMODE, (IntPtr)IME_CMODE_ALPHANUMERIC);
            }
        }

        /// <summary>
        /// 切换微软拼音到中文模式
        /// </summary>
        /// <param name="hWnd">需要需要切换模式窗口的句柄。</param>
        public static void SwitchToChineseMode(IntPtr hWnd)
        {
            IntPtr hIMEWnd = ImmGetDefaultIMEWnd(hWnd);

            if (hIMEWnd != IntPtr.Zero)
            {
                // 设置输入法为中文模式
                DLL.SendMessage(hIMEWnd, WM_IME_CONTROL, (IntPtr)IMC_SETCONVERSIONMODE, (IntPtr)IME_CMODE_NATIVE);
            }
        }


    }
}
