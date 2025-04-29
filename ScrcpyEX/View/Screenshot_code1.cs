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

            //Cursor = Cursors.Cross;
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

            //UpdateMagnifier_Tick(curPoint);
            UpdateMask();
            Console.WriteLine("截图中...");
        }

        /// <summary>
        /// 显示用户的截图和操作栏。
        /// </summary>
        /// <param name="endPoint">截图结束时的坐标点。</param>
        private void ShowCapture(Point endPoint)
        {
            _endPoint = endPoint;
            double y = endPoint.Y + stp.ActualHeight > GetMaxHeigth(endPoint) ? endPoint.Y - stp.ActualHeight : endPoint.Y;
            //Console.WriteLine($"当前y轴：{endPoint.Y},{stp.ActualHeight},当前屏幕高度：{GetMaxHeigth(endPoint)}。");

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

        RenderTargetBitmap rtb;
        private WriteableBitmap _wb;
        int i = 0;
        /// <summary>
        /// 更新放大镜，放大比例为：放大镜控件尺寸/放大区域直径。
        /// 为了性能，放大镜使用的图片统一为96DPI，不根据屏幕DPI放大。
        /// </summary>
        /// <param name="curPoint">鼠标的当前位置。</param>
        private void UpdateMagnifier_Tick(Point curPoint)
        {
            int size = 50;//需要放大区域的直径
            Int32Rect rect = new(
                (int)(curPoint.X ) - size / 2,
                (int)(curPoint.Y ) - size / 2,
                size, size);
            rect.X = rect.X < 0 ? 0 : rect.X;
            rect.Y = rect.Y < 0 ? 0 : rect.Y;

            //创建渲染目标，只渲染背景层内容，防止添加边框线
            rtb = new RenderTargetBitmap(
                (int)(gBack.ActualWidth ),
                (int)(gBack.ActualHeight ),
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(gBack);

            Console.WriteLine($"更新放大镜:{(int)(gBack.ActualWidth * DpiScaleX)},{(int)(gBack.ActualHeight * DpiScaleY)}");

            // 裁剪目标区域
            img.Source= new CroppedBitmap(rtb, rect);
            gMagnifier.Margin = new Thickness(curPoint.X + 25, curPoint.Y + 25, 0, 0);

        }

        // 初始化方法（在窗口加载或内容层尺寸变化时调用）
        private void InitializeWriteableBitmap()
        {
            _wb = new WriteableBitmap(
                (int)gBack.ActualWidth,
                (int)gBack.ActualHeight,
                96,  // DPI X
                96,  // DPI Y
                PixelFormats.Bgra32, // 必须与像素数据格式匹配
                null // 调色板（不需要时设为 null）
            );
        }

        private void UpdateMagnifierWithWriteableBitmap(Point curPoint)
        {
            // 确保 WriteableBitmap 已初始化
            if (_wb == null) InitializeWriteableBitmap();

            int size = 50;//需要放大区域的直径
            Int32Rect rect = new(
                (int)(curPoint.X) - size / 2,
                (int)(curPoint.Y) - size / 2,
                size, size);
            rect.X = rect.X < 0 ? 0 : rect.X;
            rect.Y = rect.Y < 0 ? 0 : rect.Y;

            // 定义像素数据缓冲区
            int bytesPerPixel = (_wb.Format.BitsPerPixel + 7) / 8; // 每个像素的字节数
            int stride = _wb.PixelWidth * bytesPerPixel; // 计算步长（每行字节数）
            byte[] pixels = new byte[_wb.PixelHeight * stride];

            // 从 WriteableBitmap 复制像素数据
            _wb.CopyPixels(pixels, stride, 0);

            // ----------------------------
            // 在此处处理像素数据（例如放大、修改颜色等）
            // 示例：将中心区域像素放大（伪代码）
            // ----------------------------

            // 将修改后的数据写回 WriteableBitmap
            _wb.WritePixels(
                new Int32Rect(0, 0, _wb.PixelWidth, _wb.PixelHeight), // 目标区域
                pixels,        // 像素数据
                stride,        // 步长
                0              // 偏移量
            );

            img.Source= new CroppedBitmap(_wb, rect);
            gMagnifier.Margin = new Thickness(curPoint.X + 25, curPoint.Y + 25, 0, 0);
        }

        private void UpdateMagnifier_Tick1(Point curPoint)
        {
            _wb = new WriteableBitmap(
                    (int)gBack.ActualWidth,
                    (int)gBack.ActualHeight,
                    96, 96, PixelFormats.Bgra32, null);

            // 使用 CopyPixels 直接获取像素数据
            byte[] pixels = new byte[_wb.PixelWidth * _wb.PixelHeight * 4];
            _wb.CopyPixels(pixels, _wb.BackBufferStride, 0);
        }


    }
}
