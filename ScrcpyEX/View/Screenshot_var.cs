using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScrcpyEX.View
{
    public partial class Screenshot//var
    {
        /// <summary>
        /// 截图开始的点。
        /// </summary>
        private Point _startPoint;

        /// <summary>
        /// 截图结束的点。
        /// </summary>
        private Point _endPoint;

        /// <summary>
        /// 鼠标在窗体上的当前坐标。
        /// </summary>
        private Point _curPoint;

        /// <summary>
        /// 更新放大镜的计时器。
        /// </summary>
        private readonly DispatcherTimer _timerUpdateMagnifier = new();

        /// <summary>
        /// 要放大区域的直径。
        /// </summary>
        private int _enlargeDiameter = 50;

        /// <summary>
        /// 最后的截图结果。
        /// </summary>
        public CroppedBitmap Crop { get; set; }

        /// <summary>
        /// X轴上的DPI比例。
        /// </summary>
        private double DpiScaleX => VisualTreeHelper.GetDpi(this).DpiScaleX;

        /// <summary>
        /// Y轴上的DPI比例。
        /// </summary>
        private double DpiScaleY => VisualTreeHelper.GetDpi(this).DpiScaleY;

        /// <summary>
        /// 沿X轴的DPI。
        /// </summary>
        private double PixelsPerInchX => VisualTreeHelper.GetDpi(this).PixelsPerInchX;

        /// <summary>
        /// 沿Y轴的DPI。
        /// </summary>
        private double PixelsPerInchY => VisualTreeHelper.GetDpi(this).PixelsPerInchY;

        

        private WriteableBitmap sourceBitmap;
        private WriteableBitmap magnifierBitmap;
        //private Point lastMousePos;
        //private bool isCapturing;

        private void UpdateMagnifier(Point curPoint)
        {
            //if ( sourceBitmap == null) return;

            int sourceX = (int)(curPoint.X - _enlargeDiameter / 2);
            int sourceY = (int)(curPoint.Y - _enlargeDiameter / 2);

            // 边界检查
            //sourceX = Math.Clamp(sourceX, 0, sourceBitmap.PixelWidth - 100);
            //sourceY = Math.Clamp(sourceY, 0, sourceBitmap.PixelHeight - 100);

            sourceX = sourceX < 0 ? 0 : sourceX;
            sourceY = sourceY < 0 ? 0 : sourceY;

            try
            {
                magnifierBitmap.Lock();

                // 直接复制像素数据
                sourceBitmap.CopyPixels(
                    new Int32Rect(sourceX, sourceY, _enlargeDiameter, _enlargeDiameter),
                    magnifierBitmap.BackBuffer,
                    magnifierBitmap.BackBufferStride * _enlargeDiameter,
                    magnifierBitmap.BackBufferStride);

                magnifierBitmap.AddDirtyRect(new Int32Rect(0, 0, _enlargeDiameter, _enlargeDiameter));
            }
            finally
            {
                magnifierBitmap.Unlock();
            }

            // 更新截图框位置（如果需要）
            //Canvas.SetLeft(rct, lastMousePos.X);
            //Canvas.SetTop(rct, lastMousePos.Y);
            gMagnifier.Margin = new Thickness(curPoint.X + 25, curPoint.Y + 25, 0, 0);
        }
    }
}
