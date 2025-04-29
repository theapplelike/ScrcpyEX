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
        public Screenshot()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;

            //覆盖整个屏幕，使用VirtualScreenWidth会覆盖所有显示器，PrimaryScreenWidth只能覆盖主显示器
            Left = 0;
            Top = 0;
            //Width = SystemParameters.VirtualScreenWidth;
            //Height = SystemParameters.VirtualScreenHeight;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            Console.WriteLine($"主屏幕：" +
                $"{SystemParameters.PrimaryScreenWidth}，" +
                $"{SystemParameters.PrimaryScreenHeight}," +
                $"{SystemParameters.VirtualScreenWidth}," +
                $"{SystemParameters.VirtualScreenHeight}");

            gBack.Background = new ImageBrush(CaptureCurrentScreen())
            {
                Stretch = Stretch.Fill
            };

            //_timerUpdateMagnifier.Tick += (sender, e) => UpdateMagnifier_Tick(_curPoint);
            //_timerUpdateMagnifier.Interval = TimeSpan.FromMilliseconds(1);
            //_timerUpdateMagnifier.Start();

            
        }

        private DateTime _lastUpdateTime = DateTime.MinValue;
        private void UpdateMagnifierOnRender(object sender, EventArgs e)
        {
            // 控制更新频率（例如 30 FPS）
            //double diff = (DateTime.Now - _lastUpdateTime).TotalMilliseconds;
            //if (diff >= 1000)
            //{
            //    Console.WriteLine($"一秒帧数：{i}。");
            //    _lastUpdateTime = DateTime.Now;
            //    i = 0;
            //}

            if (stp.Visibility != Visibility.Visible)
            {
                //if (i%2 == 0)
                {
                    //UpdateMagnifier_Tick(Mouse.GetPosition(this));
                    //UpdateMagnifierWithWriteableBitmap(Mouse.GetPosition(this));

                    UpdateMagnifier(Mouse.GetPosition(gBack));
                }
                i++;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //InitializeWriteableBitmap();

            magnifierBitmap = new WriteableBitmap(_enlargeDiameter, _enlargeDiameter, 96, 96, PixelFormats.Pbgra32, null);
            img.Source = magnifierBitmap;

            // 捕获背景层的快照
            var rtb = new RenderTargetBitmap(
                (int)gBack.ActualWidth,
                (int)gBack.ActualHeight,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(gBack);

            sourceBitmap = new WriteableBitmap(rtb);

            CompositionTarget.Rendering += UpdateMagnifierOnRender;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //_timerUpdateMagnifier.Stop();
            CompositionTarget.Rendering -= UpdateMagnifierOnRender;
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
            //_curPoint = e.GetPosition(this);
            //var mousePos = Mouse.GetPosition(this);
            //Console.WriteLine($"坐标1：{_curPoint.X}，{_curPoint.Y}，坐标2：{mousePos.X}，{mousePos.Y}");

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Capturing(e.GetPosition(this));
            }
            //if (stp.Visibility != Visibility.Visible)
            //{
            //    if (i%6 == 0)
            //    {
            //        UpdateMagnifier_Tick(e.GetPosition(this));
            //    }
            //    i++;
            //}
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowCapture(e.GetPosition(this));
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
