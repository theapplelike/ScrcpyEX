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
    /// 本地的目录服务。
    /// </summary>
    internal class LocalDirectoryService : IDirectoryService
    {
        public char Sep => Path.DirectorySeparatorChar;

        public string Root { get; } = @"C:\";

        public bool IsRoot(string path)
        {
            return Path.GetPathRoot(path) == path;
        }

        public string GetParentPath(string path)
        {
            if (IsRoot(path)) return path;
            return Directory.GetParent(path)?.FullName ?? path;
        }

        public async Task<IEnumerable<FileItem>> GetDirContentAsync(string path) => await Task.Run(() =>
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
                    Extension = "",
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
        });



    }
}
