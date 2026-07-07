using OpenCvSharp.ML;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// FileExplorer.xaml 的交互逻辑
    /// </summary>
    public partial class FileExplorer : UserControl
    {
        public FileExplorer()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private async void ChildGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid.DataContext is FileItem fi && fi.IsDirectory)
            {
                await NavigateTo(fi.FullPath, true);
            }
        }

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _curIndex--;
            await NavigateTo(_curIndex < 0 ? DirService.Root : _history[_curIndex]);
        }

        private async void BtnFwrd_Click(object sender, RoutedEventArgs e)
        {
            _curIndex++;
            await NavigateTo(_history[_curIndex]);
        }

        private async void BtnGoUp_Click(object sender, RoutedEventArgs e)
        {
            await NavigateTo(DirService.GetParentPath(CurPath), true);
        }

        private async void BtnNavi_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is BreadcrumbItem bi)
            {
                await NavigateTo(bi.FullPath, true);
            }
        }

        private async void BtnRoot_Click(object sender, RoutedEventArgs e)
        {
            await NavigateTo(DirService.Root, true);
        }

        /// <summary>
        /// 滚轮上：向左滚动，滚轮下：向右滚动。
        /// </summary>
        private void Scv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scv)
            {
                double delta = e.Delta > 0 ? -30 : 30;
                double offset = scv.HorizontalOffset + delta;
                offset = Math.Max(0, Math.Min(scv.ScrollableWidth, offset));
                scv.ScrollToHorizontalOffset(offset);
                e.Handled = true;
            }
        }

        private void TbkErr_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tbkErr.Visibility = Visibility.Collapsed;
        }

        
    }
}
