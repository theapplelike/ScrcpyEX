using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Net;
using System.Windows.Interop;

namespace ScrcpyEX.View
{
    public partial class Screenshot//code
    {
        /// <summary>
        /// 获取当前截屏。
        /// </summary>
        /// <returns>返回BitmapImage对象。</returns>
        public BitmapImage CaptureCurrentScreen()
        {
            //创建与屏幕大小相同的位图对象，SystemParameters与DPI无关，需要乘以DPI系数
            //Bitmap bmp = new((int)(SystemParameters.VirtualScreenWidth * DpiScaleX), (int)(SystemParameters.VirtualScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmap bmp = new((int)(SystemParameters.PrimaryScreenWidth * DpiScaleX), (int)(SystemParameters.PrimaryScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //使用位图对象来创建Graphics的对象
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;                  //设置平滑模式，抗锯齿
                g.CompositingQuality = CompositingQuality.HighQuality;      //设置合成质量
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; //设置插值模式
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;   //设置文本呈现的质量
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;            //设置呈现期间，像素偏移的方式

                //利用CopyFromScreen将当前屏幕截图内容存储在位图中
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }

            return BitmapConvert(bmp);
        }

        /// <summary>
        /// 将System.Drawing.Bitmap转换为System.Windows.Media.Imaging.BitmapImage。
        /// BitmapImage专用于WPF。
        /// </summary>
        /// <param name="bmp">System.Drawing.Bitmap对象。</param>
        /// <returns>System.Windows.Media.Imaging.BitmapImage对象。</returns>
        private BitmapImage BitmapConvert(Bitmap bmp)
        {
            BitmapImage bitmapImage;

            using (MemoryStream stream = new())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        /// <summary>
        /// 更新遮罩区域。
        /// </summary>
        private void UpdateMask()
        {
            //获取Canvas的实际尺寸，设置遮罩矩形
            double cnvWd = cnv.ActualWidth;
            double cnvHt = cnv.ActualHeight;
            RectangleGeometry canvasArea = new(new Rect(0, 0, cnvWd, cnvHt));

            //获取截图区域的位置和尺寸，设置非遮罩矩形
            double rectX = rct.Margin.Left;
            double rectY = rct.Margin.Top;
            double rectWd = rct.Width;
            double rectHt = rct.Height;
            RectangleGeometry excludeArea = new(new Rect(rectX, rectY, rectWd, rectHt));

            //设置覆盖层的Clip
            rctMask.Clip = new CombinedGeometry(GeometryCombineMode.Exclude, canvasArea, excludeArea);
        }

        /// <summary>
        /// 重新进行截图。
        /// </summary>
        private void Recapture()
        {
            rct.Width = rct.Height = 0;
            rct.Visibility = Visibility.Hidden;

            rctMask.Clip = null;
            rctMask.Visibility = Visibility.Hidden;

            stp.Visibility = Visibility.Hidden;
            tbk.Text = string.Empty;
        }

        /// <summary>
        /// 完成截图。
        /// </summary>
        private void CaptureComplete()
        {
            Console.WriteLine($"开始点：{_startPoint.X}，{_startPoint.Y}，结束点：{_endPoint.X}，{_endPoint.Y}。");

            Int32Rect cropRect = new(
                (int)(Math.Min(_startPoint.X, _endPoint.X) * DpiScaleX),
                (int)(Math.Min(_startPoint.Y, _endPoint.Y) * DpiScaleY),
                (int)(Math.Abs(_endPoint.X - _startPoint.X) * DpiScaleX),
                (int)(Math.Abs(_endPoint.Y - _startPoint.Y) * DpiScaleY));

            //未截取任何内容时退出
            if (cropRect.Width == 0 || cropRect.Height == 0)
            {
                DialogResult = false;
                return;
            }

            GetCropArea(gBack, cropRect);
            DialogResult = true;
        }

        /// <summary>
        /// 获得剪裁区域的Bitmap。
        /// </summary>
        /// <param name="e">承载整个屏幕截图的控件。</param>
        /// <param name="cropRect">说明其中要截取部分的矩形描述。</param>
        private void GetCropArea(FrameworkElement e, Int32Rect cropRect)
        {
            RenderTargetBitmap render = new((int)(e.ActualWidth * DpiScaleX), (int)(e.ActualHeight * DpiScaleY), PixelsPerInchX, PixelsPerInchY, PixelFormats.Pbgra32);
            
            render.Render(e);
            Crop = new CroppedBitmap(render, cropRect);
            //Console.WriteLine($"dpi:{PixelsPerInchX},{PixelsPerInchY}");
        }

        private BitmapImage CaptureScreen(Int32Rect rect)
        {
            using Bitmap bmp = new(rect.Width, rect.Height);
            using Graphics g = Graphics.FromImage(bmp);

            // 截屏
            g.CopyFromScreen(rect.X, rect.Y, 0, 0, new System.Drawing.Size(rect.Width, rect.Height));

            return BitmapConvert(bmp);

            // 转换为 WPF BitmapSource 并放大
            //return Imaging.CreateBitmapSourceFromHBitmap(
            //    bmp.GetHbitmap(),
            //    IntPtr.Zero,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromWidthAndHeight(
            //        rect.Width * MagnifyScale,
            //        rect.Height * MagnifyScale));
        }


    }
}
