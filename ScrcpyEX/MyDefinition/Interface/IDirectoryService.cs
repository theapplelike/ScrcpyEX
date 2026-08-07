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
        /// 当前目录是否可以执行传输任务。
        /// </summary>
        /// <returns>true--可以；false--不可以。</returns>
        bool IsCanXfer(string path);

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
        /// 删除列表中的所有文件，非文件不会删除。
        /// </summary>
        /// <param name="items">文件列表。</param>
        /// <returns>删除成功的数量。</returns>
        Task<int> DeleteFilesAsync(IEnumerable<FileItem> items);

        /// <summary>
        /// 删除列表中的所有的目录，非目录不会删除。
        /// </summary>
        /// <param name="items">目录列表。</param>
        /// <returns>删除成功的数量。</returns>
        Task<int> DeleteDirsAsync(IEnumerable<FileItem> items);

        /// <summary>
        /// 将实际路径转换为友好路径。
        /// </summary>
        /// <param name="path">要转化的路径。</param>
        /// <returns>转化后的路径。</returns>
        string ToFriendly(string path);

        /// <summary>
        /// 将友好路径转换为实际路径。
        /// </summary>
        /// <param name="path">要转化的路径。</param>
        /// <returns>转化后的路径。</returns>
        string ToReal(string path);

    }
}
