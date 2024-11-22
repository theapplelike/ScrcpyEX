using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using ScrcpyEX.ExtensionMethod;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义通用方法（Generic Method）类。
    /// </summary>
    internal class MyGM
    {
        /// <summary>
        /// 在鼠标位置显示提示。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        public static void POP(string text)
        {
            (MV.Pop.Child as TextBlock).Text = text;

            MV.Pop.IsOpen = false;
            MV.Pop.IsOpen = true;
            MV.PopTimer.Start();
        }

    }
}
