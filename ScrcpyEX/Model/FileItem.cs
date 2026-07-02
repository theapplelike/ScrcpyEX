using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 目录中的子目录、文件项。
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// 文件名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件大小（字节），目录为 null。
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// 修改时间。
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 完整的文件路径。
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 是否为目录。
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// 文件的扩展名，如果为目录则为空。
        /// </summary>
        public string Extension { get; set; } = "";


    }
}
