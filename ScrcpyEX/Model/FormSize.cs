using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 保存窗体尺寸、位置信息。
    /// </summary>
    internal class FormSize
    {
        /// <summary>
        /// 是否记录的有窗体的尺寸、位置信息。
        /// </summary>
        public bool IsRecord { get; set; } = false;

        /// <summary>
        /// 记录窗体的宽度。
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 记录窗体的高度。
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 记录窗体的左侧距离。
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// 记录窗体的上侧距离。
        /// </summary>
        public double Top { get; set; }
    }
}
