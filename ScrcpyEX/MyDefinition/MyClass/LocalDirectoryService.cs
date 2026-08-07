using Microsoft.VisualBasic.FileIO;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool IsCanXfer(string path)
        {
            return !IsRoot(path);
        }

        public string ToFriendly(string path) => path;

        public string ToReal(string path) => path;

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

        public async Task<int> DeleteFilesAsync(IEnumerable<FileItem> items)
        {
            return await DeleteItemsAsync(items.Where(i => !i.IsDirectory), i =>
            {
                if (File.Exists(i.FullPath))
                {
                    FileSystem.DeleteFile(i.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            });
        }

        public async Task<int> DeleteDirsAsync(IEnumerable<FileItem> items)
        {
            return await DeleteItemsAsync(items.Where(i => i.IsDirectory), i =>
            {
                if (Directory.Exists(i.FullPath))
                {
                    FileSystem.DeleteDirectory(i.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            });
        }

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
                items.Add(new FileItem(di.Name, di.Name));
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

        /// <summary>
        /// 通用的删除方法，处理异常和计数。
        /// </summary>
        /// <param name="items">要删除的文件项或目录项列表。</param>
        /// <param name="deleteAction">包含具体删除逻辑的委托。</param>
        /// <returns>删除成功的数量。</returns>
        private async Task<int> DeleteItemsAsync(IEnumerable<FileItem> items, Action<FileItem> deleteAction)
        {
            int deleted = 0;
            await Task.Run(() =>
            {
                foreach (FileItem item in items)
                {
                    try
                    {
                        deleteAction(item);//执行具体的删除逻辑
                        deleted++;
                    }
                    catch (Exception ex)
                    {
                        MyGM.RecordException(ex, $"{item.FullPath} 删除失败");
                    }
                }
            });
            return deleted;
        }


    }
}
