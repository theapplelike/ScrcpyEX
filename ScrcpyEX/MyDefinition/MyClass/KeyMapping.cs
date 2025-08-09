using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 键盘映射信息类，用于保存键盘映射信息。
    /// </summary>
    internal class KeyMapping
    {
        /// <summary>
        /// 映射的按键，应为单个大写英文字母或数字。
        /// </summary>
        public string Key { get; set; } = "";

        /// <summary>
        /// Popup控件的X轴偏移。
        /// </summary>
        public double OffsetX { get; set; } = 0;

        /// <summary>
        /// Popup控件的Y轴偏移。
        /// </summary>
        public double OffsetY { get; set; } = 0;

        public KeyMapping() { }

        public KeyMapping(string key, double offsetX, double offsetY)
        {
            Key = key;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
