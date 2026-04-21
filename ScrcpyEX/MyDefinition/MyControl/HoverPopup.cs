using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// 为PlacementTarget元素提供一个悬停显示的菜单。
    /// </summary>
    internal class HoverPopup : Popup
    {
        /// <summary>
        /// 提供最外层边框。
        /// </summary>
        private Border _bdOuter;

        /// <summary>
        /// 内部排列控件。
        /// </summary>
        private StackPanel _stp;

        /// <summary>
        /// 关闭悬停菜单的计时器。
        /// </summary>
        private readonly DispatcherTimer _closeTimer = new();

        /// <summary>
        /// 静态构造函数，用于注册PlacementTarget属性改变时的回调。
        /// </summary>
        static HoverPopup()
        {
            PlacementTargetProperty.OverrideMetadata(typeof(HoverPopup), new FrameworkPropertyMetadata(null, OnPlacementTargetChanged));
        }

        /// <summary>
        /// 在 PlacementTarget 依赖属性的值实际发生变化时触发。
        /// </summary>
        /// <param name="d">发生属性变化的依赖对象实例，即属性的拥有者。</param>
        /// <param name="e">提供属性变化前后的值以及属性本身的信息。</param>
        private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HoverPopup pop = (HoverPopup)d;

            if (e.OldValue is Button oldBtn)
            {
                oldBtn.Margin = new(0, 1, 0, 0);
                oldBtn.MouseEnter -= pop.Btn_MouseEnter;
                oldBtn.MouseLeave -= pop.Btn_MouseLeave;
            }
            if (e.NewValue is Button newBtn)
            {
                newBtn.Margin = new(0);
                newBtn.MouseEnter += pop.Btn_MouseEnter;
                newBtn.MouseLeave += pop.Btn_MouseLeave;
                newBtn.Click -= pop.Btn_Click;
                newBtn.Click += pop.Btn_Click;
            }
        }

        /// <summary>
        /// 一个悬停菜单，显示在PlacementTarget元素下方。
        /// </summary>
        public HoverPopup()
        {
            ControlInit();
            Loaded += HoverPopup_Loaded;
        }

        /// <summary>
        /// 初始化包含的控件。
        /// </summary>
        private void ControlInit()
        {
            //提供最外层边框
            _bdOuter = new()
            {
                Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255)),
            };
            _bdOuter.MouseEnter += (s, e) => _closeTimer.Stop();
            _bdOuter.MouseLeave += (s, e) => _closeTimer.Start();

            //内部排列控件
            _stp = new()
            {
                Margin = new(0, 5, 0, 0),
            };

            _bdOuter.Child = _stp;
            Child = _bdOuter;
            AllowsTransparency = true;
            Placement = PlacementMode.Bottom;
            PopupAnimation = PopupAnimation.Slide;
        }

        /// <summary>
        /// 控件加载完成时发生。
        /// </summary>
        private void HoverPopup_Loaded(object sender, RoutedEventArgs e)
        {
            _closeTimer.Tick += new EventHandler(Close_Tick);
            _closeTimer.Interval = TimeSpan.FromSeconds(0.2);
        }

        /// <summary>
        /// 指定延时后关闭悬停菜单。
        /// </summary>
        private void Close_Tick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            IsOpen = false;
        }

        /// <summary>
        /// 鼠标进入目标按钮时发生。
        /// </summary>
        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            _closeTimer.Stop();
            IsOpen = true;
        }

        /// <summary>
        /// 鼠标离开目标按钮时发生。
        /// </summary>
        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            _closeTimer.Start();
        }

        /// <summary>
        /// 鼠标点击时发生。
        /// </summary>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        /// <summary>
        /// 将按钮添加到悬停菜单。
        /// </summary>
        /// <param name="isTopmost">加入悬停菜单的按钮是否放到最上面。</param>
        public void Add(Button btn, bool isTopmost = false)
        {
            //先从按钮原来的父级移除
            if (btn.Parent is Panel pnl)
            {
                pnl.Children.Remove(btn);
            }

            btn.Margin = new(0, 1, 0, 0);
            btn.Click -= Btn_Click;
            btn.Click += Btn_Click;

            if (isTopmost)
            {
                _stp.Children.Insert(0, btn);
            }
            else
            {
                _stp.Children.Add(btn);
            }
        }

        /// <summary>
        /// 将悬停菜单中第一个按钮移出，并添加到指定布局控件中。
        /// </summary>
        public void MoveOutTo(Panel pnl)
        {
            if (_stp.Children.Count > 0)
            {
                if (_stp.Children[0] is Button btn)
                {
                    _stp.Children.Remove(btn);
                    btn.Margin = new(0);

                    pnl.Children.Add(btn);
                }
            }
        }

        /// <summary>
        /// 悬停菜单中的按钮和PlacementTarget按钮交换位置。
        /// </summary>
        /// <param name="newtBtn">下一个主要按钮。</param>
        public void ExchangeMainBtn(Button newtBtn)
        {
            if (!_stp.Children.Contains(newtBtn))
            {
                return;//要成为主要按钮，必须是悬停菜单中的元素
            }

            if (PlacementTarget is Button oldBtn)//之前的主要按钮
            {
                //两个按钮对调位置
                if (oldBtn.Parent is Panel pnl)
                {
                    _stp.Children.Remove(newtBtn);

                    pnl.Children.Remove(oldBtn);
                    pnl.Children.Add(newtBtn);

                    _stp.Children.Insert(0, oldBtn);
                    PlacementTarget = newtBtn;
                }
            }
        }


    }
}
