using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// Virtual-Key，虚拟键码。https://learn.microsoft.com/zh-cn/windows/win32/inputdev/virtual-key-codes
    /// </summary>
    internal class VK
    {
        /// <summary>
        /// Shift 键，只能判断Shift按下，无法判断是左右Shift。
        /// </summary>
        public const uint VK_SHIFT = 0x10;

        /// <summary>
        /// Alt 键，只能判断Alt按下，无法判断是左右Alt。
        /// </summary>
        public const uint VK_MENU = 0x12;

        /// <summary>
        /// O 键的虚拟键码。
        /// </summary>
        public const uint VK_O = 0x4F;

        /// <summary>
        /// 左Alt键，非扩展键。
        /// </summary>
        public const uint VK_LMENU = 0xA4;

        /// <summary>
        /// 右Alt键，扩展键。
        /// </summary>
        public const uint VK_RMENU = 0xA5;
    }
}
