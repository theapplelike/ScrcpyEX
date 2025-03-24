using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using ScrcpyEX.View;
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
        /// <summary>
        /// Scrcpy的版本号。
        /// </summary>
        public static string ScrcpyVersion = "v3.1";

        /// <summary>
        /// Scrcpy的路径。
        /// </summary>
        public static string ScrcpyPath { get; set; } = @".\scrcpy-win64\scrcpy.exe";

        /// <summary>
        /// adb的路径。
        /// </summary>
        public static string ADB_Path { get; set; } = @".\scrcpy-win64\adb.exe";

        /// <summary>
        /// 主窗体的引用。
        /// </summary>
        public static MainWindow MainWin { get; set; }

        /// <summary>
        /// 全局提示控件，在鼠标位置显示信息。
        /// </summary>
        public static TimePopup Pop { get; set; }

        /// <summary>
        /// 编辑键盘映射时使用的临时Popup列表。
        /// </summary>
        public static List<Popup> TempPopups { get; set; } = [];
        

    }
}
