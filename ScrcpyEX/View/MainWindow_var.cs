using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScrcpyEX.View
{
    public partial class MainWindow//var
    {
        /// <summary>
        /// 对第三方程序的引用。
        /// </summary>
        //private readonly Process _prc = new();

        /// <summary>
        /// Scrcpy主机，需要先启动主机再启动Scrcpy。
        /// </summary>
        private ScrcpyHost _scrcpyHost = null;

        /// <summary>
        /// 窗体对应的ViewModel。
        /// </summary>
        private readonly MainWindowVM _vm;

        /// <summary>
        /// scrcpy的句柄，为IntPtr.Zero时代表scrcpy没有启动。
        /// </summary>
        //private IntPtr _scHandle = IntPtr.Zero;

        /// <summary>
        /// scrcpy窗口的名称。
        /// </summary>
        private string _scName;

        /// <summary>
        /// 主程序的句柄。
        /// </summary>
        private IntPtr _mainHandle;

        /// <summary>
        /// 保存窗体的临时尺寸、位置。
        /// </summary>
        private FormSize _tempSize = null;

        /// <summary>
        /// 用于图像脚本计时器的自适应间隔，最小为5秒。
        /// </summary>
        private int _scriptInterval = 5;

        /// <summary>
        /// 准确的测量运行时间。
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// 计时脚本对应的委托。
        /// </summary>
        private EventHandler _timedHandler;

        /// <summary>
        /// 图像脚本对应的委托。
        /// </summary>
        private EventHandler _imageHandler;

        /// <summary>
        /// 调整scrcpy窗口大小的计时器。
        /// </summary>
        private readonly DispatcherTimer _resizeTimer = new();

        /// <summary>
        /// 脚本计时器，可能会绑定计时脚本或图像脚本。
        /// </summary>
        private readonly DispatcherTimer _scriptTimer = new();

        /// <summary>
        /// 对每次脚本间隔进行计数。
        /// </summary>
        private readonly DispatcherTimer _countTimer = new();

        /// <summary>
        /// 测试用计时器。
        /// </summary>
        private readonly DispatcherTimer _testTimer = new();

        /// <summary>
        /// scrcpy的默认初始宽高（在100%缩放下），使用比例为15：7。
        /// </summary>
        private readonly int _scWidth = 960, _scHeight = 448;

        /// <summary>
        /// 说明当前投屏的状态，true--横屏；false--竖屏。
        /// </summary>
        //private bool _isLandscape = false;

        /// <summary>
        /// 投屏分辨率的横向比例。
        /// </summary>
        private int _horizontalRatio;

        /// <summary>
        /// 投屏分辨率的纵向比例。
        /// </summary>
        private int _verticalRatio;

        /// <summary>
        /// 保存已选中的脚本。
        /// </summary>
        private RecInfo _recInfo = null;

        /// <summary>
        /// 鼠标左键是否处于按下状态。
        /// </summary>
        //private bool _isPressMouse = false;

        /// <summary>
        /// 窗口除开scrcpy的额外宽度，不能在构造函数中调用，控件没有显示时其ActualWidth始终为0。
        /// </summary>
        private double ExtraWidth => gShow.Margin.Left + gShow.Margin.Right;
        //private double ExtraWidth => bdShow.Margin.Left + bdShow.Margin.Right;

        /// <summary>
        /// 窗口除开scrcpy的额外高度，不能在构造函数中调用，控件没有显示时其ActualHeight始终为0。
        /// </summary>
        private double ExtraHeight => dpl.ActualHeight + gShow.Margin.Top + gShow.Margin.Bottom;
        //private double ExtraHeight => dpl.ActualHeight + bdShow.Margin.Top + bdShow.Margin.Bottom;

        /// <summary>
        /// 窗口除开scrcpy的额外左侧宽度，不能在构造函数中调用，控件没有显示时其ActualWidth始终为0。
        /// </summary>
        public double ExtraWidthLeft => gShow.Margin.Left;
        //public double ExtraWidthLeft => bdShow.Margin.Left;

        /// <summary>
        /// 窗口除开scrcpy的额外上方高度，不能在构造函数中调用，控件没有显示时其ActualHeight始终为0。
        /// </summary>
        public double ExtraHeightTop => dpl.ActualHeight + gShow.Margin.Top;
        //public double ExtraHeightTop => dpl.ActualHeight + bdShow.Margin.Top;

        /// <summary>
        /// X轴上的DPI比例。
        /// </summary>
        public double DpiScaleX => VisualTreeHelper.GetDpi(this).DpiScaleX;

        /// <summary>
        /// Y轴上的DPI比例。
        /// </summary>
        public double DpiScaleY => VisualTreeHelper.GetDpi(this).DpiScaleY;

        /// <summary>
        /// Scrcpy当前宽度和默认宽度的比值。
        /// </summary>
        private double ScRatioX => ScActualWidth / _scWidth;

        /// <summary>
        /// Scrcpy当前高度和默认高度的比值。
        /// </summary>
        private double ScRatioY => ScActualHeight / _scHeight;

        /// <summary>
        /// Scrcpy当前的实际宽度。
        /// </summary>
        public double ScActualWidth => ActualWidth - ExtraWidth;

        /// <summary>
        /// Scrcpy当前的实际高度。
        /// </summary>
        public double ScActualHeight => ActualHeight - ExtraHeight;

        /// <summary>
        /// Scrcpy是否处于关闭状态。
        /// </summary>
        private bool IsClosed => _scrcpyHost == null || _scrcpyHost.ScHandle == IntPtr.Zero;

        /// <summary>
        /// Scrcpy的句柄，为IntPtr.Zero时代表Scrcpy没有启动。
        /// </summary>
        private IntPtr ScHandle => _scrcpyHost?.ScHandle ?? IntPtr.Zero;

    }
}
