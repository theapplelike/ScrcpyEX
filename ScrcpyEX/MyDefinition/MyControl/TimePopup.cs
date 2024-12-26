using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// 定时关闭Popup控件，添加了Show()方法，可以指定Popup显示的时间。
    /// </summary>
    internal class TimePopup : Popup
    {
        /// <summary>
        /// 指定时间让Popup后消失。
        /// </summary>
        private readonly DispatcherTimer popTimer = new();

        public TimePopup()
        {
            popTimer.Tick += new EventHandler(Pop_Tick);
        }

        /// <summary>
        /// 提示控件显示指定时间后消失。
        /// </summary>
        private void Pop_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("**************");
            IsOpen = false;
            popTimer.Stop();
            Console.WriteLine("隐藏pop。");
        }

        /// <summary>
        /// 显示Popup提示，重复执行时可以显示到最新位置。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位s。</param>
        public void Show(string text, int time = 3)
        {
            if (popTimer.IsEnabled)
            {
                popTimer.Stop();
            }

            if (Child is TextBlock tbk)
            {
                tbk.Text = text;
            }
            else
            {
                MyGM.FindVisualChild<TextBlock>(Child).Text = text;
            }

            IsOpen = false;
            IsOpen = true;
            popTimer.Interval = TimeSpan.FromSeconds(time);
            popTimer.Start();
        }


    }
}
