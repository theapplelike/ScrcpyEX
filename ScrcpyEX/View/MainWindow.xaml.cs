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
            //RefreshScrcpy();
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
                    //Debug.WriteLine($"窗口是Z轴最顶端窗口，top yes。");
                }
                else
                {
                    WindowState = WindowState.Minimized;
                    //Debug.WriteLine($"窗口非Z轴最顶端窗口，top no。");
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
            if (!IsScClosed && IsActive)
            {
                _locationChangeTimer.Start();
            }
        }
        #endregion

        #region 自定义菜单栏按钮事件
        /// <summary>
        /// 点击自定义菜单栏拖拽窗口。
        /// </summary>
        private void GMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        //private void Btn_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    MyGM.SetSameColor(gMenu, Brushes.LightBlue, ((Button)sender).Tag);
        //}

        /// <summary>
        /// 定位的两个按钮颜色联动，鼠标离开时同时修改背景色。
        /// </summary>
        //private void Btn_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    MyGM.SetSameColor(gMenu, Brushes.Transparent, ((Button)sender).Tag);
        //}

        private void BtnLocOpt_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu(sender as Button, popSize);
        }

        private void BtnFuncOpt_Click(object sender, RoutedEventArgs e)
        {
            OpenMenu(sender as Button, popFunc);
        }

        private void BtnRecManage_Click(object sender, RoutedEventArgs e)
        {
            OpenRecordManage();
        }

        private void BtnRecStop_Click(object sender, RoutedEventArgs e)
        {
            StopScriptAndRecord();
        }

        private void BtnImgCompare_Click(object sender, RoutedEventArgs e)
        {
            ImgCompare(_scriptInfos[0]);
        }
        #endregion

        #region 位置和大小的菜单
        //private void Pop_Closed(object sender, EventArgs e)
        //{
        //    MyGM.SetSameColor(gMenu, Brushes.Transparent, ((Popup)sender).Tag);
        //}

        private void BtnQuarterLoc_Click(object sender, RoutedEventArgs e)
        {
            QuarterMove();
            popSize.IsOpen = false;
        }

        private void BtnOriginalLoc_Click(object sender, RoutedEventArgs e)
        {
            SetUISize();
            Locate();
            popSize.IsOpen = false;
        }

        private void BtnReduce_Click(object sender, RoutedEventArgs e)
        {
            ReduceSize();
        }

        private void BtnAmplify_Click(object sender, RoutedEventArgs e)
        {
            AmplifySize();
        }
        #endregion

        #region 功能菜单
        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            new Setup().ShowDialog();
        }

        /// <summary>
        /// 开始录制脚本。
        /// </summary>
        private void BtnRecScr_Click(object sender, RoutedEventArgs e)
        {
            StartRecord();
            popFunc.IsOpen = false;
        }

        /// <summary>
        /// 打开无线调试。
        /// </summary>
        private void BtnOpenWireless_Click(object sender, RoutedEventArgs e)
        {
            WirelessDebug();
        }

        private void BtnRemoveBlack_Click(object sender, RoutedEventArgs e)
        {
            RemoveScBlack(true);
            popFunc.IsOpen = false;
        }

        private void BtnScGotFocus_Click(object sender, RoutedEventArgs e)
        {
            ScGotFocus(null);
            popFunc.IsOpen = false;
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


    }
}
