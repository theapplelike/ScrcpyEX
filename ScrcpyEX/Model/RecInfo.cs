using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 录制操作的信息。
    /// </summary>
    internal class RecInfo
    {
        /// <summary>
        /// 录制名称。
        /// </summary>
        public string Name { get; set; } = DateTime.Now.ToString("yyMMddhhmmss");

        /// <summary>
        /// 录制的信息列表。
        /// </summary>
        public List<PointInfo> Infos { get; set; } = [];
    }
}
