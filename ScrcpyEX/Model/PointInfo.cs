using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 录制的坐标点信息。
    /// </summary>
    public class PointInfo
    {
        public PointInfo() { }

        public PointInfo(Point pt, double lastTime)
        {
            Pt = pt;
            LastTime = lastTime;
        }

        /// <summary>
        /// 点的坐标。
        /// </summary>
        public Point Pt {  get; set; }

        /// <summary>
        /// 距离上一次点击的时间。
        /// </summary>
        public double LastTime { get; set; }
    }
}
