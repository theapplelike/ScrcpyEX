using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// Toast消息框，最多堆叠三条。
    /// </summary>
    internal class ToastPopup : Popup
    {
        /// <summary>
        /// 最多同时显示的提示数量。
        /// </summary>
        private const int MaxCount = 3;

        /// <summary>
        /// 承载所有提示的容器。
        /// </summary>
        private readonly StackPanel _stpPanel = new() { Orientation = Orientation.Vertical };

        /// <summary>
        /// 当前正在显示的提示，按显示顺序排列（[0] 是最早的那条）。
        /// </summary>
        private readonly List<ToastItem> _items = [];

        /// <summary>
        /// 从 XAML 里定义的 Child 保存下来的模板，用于克隆出每一条提示。
        /// </summary>
        private UIElement _template;

        public ToastPopup()
        {
            StaysOpen = true;
            AllowsTransparency = true;
            Loaded += Pop_Loaded;

            _stpPanel.SizeChanged += Panel_SizeChanged;
        }

        /// <summary>
        /// 原始的Child保存为模板。
        /// </summary>
        private void Pop_Loaded(object sender, EventArgs e)
        {
            if (Child is UIElement template)
            {
                _template = template;
            }
            Child = _stpPanel;
        }

        /// <summary>
        /// 内容高度变化时重新计算位置，保证第一条提示的位置固定不动。
        /// </summary>
        private void Panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlacement();
        }

        /// <summary>
        /// 让提示区域的顶部固定在 PlacementTarget 下方，这样新增提示时不会整体上移。
        /// </summary>
        private void UpdatePlacement()
        {
            if (Placement == PlacementMode.Center && PlacementTarget is FrameworkElement fe)
            {
                VerticalOffset = (fe.ActualHeight + _stpPanel.ActualHeight) / 2 + 5;
            }
        }

        /// <summary>
        /// 显示 Popup 提示，重复执行时会叠加显示，超过 3 条时顶掉最早的一条。此方法线程安全。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup 控件显示的时间，单位 s。</param>
        public void Show(string text, int time = 3)
        {
            if (CheckAccess())
            {
                ShowPop(text, time);
            }
            else
            {
                Dispatcher.BeginInvoke(() => ShowPop(text, time));
            }
        }

        /// <summary>
        /// 已经显示满 3 条时，会把最早的那条顶掉。
        /// </summary>
        private void ShowPop(string text, int time)
        {
            while (_items.Count >= MaxCount)
            {
                RemoveItem(_items[0]);
            }

            ToastItem item = CreateItem(text, time);
            _items.Add(item);
            _stpPanel.Children.Add(item.Element);

            if (!IsOpen)
            {
                IsOpen = true;
            }
        }

        /// <summary>
        /// 根据模板克隆出一条新的提示，并启动它的定时器。
        /// </summary>
        private ToastItem CreateItem(string text, int time)
        {
            UIElement element = CreateToastElement(text);
            DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(time) };
            ToastItem item = new(element, timer);

            PlayEnterAnimation(element);
            timer.Tick += (_, _) =>
            {
                RemoveItem(item);
            };
            timer.Start();

            return item;
        }

        /// <summary>
        /// 克隆模板并设置文本。
        /// </summary>
        private UIElement CreateToastElement(string text)
        {
            UIElement element;

            if (_template != null)
            {
                element = (UIElement)XamlReader.Parse(XamlWriter.Save(_template));
            }
            else
            {
                element = CreateDefaultElement();
            }
            element.FindVisualChild<TextBlock>().Text = text;

            return element;
        }

        /// <summary>
        /// 创建默认的提示。
        /// </summary>
        private static UIElement CreateDefaultElement() => new TextBlock
        {
            FontSize = 20,
            Padding = new Thickness(10),
            Foreground = Brushes.Black
        };

        /// <summary>
        /// 移除一条提示。
        /// </summary>
        private void RemoveItem(ToastItem item)
        {
            item.Timer.Stop();
            _items.Remove(item);
            _stpPanel.Children.Remove(item.Element);

            if (_items.Count == 0)
            {
                IsOpen = false;
            }
        }

        /// <summary>
        /// 为新加入的 ToastItem 播放入场动画：从下方滑入 + 淡入。
        /// </summary>
        private static void PlayEnterAnimation(UIElement element)
        {
            DoubleAnimation up = new()
            {
                From = 50,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(240),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            DoubleAnimation fade = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
            };

            element.Opacity = 0;
            element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, up);
            element.BeginAnimation(OpacityProperty, fade);
        }

        /// <summary>
        /// 如果当前 Popup 是显示状态，则立刻隐藏所有提示。
        /// </summary>
        public void Close()
        {
            foreach (ToastItem item in _items.ToArray())
            {
                RemoveItem(item);
            }
        }

        /// <summary>
        /// 一条提示的数据。
        /// </summary>
        private sealed class ToastItem(UIElement element, DispatcherTimer timer)
        {
            /// <summary>
            /// 提示的UI元素。
            /// </summary>
            public UIElement Element { get; } = element;

            /// <summary>
            /// 提示的显示计时器。
            /// </summary>
            public DispatcherTimer Timer { get; } = timer;
        }


    }
}
