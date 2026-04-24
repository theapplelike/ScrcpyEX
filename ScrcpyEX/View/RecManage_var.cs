using ScrcpyEX.Model;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
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
        /// 是否处于脚本名称的编辑状态。
        /// </summary>
        //private bool _isEditing = false;

        /// <summary>
        /// 说明截图结果位置大小的矩形。
        /// </summary>
        private Rect _cropRect;

        /// <summary>
        /// 存储当前编辑脚本的UI对象的引用。
        /// </summary>
        private Grid _g = null;

        /// <summary>
        /// 保存当前正在编辑名称的脚本。
        /// </summary>
        private RecInfo _lastItem = null;

        /// <summary>
        /// Scrcpy的句柄。
        /// </summary>
        private IntPtr _scHandle = IntPtr.Zero;

        /// <summary>
        /// 当前选中的脚本信息。
        /// </summary>
        public RecInfo CurrRecInfo { get; set; } = null;


    }
}
