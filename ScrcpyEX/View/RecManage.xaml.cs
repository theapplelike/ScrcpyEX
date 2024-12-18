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

        
    }
}
