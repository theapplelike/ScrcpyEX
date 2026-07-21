using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScrcpyEX.View
{
    public partial class DataSync//var
    {
        /// <summary>
        /// 所有文件是否传输完毕。
        /// </summary>
        private bool _isXferComplete = false;

        /// <summary>
        /// 是否正在传输文件。
        /// </summary>
        private bool _isXfering = false;

        /// <summary>
        /// 文件传输取消令牌。
        /// </summary>
        private CancellationTokenSource _cts = null;


    }
}
