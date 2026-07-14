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
        public char Sep { get; } = '/';

        public string Root { get; } = "sdcard/";

        public bool IsRoot(string path)
        {
            return path == Root;
        }

        public string GetParentPath(string curPath)
        {
            string parent = Path.GetDirectoryName(curPath).Replace('\\', '/');
            if (parent == Root.TrimEnd('/'))
            {
                parent = Root;
            }

            return parent;
        }

        /// <summary>
        /// 获取指定路径下的文件和子目录列表。
        /// </summary>
        /// <param name="tgtPath">指定的路径。</param>
        /// <returns>返回文件和子目录列表，如果ADB执行失败则返回null。</returns>
        public async Task<IEnumerable<FileItem>> GetDirContentAsync(string tgtPath)
        {
            (string otp, string Err, int ExitCode) = await MyGM.AdbMoreAsync($"shell ls -la {tgtPath}");

            if (Err != "" && ExitCode != 0)
            {
                throw new Exception($"终止值：{ExitCode}\nADB 执行错误: {Err}");
            }
            return ParseDir(otp, tgtPath.TrimEnd('/'));
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
        /// <para>
        /// ADB获得的文件列表格式：-rw-rw---- 1 root sdcard_rw 301 2026-04-04 19:25 shumei.txt
        /// </para>
        /// <para>按空格分隔分别是：</para>
        /// <para>parts[0] 权限, parts[1] 链接数, parts[2] 所有者, parts[3] 组,</para>
        /// <para>parts[4] 大小, parts[5] 日期值, parts[6] 时间值, parts[7] 名称。</para>
        /// </summary>
        /// <param name="otp">ADB获得的文件列表。</param>
        /// <param name="tgtPath">目标目录。</param>
        private IEnumerable<FileItem> ParseDir(string otp, string tgtPath)
        {
            string[] dirs = [.. otp.SplitReal(Environment.NewLine).Skip(1)];
            foreach (string di in dirs)
            {
                string[] parts = di.SplitReal(' ');
                string name = parts[7];
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


    }
}
