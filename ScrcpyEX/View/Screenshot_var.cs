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
        /// 要放大区域的直径。
        /// </summary>
        private readonly static int _d = 50;

        /// <summary>
        /// 保存背景层的静态快照。
        /// </summary>
        private WriteableBitmap _sourceBitmap;

        /// <summary>
        /// 用于放大镜显示的像素区域，DPI固定为96，节约性能。
        /// </summary>
        private readonly WriteableBitmap _magnifierBitmap = new(_d, _d, 96, 96, PixelFormats.Pbgra32, null);

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


    }
}
