using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool IsCanSend(string desc)
        {
            if (!fleLcl.IsCanXfer() || !fleAdb.IsCanXfer())
            {
                MyGM.POP($"当前存在逻辑目录，不能{desc}文件。");
                return false;
            }
            if(!fleLcl.IsLoadSuccess || !fleAdb.IsLoadSuccess)
            {
                MyGM.POP($"目录未正确加载，不能{desc}文件。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 在指定的本地目录和ADB设备目录中发送文件。
        /// </summary>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        /// <param name="isCover">如果已存在文件，是否覆盖。</param>
        /// <param name="fileItems">指定要发送的文件列表。</param>
        private async Task SendFile(bool isPush, bool isCover = false, List<FileItem> fileItems = null)
        {
            string desc = isPush ? "推送" : "拉取";
            if (!IsCanSend(desc))
            {
                return;
            }

            List<string> files = (isCover, fileItems) switch
            {
                (false, null) => await GetFiles(isPush),
                (false, not null) => await GetFiles(isPush, fileItems),
                (true, not null) => [.. fileItems.Select(f => f.Name)],
                _ => []
            };
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
            if (!IsCanSend("同步"))
            {
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
        /// 获取要推送/拉取的文件列表。
        /// </summary>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        /// <param name="fileItems">指定要发送的文件列表。</param>
        /// <returns>要推送/拉取的文件列表。</returns>
        private async Task<List<string>> GetFiles(bool isPush, List<FileItem> fileItems)
        {
            if (isPush)
            {
                HashSet<string> adb = ParseFile(await MyGM.AdbAsync($"shell ls -l {fleAdb.CurPath}"));
                HashSet<string> lcl = [.. fileItems.Select(f => f.Name)];

                return [.. lcl.Except(adb)];
            }
            else
            {
                HashSet<string> adb = [.. fileItems.Select(f => f.Name)];
                HashSet<string> lcl = [.. Directory.GetFiles(fleLcl.CurPath).Select(Path.GetFileName)];

                return [.. adb.Except(lcl)];
            }
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
            int total = fileNames.Count, i = 0;

            ShowRing(total);
            foreach (string name in fileNames)
            {
                string lclPath = Path.Combine(fleLcl.CurPath, name);
                string adbPath = $@"{fleAdb.CurPath}/{name}";
                (_, string err, int exitCode) = await MyGM.AdbMoreAsync(GetCmd(lclPath, adbPath, isPush));

                if (err != "" && exitCode != 0)
                {
                    MyGM.ShwError($"终止值：{exitCode}\nADB 执行错误: {err}");
                    continue;
                }

                ring.Desc = $"传输中【{++i}/{total}】...";
                ring.AddMsg($"【{name}】传输成功，{ParseOutput(err)}");
            }
            //gMask.Visibility = Visibility.Collapsed;
            ring.Desc = $"传输完毕【{i}/{total}】...";
            ring.StopSpin();
            _isXferComplete = true;
        }

        /// <summary>
        /// 显示进度环并初始化。
        /// </summary>
        private void ShowRing(int total)
        {
            ring.Init($"传输中【0/{total}】...");
            gMask.Visibility = Visibility.Visible;
            _isXferComplete = false;
        }

        /// <summary>
        /// 解析输出中的传输速度。
        /// </summary>
        /// <param name="otp">ADB的输出。</param>
        /// <returns>说明速度和大小的字符串，格式：36.7 MB/s(11.389秒内传输438,006,392字节）。</returns>
        private string ParseOutput(string otp)
        {
            Match match = Regex.Match(otp, @"(\d+\.?\d*\s*MB/s)\s*\((\d+)\s*bytes in\s*([\d.]+)s\)");
            if (match.Success)
            {
                string speed = match.Groups[1].Value;
                string bytes = long.Parse(match.Groups[2].Value).ToString("#,##0");
                string seconds = match.Groups[3].Value;

                return $"{speed}（{seconds}秒内传输{bytes}字节）";
            }
            return "速度解析失败";
        }


    }
}
