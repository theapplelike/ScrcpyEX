using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// 和钩子相关的系统函数。
    /// </summary>
    internal class HookDLL
    {
        /// <summary>
        /// 定义一个委托来表示鼠标钩子回调函数。
        /// </summary>
        /// <param name="nCode">事件状态码，如果小于0则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。</param>
        /// <param name="wParam">鼠标消息的标识符。可以是WM_LBUTTONDOWN等常量。</param>
        /// <param name="lParam">指向MSLLHOOKSTRUCT结构的指针。</param>
        /// <returns>0--允许该事件继续传播；大于0--此事件到此为止，不再向下传播。建议使用CallNextHookEx函数的处理值。</returns>
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 安装钩子来监听系统的某些类型的事件。 
        /// </summary>
        /// <param name="idHook">指定要监听的钩子类型。</param>
        /// <param name="lpfn">指向钩子过程的指针，也就是钩子触发时要执行的委托。</param>
        /// <param name="hMod">表示持有钩子函数的进程号。</param>
        /// <param name="dwThreadId">要与之关联的钩子过程的线程的标识符。 </param>
        /// <returns>函数成功，返回钩子过程的句柄；函数失败，返回null。</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetMouseHook(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// 定义一个委托来表示键盘钩子回调函数。
        /// </summary>
        /// <param name="nCode">事件状态码，如果小于0则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。</param>
        /// <param name="wParam">键盘消息的标识符。可以是WM_KEYDOWN、WM_KEYUP等常量。</param>
        /// <param name="lParam">指向KBDLLHOOKSTRUCT结构的指针。</param>
        /// <returns>使用CallNextHookEx函数的处理值。</returns>
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 安装钩子来监听系统的某些类型的事件。 
        /// </summary>
        /// <param name="idHook">指定要监听的钩子类型。</param>
        /// <param name="lpfn">指向钩子过程的指针，也就是钩子触发时要执行的委托。</param>
        /// <param name="hMod">表示持有钩子函数的进程号。</param>
        /// <param name="dwThreadId">要与之关联的钩子过程的线程的标识符。 </param>
        /// <returns>函数成功，返回钩子过程的句柄；函数失败，返回null。</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetKeyboardHook(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// 删除SetWindowsHookEx函数安装在挂钩链中的挂钩过程。
        /// </summary>
        /// <param name="hhk">要移除的挂钩的句柄。</param>
        /// <returns>true--函数成功；false--函数失败。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// 将挂钩信息传递给当前挂钩链中的下一个挂钩过程。
        /// </summary>
        /// <param name="hhk">挂钩句柄。</param>
        /// <param name="nCode">传递给当前挂钩过程的事件状态码。</param>
        /// <param name="wParam">传递给当前挂钩过程的wParam值。</param>
        /// <param name="lParam">传递给当前挂钩过程的lParam值。</param>
        /// <returns>当前挂钩过程还必须返回此值。返回值的含义取决于挂钩类型。 </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 检索指定模块的句柄，该模块必须由调用进程加载。
        /// </summary>
        /// <param name="lpModuleName">加载的模块的名称（.dll 或 .exe 文件）。</param>
        /// <returns>函数成功，返回指定模块的句柄；函数失败，返回null。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);


    }
}
