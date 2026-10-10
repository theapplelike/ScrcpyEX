using ScrcpyEX.ExtensionMethod;
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
    /// Setup.xaml 的交互逻辑
    /// </summary>
    public partial class Setup : Window
    {
        public Setup()
        {
            InitializeComponent();
            _vm = (SetupVM)DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyInit();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MyClose();
        }

        private void BtnScrcpy_Click(object sender, RoutedEventArgs e)
        {
            MenuStateTransition(stp1, 0);
        }

        private void BtnProg_Click(object sender, RoutedEventArgs e)
        {
            MenuStateTransition(stp2, 1);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnOpenPath_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", $"\"{MV.SuccessImgPath}\"");
        }
    }
}
