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
using System.Windows.Input;

namespace ScrcpyEX.View
{
    public partial class Screenshot//code
    {
        private void MyInit()
        {
            //捕获背景层的快照，存储在一个WriteableBitmap中，直接操作WriteableBitmap可以提高性能
            RenderTargetBitmap rtb = new(
                (int)(gBack.ActualWidth * DpiScaleX),
                (int)(gBack.ActualHeight * DpiScaleY),
                96 * DpiScaleX, 96 * DpiScaleY, PixelFormats.Pbgra32);
            rtb.Render(gBack);

            _sourceBitmap = new WriteableBitmap(rtb);
            img.Source = _magnifierBitmap;

            //每一帧都将触发的事件
            CompositionTarget.Rendering += OnRendering;
        }

        /// <summary>
        /// 界面渲染的每一帧都将触发的事件。
        /// </summary>
        private void OnRendering(object sender, EventArgs e)
        {
            if (stp.Visibility != Visibility.Visible)
            {
                UpdateMagnifier(Mouse.GetPosition(gBack));
            }
        }

        /// <summary>
        /// 获取当前截屏。
        /// </summary>
        /// <returns>返回BitmapImage对象。</returns>
        public BitmapImage CaptureCurrentScreen()
        {
            //创建与屏幕大小相同的位图对象，SystemParameters与DPI无关，需要乘以DPI系数
            Bitmap bmp = new((int)(SystemParameters.VirtualScreenWidth * DpiScaleX), (int)(SystemParameters.VirtualScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //Bitmap bmp = new((int)(SystemParameters.PrimaryScreenWidth * DpiScaleX), (int)(SystemParameters.PrimaryScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

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
            //Console.WriteLine($"开始点：{_startPoint.X}，{_startPoint.Y}，结束点：{_endPoint.X}，{_endPoint.Y}。");

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
            CropRect = cropRect;
        }


    }
}
