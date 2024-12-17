using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace ScrcpyEX.View
{
    /// <summary>
    /// RecManage.xaml 的交互逻辑
    /// </summary>
    public partial class RecManage : Window
    {
        private readonly RecManageVM _vm;

        public RecManage()
        {
            InitializeComponent();
            _vm = (RecManageVM)DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.RecInfos = MyCfg.Cfg.RecInfos;
            foreach (RecInfo recInfo in MyCfg.Cfg.RecInfos)
            {
                Console.WriteLine($"///////////，{recInfo.Name}。");
                foreach (PointInfo info in recInfo.Infos)
                {
                    Console.WriteLine($"{info.Pt.X},{info.Pt.Y},{info.LastTime}。");
                }
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine($"{obj.GetType()}，{lst.ItemTemplate.GetType()}.");

            InitListBox(lst);

            DependencyObject obj = e.OriginalSource as DependencyObject;
            Grid parent = MyGM.GetParentObject<Grid>(obj);
            TextBox txt = MyGM.FindVisualChild<TextBox>(parent);
            TextBlock tbk = MyGM.FindVisualChild<TextBlock>(parent);
            
            if (txt.Visibility == Visibility.Hidden)
            {
                ((Button)sender).Content = "完成";
                txt.Visibility = Visibility.Visible;
                tbk.Visibility = Visibility.Hidden;
            }
            else
            {
                ((Button)sender).Content = "编辑";
                txt.Visibility = Visibility.Hidden;
                tbk.Visibility = Visibility.Visible;
                MyCfg.Save();
            }
        }

        /// <summary>
        /// 每次编辑之前初始化ListBox的状态。
        /// </summary>
        /// <param name="obj">ListBox元素。</param>
        private void InitListBox(DependencyObject obj)
        {
            if (obj == null) return;
            if (obj is Button btn && btn.Name == "btnEdit")
            {
                btn.Content = "编辑";
            }
            if (obj is TextBox txt && txt.Name == "txtEdit")
            {
                txt.Visibility = Visibility.Hidden;
            }
            if (obj is TextBlock tbk && tbk.Name == "tbkEdit")
            {
                tbk.Visibility = Visibility.Visible;
            }

            // 递归遍历子控件
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                InitListBox(child);
            }
        }




    }
}
