using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.Interface
{
    public interface IDirectoryService
    {
        /// <summary> 
        /// 路径分隔符。
        /// </summary>
        char Sep { get; }

        /// <summary>
        /// 说明当前的根目录。
        /// </summary>
        string Root { get; }

        /// <summary> 
        /// 判断路径是否为根目录（无法再向上）。
        /// </summary>
        bool IsRoot(string path);

        /// <summary>
        /// 获取父目录路径。
        /// </summary>
        string GetParentPath(string path);

        /// <summary>
        /// 获取指定路径下的文件和子目录列表。
        /// </summary>
        Task<IEnumerable<FileItem>> GetDirContentAsync(string path);


    }
}
