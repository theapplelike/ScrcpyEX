using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.MyDefinition
{
    internal class DLL
    {
        /// <summary>
        /// 获得一个与给定名称相匹配的顶层窗口的句柄，不查找子窗口，查找时不区分大小写。至少需要其中一个参数。
        /// 若要从指定的子窗口开始搜索子窗口，请使用FindWindowEx函数。
        /// </summary>
        /// <param name="lpClassName">要查找窗口的类。比如笔记本的类是"Notepad"。</param>
        /// <param name="lpWindowName">要查找窗口的标题。如果该参数为空，则匹配所有窗口。<</param>
        /// <returns>成功 -- 返回匹配窗口的句柄；失败 -- 返回值为NULL。</returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 更改指定子窗口的父窗口。
        /// </summary>
        /// <param name="hWndChild">作为子窗口的句柄。</param>
        /// <param name="hWndNewParent">新父窗口的句柄。如果此参数为 NULL，桌面窗口将成为新的父窗口。如果此参数 HWND_MESSAGE，则子窗口将成为仅消息窗口。</param>
        /// <returns>如果函数成功，则返回值是上一个父窗口的句柄。如果函数失败，则返回值为NULL。</returns>
        [DllImport("User32.dll", EntryPoint = "SetParent")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// 设置指定窗口的显示状态。
        /// </summary>
        /// <param name="hwnd">窗口的句柄。</param>
        /// <param name="nCmdShow">控制窗口的显示方式。更多参考：https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-showwindow。</param>
        /// <returns>如果窗口以前可见，则返回值为非零值。如果以前隐藏窗口，则返回值为零。</returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

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
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool repaint);

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
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 检索有关指定窗口的信息。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="nIndex">要检索的值的从零开始的偏移量。值的含义参考：https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-getwindowlonga。</param>
        /// <returns>如果函数成功，则返回值是请求的值。如果函数失败，则返回值为零。 </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 将消息置于消息队列中，该消息与创建指定窗口的线程相关联，并返回，而无需等待线程处理消息。
        /// </summary>
        /// <param name="hWnd">句柄，消息不会发布到子窗口。</param>
        /// <param name="Msg">要发布的消息，系统提供的消息列表参阅：https://learn.microsoft.com/zh-cn/windows/win32/winmsg/about-messages-and-message-queues。</param>
        /// <param name="wParam">其他特定于消息的信息，取值根据第二个参数Msg而定。</param>
        /// <param name="lParam">其他特定于消息的信息，取值根据第二个参数Msg而定。</param>
        /// <returns>函数成功--返回值为非零；函数失败--返回值为零。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 检索前台窗口的句柄， 即用户当前正在使用的窗口。
        /// </summary>
        /// <returns>返回值是前台窗口的句柄。在某些情况下（例如，当窗口丢失激活时），前台窗口可以为NULL。</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 指向RECT结构的指针，该结构接收窗口左上角和右下角的屏幕坐标。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LpRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public readonly int Width => Right - Left;
            public readonly int Height => Bottom - Top;
        }

        /// <summary>
        /// 检索指定窗口的边界矩形的尺寸。尺寸以相对于屏幕左上角的屏幕坐标提供。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <param name="lpRect">指向RECT结构的指针，该结构接收窗口左上角和右下角的屏幕坐标。</param>
        /// <returns>true--函数成功；false--函数失败，若要获取扩展的错误信息，请调用GetLastError。</returns>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out LpRect lpRect);

        /// <summary>
        /// 确定指定的窗口是否最小化。
        /// </summary>
        /// <param name="hWnd">窗口的句柄。</param>
        /// <returns>true--指定窗口为最小化；false--指定窗口不为最小化。</returns>
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        /// <summary>
        /// PrintWindow 函数将可视窗口复制到指定的设备上下文。
        /// </summary>
        /// <param name="hwnd">要复制的窗口的句柄。</param>
        /// <param name="hdcBlt">设备上下文的句柄。</param>
        /// <param name="nFlags">绘图选项。0x1，捕获窗口的客户区，即窗口内部内容，排除标题栏、边框等非客户区。0x2，捕获窗口的完整内容，包括非客户区（标题栏、边框等）</param>
        /// <returns>true--函数成功；false--函数失败。</returns>
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        /// <summary>
        /// 删除逻辑笔、画笔、字体、位图、区域或调色板，从而释放与对象关联的所有系统资源。删除对象后，指定的句柄不再有效。
        /// </summary>
        /// <param name="hObject">逻辑笔、画笔、字体、位图、区域或调色板的句柄。</param>
        /// <returns>true--函数执行成功；false--指定的句柄无效或当前已选择到DC中。</returns>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// 合成鼠标运动和按钮单击，参考：https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-mouse_event。
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        /// <summary>
        /// 销毁指定句柄的窗口，销毁成功后句柄会置为0。函数将 WM_DESTROY 和 WM_NCDESTROY 消息发送到窗口，以停用窗口并从窗口中删除键盘焦点。
        /// </summary>
        /// <param name="hwnd">要销毁的窗口的句柄。</param>
        /// <returns>true--函数成功；false--函数失败。</returns>
        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        /// <summary>
        /// 将键盘焦点设置为指定的窗口。 窗口必须附加到调用线程的消息队列。
        /// </summary>
        /// <param name="hWnd">将接收键盘输入的窗口的句柄。 如果此参数为 NULL，则忽略击键。</param>
        /// <returns>如果函数成功，则返回值是以前具有键盘焦点的窗口的句柄。如果hWnd参数无效或窗口未附加到调用线程的消息队列，则返回值为NULL。</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// 将字符转换为其对应的虚拟键码和Shift状态。
        /// </summary>
        /// <param name="ch">要转换的字符。</param>
        /// <returns>低字节(0-7位): 虚拟键码；高字节(8-15位): Shift状态标志位。在当前键盘布局下没有对应的虚拟键码时返回-1。</returns>
        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char ch);

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);
        //
        //[DllImport("user32.dll")] 
        //public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        //
        //[DllImport("user32.dll")]
        //public static extern int SetCursorPos(int x, int y);//让鼠标移动到指定位置

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool SetForegroundWindow(IntPtr hWnd);

    }
}
