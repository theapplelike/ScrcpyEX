using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// ADB目录服务。
    /// </summary>
    internal class AdbDirectoryService : IDirectoryService
    {
        /// <summary>
        /// Android 存储空间中系统固定的目录，不在此目录中的判定为SD卡。
        /// </summary>
        private readonly List<string> _sysDir = ["emulated", "self", "sdcard0", "sdcard"];

        /// <summary>
        /// 友好路径字典，键是真实路径，值是友好路径。
        /// </summary>
        private readonly Dictionary<string, string> _friendly = [];

        public char Sep { get; } = '/';

        public string Root { get; } = "Android";

        public bool IsRoot(string path)
        {
            return path == Root || path.TrimEnd(Sep) == Root;
        }

        public bool IsCanXfer(string path)
        {
            return !IsRoot(path);
        }

        public string ToFriendly(string path)
        {
            //从长目录开始比对
            foreach (KeyValuePair<string, string> kvp in _friendly.OrderByDescending(k => k.Key.Length))
            {
                if (path.StartsWith(kvp.Key, StringComparison.Ordinal))
                {
                    return kvp.Value + path.Substring(kvp.Key.Length);
                }
            }

            return path;
        }

        public string ToReal(string path)
        {
            foreach (KeyValuePair<string, string> kvp in _friendly.OrderByDescending(k => k.Value.Length))
            {
                if (path.StartsWith(kvp.Value, StringComparison.Ordinal))
                {
                    return kvp.Key + path.Substring(kvp.Value.Length);
                }
            }

            return path;
        }

        /// <summary>
        /// 获取父目录路径。
        /// </summary>
        /// <param name="path">当前的路径。</param>
        /// <returns>父目录路径，如果为根目录或者内部存储，直接返回根目录。</returns>
        public string GetParentPath(string path)
        {
            if (IsRoot(path) || path == "/sdcard/") return Root;
            string parent = Path.GetDirectoryName(path.TrimEnd(Sep)).Replace('\\', '/');
            string actual = $"{parent.TrimEnd(Sep)}{Sep}";
            if (actual == "/storage/") return Root;

            return actual;
        }

        /// <summary>
        /// 获取指定路径下的文件和子目录列表。
        /// </summary>
        /// <param name="path">指定的路径。</param>
        /// <returns>返回文件和子目录列表，如果ADB执行失败则返回null。</returns>
        public async Task<IEnumerable<FileItem>> GetDirContentAsync(string path)
        {
            if (IsRoot(path) || path == "")
            {
                return await GetStorage();
            }
            else
            {
                return await GetDirContent(path);
            }
        }

        public async Task<int> DeleteFilesAsync(IEnumerable<FileItem> items)
        {
            if (items.Count() <= 0) return 0;
            return await DeleteItemsAsync(items.Where(i => !i.IsDirectory), false);
        }

        public async Task<int> DeleteDirsAsync(IEnumerable<FileItem> items)
        {
            if (items.Count() <= 0) return 0;
            return await DeleteItemsAsync(items.Where(i => i.IsDirectory), true);
        }

        /// <summary>
        /// 获得路径下的子目录和文件，-la会读取隐藏文件。
        /// </summary>
        /// <param name="path">指定的路径。</param>
        /// <returns>路径下子目录和文件的集合。</returns>
        private async Task<IEnumerable<FileItem>> GetDirContent(string path)
        {
            (string otp, string err, int exc) = await MyGM.AdbMoreAsync($"shell ls -la {path}");

            if (otp != "")
            {
                return ParseDir(otp, path.TrimEnd('/'));
            }
            if (exc != 0 && err != "")
            {
                throw new Exception($"终止值：【{exc}】\nADB 执行错误: {err}");
            }
            return [];
        }

        /// <summary>
        /// 解析时不能简单用空格分隔，因为最后一个文件名可能包含多个空格。
        /// </summary>
        /// <param name="otp">ADB的ls -la命令获得的文件列表。</param>
        /// <param name="tgtPath">目标目录。</param>
        /// <returns>FileItem的集合，使用正则表达式保证获取原始的文件名。</returns>
        private IEnumerable<FileItem> ParseDir(string otp, string tgtPath)
        {
            string[] lines = otp.SplitReal(['\r', '\n']);
            foreach (string line in lines)
            {
                if (line.TrimStart().StartsWith("total")) continue;

                Match match = MyGM.LsLineRegex.Match(line);
                if (!match.Success) continue;

                string typeChar = match.Groups[1].Value;
                string sizeStr = match.Groups[3].Value;
                string date = match.Groups[4].Value;
                string time = match.Groups[5].Value;
                string name = match.Groups[6].Value;

                if (name == "." || name == "..") continue;

                bool isDir = typeChar == "d";
                yield return new FileItem
                {
                    Name = name,
                    Size = isDir ? null : long.Parse(sizeStr),
                    Time = DateTime.Parse($"{date} {time}"),
                    FullPath = $"{tgtPath}/{name}",
                    IsDirectory = isDir,
                    Extension = Path.GetExtension(name)
                };
            }
        }

        /// <summary>
        /// <para>获得Android用户可用的最顶级根存储。</para>
        /// <para>/storage/emulated/0：固定指向机主（用户0）的空间。</para>
        /// <para>/storage/self/primary/：动态指向当前用户的空间。</para>
        /// <para>/sdcard/：动态指向当前用户的空间。</para>
        /// </summary>
        /// <returns>包括内部存储和可选的SD卡。</returns>
        private async Task<IEnumerable<FileItem>> GetStorage()
        {
            List<FileItem> items = [];
            string sdName = await GetSdCard();

            items.Add(new FileItem("内部存储", "/sdcard/"));
            _friendly["/sdcard/"] = "内部存储/";

            if (sdName != "")
            {
                items.Add(new FileItem($"SD卡({sdName})", $"/storage/{sdName}/"));
                _friendly[$"/storage/{sdName}/"] = $"SD卡({sdName})/";
            }
            return items;
        }

        /// <summary>
        /// 获得手机存储空间中SD卡的名称。
        /// </summary>
        /// <returns>不存在SD卡时返回空。</returns>
        private async Task<string> GetSdCard()
        {
            (string otp, string err, int exc) = await MyGM.AdbMoreAsync("shell ls /storage/");
            string[] entries = otp.SplitReal(['\r', '\n']);

            if (exc != 0 && err != "")
            {
                throw new Exception($"终止值：【{exc}】，ADB 执行错误:\n {err}");
            }
            foreach (string entry in entries)
            {
                if (_sysDir.Contains(entry)) continue;
                return entry;
            }
            return string.Empty;
        }

        /// <summary>
        /// 通用的删除方法，处理异常和计数。
        /// </summary>
        /// <param name="items">要删除的文件项或目录项列表。</param>
        /// <param name="isDelDir">true--删除目录；false--删除文件。</param>
        /// <returns>删除成功的数量。</returns>
        private async Task<int> DeleteItemsAsync(IEnumerable<FileItem> items, bool isDelDir)
        {
            int deleted = 0;
            foreach (FileItem item in items)
            {
                string dir = isDelDir ? "-r" : "";
                string cmd = $"shell rm {dir} \"{EscapeAdbPath(item.FullPath)}\"";

                await MyGM.AdbAsync(cmd);
                deleted++;
            }
            return deleted;
        }

        /// <summary>
        /// 转义ADB路径中的空格、双引号、单引号等，防止被截断。
        /// </summary>
        /// <param name="path">ADB路径。</param>
        /// <returns>特殊字符被转义后的路径。</returns>
        private string EscapeAdbPath(string path)
        {
            return path.Replace(" ", "\\ ")
                       .Replace("'", "\\'")
                       .Replace("\"", "\\\"");
        }


    }
}
