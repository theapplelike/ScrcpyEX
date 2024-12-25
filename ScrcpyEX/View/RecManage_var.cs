using ScrcpyEX.Model;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private bool _isEditing = false;

        /// <summary>
        /// 临时存储正在编辑的脚本信息。
        /// </summary>
        public RecInfo TempRecInfo { get; set; } = null;


    }
}
