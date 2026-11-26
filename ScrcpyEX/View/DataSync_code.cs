using OpenCvSharp.Aruco;
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
using System.Threading;
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
        /// <param name="fileItems">指定要发送的文件列表，为null时会根据推送模式检索所有文件。</param>
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
                await fleAdb.Reload();
            }
            else
            {
                await fleLcl.Reload();
            }
            if (_isXferComplete) MyGM.POP($"文件{desc}完成。");
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
                await fleAdb.Reload();
            }
            if (toPull.Count > 0)
            {
                await TransmitFile(toPull, false);
                await fleLcl.Reload();
            }
            if (_isXferComplete) MyGM.POP("双方目录中的文件同步完成。");
        }

        /// <summary>
        /// 解析时不能简单用空格分隔，因为最后一个文件名可能包含多个空格。
        /// </summary>
        /// <param name="otp">ADB的ls -l命令获得的文件列表。</param>
        /// <returns>文件名的集合，使用正则表达式保证获取原始的文件名。</returns>
        private HashSet<string> ParseFile(string otp)
        {
            HashSet<string> result = [];
            string[] lines = [.. otp.SplitReal(['\r', '\n']).Skip(1)];

            foreach (string line in lines)
            {
                Match match = MyGM.LsLineRegex.Match(line);
                if (match.Success)
                {
                    string name = match.Groups["name"].Value;//包含所有空格
                    result.Add(name);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取要推送/拉取的文件列表，-l不显示以 . 开头的隐藏文件（也不显示 . 和 ..）。
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
        /// 获取要推送/拉取的文件列表，-l不显示以 . 开头的隐藏文件（也不显示 . 和 ..）。
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
            Stopwatch st = Stopwatch.StartNew();
            string desc = isPush ? "推送" : "拉取";

            InitXfer(total, desc);
            try
            {
                foreach (string name in fileNames)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    string lclPath = Path.Combine(fleLcl.CurPath, name);
                    string adbPath = $@"{fleAdb.CurPath}/{name}";
                    (_, string err, int exitCode) = await MyGM.AdbMoreAsync(GetCmd(lclPath, adbPath, isPush));

                    if (err != "" && exitCode != 0)
                    {
                        MyGM.ShwError($"终止值：{exitCode}\nADB 执行错误: {err}");
                        continue;
                    }

                    prgLoad.Text = $"{desc}中【{++i}/{total}】...";
                    prgLoad.AddMsg($"【{name}】传输成功，{ParseOutput(err)}");
                }
                _isXferComplete = true;
                prgLoad.Text = $"传输完毕【{i}/{total}】...";
            }
            catch (OperationCanceledException)
            {
                prgLoad.Text = $"停止传输【{i}/{total}】...";
            }
            catch (Exception ex)
            {
                MyGM.ShwError($"传输时发生异常：{ex}");
            }
            finally
            {
                st.Stop();
                prgLoad.StopSpin();
                _isXfering = false;

                //传输成功时，传输时长小于3秒时隐藏说明遮罩
                if (_isXferComplete && st.Elapsed.TotalSeconds < 3)
                {
                    gMask.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnStop.Visibility = Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// 初始化传输操作，显示进度环。
        /// </summary>
        /// <param name="isPush">是否为推送模式（本地->安卓）。</param>
        private void InitXfer(int total, string desc)
        {
            prgLoad.Init($"{desc}中【0/{total}】...");
            gMask.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Visible;
            _isXfering = true;
            _isXferComplete = false;
            _cts = new CancellationTokenSource();
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

        /// <summary>
        /// 显示名称相同、但大小不同的文件进行比较，再次调用取消比较。
        /// </summary>
        private void CompareSize()
        {
            if (stpOper.IsEnabled)
            {
                List<string> diffSize = GetDiffSizeLists();
                fleLcl.FilterDiffSize(diffSize);
                fleAdb.FilterDiffSize(diffSize);
                btnCompare.Content = "\uf0e7";
            }
            else
            {
                fleLcl.CancelFilter();
                fleAdb.CancelFilter();
                btnCompare.Content = "\ue735";
            }
            stpOper.IsEnabled = !stpOper.IsEnabled;
        }

        /// <summary>
        /// 找出两侧目录中名称相同、但大小不同的所有文件名。
        /// </summary>
        private List<string> GetDiffSizeLists()
        {
            Dictionary<string, FileItem> adbMap = fleAdb.CurDirs
                .GroupBy(x => x.Name)
                .ToDictionary(g => g.Key, g => g.First());

            return [.. fleLcl.CurDirs
                    .Where(l => adbMap.TryGetValue(l.Name, out FileItem a) && l.Size != a.Size)
                    .Select(l => l.Name)];
        }


    }
}
