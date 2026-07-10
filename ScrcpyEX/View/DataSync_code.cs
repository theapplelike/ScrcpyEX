using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ScrcpyEX.View
{
    public partial class DataSync//code
    {
        /// <summary>
        /// 自定义初始化。
        /// </summary>
        private void MyInit()
        {

        }

        /// <summary>
        /// 检查本地和ADB间是否可以发送文件。
        /// </summary>
        /// <returns>true--可以发送文件；false--不能发送文件。</returns>
        private bool IsCanSend()
        {
            return fleLcl.IsCanSend && fleAdb.IsCanSend;
        }

        /// <summary>
        /// 在指定的本地目录和ADB设备目录中发送文件。
        /// </summary>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        /// <param name="desc">说明文字。</param>
        private async Task SendFile(bool isPush, string desc)
        {
            if (!IsCanSend())
            {
                MyGM.POP($"设备未正确连接，不能{desc}文件。");
                return;
            }

            List<string> files = await GetFiles(isPush);
            if (files.Count == 0)
            {
                MyGM.POP($"所有文件已存在，无需{desc}。");
                return;
            }

            await TransmitFile(files, isPush);
            if (isPush)
            {
                fleAdb.Reload();
            }
            else
            {
                fleLcl.Reload();
            }
            MyGM.POP($"文件{desc}完成。");
        }

        /// <summary>
        /// 同步本地目录和安卓目录中的所有文件。
        /// </summary>
        private async Task SyncFile()
        {
            if (!IsCanSend())
            {
                MyGM.POP("设备未正确连接，不能同步文件。");
                return;
            }

            List<string> toPush = await GetFiles(true);
            List<string> toPull = await GetFiles(false);

            if (toPush.Count > 0)
            {
                await TransmitFile(toPush, true);
                fleAdb.Reload();
            }
            if (toPull.Count > 0)
            {
                await TransmitFile(toPull, false);
                fleLcl.Reload();
            }
            MyGM.POP("双方目录中的文件同步完成。");
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
        /// <returns>字符串中解析出来的所有文件名的集合。</returns>
        private HashSet<string> ParseFile(string otp)
        {
            return [.. otp
                    .SplitReal(Environment.NewLine)
                    .Skip(1) //跳过 total 行
                    .Select(line => line.SplitReal(' '))
                    .Where(parts => parts.Length >= 8 && parts[0][0] == '-')//只保留普通文件(权限首字符为'-')
                    .Select(parts => parts[7])
                    .Where(name => name != "." && name != "..")];
        }

        /// <summary>
        /// 获取要推送/拉取的文件列表。
        /// </summary>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        /// <returns>要推送/拉取的文件列表。</returns>
        private async Task<List<string>> GetFiles(bool isPush)
        {
            HashSet<string> adb = ParseFile(await MyGM.AdbAsync($"shell ls -l {fleAdb.CurPath}"));
            HashSet<string> lcl = [.. Directory.GetFiles(fleLcl.CurPath).Select(Path.GetFileName)];

            return isPush ? [.. lcl.Except(adb)] : [.. adb.Except(lcl)];
        }

        /// <summary>
        /// 组合推送/拉取的ADB命令。
        /// </summary>
        /// <param name="lclPath">本地目录。</param>
        /// <param name="adbPath">安卓目录。</param>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        /// <returns>推送/拉取的ADB命令。</returns>
        private string GetCmd(string lclPath, string adbPath, bool isPush)
        {
            return isPush ? $"push \"{lclPath}\" \"{adbPath}\"" : $"pull \"{adbPath}\" \"{lclPath}\"";
        }

        /// <summary>
        /// 执行ADB命令，推送/拉取列表中的所有的文件到目标目录。
        /// </summary>
        /// <param name="fileNames">文件的名称列表。</param>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        private async Task TransmitFile(List<string> fileNames, bool isPush)
        {
            gMask.Visibility = Visibility.Visible;
            foreach (string name in fileNames)
            {
                string lclPath = Path.Combine(fleLcl.CurPath, name);
                string adbPath = $@"{fleAdb.CurPath}/{name}";
                await MyGM.AdbAsync(GetCmd(lclPath, adbPath, isPush));
            }
            gMask.Visibility = Visibility.Collapsed;
        }


    }
}
