using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 面包屑导航项。
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>
        /// 显示的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 完整的路径。
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 是否为最后一项。
        /// </summary>
        public Visibility IsLast { get; set; }
    }
}
