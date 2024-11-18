using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScrcpyEX.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _resizeTimer.Stop();

            SaveCfg();
            ClosePrc();
        }

        private async void BtnWired_Click(object sender, RoutedEventArgs e)
        {
            if (_scHandle == IntPtr.Zero)
            {
                BtnWired.IsEnabled = false;
                await ConnectScrcpy();
                BtnWired.IsEnabled = true;
            }
        }

        /// <summary>
        /// 外部程序被Kill()后，会执行Exited事件，前提是EnableRaisingEvents要设置为true。
        /// </summary>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_scHandle != IntPtr.Zero)
            {
                _prc.Kill();
                Console.WriteLine("手动结束scrcpy程序。");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                BtnMaximize.Content = "\ue923";
                Stp.Margin = new Thickness(0);
            }
            else
            {
                WindowState = WindowState.Maximized;
                BtnMaximize.Content = "\ue922";
                Stp.Margin = new Thickness(6);
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 点击自定义标题栏拖拽窗口。
        /// </summary>
        private void Dpl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine($"窗体大小进行改变：{Keyboard.FocusedElement}。");

            //第三方程序句柄不为空调整scrcpy大小
            if (_scHandle != IntPtr.Zero)
            {
                _resizeTimer.Start();
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine($"gShow的大小:{gShow.ActualWidth}，{gShow.ActualHeight}，{gShow.Width}，{gShow.Height}。");

            //Console.WriteLine($"stp的大小:{Stp.ActualWidth}，{Stp.ActualHeight}，{Stp.Width}，{Stp.Height}。");
            //Console.WriteLine($"wfh的大小:{wfh.ActualWidth}，{wfh.ActualHeight}，{wfh.Width}，{wfh.Height}。");
            //Console.WriteLine($"panel的大小：{pnlShow.Width}，{pnlShow.Height}。");
            //Console.WriteLine($"焦点：{Keyboard.FocusedElement}。");
            //_isResize = false;
            //await ChgScrcpySize();

            pop.IsOpen = false;
            pop.IsOpen = true;
            _popTimer.Start();
        }

        private void pnlShow_GotFocus(object sender, EventArgs e)
        {
            //Console.WriteLine("pnlShow获得焦点。");
        }

        private void wfh_GotFocus(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("wfh获得焦点。");
        }

        
    }
}
