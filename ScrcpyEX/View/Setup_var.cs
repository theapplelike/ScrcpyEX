using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.View
{
    public partial class Setup//var
    {
        /// <summary>
        /// 窗体对应的ViewModel。
        /// </summary>
        private readonly SetupVM _vm;

        /// <summary>
        /// 原来的IsCanResize属性。
        /// </summary>
        private bool _oldIsCanResize = false;

        /// <summary>
        /// 记录每组菜单是否展开的数组。
        /// </summary>
        private bool[] _isExpands = [false, false];
    }
}
