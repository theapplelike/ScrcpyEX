using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using ScrcpyEX.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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
            Directory.CreateDirectory(ScriptPath);
        }

        /// <summary>
        /// Scrcpy的版本号。
        /// </summary>
        public static string ScVersion = "v4.0";

        /// <summary>
        /// Scrcpy的路径。
        /// </summary>
        public static string ScPath { get; set; } = @$".\scrcpy-win64-{ScVersion}\scrcpy.exe";

        /// <summary>
        /// adb的路径。
        /// </summary>
        public static string ADB_Path { get; set; } = @$".\scrcpy-win64-{ScVersion}\adb.exe";

        /// <summary>
        /// 脚本图片的保存路径。
        /// </summary>
        public static string ScriptPath { get; set; } = @".\main\Local\Script";

        /// <summary>
        /// 主窗体的引用。
        /// </summary>
        public static MainWindow MainWin { get; set; }

        /// <summary>
        /// 按键映射Popup列表。
        /// </summary>
        public static List<DragPopup> DragPopups { get; set; } = [];

        /// <summary>
        /// 屏幕X轴上的DPI比例。
        /// </summary>
        public static double DpiScaleX { get; set; }

        /// <summary>
        /// 屏幕Y轴上的DPI比例。
        /// </summary>
        public static double DpiScaleY { get; set; }


    }
}
