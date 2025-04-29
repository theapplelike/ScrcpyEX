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

            stp.Visibility = Visibility.Hidden;
            //Console.WriteLine($"开始的位置:{_startPoint.X},{_startPoint.Y}。");
        }

        /// <summary>
        /// 右击的时候返回上一个步骤，已有截图则重新截图；未截图则关闭截图界面。
        /// </summary>
        private void RightClick()
        {
            if (stp.Visibility == Visibility.Visible)
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
            Console.WriteLine("截图中...");
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
        /// 显示用户的截图和操作栏。
        /// </summary>
        /// <param name="endPoint">截图结束时的坐标点。</param>
        private void ShowCapture(Point endPoint)
        {
            _endPoint = endPoint;
            double y = endPoint.Y + stp.ActualHeight > GetMaxHeigth(endPoint) ? endPoint.Y - stp.ActualHeight : endPoint.Y;
            Console.WriteLine($"当前y轴：{endPoint.Y},{stp.ActualHeight},当前屏幕高度：{GetMaxHeigth(endPoint)}。");

            foreach (Screen screen in Screen.AllScreens)
            {
                // 检查鼠标坐标是否在显示器范围内
                if (screen.Bounds.Contains(new System.Drawing.Point((int)endPoint.X*2, (int)endPoint.Y*2)))
                {
                    bool isPrimary = screen.Primary;
                    string info = $"{(isPrimary ? "主显示器" : "副显示器")}，" +
                                         $"分辨率：{screen.Bounds.Width}x{screen.Bounds.Height}";
                    Console.WriteLine(info);
                    break;
                }
            }

            stp.Margin = new Thickness(endPoint.X - stp.ActualWidth, y, 0, 0);
            stp.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 获得指定坐标所在显示屏的高度，用于多显示屏的情况。
        /// </summary>
        /// <param name="point">指定的坐标点。</param>
        private double GetMaxHeigth(Point point)
        {
            if (point.X < SystemParameters.PrimaryScreenWidth)
            {
                return SystemParameters.PrimaryScreenHeight;//主显示屏高度
            }
            else
            {
                return SystemParameters.VirtualScreenHeight;//虚拟屏幕高度
            }
        }

        /// <summary>
        /// 更新放大镜内容和位置。
        /// </summary>
        /// <param name="curPoint">鼠标的当前位置。</param>
        private void UpdateMagnifier(Point curPoint)
        {
            int sourceX = (int)(curPoint.X - _d / 2);
            int sourceY = (int)(curPoint.Y - _d / 2);

            // 边界检查
            sourceX = MyGM.Clamp(sourceX, 0, _sourceBitmap.PixelWidth - _d);
            sourceY = MyGM.Clamp(sourceY, 0, _sourceBitmap.PixelHeight - _d);
            try
            {
                _magnifierBitmap.Lock();

                // 将源背景图的指定像素直接复制给放大镜位图
                _sourceBitmap.CopyPixels(
                    new Int32Rect(sourceX, sourceY, _d, _d),
                    _magnifierBitmap.BackBuffer,
                    _magnifierBitmap.BackBufferStride * _d,
                    _magnifierBitmap.BackBufferStride);

                _magnifierBitmap.AddDirtyRect(new Int32Rect(0, 0, _d, _d));
            }
            finally
            {
                _magnifierBitmap.Unlock();
            }

            gMagnifier.Margin = new Thickness(curPoint.X + 25, curPoint.Y + 25, 0, 0);
        }


    }
}
