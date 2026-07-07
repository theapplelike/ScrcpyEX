using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
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
using static System.Net.WebRequestMethods;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义通用方法（Generic Method）类。
    /// </summary>
    internal class MyGM
    {
        /// <summary>
        /// 在鼠标位置显示Popup提示，重复执行时可以显示到最新位置。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位s。</param>
        public static void POP(string text, int time = 3)
        {
            MV.MainWin.pop.Show(text, time);
        }

        /// <summary>
        /// 立刻关闭随鼠标显示的Popup。
        /// </summary>
        public static void POP()
        {
            MV.MainWin.pop.Close();
        }

        /// <summary>
        /// 在中间位置显示Popup提示，重复执行时覆盖上一个提示。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        public static void MidPOP(string text)
        {
            MV.MainWin.popMid.Show(text);
        }

        /// <summary>
        /// 显示信息弹窗。
        /// </summary>
        public static void ShwInfo(string txt, string cpt = "提示信息")
        {
            MessageBox.Show(txt, cpt, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 显示错误弹窗。
        /// </summary>
        public static void ShwError(string txt, string cpt = "未处理异常")
        {
            MessageBox.Show(txt, cpt, MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// 获得光标的当前坐标，以屏幕为基准，且此值已乘以了DPI比例（与DPI相关），WPF框架。
        /// </summary>
        /// <returns>一个System.Windows.Point类型的值。</returns>
        public static System.Windows.Point GetPosnW()
        {
            System.Drawing.Point point = System.Windows.Forms.Cursor.Position;
            return new System.Windows.Point(point.X, point.Y);
        }

        /// <summary>
        /// 获得光标的当前坐标，以屏幕为基准，且此值已乘以了DPI比例（与DPI相关），WinForms框架。
        /// </summary>
        /// <returns>一个System.Drawing.Point类型的值。</returns>
        public static System.Drawing.Point GetPosnD()
        {
            return System.Windows.Forms.Cursor.Position;
        }

        /// <summary>
        /// 如果min≤value≤max，则为value。value＜min，则为min。max＜value，则为max。
        /// </summary>
        /// <param name="value">要判定范围的值。</param>
        /// <param name="min">结果的下限。</param>
        /// <param name="max">结果的上限。</param>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// 如果min≤value≤max，则为value。value＜min，则为min。max＜value，则为max。
        /// </summary>
        /// <param name="value">要判定范围的值。</param>
        /// <param name="min">结果的下限。</param>
        /// <param name="max">结果的上限。</param>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
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
            //创建与屏幕大小相同的位图对象，SystemParameters与DPI无关，需要乘以DPI系数
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
        /// <param name="imgName">图片的名称。</param>
        /// <returns>返回一个位图源，名称为空时返回null。</returns>
        public static BitmapImage GetImage(string imgName)
        {
            if (imgName == "")
            {
                return null;
            }

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
        public static void SavaImg(string name, BitmapSource source)
        {
            using FileStream fs = new(@$"{MV.ScriptPath}\{name}", FileMode.Create);
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
        /// <param name="isReturnErr">返回结果是否为错误流。</param>
        /// <returns>执行命令后的结果。</returns>
        public static async Task<string> AdbAsync(string adbCmd, bool isReturnErr = false)
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

            Debug.WriteLineIf(otp == "" && err == "", $"【{adbCmd}】");
            Debug.WriteLineIf(otp != "", $"{adbCmd}，执行正常：\n{Fmt(otp)}");
            Debug.WriteLineIf(err != "", $"{adbCmd}，执行错误({adbPrc.ExitCode})：\n{Fmt(err)}");
            Debug.WriteLine("- - - - - - - - - -分隔符- - - - - - - - - -");

            return isReturnErr ? err : otp;
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

            Debug.WriteLineIf(otp == "" && err == "", $"【{adbCmd}】");
            Debug.WriteLineIf(otp != "", $"{adbCmd}，执行正常：\n{Fmt(otp)}");
            Debug.WriteLineIf(err != "", $"{adbCmd}，执行错误({adbPrc.ExitCode})：\n{Fmt(err)}");
            Debug.WriteLine("- - - - - - - - - -分隔符- - - - - - - - - -");

            return (otp, err, adbPrc.ExitCode);
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
