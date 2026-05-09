using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ScrcpyEX.View
{
    public partial class Setup//code
    {
        private void MyInit()
        {
            stp1.Visibility = Visibility.Collapsed;
            stp2.Visibility = Visibility.Collapsed;
            _vm.Cfg = MyCfg.Cfg;
            _oldIsCanResize = MyCfg.Cfg.IsCanResize;
        }

        private void MyClose()
        {
            MyCfg.Save();

            if (_oldIsCanResize != MyCfg.Cfg.IsCanResize)
            {
                AdjResizeMode(MV.MainWin);
            }
        }

        /// <summary>
        /// 调整主窗口的ResizeMode属性。
        /// </summary>
        private void AdjResizeMode(MainWindow main)
        {
            if (MyCfg.Cfg.IsCanResize)
            {
                main.ResizeMode = ResizeMode.CanResize;
            }
            else
            {
                main.ResizeMode = ResizeMode.CanMinimize;
            }
        }

        /// <summary>
        /// 菜单显示状态转换，使用动画过度效果。
        /// </summary>
        /// <param name="stp">要进行状态转换的菜单。</param>
        private void MenuStateTransition(StackPanel stp)
        {
            // 停止可能正在进行的动画，避免冲突
            stp.BeginAnimation(HeightProperty, null);

            if (stp.IsVisible) // 当前可见 → 折叠
            {
                // 获取当前实际高度作为动画起始值
                double fromHeight = stp.ActualHeight;

                DoubleAnimation animation = new()
                {
                    From = fromHeight,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                animation.Completed += (s, _) => stp.Visibility = Visibility.Collapsed;
                stp.BeginAnimation(HeightProperty, animation);
            }
            else // 当前隐藏 → 展开
            {
                // 必须先设为可见，让面板参与布局，才能获取正确的目标高度
                stp.Visibility = Visibility.Visible;

                // 强制立即更新布局，确保 ActualHeight 准确
                // 注意：UpdateLayout 可能较耗时，但在简单场景下可用；也可使用 Dispatcher.BeginInvoke 延迟执行动画，给布局更新留出时间
                stp.UpdateLayout();

                // 获取内容实际高度（目标高度）
                double targetHeight = stp.ActualHeight;

                // 准备动画从 0 到目标高度
                DoubleAnimation animation = new()
                {
                    From = 0,
                    To = targetHeight,
                    Duration = TimeSpan.FromSeconds(1),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                stp.BeginAnimation(HeightProperty, animation);
            }
        }


    }
}
