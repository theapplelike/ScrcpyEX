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
        public SelectDevice(List<string> devices)
        {
            InitializeComponent();
            _devices = devices;
            _vm = (SelectDeviceVM)DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetDeviceInfo();
        }

        private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lst.SelectedItem != null)
            {
                //Console.WriteLine($"选中了: {lst.SelectedItem},{lst.SelectedIndex}");
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
