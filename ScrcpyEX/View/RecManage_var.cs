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
        private Int32Rect _cropRect;

        /// <summary>
        /// 存储当前编辑脚本的UI对象的引用。
        /// </summary>
        private Grid _g = null;

        /// <summary>
        /// 临时存储正在编辑的脚本信息。
        /// </summary>
        public RecInfo TempRecInfo { get; set; } = null;


    }
}
