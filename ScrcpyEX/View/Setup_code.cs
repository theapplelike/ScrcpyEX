using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace ScrcpyEX.View
{
    public partial class Setup//code
    {
        private void MyInit()
        {
            stp1.Visibility = Visibility.Collapsed;
            stp2.Visibility = Visibility.Collapsed;
            _vm.Cfg = MyCfg.Cfg;
        }

        private void MyClose()
        {
            MyCfg.Save();
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

        private void MenuStateTransition1(StackPanel stp)
        {
            EnsureTransform(stp);
            if (stp.IsVisible)
            {
                // 创建动画作用于 Y 属性
                DoubleAnimation animation = new()
                {
                    To = -stp.ActualHeight,
                    From = 0,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                // 动画完成后隐藏面板
                animation.Completed += (s, _) => stp.Visibility = Visibility.Collapsed;

                // 应用动画
                stp.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
            else
            {
                stp.Visibility = Visibility.Visible;

                // 创建动画作用于 Y 属性
                DoubleAnimation animation = new()
                {
                    To = 0,
                    From = -stp.ActualHeight,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                // 应用动画
                stp.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        /// <summary>
        /// 确保控件有可用的 TranslateTransform。
        /// </summary>
        /// <param name="element">指定的控件。</param>
        private void EnsureTransform(UIElement element)
        {
            if (element.RenderTransform == null ||
                element.RenderTransform is not TranslateTransform ||
                element.RenderTransform.IsFrozen)
            {
                element.RenderTransform = new TranslateTransform();
            }
        }


    }
}
