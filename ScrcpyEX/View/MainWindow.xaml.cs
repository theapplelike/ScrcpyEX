using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            _vm = (MainWindowVM)DataContext;
        }

        #region Window界面事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MyClose();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine($"窗体大小进行改变，scrcpy句柄：{_scHandle}。");

            //第三方程序句柄不为空调整scrcpy大小
            if (_scHandle != IntPtr.Zero)
            {
                _resizeTimer.Start();
            }
        }
        #endregion

        #region 自定义菜单栏事件
        /// <summary>
        /// 点击自定义标题栏拖拽窗口。
        /// </summary>
        private void Dpl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void BtnWired_Click(object sender, RoutedEventArgs e)
        {
            if (_scHandle == IntPtr.Zero)
            {
                btnWired.IsEnabled = false;
                await ConnectScrcpy();
                btnWired.IsEnabled = true;
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
            Maximize();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 定位的两个按钮颜色联动，鼠标进入时同时修改背景色。
        /// </summary>
        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            MyGM.SetSameColor(dpl, Brushes.LightBlue, ((Button)sender).Tag);
        }

        /// <summary>
        /// 定位的两个按钮颜色联动，鼠标离开时同时修改背景色。
        /// </summary>
        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            MyGM.SetSameColor(dpl, Brushes.Transparent, ((Button)sender).Tag);
        }

        private void BtnLocate_Click(object sender, RoutedEventArgs e)
        {
            Locate();
        }

        private void BtnLocOpt_Click(object sender, RoutedEventArgs e)
        {
            //联动两个定位按钮，显示一致的背景色
            popMenu.IsOpen = true;
            btnLocate.Background = btnLocOpt.Background = Brushes.LightBlue;
        }

        private void BtnRecord_Click(object sender, RoutedEventArgs e)
        {
            //popRec.IsOpen = true;
            //btnRecord.Background = btnRecOpt.Background = Brushes.LightBlue;
            StartRecord();
        }

        private void BtnRecOpt_Click(object sender, RoutedEventArgs e)
        {
            //StartRecord();
            //bool? reult = new RecManage().ShowDialog();
            //Console.WriteLine($"结果:{reult}。");
            RecManage rec = new();
            if (rec.ShowDialog() == true)
            {
                _pts = rec._pts;
                _clickTimer.Interval = TimeSpan.FromSeconds(10);
                _clickTimer.Start();
            }
        }
        #endregion

        #region 弹出菜单事件
        private void PopMid_Opened(object sender, EventArgs e)
        {
            popMid.VerticalOffset = (ExtraHeightTop + popMid.Child.RenderSize.Height) / 2 + wfh.Margin.Top;
        }

        private void Pop_Closed(object sender, EventArgs e)
        {
            MyGM.SetSameColor(dpl, Brushes.Transparent, ((Popup)sender).Tag);
        }

        private void BtnQuarterLoc_Click(object sender, RoutedEventArgs e)
        {
            QuarterMove();
            popMenu.IsOpen = false;
        }

        private void BtnOriginalLoc_Click(object sender, RoutedEventArgs e)
        {
            SetUISize();
            Locate();
            popMenu.IsOpen = false;
        }

        private void BtnRecManage_Click(object sender, RoutedEventArgs e)
        {
            //RecManage rec = new();
            //rec.ShowDialog();
        }

        private void BtnStartRec_Click(object sender, RoutedEventArgs e)
        {
            //StartRecord();
            //popRec.IsOpen = false;
            //btnRecOpt.IsEnabled = true;
        }
        #endregion

        private void pnlShow_GotFocus(object sender, EventArgs e)
        {
            //Console.WriteLine("pnlShow获得焦点。");
        }

        private void wfh_GotFocus(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("wfh获得焦点。");
        }

        private void wfh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("鼠标在wfh按下。");
        }

        private void PnlShow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Console.WriteLine("鼠标在pnlShow按下。");
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("鼠标在窗体按下。");
        }

        
    }
}
