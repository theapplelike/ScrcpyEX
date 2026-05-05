using ScrcpyEX.Model;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ScrcpyEX.View
{
    public partial class RecManage//var
    {
        /// <summary>
        /// 窗体对应的ViewModel。
        /// </summary>
        private readonly RecManageVM _vm;

        /// <summary>
        /// Scrcpy的句柄。
        /// </summary>
        private readonly IntPtr _scHandle = IntPtr.Zero;

        /// <summary>
        /// 坐标点的DataTable。
        /// </summary>
        private readonly DataTable _pointDt = new();

        /// <summary>
        /// 说明截图结果位置大小的矩形。
        /// </summary>
        private Rect _cropRect = Rect.Empty;

        /// <summary>
        /// 存储当前编辑脚本的UI对象的引用。
        /// </summary>
        private Grid _g = null;

        /// <summary>
        /// 保存当前正在编辑名称的脚本。
        /// </summary>
        private ScriptItem _lastItem = null;

        /// <summary>
        /// 当前选中脚本的索引。
        /// </summary>
        //private int _index = -1;

        /// <summary>
        /// ListBox项拖动起点。
        /// </summary>
        private Point _dragStartPoint;

        /// <summary>
        /// ListBox中按下时的源项。
        /// </summary>
        private ListBoxItem _dragSourceItem;

        /// <summary>
        /// 当前选中的脚本信息。
        /// </summary>
        //public ScriptItem CurrSc { get; set; } = null;

        /// <summary>
        /// 当前选中的脚本项的引用，使用时需要判断是单脚本还是组脚本。
        /// </summary>
        private ScriptItem _currScript = null;


    }
}
