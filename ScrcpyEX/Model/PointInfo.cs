using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 脚本坐标点信息类。
    /// </summary>
    public class PointInfo
    {
        /// <summary>
        /// 点的坐标。
        /// </summary>
        public Point Pt { get; set; }

        /// <summary>
        /// 点击的延迟，单位秒。
        /// </summary>
        public double LastTime { get; set; }

        public PointInfo() { }

        /// <summary>
        /// 创建新的脚本点击坐标。
        /// </summary>
        /// <param name="pt">System.Windows.Point类型的数据。</param>
        /// <param name="lastTime">点击的延迟，单位秒。</param>
        public PointInfo(Point pt, double lastTime)
        {
            Pt = pt;
            LastTime = lastTime;
        }

        /// <summary>
        /// 创建新的脚本点击坐标。
        /// </summary>
        /// <param name="x">X轴。</param>
        /// <param name="y">Y轴</param>
        /// <param name="lastTime">点击的延迟，单位秒。</param>
        public PointInfo(double x, double y, double lastTime)
        {
            Pt = new Point(x, y);
            LastTime = lastTime;
        }


    }
}
