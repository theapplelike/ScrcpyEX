using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.View
{
    public partial class SelectDevice//var
    {
        /// <summary>
        /// 窗体对应的ViewModel。
        /// </summary>
        private readonly SelectDeviceVM _vm;

        /// <summary>
        /// 设备列表，可能是设备的序列号，也可能是设备的IP。
        /// </summary>
        private readonly List<string> _devices;

        /// <summary>
        /// 是否有线连接。
        /// </summary>
        private readonly bool _isWired;

        /// <summary>
        /// 选中的设备的索引。
        /// </summary>
        public int Index {  get; set; }


    }
}
