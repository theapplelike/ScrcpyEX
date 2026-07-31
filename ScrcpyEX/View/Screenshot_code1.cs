using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ScrcpyEX.MyDefinition;
using System.Windows.Forms;
using System.Diagnostics;

namespace ScrcpyEX.View
{
    public partial class Screenshot//code1
    {
        /// <summary>
        /// 开始截图。
        /// </summary>
        /// <param name="point">当前鼠标在窗体上的位置。</param>
        private void StartCapture(Point point)
        {
            _startPoint = point;

            rct.Width = 0;
            rct.Height = 0;
            rct.Visibility = Visibility.Visible;
            rctMask.Visibility = Visibility.Visible;

            popTool.IsOpen = false;
        }

        /// <summary>
        /// 右击的时候返回上一个步骤，已有截图则重新截图；未截图则关闭截图界面。
        /// </summary>
        private void RightClick()
        {
            if (popTool.IsOpen)
            {
                Recapture();
            }
            else
            {
                DialogResult = false;
            }
        }

        /// <summary>
        /// 用户正在截图时要进行的操作。
        /// </summary>
        /// <param name="curPoint">当前鼠标在窗体上的位置。</param>
        private void Capturing(Point curPoint)
        {
            double x = Math.Min(curPoint.X, _startPoint.X);
            double y = Math.Min(curPoint.Y, _startPoint.Y);
            double wd = Math.Abs(curPoint.X - _startPoint.X);
            double ht = Math.Abs(curPoint.Y - _startPoint.Y);

            //更新截图区域，不必乘以dpi比例，控件的大小和边距会根据dpi自动调节
            rct.Margin = new Thickness(x, y, 0, 0);
            rct.Width = wd;
            rct.Height = ht;

            tbk.Margin = new Thickness(x, y - 20, 0, 0);
            tbk.Text = $"{wd}×{ht}";

            UpdateMask();
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
        /// 显示工具栏。
        /// </summary>
        /// <param name="endPoint">截图结束时的坐标点。</param>
        private void ShowToolBar(Point endPoint)
        {
            _endPoint = endPoint;

            popTool.HorizontalOffset = endPoint.X + 10;
            popTool.VerticalOffset = GetY(endPoint);
            popTool.IsOpen = true;
        }

        /// <summary>
        /// 获得工具栏的y轴坐标，对于多屏幕的情况，副屏会和主屏的DPI保持一致，如果副屏DPI比主屏小，
        /// 获得的副屏分辨率会乘上主屏的DPI（比如2），这样会导致鼠标已经到副屏的底部，但参数计算
        /// 出来鼠标在屏幕垂直一半的位置上，所有副屏上菜单始终向上靠拢。
        /// </summary>
        /// <param name="endPoint">截图结束时的坐标点。</param>
        private double GetY(Point endPoint)
        {
            if (endPoint.X < SystemParameters.PrimaryScreenWidth)
            {
                if (endPoint.Y + stp.ActualHeight > SystemParameters.PrimaryScreenHeight)
                {
                    return endPoint.Y - stp.ActualHeight;
                }
                else
                {
                    return endPoint.Y;
                }
            }
            else
            {
                if (_startPoint.Y > stp.ActualWidth)
                {
                    return _startPoint.Y - stp.ActualHeight;
                }
                else if (_startPoint.Y < stp.ActualWidth && rct.ActualHeight > stp.ActualWidth * 2)
                {
                    return endPoint.Y - stp.ActualHeight;
                }
                else
                {
                    return endPoint.Y;
                }
            }
        }

        /// <summary>
        /// 更新放大镜内容和位置。放大比例为：放大镜控件宽/放大区域直径，不计算DPI系数。
        /// </summary>
        /// <param name="curPoint">鼠标的当前位置。</param>
        private void UpdateMagnifier(Point curPoint)
        {
            // 以鼠标为中心计算请求的源矩形
            int requestX = (int)(curPoint.X * DpiScaleX - _d / 2);
            int requestY = (int)(curPoint.Y * DpiScaleY - _d / 2);
            Int32Rect requestRect = new(requestX, requestY, _d, _d);

            // 源位图的有效区域
            Int32Rect sourceBounds = new(0, 0, _sourceBitmap.PixelWidth, _sourceBitmap.PixelHeight);

            // 求交集（如果请求区域完全在屏幕外，则交集为空）
            Int32Rect intersect = GetIntersection(requestRect, sourceBounds);

            // 先将整个放大镜位图填充为黑色透明(BGRA = 0,0,0,0)
            int stride = _magnifierBitmap.BackBufferStride;
            byte[] blackPixels = new byte[stride * _d];
            _magnifierBitmap.WritePixels(new Int32Rect(0, 0, _d, _d), blackPixels, stride, 0);

            // 如果有交集，将其从源位图复制到放大镜位图的对应位置
            if (!intersect.IsEmpty)
            {
                // 目标矩形：交集在放大镜中的偏移
                Int32Rect destRect = new(
                    intersect.X - requestX,
                    intersect.Y - requestY,
                    intersect.Width,
                    intersect.Height);

                // 将交集像素复制到临时数组（按目标 stride 存放，方便直接写入）
                byte[] srcPixels = new byte[stride * intersect.Height];
                _sourceBitmap.CopyPixels(intersect, srcPixels, stride, 0);

                // 写入放大镜位图的有效部分（周围已为黑色透明）
                _magnifierBitmap.WritePixels(destRect, srcPixels, stride, 0);
            }

            //Popup控件的偏移和DPI相关，所以给出的原始值要与DPI无关
            (double left, double top) = GetLocation(PointToScreen(curPoint), 15);
            popMagnifier.HorizontalOffset = left / DpiScaleX;
            popMagnifier.VerticalOffset = top / DpiScaleY;
        }

        /// <summary>
        /// 获得两个Int32Rect的交集。
        /// </summary>
        /// <param name="r1">第一个Int32Rect。</param>
        /// <param name="r2">第二个Int32Rect。</param>
        /// <returns>两个Int32Rect的交集。</returns>
        private Int32Rect GetIntersection(Int32Rect r1, Int32Rect r2)
        {
            int x = Math.Max(r1.X, r2.X);
            int y = Math.Max(r1.Y, r2.Y);
            int right = Math.Min(r1.X + r1.Width, r2.X + r2.Width);
            int bottom = Math.Min(r1.Y + r1.Height, r2.Y + r2.Height);
            int width = Math.Max(0, right - x);
            int height = Math.Max(0, bottom - y);
            return new Int32Rect(x, y, width, height);
        }

        /// <summary>
        /// 获得放大镜的位置，会确保放大镜始终在可视范围内。
        /// </summary>
        /// <param name="curPoint">鼠标的当前位置。</param>
        /// <param name="r">放大镜和鼠标的距离。</param>
        /// <returns>返回左和上的位置。</returns>
        private (double, double) GetLocation(Point curPoint, double r)
        {
            double left, top;
            System.Drawing.Rectangle rect = Screen.FromPoint(MyGM.GetPosnD()).WorkingArea;

            if (curPoint.X + r + gMagnifier.Width > rect.Width)
            {
                left = curPoint.X - r - gMagnifier.Width;
            }
            else
            {
                left = curPoint.X + r;
            }
            if (curPoint.Y + r + gMagnifier.Height > rect.Height)
            {
                top = curPoint.Y - r - gMagnifier.Height;
            }
            else
            {
                top = curPoint.Y + r;
            }

            return (left, top);
        }


    }
}
