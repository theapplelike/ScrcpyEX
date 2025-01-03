using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ScrcpyEX.View
{
    public partial class Setup//code
    {
        private void Init()
        {
            stp1.Visibility = Visibility.Collapsed;
            _vm.Cfg = MyCfg.Cfg;
        }

        /// <summary>
        /// 菜单状态转换，如果菜单显示，持续减少高度后隐藏；如果菜单隐藏，显示菜单后，持续增加高度到最大值。
        /// </summary>
        /// <param name="pnl">要进行状态转换的菜单。</param>
        private async void MenuStateTransition(StackPanel stp)
        {
            if (stp.IsVisible)
            {
                stp.Height = stp.ActualHeight;
                for (int i = 0; i < 5; i++)
                {
                    stp.Height -= (int)(stp.Height * 0.2);
                    //Console.WriteLine($"降低高度：{stp.Height},{stp.ActualHeight}。");
                    await Task.Delay(10);
                }
                stp.Visibility = Visibility.Collapsed;
            }
            else
            {
                stp.Visibility = Visibility.Visible;
                for (int i = 0; i < 5; i++)
                {
                    stp.Height += (int)(stp.Height * 0.2);
                    //Console.WriteLine($"++增加高度：{stp.Height},{stp.ActualHeight}。");
                    await Task.Delay(10);
                }
                stp.Height = stp.ActualHeight;
            }
        }


    }
}
