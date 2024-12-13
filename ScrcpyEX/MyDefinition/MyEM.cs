using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义扩展方法（Extension Method）类。
    /// </summary>
    internal static class MyEM
    {
        /// <summary>
        /// Popup控件的扩展方法，显示带有文本的Popup，并在指定时间后关闭，当前Popup未显示完前不能显示下一个。
        /// </summary>
        /// <param name="pop">Popup控件。</param>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位ms。</param>
        public static async Task SHW(this Popup pop, string text, int time = 3000)
        {
            if (pop.IsOpen)
            {
                return;
            }

            MyGM.FindVisualChild<TextBlock>(pop.Child).Text = text;
            pop.IsOpen = true;
            
            await Task.Delay(time);
            pop.IsOpen = false;
        }

    }
}
