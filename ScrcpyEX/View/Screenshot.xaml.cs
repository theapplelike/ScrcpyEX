using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScrcpyEX.View
{
    /// <summary>
    /// Screenshot.xaml 的交互逻辑
    /// </summary>
    public partial class Screenshot : Window
    {
        /// <summary>
        /// 用于给Scrcpy截图的窗口。
        /// </summary>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        public Screenshot(IntPtr scHandle)
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;

            //截图区域创建到Scrcpy的上方
            DLL.LpRect rect = MyGM.GetHandleRect(scHandle);
            Top = rect.Top / DpiScaleY;
            Left = rect.Left / DpiScaleX;
            Width = rect.Width / DpiScaleX;
            Height = rect.Height / DpiScaleY;

            //Left = MV.MainWin.Left;
            //Top = MV.MainWin.Top;
            //Width = MV.MainWin.Width;
            //Height = MV.MainWin.Height;

            //gBack.Background = new ImageBrush(MyGM.CaptureProcessArea(DpiScaleX, DpiScaleY))
            //{
            //    Stretch = Stretch.Fill
            //};
            gBack.Background = new ImageBrush(MyGM.GetHandleImage(scHandle))
            {
                Stretch = Stretch.Fill
            };

            //覆盖整个屏幕，使用VirtualScreenWidth会覆盖所有显示器，PrimaryScreenWidth只能覆盖主显示器
            //Left = 0;
            //Top = 0;
            //Width = SystemParameters.VirtualScreenWidth;
            //Height = SystemParameters.VirtualScreenHeight;
            //Width = SystemParameters.PrimaryScreenWidth;
            //Height = SystemParameters.PrimaryScreenHeight;

            //Console.WriteLine($"主屏幕：" +
            //    $"{SystemParameters.PrimaryScreenWidth}，" +
            //    $"{SystemParameters.PrimaryScreenHeight}," +
            //    $"{SystemParameters.VirtualScreenWidth}," +
            //    $"{SystemParameters.VirtualScreenHeight}");
            //
            //gBack.Background = new ImageBrush(CaptureCurrentScreen())
            //{
            //    Stretch = Stretch.Fill
            //};

            //放大镜根据DPI缩放
            _d = (int)(50 * DpiScaleX);
            _magnifierBitmap = new(_d, _d, 96 * DpiScaleX, 96 * DpiScaleY, PixelFormats.Pbgra32, null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                StartCapture(e.GetPosition(this));
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Capturing(e.GetPosition(this));
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowToolBar(e.GetPosition(this));
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RightClick();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            CaptureComplete();
        }

        
    }
}
