using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            MV.Pop.Show(text, time);
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
        /// 获得光标的当前坐标，以屏幕为基准，且此值已乘以了DPI比例（与DPI相关）。
        /// </summary>
        /// <returns>一个System.Windows.Point类型的值。</returns>
        public static Point GetPosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Cursor.Position;
            return new Point(point.X, point.Y);
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
        /// 将System.Drawing.Bitmap转换为System.Windows.Media.Imaging.BitmapSource。
        /// BitmapSource用于WPF。
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
                MessageBox.Show($"加载脚本图片失败: {ex.Message}");
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
            //Console.WriteLine($"句柄窗口尺寸：{rc.Left},{rc.Top},{rc.Right},{rc.Bottom},{rc.Width},{rc.Height}");
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


    }
}
