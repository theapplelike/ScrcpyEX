using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 按键映射信息类，用于保存按键映射信息。
    /// </summary>
    internal class KeyMapping
    {
        /// <summary>
        /// 映射的字符，应为单个大写英文字母、数字、以及允许的特殊字符。
        /// </summary>
        public string Char { get; set; } = "";

        /// <summary>
        /// 映射的虚拟键码(Virtual Key Code)，是Windows定义的与硬件和键盘布局无关的键码。表示一个物理按键，而不是字符。
        /// </summary>
        public Keys VkCode { get; set; } = Keys.None;

        /// <summary>
        /// Popup控件的X轴偏移。
        /// </summary>
        public double OffsetX { get; set; } = 0;

        /// <summary>
        /// Popup控件的Y轴偏移。
        /// </summary>
        public double OffsetY { get; set; } = 0;

        public KeyMapping() { }

        /// <summary>
        /// 在中心位置创建一个包含空字符的按键映射信息类。
        /// </summary>
        /// <param name="ScActualWidth">Scrcpy的实际宽度。</param>
        /// <param name="ScActualHeight">Scrcpy的实际高度。</param>
        /// <param name="popDiameter">Popup的直径。</param>
        public KeyMapping(double ScActualWidth, double ScActualHeight, double popDiameter)
        {
            OffsetX = (ScActualWidth - popDiameter) / 2;
            OffsetY = (ScActualHeight - popDiameter) / 2;
        }

        /// <summary>
        /// 根据已有的数据创建按键映射信息类。
        /// </summary>
        /// <param name="ch">映射的字符。</param>
        /// <param name="vkCode">映射的虚拟键码(Virtual Key Code)。</param>
        /// <param name="offsetX">Popup控件的X轴偏移。</param>
        /// <param name="offsetY">Popup控件的Y轴偏移。</param>
        public KeyMapping(string ch, Keys vkCode, double offsetX, double offsetY)
        {
            Char = ch;
            VkCode = vkCode;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
