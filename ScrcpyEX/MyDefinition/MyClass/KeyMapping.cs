using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 按键映射类。
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
        public double OffsetX { get; set; } = 100;

        /// <summary>
        /// Popup控件的Y轴偏移。
        /// </summary>
        public double OffsetY { get; set; } = 100;

        /// <summary>
        /// Popup控件的半径。
        /// </summary>
        //public double Radius { get; set; }

        /// <summary>
        /// 映射显示在界面上的图形界面。
        /// </summary>
        //public Popup Pop { get; set; }

        public KeyMapping() { }

        public KeyMapping(string key, double offsetX, double offsetY)
        {
            Key = key;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
