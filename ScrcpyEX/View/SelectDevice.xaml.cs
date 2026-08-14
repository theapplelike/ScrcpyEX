using OpenCvSharp.ML;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace ScrcpyEX.View
{
    /// <summary>
    /// SelectDevice.xaml 的交互逻辑
    /// </summary>
    public partial class SelectDevice : Window
    {
        /// <summary>
        /// 有多个设备时选择启动设备的界面。
        /// </summary>
        /// <param name="devices">通过ADB获得的设备 序列号 或 IP地址。</param>
        /// <param name="isWired">是否有线连接。</param>
        public SelectDevice(List<string> devices, bool isWired)
        {
            InitializeComponent();
            _vm = (SelectDeviceVM)DataContext;
            _devices = devices;
            _isWired = isWired;
            _vm.BtnChar = isWired ? "\uecf0" : "\uee77";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDeviceInfo();
        }

        private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lst.SelectedItem != null)
            {
                Index = lst.SelectedIndex;
                DialogResult = true;
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Index = lst.Items.IndexOf((sender as Button).DataContext);
            DialogResult = true;
        }
    }
}
