using System;
using System.Collections.Generic;
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

namespace ScrcpyEX.View
{
    /// <summary>
    /// DataSync.xaml 的交互逻辑
    /// </summary>
    public partial class DataSync : Window
    {
        public DataSync()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private async void BtnToAdb_Click(object sender, RoutedEventArgs e)
        {
            await SendFile(true, "推送");
        }

        private async void BtnToLcl_Click(object sender, RoutedEventArgs e)
        {
            await SendFile(false, "拉取");
        }

        private async void BtnSync_Click(object sender, RoutedEventArgs e)
        {
            await SyncFile();
        }
    }
}
