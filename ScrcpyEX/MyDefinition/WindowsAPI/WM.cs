using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// WindowsMessage（Windows消息常量）类，类型统一为int。
    /// </summary>
    internal class WM
    {
        /// <summary>
        /// 当窗口的激活状态发生变化，即被激活或被停用时，发送这个消息。
        /// </summary>
        public const int ACTIVATE = 0x006;

        /// <summary>
        /// 配合SendMessage函数使用，wParam为false禁止窗口重绘，为true允许重绘。
        /// </summary>
        public const int SETREDRAW = 0x000B;

        #region 键盘L相关的消息常量。
        /// <summary>
        /// 按下一个非系统键，wParam代表虚拟键码，lParam代表额外的参数。
        /// </summary>
        public const int KEYDOWN = 0x0100;

        /// <summary>
        /// 释放一个非系统键。
        /// </summary>
        public const int KEYUP = 0x0101;

        /// <summary>
        /// 按下一个系统键，包括Alt、Alt组合键（Alt + F4；Alt + Tab等）、F10。
        /// </summary>
        public const int SYSKEYDOWN = 0x0104;

        /// <summary>
        /// 释放一个系统键，包括Alt、Alt组合键（Alt + F4；Alt + Tab等）、F10。
        /// </summary>
        public const int SYSKEYUP = 0x0105;
        #endregion

        #region 鼠标相关的消息常量
        /// <summary>
        /// 鼠标移动消息。
        /// </summary>
        public const int MOUSEMOVE = 0x0200;

        /// <summary>
        /// 按下鼠标左键消息。
        /// </summary>
        public const int LBUTTONDOWN = 0x0201;

        /// <summary>
        /// 抬起鼠标左键消息。
        /// </summary>
        public const int LBUTTONUP = 0x0202;

        public const int LBUTTONDBLCLK = 0x0203;//双击鼠标左键
        public const int RBUTTONDOWN = 0x0204;  //按下鼠标右键
        public const int RBUTTONUP = 0x0205;    //抬起鼠标右键
        public const int RBUTTONDBLCLK = 0x0206;//双击鼠标右键
        #endregion

        /// <summary>
        /// 当窗口的有效的每英寸点数 (DPI) 发生更改时发送。
        /// </summary>
        public const int DPICHANGED = 0x02E0;

        
    }
}
