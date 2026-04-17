using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// 可以定时关闭的Popup，添加了Show()方法，可以指定Popup显示的时间。
    /// </summary>
    internal class TimePopup : Popup
    {
        /// <summary>
        /// 指定时间让Popup后消失。
        /// </summary>
        private readonly DispatcherTimer _hideTimer = new();

        public TimePopup()
        {
            StaysOpen = true;
            AllowsTransparency = true;

            _hideTimer.Tick += new EventHandler(Pop_Tick);
        }

        /// <summary>
        /// 提示控件显示指定时间后消失。
        /// </summary>
        private void Pop_Tick(object sender, EventArgs e)
        {
            IsOpen = false;
            _hideTimer.Stop();
        }

        /// <summary>
        /// 显示Popup提示，重复执行时可以显示到最新位置，此方法线程安全。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位s。</param>
        public void Show(string text, int time = 3)
        {
            if (CheckAccess())
            {
                ShowPop(text, time);
            }
            else
            {
                Dispatcher.BeginInvoke(() => { ShowPop(text, time); });
            }
        }

        /// <summary>
        /// 显示Popup提示，重复执行时可以显示到最新位置。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位s。</param>
        private void ShowPop(string text, int time = 3)
        {
            if (_hideTimer.IsEnabled)
            {
                _hideTimer.Stop();
            }

            if (Child is TextBlock tbk)
            {
                tbk.Text = text;
            }
            else
            {
                Child.FindVisualChild<TextBlock>().Text = text;
            }

            IsOpen = false;
            IsOpen = true;
            _hideTimer.Interval = TimeSpan.FromSeconds(time);
            _hideTimer.Start();
        }


    }
}
