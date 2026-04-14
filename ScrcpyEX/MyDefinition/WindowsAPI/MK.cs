using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Mouse Keyboard消息类，描述当消息产生时，鼠标或键盘上各个按键的实时状态，可以组合。
    /// </summary>
    internal class MK
    {
        /// <summary>
        /// 鼠标左键被按下。
        /// </summary>
        public const uint MK_LBUTTON = 0x0001;

        /// <summary>
        /// 鼠标右键被按下。
        /// </summary>
        public const uint MK_RBUTTON = 0x0002;

        /// <summary>
        /// 键盘上的 Shift 键被按下。
        /// </summary>
        public const uint MK_SHIFT = 0x0004;

        /// <summary>
        /// 键盘上的 Ctrl 键被按下。
        /// </summary>
        public const uint MK_CONTROL = 0x0008;

        /// <summary>
        /// 鼠标中键（滚轮键）被按下。
        /// </summary>
        public const uint MK_MBUTTON = 0x0010;

        /// <summary>
        /// 鼠标的第一个扩展按键（通常是靠近手心的前进侧键）被按下。
        /// </summary>
        public const uint MK_XBUTTON1 = 0x0020;

        /// <summary>
        /// 鼠标的第二个扩展按键（通常是远离手心的后退侧键）被按下。
        /// </summary>
        public const uint MK_XBUTTON2 = 0x0040;
    }
}
