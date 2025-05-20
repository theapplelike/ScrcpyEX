using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// ShowWindow函数(winuser.h)，使用的参数。
    /// </summary>
    internal class SW
    {
        /// <summary>
        /// 隐藏窗口并激活另一个窗口。
        /// </summary>
        public const int SW_HIDE = 0;

        /// <summary>
        /// 激活并显示窗口。如果窗口最小化、最大化或排列，系统会将其还原到其原始大小和位置。
        /// 应用程序应在首次显示窗口时指定此标志。SW_NORMAL 和此值一致。
        /// </summary>
        public const int SW_SHOWNORMAL = 1;

        /// <summary>
        /// 激活窗口并将其显示为最小化窗口。
        /// </summary>
        public const int SW_SHOWMINIMIZED = 2;

        /// <summary>
        /// 激活窗口并显示最大化的窗口。SW_MAXIMIZE 和此值一致
        /// </summary>
        public const int SW_SHOWMAXIMIZED = 3;

        /// <summary>
        /// 以最近的大小和位置显示窗口。此值类似于 SW_SHOWNORMAL，只是窗口未激活。
        /// </summary>
        public const int SW_SHOWNOACTIVATE = 4;

        /// <summary>
        /// 激活窗口并以当前大小和位置显示窗口。
        /// </summary>
        public const int SW_SHOW = 5;

        /// <summary>
        /// 最小化指定的窗口，并按Z顺序激活下一个顶级窗口。
        /// </summary>
        public const int SW_MINIMIZE = 6;

        /// <summary>
        /// 将窗口显示为最小化窗口。 此值类似于 SW_SHOWMINIMIZED 但窗口未激活。
        /// </summary>
        public const int SW_SHOWMINNOACTIVE = 7;

        /// <summary>
        /// 以当前大小和位置显示窗口。 此值类似于 SW_SHOW，只是窗口未激活。
        /// </summary>
        public const int SW_SHOWNA = 8;

        /// <summary>
        /// 激活并显示窗口。如果窗口最小化、最大化或排列，系统会将其还原到其原始大小和位置。
        /// 还原最小化窗口时，应用程序应指定此标志。
        /// </summary>
        public const int SW_RESTORE = 9;

        /// <summary>
        /// 根据启动应用程序的程序传递给 CreateProcess 函数的 STARTUPINFO 结构中指定的SW_值设置显示状态。
        /// </summary>
        public const int SW_SHOWDEFAULT = 10;

        /// <summary>
        /// 最小化窗口，即使拥有窗口的线程没有响应。仅当最小化不同线程的窗口时，才应使用此标志。
        /// </summary>
        public const int SW_FORCEMINIMIZE = 11;


    }
}
