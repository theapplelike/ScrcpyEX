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

            if (scrcpyHandle != (IntPtr)(-1))
            {
                prc.Kill();// 如果外部程序没有结束运行则强行终止
                Console.WriteLine("退出程序应用。");
            }
        }

        private async void BtnWired_Click(object sender, RoutedEventArgs e)
        {
            await ConnectScrcpy();
        }

        private void Stp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine($"stp大小变更:{Stp.ActualWidth}，{Stp.ActualHeight}，{Stp.Width}，{Stp.Height}。");
            //bd.Width = Stp.ActualWidth - 37;
            //bd.Height = Stp.ActualHeight - 314.5;
            Console.WriteLine($"差:{Stp.ActualWidth-bd.ActualWidth}，{Stp.ActualHeight-bd.ActualHeight}。");
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (scrcpyHandle != (IntPtr)(-1))
            {
                prc.Kill();// 如果外部程序没有结束运行则强行终止
                scrcpyHandle = (IntPtr)(-1);
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
            }
            else
            {
                WindowState = WindowState.Maximized;
                BtnMaximize.Content = "▣";
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Dpl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Dpl_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
