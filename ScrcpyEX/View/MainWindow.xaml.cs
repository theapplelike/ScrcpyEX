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
            SaveCfg();

            if (_scHandle != (IntPtr)(-1))
            {
                _prc.Kill();// 如果外部程序没有结束运行则强行终止
                Console.WriteLine("退出程序应用。");
            }
        }

        private async void BtnWired_Click(object sender, RoutedEventArgs e)
        {
            await ConnectScrcpy();
        }

        private async void Stp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isResize)
            {
                await ChgScrcpySize();
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_scHandle != (IntPtr)(-1))
            {
                _prc.Kill();// 如果外部程序没有结束运行则强行终止
                _scHandle = (IntPtr)(-1);
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
                BtnMaximize.Content = "▢";
                Stp.Margin = new Thickness(0);
            }
            else
            {
                WindowState = WindowState.Maximized;
                BtnMaximize.Content = "▣";
                Stp.Margin = new Thickness(6);
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Dpl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();//点击自定义标题栏拖拽窗口
        }

        private async void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine("窗体大小进行改变");
            //_isResize = true;
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("窗体鼠标：down");
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("窗体鼠标：up");
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"stp的大小:{Stp.ActualWidth}，{Stp.ActualHeight}，{Stp.Width}，{Stp.Height}。");
            Console.WriteLine($"gShow的大小:{gShow.ActualWidth}，{gShow.ActualHeight}，{gShow.Width}，{gShow.Height}。");
            Console.WriteLine($"wfh的大小:{wfh.ActualWidth}，{wfh.ActualHeight}，{wfh.Width}，{wfh.Height}。");
            Console.WriteLine($"panel的大小：{pnlShow.Width}，{pnlShow.Height}。");
            _isResize = false;
            await ChgScrcpySize();
        }
    }
}
