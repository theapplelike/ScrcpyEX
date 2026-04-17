using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
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
            SourceInitialized += OnSourceInitialized;
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
            WindowSizeChanged(e);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //Console.WriteLine($"-------------窗口状态改变：{WindowState}");
            RefreshScrcpy();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //Console.WriteLine($"++++++++++窗口激活,{IsActive},{DLL.GetForegroundWindow()}");

            // 延迟检查，确保窗口已经完全激活
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DLL.GetForegroundWindow() == _mainHandle)
                {
                    ShowKeyMapping();
                    Debug.WriteLine($"窗口是Z轴最顶端窗口，top yes。");
                }
                else
                {
                    WindowState = WindowState.Minimized;
                    Debug.WriteLine($"窗口非Z轴最顶端窗口，top no。");
                }
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        
        private void Window_Deactivated(object sender, EventArgs e)
        {
            //Console.WriteLine($"++++++++++窗口失活,{IsActive},{WindowState}");
            HideKeyMapping();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //Console.WriteLine($"窗口位置改变,{IsActive}");
            if (!IsScClosed && IsActive)
            {
                _locationChangeTimer.Start();
            }
        }
        #endregion

        #region 自定义菜单栏按钮事件
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
            CloseScrcpy();
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
            Button btn = sender as Button;

            //在溢出菜单中时，不再依赖按钮，在鼠标位置弹出菜单
            if (ToolBar.GetIsOverflowItem(btn))
            {
                popMenu.PlacementTarget = null;
                popMenu.Placement = PlacementMode.MousePoint;
                
            }
            else
            {
                popMenu.PlacementTarget = btnLocOpt;
                popMenu.Placement = PlacementMode.Bottom;
            }

            popMenu.IsOpen = true;
        }

        private void BtnRecManage_Click(object sender, RoutedEventArgs e)
        {
            OpenRecordManage();
        }

        private void BtnRecOption_Click(object sender, RoutedEventArgs e)
        {
            popRec.IsOpen = true;
        }

        private void BtnRecStop_Click(object sender, RoutedEventArgs e)
        {
            StopScriptAndRecord();
        }

        private void BtnImgCompare_Click(object sender, RoutedEventArgs e)
        {
            ImgCompare(_recInfo);
        }
        #endregion

        #region 弹出菜单事件

        private void PopMid_Opened(object sender, EventArgs e)
        {
            popMid.VerticalOffset = (ExtraHeightTop + popMid.Child.RenderSize.Height) / 2 + gShow.Margin.Top;
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

        private void BtnScGotFocus_Click(object sender, RoutedEventArgs e)
        {
            ScGotFocus(null);
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

        /// <summary>
        /// 开始录制脚本。
        /// </summary>
        private void BtnScr_Click(object sender, RoutedEventArgs e)
        {
            StartRecord();
            popRec.IsOpen = false;
        }

        /// <summary>
        /// 打开无线调试。
        /// </summary>
        private void BtnOpenWireless_Click(object sender, RoutedEventArgs e)
        {
            WirelessDebug();
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            new Setup().ShowDialog();
        }

        /// <summary>
        /// 映射按钮编辑。
        /// </summary>
        private void BtnMappingEdit_Click(object sender, RoutedEventArgs e)
        {
            MappingEdit(btnMappingEdit.Content.ToString());
        }

        private void BtnMappingAdd_Click(object sender, RoutedEventArgs e)
        {
            MappingAdd();
        }

        private void BtnMappingBack_Click(object sender, RoutedEventArgs e)
        {
            MappingCancel();
        }
        #endregion

        #region 右侧溢出菜单
        private void PopRightOvfMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            _rightOvfMenuTimer.Stop();
        }

        private void PopRightOvfMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            _rightOvfMenuTimer.Start();
        }

        private void BtnClosePrg_MouseEnter(object sender, MouseEventArgs e)
        {
            _rightOvfMenuTimer.Stop();
            popRightOvfMenu.IsOpen = true;
        }

        private void BtnClosePrg_MouseLeave(object sender, MouseEventArgs e)
        {
            _rightOvfMenuTimer.Start();
        }
        #endregion

        #region Scrcpy功能菜单
        private void PopScOpt_MouseEnter(object sender, MouseEventArgs e)
        {
            _scOptMenuTimer.Stop();
        }

        private void PopScOpt_MouseLeave(object sender, MouseEventArgs e)
        {
            _scOptMenuTimer.Start();
        }

        /// <summary>
        /// 此事件由主要Scrcpy功能按钮绑定。
        /// </summary>
        private void BtnScMain_MouseEnter(object sender, MouseEventArgs e)
        {
            _scOptMenuTimer.Stop();
            popScOpt.IsOpen = true;

            Debug.WriteLine($"主按钮触发：BtnScMain_MouseEnter+++");
        }

        /// <summary>
        /// 此事件由主要Scrcpy功能按钮绑定。
        /// </summary>
        private void BtnScMain_MouseLeave(object sender, MouseEventArgs e)
        {
            _scOptMenuTimer.Start();
            Debug.WriteLine($"主按钮触发：BtnScMain_MouseLeave---");
        }
        #endregion

        
    }
}
