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
            //Console.WriteLine($"窗体大小进行改变，Scrcpy当前：（{ScActualWidth}，{ScActualHeight}），比例：（{ScRatioX}，{ScRatioY}）。");

            //scrcpy程序句柄不为空调整scrcpy大小
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

        private void BtnWired_Click(object sender, RoutedEventArgs e)
        {
            StartScrcpy(true);
        }

        private void BtnWirel_Click(object sender, RoutedEventArgs e)
        {
            StartScrcpy(false);
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            HandStopScrcpy();
        }

        private void BtnClosePrg_Click(object sender, RoutedEventArgs e)
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

        private void BtnRecManage_Click(object sender, RoutedEventArgs e)
        {
            OpenRecordManage();
        }

        private void BtnRecOption_Click(object sender, RoutedEventArgs e)
        {
            popRec.IsOpen = true;
            btnRecManage.Background = btnRecOption.Background = Brushes.LightBlue;
        }

        private void BtnRecStop_Click(object sender, RoutedEventArgs e)
        {
            StopScriptAndRecord();
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

        private void BtnRemoveBlack_Click(object sender, RoutedEventArgs e)
        {
            RemoveBlack();
            popMenu.IsOpen = false;
        }

        private void BtnReduce_Click(object sender, RoutedEventArgs e)
        {
            ReduceSize();
        }

        private void BtnAmplify_Click(object sender, RoutedEventArgs e)
        {
            AmplifySize();
        }

        private void BtnScr_Click(object sender, RoutedEventArgs e)
        {
            StartRecord();
            popRec.IsOpen = false;
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            new Setup().ShowDialog();
        }

        private void BtnMappingEdit_Click(object sender, RoutedEventArgs e)
        {
            MappingEdit(btnMappingEdit.Content.ToString());
        }

        private void BtnMappingAdd_Click(object sender, RoutedEventArgs e)
        {
            MappingAdd();
        }

        #endregion

        
    }
}
