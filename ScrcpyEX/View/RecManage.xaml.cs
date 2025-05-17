using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
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

namespace ScrcpyEX.View
{
    /// <summary>
    /// RecManage.xaml 的交互逻辑
    /// </summary>
    public partial class RecManage : Window
    {
        public RecManage()
        {
            InitializeComponent();
            _vm = (RecManageVM)DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private void Root_Closed(object sender, EventArgs e)
        {
            _vm.SaveData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditName(sender, e);
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScript((Button)sender);
        }

        private void BtnSet_Click(object sender, RoutedEventArgs e)
        {
            EditScript((Button)sender);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveScript();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            //if (TempRecInfo != null)
            //{
            //    ValToUI(TempRecInfo);
            //    _vm.Canel();
            //}
            DialogResult = false;
        }

        private void BtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            DoScreenshot();
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowImg showImg = new(img.Source);
            showImg.ShowDialog();
        }

        
    }
}
