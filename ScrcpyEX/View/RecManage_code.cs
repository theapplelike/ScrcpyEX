using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ScrcpyEX.MyDefinition;

namespace ScrcpyEX.View
{
    public partial class RecManage//code
    {
        private void MyInit()
        {
            _vm.RecInfos = new(MyCfg.Cfg.RecInfos);
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
                btn.Content = "\ue8ac";
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

        /// <summary>
        /// 编辑脚本名称。
        /// </summary>
        /// <param name="sender">触发事件的按钮对象。</param>
        /// <param name="e">事件参数。</param>
        private void EditName(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            Grid parent = MyGM.GetParentObject<Grid>(obj);
            TextBox txt = MyGM.FindVisualChild<TextBox>(parent);
            TextBlock tbk = MyGM.FindVisualChild<TextBlock>(parent);

            if (txt.Visibility == Visibility.Hidden)
            {
                Console.WriteLine("进行编辑");
                //如果当前项要变成编辑状态，但已有其他项为编辑状态，则初始化所有项
                if (_isEditing)
                {
                    InitListBox(lst);
                }

                _isEditing = true;
                ((Button)sender).Content = "\uf78c";
                txt.Visibility = Visibility.Visible;
                tbk.Visibility = Visibility.Hidden;

                txt.Focus();
                txt.Select(txt.Text.Length, 0);
            }
            else
            {
                Console.WriteLine("编辑完成");
                _isEditing = false;
                ((Button)sender).Content = "\ue8ac";
                txt.Visibility = Visibility.Hidden;
                tbk.Visibility = Visibility.Visible;
            }
        }


    }
}
