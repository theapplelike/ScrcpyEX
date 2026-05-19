using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 该类描述窗体尺寸、位置信息。
    /// </summary>
    internal class FormSize
    {
        public FormSize() { }

        /// <summary>
        /// 初始化时提供尺寸和位置的值。
        /// </summary>
        public FormSize(double top, double left, double width, double height)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// 是否记录的有窗体的尺寸、位置信息。
        /// </summary>
        public bool IsRecord { get; set; } = false;

        /// <summary>
        /// 记录窗体的上侧距离。
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// 记录窗体的左侧距离。
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// 记录窗体的宽度。
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 记录窗体的高度。
        /// </summary>
        public double Height { get; set; }


    }
}
