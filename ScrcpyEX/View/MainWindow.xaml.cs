using ScrcpyEX.ExtensionMethod;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScrcpyEX.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveCfg();

            if (scrcpyHandle != (IntPtr)(-1))
            {
                prc.Kill();// 如果外部程序没有结束运行则强行终止
                Console.WriteLine("退出程序应用。");
            }
        }

        private async void BtnWired_Click(object sender, RoutedEventArgs e)
        {
            if (scrcpyHandle != (IntPtr)(-1)) { return; }

            string arg = $"{GetAllArguments()} --select-usb";
            
            ProcessStartInfo info = new(MV.ScrcpyPath)
            {
                Arguments = arg,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                //RedirectStandardInput = true,
            };
            prc.StartInfo = info;
            prc.Start();
            prc.BeginOutputReadLine();
            prc.BeginErrorReadLine();
            
            do
            {
                await Task.Delay(500);
                if (EmbeddedForm())
                {
                    break;
                }
                Console.WriteLine($">>>>>>>>>>>>>>查找程序：{scrcpyName}。");
            }
            while (true);
        }

        
    }
}
