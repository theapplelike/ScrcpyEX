using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using ScrcpyEX.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            if (!Directory.Exists(ScriptPath))
            {
                Directory.CreateDirectory(ScriptPath);
            }
        }

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
        /// 脚本图片的保存路径。
        /// </summary>
        public static string ScriptPath { get; set; } = @".\main\Local\Script";

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
        //public static List<Popup> TempPopups { get; set; } = [];

        /// <summary>
        /// 按键映射Popup列表。
        /// </summary>
        public static List<MyDragMove> DragMoves { get; set; } = [];


    }
}
