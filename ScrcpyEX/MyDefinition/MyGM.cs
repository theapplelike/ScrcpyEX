using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义通用方法（Generic Method）类。
    /// </summary>
    internal static class MyGM
    {
        /// <summary>
        /// 错误日志路径。
        /// </summary>
        private static readonly string _errPath = @".\main\Local\ErrorLog";

        /// <summary>
        /// <para>ADB的ls命令结果：-rw-rw---- 1 root sdcard_rw 301 2026-04-04 19:25 shumei.txt</para>
        /// <para>按空格分隔分别是：</para>
        /// <para>parts[0] 权限, parts[1] 链接数, parts[2] 所有者, parts[3] 组,</para>
        /// <para>parts[4] 大小, parts[5] 日期值, parts[6] 时间值, parts[7] 名称。</para>
        /// <para>最后的名称可能带有空格，且可能有连续空格，所以使用正则表达式匹配最后的文件名保证空格完整。</para>
        /// <para>此正则表达式的结果依次是：</para>
        /// <para>Groups[0]，原始字符串。</para>
        /// <para>Groups[1]，文件类型：-,d,l 等。</para>
        /// <para>Groups[2]，权限位（9个字符）：rw-rw----。</para>
        /// <para>Groups[3]，文件大小：301。</para>
        /// <para>Groups[4]，日期：2026-04-04。</para>
        /// <para>Groups[5]，时间：19:25。</para>
        /// <para>Groups[6]，完整文件名（含所有空格）。</para>
        /// </summary>
        public static Regex LsLineRegex { get; } = new(
            @"^(?<type>[\-dlspbcD])(?<perm>[\-rwxsStT]{9})\s+\d+\s+\S+\s+\S+\s+(?<size>\d+)\s+(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2})\s+(?<name>.+)$",
            RegexOptions.Compiled
        );

        /// <summary>
        /// 在鼠标位置显示Popup提示，重复执行时可以显示到最新位置。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位s。</param>
        public static void POP(string text, int time = 3) => MV.MainWin.pop.Show(text, time);

        /// <summary>
        /// 立刻关闭随鼠标显示的Popup。
        /// </summary>
        public static void POP() => MV.MainWin.pop.Close();

        /// <summary>
        /// 在中间位置显示Popup提示，重复执行时覆盖上一个提示。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        public static void MidPOP(string text) => MV.MainWin.popMid.Show(text);

        /// <summary>
        /// 立刻关闭随鼠标显示的Popup。
        /// </summary>
        public static void MidPOP() => MV.MainWin.popMid.Close();

        /// <summary>
        /// 显示信息弹窗。
        /// </summary>
        public static void ShwInfo(string txt, string cpt = "提示信息") => MessageBox.Show(txt, cpt, MessageBoxButton.OK, MessageBoxImage.Information);

        /// <summary>
        /// 显示错误弹窗。
        /// </summary>
        public static void ShwError(string txt, string cpt = "未处理异常") => MessageBox.Show(txt, cpt, MessageBoxButton.OK, MessageBoxImage.Error);

        /// <summary>
        /// 获得光标的当前坐标，以屏幕为基准，物理像素坐标。
        /// </summary>
        /// <returns>一个System.Drawing.Point类型的值，WinForms框架。</returns>
        public static System.Drawing.Point GetPosnD() => System.Windows.Forms.Cursor.Position;

        /// <summary>
        /// 获得光标的当前坐标，以屏幕为基准，物理像素坐标。
        /// </summary>
        /// <returns>一个System.Windows.Point类型的值，WPF框架。</returns>
        public static System.Windows.Point GetPosnW()
        {
            System.Drawing.Point point = System.Windows.Forms.Cursor.Position;
            return new System.Windows.Point(point.X, point.Y);
        }

        /// <summary>
        /// 指定元素下Tag属性相同的Button设置一样的背景色。
        /// </summary>
        /// <param name="obj">指定的父级元素。</param>
        /// <param name="brush">要设置的颜色。</param>
        /// <param name="tag">指定的tag值。</param>
        public static void SetSameColor(DependencyObject obj, Brush brush, object tag)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is Button btn && btn.Tag != null)
                {
                    if (btn.Tag.ToString() == tag.ToString())
                    {
                        btn.Background = brush;
                    }
                }
            }
        }

        /// <summary>
        /// 如果min≤value≤max，则为value。value＜min，则为min。max＜value，则为max。
        /// </summary>
        /// <param name="value">要判定范围的值。</param>
        /// <param name="min">结果的下限。</param>
        /// <param name="max">结果的上限。</param>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        /// <summary>
        /// 根据比例限定值的范围，（orgValue*orgRatio）限定在（1±limitRatio）中。
        /// </summary>
        /// <param name="orgValue">原始值。</param>
        /// <param name="orgRatio">原始比例。</param>
        /// <param name="limitRatio">限定的比例。</param>
        /// <returns>如果乘值在限定的比例中，则返回乘值，否则返回上下限。</returns>
        public static double RatioClamp(double orgValue, double orgRatio, double limitRatio)
        {
            double value = orgValue * orgRatio;

            if (value < orgValue * (1 - limitRatio))
            {
                return orgValue * (1 - limitRatio);
            }
            else if (value > orgValue * (1 + limitRatio))
            {
                return orgValue * (1 + limitRatio);
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// 捕获主程序区域的截图。
        /// </summary>
        /// <param name="DpiScaleX">X轴上的DPI比例。</param>
        /// <param name="DpiScaleY">Y轴上的DPI比例。</param>
        /// <returns>主程序区域的截图。</returns>
        public static CroppedBitmap CaptureProcessArea(double DpiScaleX, double DpiScaleY)
        {
            //创建与屏幕大小相同的位图对象，SystemParameters的值是逻辑坐标，需要乘以DPI系数
            System.Drawing.Bitmap bmp = new((int)(SystemParameters.VirtualScreenWidth * DpiScaleX), (int)(SystemParameters.VirtualScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //使用位图对象来创建Graphics的对象
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;                  //设置平滑模式，抗锯齿
                g.CompositingQuality = CompositingQuality.HighQuality;      //设置合成质量
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; //设置插值模式
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;   //设置文本呈现的质量
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;            //设置呈现期间，像素偏移的方式

                //利用CopyFromScreen将当前屏幕截图内容存储在位图中
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size, System.Drawing.CopyPixelOperation.SourceCopy);
            }

            //将Bitmap转化为BitmapSource
            BitmapSource source = ConvertToBitmapSource(bmp);

            //定义要截取的矩形区域，主程序左上角的坐标和宽高
            Point pt = MV.MainWin.PointToScreen(new Point(0, 0));
            Int32Rect cropRect = new(
                (int)pt.X,
                (int)pt.Y,
                (int)(MV.MainWin.Width * DpiScaleX),
                (int)(MV.MainWin.Height * DpiScaleY));

            //Console.WriteLine($"非工作区：{SystemParameters.WindowNonClientFrameThickness.Left},{SystemParameters.WindowNonClientFrameThickness.Right}," +
            //    $"比例：{DpiScaleX},{DpiScaleY}," +
            //    $"起点：{cropRect.X}，{cropRect.Y}" +
            //    $"宽高：{cropRect.Width},{cropRect.Height}" +
            //    $"终点：{cropRect.X+ cropRect.Width}，{cropRect.Y+ cropRect.Height}，" +
            //    $"图片大小：{source.Width}，{source.Height}," +
            //    $"程序大小：{MV.MainWin.Width}，{MV.MainWin.Height}");
            
            if (cropRect.X + cropRect.Width > (int)source .Width)
            {
                cropRect.Width -= cropRect.X + cropRect.Width - (int)source.Width;
            }
            if (cropRect.Y + cropRect.Height > (int)source.Height)
            {
                cropRect.Height -= cropRect.Y + cropRect.Height - (int)source.Height;
            }

            CroppedBitmap croppedBitmap = new(source, cropRect);
            return croppedBitmap;
        }

        /// <summary>
        /// 将System.Drawing.Bitmap转换为System.Windows.Media.Imaging.BitmapSource，用于WPF。
        /// </summary>
        /// <param name="bitmap">System.Drawing.Bitmap对象。</param>
        /// <returns>返回一个托管的BitmapSource。</returns>
        public static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            finally
            {
                DLL.DeleteObject(hBitmap);//确保释放GDI对象
            }
        }

        /// <summary>
        /// 获得指定名称的脚本图片。
        /// </summary>
        /// <param name="imgName">图片的名称，不包括路径。</param>
        /// <returns>返回一个位图源，名称为空时返回null。</returns>
        public static BitmapImage GetScriptImg(string imgName)
        {
            if (imgName == "") return null;
            try
            {
                //创建BitmapImage并设置Source
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($@"{MV.ScriptPath}\{imgName}", UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;//强制立即加载
                bitmap.EndInit();

                return bitmap;
            }
            catch (Exception ex)
            {
                ShwError($"加载脚本图片失败: {ex.Message}", "错误信息");
                return null;
            }
        }

        /// <summary>
        /// 保存脚本图片到本地。
        /// </summary>
        /// <param name="name">图片名称，不包括路径。</param>
        /// <param name="source">图片的源。</param>
        public static void SavaScriptImg(string name, BitmapSource source)
        {
            using FileStream fs = new(@$"{MV.ScriptPath}\{name}", FileMode.Create);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(fs);
        }

        /// <summary>
        /// 保存调试的图片到本地。
        /// </summary>
        /// <param name="name">图片名称，不包括路径。</param>
        /// <param name="source">图片的源。</param>
        public static void SavaSuccessImg(string name, BitmapSource source)
        {
            using FileStream fs = new(@$"{MV.SuccessImgPath}\{name}", FileMode.Create);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(fs);
        }

        /// <summary>
        /// 获得指定句柄窗口的矩形大小。
        /// </summary>
        /// <param name="handle">窗口的句柄。</param>
        /// <returns>LpRect的数据。</returns>
        public static DLL.LpRect GetHandleRect(IntPtr handle)
        {
            DLL.GetWindowRect(handle, out DLL.LpRect rc);
            return rc;
        }

        /// <summary>
        /// 获得指定句柄窗口的图像。
        /// </summary>
        /// <param name="handle">窗口的句柄。</param>
        /// <returns>返回BitmapSource类型的图像。</returns>
        public static BitmapSource GetHandleImage(IntPtr handle)
        {
            DLL.LpRect rc = GetHandleRect(handle);
            if (rc.Width <= 0 || rc.Height <= 0)
            {
                return null;
            }

            //将Scrcpy的窗口复制到Bitmap中
            System.Drawing.Bitmap bmp = new(rc.Width, rc.Height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                bool success = DLL.PrintWindow(handle, hdc, 0x2);//PW_RENDERFULLCONTENT
                g.ReleaseHdc(hdc);
                if (!success)
                {
                    return null;
                }
            }

            return ConvertToBitmapSource(bmp);
        }

        /// <summary>
        /// 将Rect转化为Int32Rect，会进行四舍五入。
        /// </summary>
        /// <param name="rect">Rect类型的数据。</param>
        /// <returns>Int32Rect类型的数据。</returns>
        public static Int32Rect ConvertToInt32Rect(Rect rect)
        {
            Int32Rect int32Rect = new(
                (int)Math.Round(rect.X),
                (int)Math.Round(rect.Y),
                (int)Math.Round(rect.Width),
                (int)Math.Round(rect.Height));

            return int32Rect;
        }

        /// <summary>
        /// 启动ADB发送命令。
        /// </summary>
        /// <param name="adbCmd">需要执行的ADB命令。</param>
        /// <returns>执行命令后的结果。</returns>
        public static async Task<string> AdbAsync(string adbCmd)
        {
            (string otp, _, _) = await AdbMoreAsync(adbCmd);
            return otp;
        }

        /// <summary>
        /// 启动ADB发送命令，将返回更多信息。
        /// </summary>
        /// <param name="adbCmd">需要执行的ADB命令。</param>
        /// <returns>返回 输出流、错误流、终止值。</returns>
        public static async Task<(string, string, int)> AdbMoreAsync(string adbCmd)
        {
            using Process adbPrc = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = MV.ADB_Path,         //adb的路径
                    Arguments = adbCmd,             //执行完命令后adb会直接关闭
                    RedirectStandardOutput = true,  //重定向输出
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,        // 必须设置为false以允许设置Arguments属性
                    CreateNoWindow = true,          // 不创建窗口
                },
                EnableRaisingEvents = true
            };

            TaskCompletionSource<object> tcs = new();
            adbPrc.Exited += (_, _) => tcs.TrySetResult(null);
            adbPrc.Start();

            // 立刻启动异步读取，让管道数据被消费
            Task<string> otpTask = adbPrc.StandardOutput.ReadToEndAsync();
            Task<string> errTask = adbPrc.StandardError.ReadToEndAsync();
            await tcs.Task;// 异步等待进程退出（不阻塞线程）

            string otp = await otpTask;
            string err = await errTask;
            int exc = adbPrc.ExitCode;

            Debug.WriteLineIf(otp == "" && err == "", $"【{adbCmd}】，无输出流({exc})");
            Debug.WriteLineIf(otp != "" && exc == 0, $"{adbCmd}，执行正常：\n{Fmt(otp)}");
            Debug.WriteLineIf(otp != "" && exc != 0, $"{adbCmd}，正常输出({exc})：\n{Fmt(otp)}");
            Debug.WriteLineIf(err != "" && exc == 0, $"{adbCmd}，日志信息({exc})：\n{Fmt(err)}");
            Debug.WriteLineIf(err != "" && exc != 0, $"{adbCmd}，执行错误({exc})：\n{Fmt(err)}");
            Debug.WriteLine("- - - - - - - - - -分隔符- - - - - - - - - -");

            return (otp, err, exc);
        }

        /// <summary>
        /// 格式化Adb的输出内容，去掉空字符行，剩余行前面增加4个空格的缩进。
        /// </summary>
        /// <param name="output">要格式化的字符串。</param>
        /// <returns>格式化后的字符串。</returns>
        private static string Fmt(string output)
        {
            return string.Join(
                Environment.NewLine,
                output.SplitReal(Environment.NewLine).Select(s => $"    {s}"));
        }

        /// <summary>
        /// 停止ADB服务。
        /// </summary>
        public static void StopAdbServer()
        {
            ProcessStartInfo psi = new()
            {
                FileName = MV.ADB_Path,
                Arguments = "kill-server",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(psi);
        }

        /// <summary>
        /// 记录异常信息。
        /// </summary>
        /// <param name="ex">异常对象。</param>
        /// <param name="processEvent">处理异常的事件名称。</param>
        /// <param name="isShwBox">是否需要显示错误弹窗。</param>
        public static void RecordException(Exception ex, string processEvent, bool isShwBox = true)
        {
            StringBuilder sb = new();
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
            sb.AppendLine("【处理事件】：" + processEvent);
            sb.AppendLine("【异常类型】：" + ex.GetType().Name);
            sb.AppendLine("【异常信息】：" + ex.Message);
            sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
            sb.AppendLine("****************************--------****************************");
            Directory.CreateDirectory(_errPath);

            //文件存在则追加，不存在则新建文件
            using StreamWriter sw = new($@"{_errPath}\ErrorLog.txt", true, Encoding.UTF8);
            sw.WriteLine(sb);

            if (isShwBox) ShwError($"{processEvent}：{ex}");
        }

        /// <summary>
        /// 获取指定句柄所在屏幕的工作区。
        /// </summary>
        /// <param name="handle">程序句柄。</param>
        /// <returns>一个System.Drawing.Rectangle，表示所在屏幕的工作区。</returns>
        public static System.Drawing.Rectangle GetCurScreenRect(IntPtr handle)
        {
            return System.Windows.Forms.Screen.FromHandle(handle).WorkingArea;
        }

        /// <summary>
        /// 从可能包含端口的字符串中提取 IP 地址。
        /// <para>支持格式：192.168.1.1:8080、[::1]:5555、192.168.1.1（无端口）、::1（无端口）。</para>
        /// </summary>
        /// <param name="input">要解析的字符串。</param>
        /// <returns>返回有效的 IP，若无法提取有效的 IP，返回空字符串。</returns>
        public static string ExtractIpAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            if (IPAddress.TryParse(input, out _)) return input;

            // 处理 IPv6 带端口格式：[IPv6]:port
            if (input.StartsWith("[") && input.Contains("]:"))
            {
                int start = input.IndexOf('[');
                int end = input.IndexOf(']');
                if (start >= 0 && end > start)
                {
                    string ipv6Candidate = input.Substring(start + 1, end - start - 1);
                    if (IPAddress.TryParse(ipv6Candidate, out _))
                    {
                        return ipv6Candidate;
                    }
                }
            }

            // 处理 IPv4 带端口或 IPv6 不带方括号
            int lastColon = input.LastIndexOf(':');
            if (lastColon > 0)
            {
                string candidate = input.Substring(0, lastColon);
                if (IPAddress.TryParse(candidate, out _))
                {
                    return candidate;
                }
            }

            return "";
        }

        /// <summary>
        /// 根据传入的ScriptItem基类获得脚本信息列表，无论单/组脚本都会转化成 List&lt;ScriptInfo&gt;。
        /// </summary>
        /// <param name="scriptItem">传入的ScriptItem基类。</param>
        /// <returns>脚本信息列表。</returns>
        public static List<ScriptInfo> GetScriptList(ScriptItem scriptItem)
        {
            if (scriptItem is ScriptInfo info)
            {
                return [info];
            }
            else if (scriptItem is ScriptGroup group)
            {
                return [.. group.Scripts];
            }
            return [];
        }

        /// <summary>
        /// 记录普通日志。
        /// </summary>
        /// <param name="text">日志内容。</param>
        //public static void Log(string text)
        //{
        //    StringBuilder sb = new();
        //    sb.AppendLine($"【{DateTime.Now}】：{text}");
        //    Directory.CreateDirectory(@".\main\Local\Log");
        //
        //    using StreamWriter sw = new(@".\main\Local\Log\Log.txt", true, Encoding.UTF8);
        //    sw.WriteLine(sb);
        //}


    }
}
