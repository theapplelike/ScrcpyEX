using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.Interface
{
    /// <summary>
    /// 目录服务接口。
    /// </summary>
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
        /// <param name="path">指定的路径。</param>
        /// <returns>指定路径下的文件和子目录列表。</returns>
        Task<IEnumerable<FileItem>> GetDirContentAsync(string path);

        /// <summary>
        /// 删除列表中的所有文件。
        /// </summary>
        /// <param name="items">文件列表。</param>
        /// <returns>删除成功的数量。</returns>
        Task<int> DeleteFilesAsync(IEnumerable<FileItem> items);


    }
}
