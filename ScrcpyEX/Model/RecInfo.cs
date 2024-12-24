using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// 进行有限次的循环。
        /// </summary>
        public bool Limited { get; set; } = false;

        /// <summary>
        /// 进行无限次的循环，直到手动停止。
        /// </summary>
        public bool HandStop { get; set; } = true;

        /// <summary>
        /// 指定有限次循环的次数。
        /// </summary>
        public int CycleNumber { get; set; } = 10;

        /// <summary>
        /// 指定每次循环的间隔。
        /// </summary>
        public int CycleInterval { get; set; } = 100;

        /// <summary>
        /// 录制的坐标点信息列表。
        /// </summary>
        public List<PointInfo> Infos { get; set; } = [];
    }
}
