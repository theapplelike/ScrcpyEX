using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ScrcpyEX.View
{
    public partial class MainWindow//var
    {
        /// <summary>
        /// 对第三方程序的引用。
        /// </summary>
        private readonly Process _prc = new();

        /// <summary>
        /// 窗体对应的ViewModel。
        /// </summary>
        private readonly MainWindowVM _vm;

        /// <summary>
        /// scrcpy的句柄，为-1时代表scrcpy没有启动。
        /// </summary>
        private IntPtr _scHandle = IntPtr.Zero;

        /// <summary>
        /// scrcpy窗口的名称。
        /// </summary>
        private string _scName;

        /// <summary>
        /// 判断顶级窗体是否在调整大小。
        /// </summary>
        //private bool _isResize = false;

        /// <summary>
        /// 调整scrcpy窗口大小的计时器。
        /// </summary>
        private readonly DispatcherTimer _resizeTimer = new();

        /// <summary>
        /// 窗口除开scrcpy的额外高度，不能在构造函数中，控件没有显示时其ActualWidth始终为0。
        /// </summary>
        private double ExtraWidth => wfh.Margin.Left + wfh.Margin.Right;

        /// <summary>
        /// 窗口除开scrcpy的额外高度，不能在构造函数中，控件没有显示时其ActualHeight始终为0。
        /// </summary>
        private double ExtraHeight => dpl.ActualHeight + wfh.Margin.Top + wfh.Margin.Bottom;

        


    }
}
