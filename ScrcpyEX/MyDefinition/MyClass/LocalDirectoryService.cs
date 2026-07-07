using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 本地目录的服务。
    /// </summary>
    internal class LocalDirectoryService : IDirectoryService
    {
        public char Sep => Path.DirectorySeparatorChar;

        public string Root { get; } = @"PC";

        public bool IsRoot(string path)
        {
            return path == Root || path.TrimEnd(Sep) == Root;
        }

        /// <summary>
        /// 获取父目录路径。
        /// </summary>
        /// <param name="path">当前的路径。</param>
        /// <returns>父目录路径，如果为根目录或者盘符，直接返回根目录。</returns>
        public string GetParentPath(string path)
        {
            if (IsRoot(path) || path == Path.GetPathRoot(path)) return Root;
            return Directory.GetParent(path)?.FullName ?? path;
        }

        public async Task<IEnumerable<FileItem>> GetDirContentAsync(string path) => await Task.Run(() =>
        {
            if (IsRoot(path))
            {
                return GetDriveInfo();
            }
            else
            {
                return GetDirContent(path);
            }
        });

        /// <summary>
        /// 获得驱动器信息。
        /// </summary>
        /// <returns>逻辑驱动器信息集合。</returns>
        private IEnumerable<FileItem> GetDriveInfo()
        {
            DriveInfo[] drives = [.. DriveInfo.GetDrives().Where(d => d.IsReady)];
            List<FileItem> items = [];

            foreach (DriveInfo di in drives)
            {
                items.Add(new FileItem
                {
                    Name = di.Name,
                    FullPath = di.Name,
                    IsDirectory = true,
                });
            }
            return items;
        }

        /// <summary>
        /// 获得路径下的子目录和文件。
        /// </summary>
        /// <param name="path">指定的路径。</param>
        /// <returns>路径下子目录和文件的集合。</returns>
        private IEnumerable<FileItem> GetDirContent(string path)
        {
            List<FileItem> items = [];
            DirectoryInfo dirInfo = new(path);

            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                items.Add(new FileItem
                {
                    Name = dir.Name,
                    FullPath = dir.FullName,
                    IsDirectory = true,
                    Time = dir.LastWriteTime,
                });
            }
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                items.Add(new FileItem
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    IsDirectory = false,
                    Size = file.Length,
                    Time = file.LastWriteTime,
                    Extension = file.Extension,
                });
            }
            return items;
        }


    }
}
