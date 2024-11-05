using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.View
{
    public partial class MainWindow//var
    {
        Process prc = new();

        /// <summary>
        /// scrcpy的句柄，为-1时代表scrcpy没有启动。
        /// </summary>
        private IntPtr scrcpyHandle = (IntPtr)(-1);

        /// <summary>
        /// scrcpy窗口的名称。
        /// </summary>
        private string scrcpyName;

        /// <summary>
        /// 判断顶级窗体是否在调整大小。
        /// </summary>
        private bool isResize;

        /// <summary>
        /// 获取窗体的句柄函数
        /// </summary>
        /// <param name="lpClassName">窗口类名</param>
        /// <param name="lpWindowName">窗口标题名</param>
        /// <returns>返回句柄</returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 更改指定子窗口的父窗口。
        /// </summary>
        /// <param name="hWndChild">作为子窗口的句柄。</param>
        /// <param name="hWndNewParent">新父窗口的句柄。如果此参数为 NULL，桌面窗口将成为新的父窗口。如果此参数 HWND_MESSAGE，则子窗口将成为仅消息窗口。</param>
        /// <returns>如果函数成功，则返回值是上一个父窗口的句柄。</returns>
        [DllImport("User32.dll", EntryPoint = "SetParent")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// 设置指定窗口的显示状态。
        /// </summary>
        /// <param name="hwnd">窗口的句柄。</param>
        /// <param name="nCmdShow">控制窗口的显示方式。更多参考：https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-showwindow。</param>
        /// <returns>如果窗口以前可见，则返回值为非零值。如果以前隐藏窗口，则返回值为零。</returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        /// <summary>
        /// 更改指定窗口的位置和尺寸。对于顶级窗口，位置和尺寸是相对于屏幕左上角的。对于子窗口，它们相对于父窗口工作区的左上角。
        /// </summary>
        /// <param name="hwnd">窗口的句柄。</param>
        /// <param name="x">窗口左侧的新位置。</param>
        /// <param name="y">窗口顶部的新位置。</param>
        /// <param name="nWidth">窗口的新宽度。</param>
        /// <param name="nHeight">窗口的新高度。</param>
        /// <param name="repaint">指示是否重新绘制窗口。如果此参数为TRUE，则窗口将收到消息。如果参数为FALSE，则不会进行任何类型的重新绘制。这适用于工作区、非工作区 (包括标题栏和滚动条) ，以及由于移动子窗口而发现父窗口的任何部分。</param>
        /// <returns>如果该函数成功，则返回值为非零值。如果函数失败，则返回值为零。 要获得更多的错误信息，请调用 GetLastError。</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool repaint);

        /// <summary>
        /// 将焦点切换到指定的窗口，并将其带到前台，最小化时会将窗口弹出。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="fAltTab">TRUE--指示窗口正切换到使用Alt/Ctl+Tab键序列；否则此参数应为FALSE。</param>
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 更改指定窗口的属性。 
        /// </summary>
        /// <param name="hWnd">窗口的句柄，以及窗口所属的类的间接句柄。</param>
        /// <param name="nIndex">要设置的值的从零开始的偏移量。更多参考：https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-setwindowlonga。</param>
        /// <param name="dwNewLong">替换值。</param>
        /// <returns>如果函数成功，则返回值为指定 32 位整数的上一个值。如果函数失败，则返回值为零。</returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 检索有关指定窗口的信息。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="nIndex">要检索的值的从零开始的偏移量。更多参考：https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-getwindowlonga。</param>
        /// <returns>如果函数成功，则返回值是请求的值。如果函数失败，则返回值为零。 </returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //用于指定要获取的窗口样式部分
        private const int GWL_STYLE = -16;
        //是窗口标题栏的样式标志
        private const int WS_CAPTION = 0xC00000;
        //允许通过拖动边缘来调整窗口大小
        private const int WS_THICKFRAME = 0x00040000;
        //在窗口的标题栏中添加“最大化”按钮
        private const int WS_MAXIMIZEBOX = 0x00010000;


    }
}
