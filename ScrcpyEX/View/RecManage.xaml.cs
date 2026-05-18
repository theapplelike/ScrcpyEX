using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace ScrcpyEX.View
{
    /// <summary>
    /// RecManage.xaml 的交互逻辑
    /// </summary>
    public partial class RecManage : Window
    {
        /// <summary>
        /// 录制的脚本管理界面。
        /// </summary>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        public RecManage(IntPtr scHandle)
        {
            InitializeComponent();
            _vm = (RecManageVM)DataContext;
            _scHandle = scHandle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private void Root_Closed(object sender, EventArgs e)
        {
            MyClose();
        }

        private void G_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditName(((Button)sender).DataContext as ScriptItem);
        }

        private void BtnAct_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScript((Button)sender);
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            EditScript((Button)sender);
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            DeleteScript((Button)sender);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveScript();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            DoScreenshot();
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowImg showImg;
            if (_cropRect.Width > 0 && _cropRect.Height > 0)
            {
                showImg = new(img.Source, _cropRect);
            }
            else
            {
                showImg = new(img.Source, GetCurrScript().CropRect);
            }
            showImg.ShowDialog();
        }

        private void RdoImage_Checked(object sender, RoutedEventArgs e)
        {
            gSuboption.Visibility = Visibility.Visible;
        }

        private void RdoImage_Unchecked(object sender, RoutedEventArgs e)
        {
            gSuboption.Visibility = Visibility.Hidden;
        }

        private void CmbMultipleScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMultipleScript.SelectedItem is ScriptInfo script)
            {
                ValToUI(script);
                LoadPointInfos(script.PointInfos);
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragSourceItem = sender as ListBoxItem;
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            DragListBoxItem(_dragSourceItem, e);
        }

        private void ListBoxItem_DragEnter(object sender, DragEventArgs e)
        {
            // 只接受来自内部的自定义数据格式
            if (e.Data.GetDataPresent(typeof(ScriptInfo)) || e.Data.GetDataPresent(typeof(ScriptGroup)))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            CreatGroupScript((sender as ListBoxItem).DataContext, e.Data.GetData(typeof(ScriptInfo)));
            e.Handled = true;
        }

        private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditScript(e.AddedItems[0]);
        }

        /// <summary>
        /// 文本框获得焦点的时候选择其中所有内容。
        /// </summary>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt)
            {
                txt.SelectAll();
                Dispatcher.BeginInvoke(() => txt.SelectAll(), DispatcherPriority.Input);
            }
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            string desc = "执行组脚本时，如果其中第一个是计时脚本，则执行参数以第一个脚本为准。" +
                "每次执行都会将其中所有脚本的点击操作执行一遍。" +
                "如果第一个是图像脚本，则其中所有的脚本都需要是图像脚本，且有图像。" +
                "如果有图像脚本包含（A，B），则会按照A-B-A-B的顺序比较图像相似度。";
            MyGM.ShwInfo(desc);
        }
    }
}
