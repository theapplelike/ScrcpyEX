using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ScrcpyEX.ExtensionMethod
{
    /// <summary>
    /// 自定义公共变量。
    /// </summary>
    internal static class MV
    {
        static MV()
        {
            PopTimer.Tick += new EventHandler(Pop_Tick);
            PopTimer.Interval = TimeSpan.FromSeconds(3);
        }

        /// <summary>
        /// Scrcpy的路径。
        /// </summary>
        public static string ScrcpyPath { get; set; } = @".\scrcpy-win64-v2.7\scrcpy.exe";

        /// <summary>
        /// 全局提示控件。
        /// </summary>
        public static Popup Pop { get; set; } = new();

        /// <summary>
        /// 全局提示控件的计时器，指定时间后消失。
        /// </summary>
        public static DispatcherTimer PopTimer = new();

        /// <summary>
        /// 提示控件显示指定时间后消失。
        /// </summary>
        private static void Pop_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("**************");
            Pop.IsOpen = false;
            PopTimer.Stop();
            Console.WriteLine("隐藏pop。");
        }
    }
}
