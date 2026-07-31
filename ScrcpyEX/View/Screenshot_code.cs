using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScrcpyEX.View
{
    public partial class Screenshot//code
    {
        /// <summary>
        /// 自定义初始化。
        /// </summary>
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
            popMagnifier.IsOpen = true;
        }

        /// <summary>
        /// 自定义关闭事件。
        /// </summary>
        private void MyClose()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        /// <summary>
        /// 界面渲染的每一帧都将触发的事件。
        /// </summary>
        private void OnRendering(object sender, EventArgs e)
        {
            if (!popTool.IsOpen)
            {
                System.Windows.Point mousePos = Mouse.GetPosition(this);

                if (mousePos.X >= 0 && mousePos.X <= ActualWidth &&
                    mousePos.Y >= 0 && mousePos.Y <= ActualHeight)
                {
                    // 鼠标在窗口内，打开放大镜（如果已关闭则重新打开）
                    if (!popMagnifier.IsOpen) { popMagnifier.IsOpen = true; }
                    UpdateMagnifier(mousePos);
                }
                else
                {
                    if (popMagnifier.IsOpen) { popMagnifier.IsOpen = false; }
                }
            }
            else
            {
                if (popMagnifier.IsOpen) { popMagnifier.IsOpen = false; }
            }
        }

        /// <summary>
        /// 获取当前截屏。
        /// </summary>
        /// <returns>返回BitmapImage对象。</returns>
        //public BitmapImage CaptureCurrentScreen()
        //{
        //    //创建与屏幕大小相同的位图对象，SystemParameters与DPI无关，需要乘以DPI系数
        //    //使用VirtualScreenWidth会覆盖所有显示器，PrimaryScreenWidth只能覆盖主显示器
        //    Bitmap bmp = new((int)(SystemParameters.VirtualScreenWidth * DpiScaleX), (int)(SystemParameters.VirtualScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //    //Bitmap bmp = new((int)(SystemParameters.PrimaryScreenWidth * DpiScaleX), (int)(SystemParameters.PrimaryScreenHeight * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        //
        //    //使用位图对象来创建Graphics的对象
        //    using (Graphics g = Graphics.FromImage(bmp))
        //    {
        //        g.SmoothingMode = SmoothingMode.AntiAlias;                  //设置平滑模式，抗锯齿
        //        g.CompositingQuality = CompositingQuality.HighQuality;      //设置合成质量
        //        g.InterpolationMode = InterpolationMode.HighQualityBicubic; //设置插值模式
        //        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;   //设置文本呈现的质量
        //        g.PixelOffsetMode = PixelOffsetMode.HighQuality;            //设置呈现期间，像素偏移的方式
        //
        //        //利用CopyFromScreen将当前屏幕截图内容存储在位图中
        //        g.CopyFromScreen(0, 0, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        //    }
        //
        //    return BitmapConvert(bmp);
        //}

        /// <summary>
        /// 重新进行截图。
        /// </summary>
        private void Recapture()
        {
            rct.Width = rct.Height = 0;
            rct.Visibility = Visibility.Hidden;

            rctMask.Clip = null;
            rctMask.Visibility = Visibility.Hidden;

            popTool.IsOpen = false;
            tbk.Text = string.Empty;
        }

        /// <summary>
        /// 完成截图。
        /// </summary>
        private void CaptureComplete()
        {
            Rect cropRect = new(
                Math.Min(_startPoint.X, _endPoint.X) * DpiScaleX,
                Math.Min(_startPoint.Y, _endPoint.Y) * DpiScaleY,
                Math.Abs(_endPoint.X - _startPoint.X) * DpiScaleX,
                Math.Abs(_endPoint.Y - _startPoint.Y) * DpiScaleY);

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
        private void GetCropArea(FrameworkElement e, Rect cropRect)
        {
            RenderTargetBitmap render = new((int)(e.ActualWidth * DpiScaleX), (int)(e.ActualHeight * DpiScaleY), PixelsPerInchX, PixelsPerInchY, PixelFormats.Pbgra32);
            Rect saveRect = new(
                cropRect.X/ MV.MainWin.ScRatioX,
                cropRect.Y/ MV.MainWin.ScRatioY,
                cropRect.Width / MV.MainWin.ScRatioX,
                cropRect.Height / MV.MainWin.ScRatioY);

            render.Render(e);
            Crop = new CroppedBitmap(render, MyGM.ConvertToInt32Rect(cropRect));
            CropRect = saveRect;
        }


    }
}
