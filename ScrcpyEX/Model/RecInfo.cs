using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 录制的脚本信息。
    /// </summary>
    public class RecInfo
    {
        /// <summary>
        /// 录制名称。
        /// </summary>
        public string Name { get; set; } = $"脚本{DateTime.Now:yyMMddhhmmss}";

        /// <summary>
        /// true--进行有限次的循环；false--进行无限次的循环，直到手动停止。
        /// </summary>
        public bool IsLimited { get; set; } = false;

        /// <summary>
        /// 指定有限次循环的次数。
        /// </summary>
        public int CycleNumber { get; set; } = 10;

        /// <summary>
        /// true--指定时间后进行下一次脚本；false--指定区域出现相似图片后进行下一次脚本。
        /// </summary>
        public bool IsTime { get; set; } = true;

        /// <summary>
        /// 指定每次循环的间隔。
        /// </summary>
        public int CycleInterval { get; set; } = 100;

        /// <summary>
        /// 局部图片的路径，默认为空字符串。
        /// </summary>
        public string ImgName { get; set; } = "";

        /// <summary>
        /// 说明局部图片位置大小的矩形。
        /// </summary>
        public Int32Rect CropRect { get; set; }

        /// <summary>
        /// 录制的坐标点信息列表。
        /// </summary>
        public List<PointInfo> Infos { get; set; } = [];
    }
}
