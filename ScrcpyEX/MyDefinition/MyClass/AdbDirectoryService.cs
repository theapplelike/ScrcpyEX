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
            return path == Root;
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
            if (items == null) return 0;
            int deleted = 0;
            foreach (FileItem item in items)
            {
                if (item.IsDirectory) continue;
                string output = await MyGM.AdbAsync($"shell rm -f \"{item.FullPath}\"");
                deleted++;
            }
            return deleted;
        }

        /// <summary>
        /// 获得路径下的子目录和文件。
        /// </summary>
        /// <param name="path">指定的路径。</param>
        /// <returns>路径下子目录和文件的集合。</returns>
        private async Task<IEnumerable<FileItem>> GetDirContent(string path)
        {
            (string otp, string Err, int exc) = await MyGM.AdbMoreAsync($"shell ls -la {path}");

            if (otp != "")
            {
                return ParseDir(otp, path.TrimEnd('/'));
            }
            if (Err != "" && exc != 0)
            {
                throw new Exception($"终止值：{exc}\nADB 执行错误: {Err}");
            }
            return [];
        }

        /// <summary>
        /// <para>
        /// ADB获得的文件列表格式：-rw-rw---- 1 root sdcard_rw 301 2026-04-04 19:25 shumei.txt
        /// </para>
        /// <para>按空格分隔分别是：</para>
        /// <para>parts[0] 权限, parts[1] 链接数, parts[2] 所有者, parts[3] 组,</para>
        /// <para>parts[4] 大小, parts[5] 日期值, parts[6] 时间值, parts[7] 名称。</para>
        /// <para>最后的名称可能带有空格，会被截断，需要组合索引7及之后的所有字符串。</para>
        /// </summary>
        /// <param name="otp">ADB获得的文件列表。</param>
        /// <param name="tgtPath">目标目录。</param>
        private IEnumerable<FileItem> ParseDir(string otp, string tgtPath)
        {
            string[] dirs = [.. otp.SplitReal(Environment.NewLine).Skip(1)];
            foreach (string di in dirs)
            {
                string[] parts = di.SplitReal(' ');
                string name = string.Join(" ", parts, 7, parts.Length - 7);
                if (name == "." || name == "..") continue;
                bool isDir = parts[0][0] == 'd';//权限的第一个字符为d说明为目录

                yield return new FileItem
                {
                    Name = name,
                    Size = isDir ? null : long.Parse(parts[4]),
                    Time = DateTime.Parse($"{parts[5]} {parts[6]}"),
                    FullPath = $"{tgtPath}/{name}",
                    IsDirectory = isDir,
                    Extension = Path.GetExtension(name),
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
            (string otp, _, _) = await MyGM.AdbMoreAsync("shell ls /storage/");
            string[] entries = otp.SplitReal(['\r', '\n']);

            foreach (string entry in entries)
            {
                if (_sysDir.Contains(entry)) continue;
                return entry;
            }
            return string.Empty;
        }

        


    }
}
