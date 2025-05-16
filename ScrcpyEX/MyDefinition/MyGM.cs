using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ScrcpyEX.ExtensionMethod;
using System.Windows.Media;
using System.Windows;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.IO;

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
        /// 在指定元素的Visual树中查找类型匹配的子元素。
        /// </summary>
        /// <typeparam name="T">要查找的子元素类型，比如Button。</typeparam>
        /// <param name="obj">指定的父级元素。</param>
        /// <returns>返回在指定的父级元素中查找到的第一个类型匹配的子元素，可以查找到孙级元素，如果没找到返回null。</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t)
                {
                    return t;
                }

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
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
        /// 获得元素的第一个指定父元素，未查找到时会继续向上查找祖级元素。
        /// </summary>
        /// <typeparam name="T">指定的父元素类型。</typeparam>
        /// <param name="obj">要查找父元素的元素。</param>
        /// <returns>返回查找到的第一个指定父元素，未查到时返回null。</returns>
        public static T GetParentObject<T>(DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T t)
                {
                    return t;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// 获得光标的当前坐标，以屏幕为基准，且此值已乘以了Dpi比例。
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
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            //定义要截取的矩形区域，主程序左上角的坐标和宽高
            Point pt = MV.MainWin.PointToScreen(new Point(0, 0));
            Int32Rect cropRect = new(
                (int)pt.X,
                (int)pt.Y,
                (int)(MV.MainWin.Width * DpiScaleX),
                (int)(MV.MainWin.Height * DpiScaleY));

            CroppedBitmap croppedBitmap = new(source, cropRect);
            return croppedBitmap;
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


    }
}
