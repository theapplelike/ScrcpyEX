using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 保存文件管理器数据的类。
    /// </summary>
    internal class FeData
    {
        /// <summary>
        /// 本地最后使用的目录。
        /// </summary>
        public string LastLclDir { get; set; } = "";

        /// <summary>
        /// 安卓最后使用的目录。
        /// </summary>
        public string LastAdbDir { get; set; } = "";


    }
}
