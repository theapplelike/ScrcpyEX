using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
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
        /// scrcpy的句柄，为IntPtr.Zero时代表scrcpy没有启动。
        /// </summary>
        private IntPtr _scHandle = IntPtr.Zero;

        /// <summary>
        /// scrcpy窗口的名称。
        /// </summary>
        private string _scName;

        /// <summary>
        /// 调整scrcpy窗口大小的计时器。
        /// </summary>
        private readonly DispatcherTimer _resizeTimer = new();

        private readonly DispatcherTimer _clickTimer = new();

        private readonly DispatcherTimer _testTimer = new();

        /// <summary>
        /// scrcpy的默认宽高（在100%缩放下），使用比例为15：7。
        /// </summary>
        private readonly int _scWidth = 960, _scHeight = 448;

        /// <summary>
        /// 是否处于录制操作下。
        /// </summary>
        private bool _isRecord = false;

        /// <summary>
        /// 窗口除开scrcpy的额外宽度，不能在构造函数中调用，控件没有显示时其ActualWidth始终为0。
        /// </summary>
        private double ExtraWidth => wfh.Margin.Left + wfh.Margin.Right;

        /// <summary>
        /// 窗口除开scrcpy的额外高度，不能在构造函数中调用，控件没有显示时其ActualHeight始终为0。
        /// </summary>
        private double ExtraHeight => dpl.ActualHeight + wfh.Margin.Top + wfh.Margin.Bottom;

        /// <summary>
        /// 窗口除开scrcpy的额外左侧宽度，不能在构造函数中调用，控件没有显示时其ActualWidth始终为0。
        /// </summary>
        private double ExtraWidthLeft => wfh.Margin.Left;

        /// <summary>
        /// 窗口除开scrcpy的额外上方高度，不能在构造函数中调用，控件没有显示时其ActualHeight始终为0。
        /// </summary>
        private double ExtraHeightTop => dpl.ActualHeight + wfh.Margin.Top;

        /// <summary>
        /// X轴上的DPI比例。
        /// </summary>
        private double ScaleX => VisualTreeHelper.GetDpi(this).DpiScaleX;

        /// <summary>
        /// Y轴上的DPI比例。
        /// </summary>
        private double ScaleY => VisualTreeHelper.GetDpi(this).DpiScaleY;


    }
}
