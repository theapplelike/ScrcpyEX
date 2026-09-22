using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScrcpyEX.View
{
    public partial class MainWindow//var
    {
        /// <summary>
        /// Scrcpy主机，封装Scrcpy启动、关闭、大小调整的功能。
        /// </summary>
        private ScrcpyHost _scHost = null;

        /// <summary>
        /// 窗体对应的ViewModel。
        /// </summary>
        private readonly MainWindowVM _vm;

        /// <summary>
        /// 主程序的句柄。
        /// </summary>
        private IntPtr _mainHandle;

        /// <summary>
        /// 句柄源，用于注册消息钩子。
        /// </summary>
        private HwndSource _hwndSource;

        /// <summary>
        /// 保存窗体的临时尺寸、位置。
        /// </summary>
        private FormSize _tempSize = null;

        /// <summary>
        /// 准确的测量运行时间。
        /// </summary>
        private Stopwatch _stopwatch = new();

        /// <summary>
        /// 调整窗口大小改变时触发的计时器。
        /// </summary>
        private readonly DispatcherTimer _resizeTimer = new();

        /// <summary>
        /// 窗口位置改变时触发的计时器。
        /// </summary>
        private readonly DispatcherTimer _locationChangeTimer = new();

        /// <summary>
        /// 图像脚本任务锁。
        /// </summary>
        private readonly SemaphoreSlim _imageScriptLock = new(1, 1);

        /// <summary>
        /// 计时脚本对应的委托。
        /// </summary>
        private ElapsedEventHandler _timedHandler;

        /// <summary>
        /// 图像脚本对应的委托。
        /// </summary>
        private ElapsedEventHandler _imageHandler;

        /// <summary>
        /// 脚本计时器，可能会绑定计时脚本或图像脚本。
        /// </summary>
        private readonly System.Timers.Timer _scriptTimer = new();

        /// <summary>
        /// 对每次脚本间隔进行计数。
        /// </summary>
        private readonly System.Timers.Timer _countTimer = new();

        /// <summary>
        /// 测试用计时器。
        /// </summary>
        private readonly System.Timers.Timer _testTimer = new();

        /// <summary>
        /// Scrcpy的默认初始宽高（在100%缩放下），单位是逻辑像素，使用比例为15：7。
        /// </summary>
        private readonly int _scWidth = 960, _scHeight = 448;
        //private readonly int _scWidth = 1156, _scHeight = 540;

        /// <summary>
        /// SC视频帧的宽度（像素）。
        /// </summary>
        private int _ScFrameWd;

        /// <summary>
        /// SC视频帧的高度（像素）。
        /// </summary>
        private int _ScFrameHt;

        /// <summary>
        /// SC视频帧的宽高比。
        /// </summary>
        private double _ScAspectRatio = 0;

        /// <summary>
        /// 保存已选中的原始脚本。
        /// </summary>
        private ScriptItem _scriptItem = null;

        /// <summary>
        /// 保存已选中的脚本列表。
        /// </summary>
        private List<ScriptInfo> _scriptInfos = null;

        /// <summary>
        /// ScriptInfo，执行图像组脚本时顺序模式下，指向组列表下标的变量。
        /// </summary>
        private int _siOrder = 0;

        /// <summary>
        /// ScriptInfo，执行图像组脚本时成功继续模式下，指向组列表下标的变量。
        /// </summary>
        private int _siSuccess = 0;

        /// <summary>
        /// 现在是否可以调整映射Popup。
        /// </summary>
        private bool _isAdjustPop = true;

        /// <summary>
        /// 保存录制脚本时截取的临时图像。
        /// </summary>
        private CroppedBitmap _cropImg = null;

        /// <summary>
        /// 说明截图结果位置大小的矩形。
        /// </summary>
        private System.Windows.Rect _cropRect = new();

        /// <summary>
        /// 窗口除开SC的额外宽度，单位为逻辑像素，与屏幕DPI无关。不能在构造函数中调用，控件没有显示时其ActualWidth始终为0。
        /// </summary>
        private double ExtraWidth
        {
            get
            {
                if (gShow.CheckAccess()) //检查是否在UI线程
                {
                    return gShow.Margin.Left + gShow.Margin.Right;
                }
                else
                {
                    return Dispatcher.Invoke(() => gShow.Margin.Left + gShow.Margin.Right);
                }
            }
        }

        /// <summary>
        /// 窗口除开SC的额外高度，单位为逻辑像素，与屏幕DPI无关。不能在构造函数中调用，控件没有显示时其ActualHeight始终为0。
        /// </summary>
        private double ExtraHeight
        {
            get
            {
                if (gShow.CheckAccess()) //检查是否在UI线程
                {
                    return gMenu.ActualHeight + gShow.Margin.Top + gShow.Margin.Bottom;
                }
                else
                {
                    return Dispatcher.Invoke(() => gMenu.ActualHeight + gShow.Margin.Top + gShow.Margin.Bottom);
                }
            }
        }

        /// <summary>
        /// 屏幕X轴上的DPI比例。
        /// </summary>
        //public double DpiScaleX => VisualTreeHelper.GetDpi(this).DpiScaleX;

        /// <summary>
        /// 屏幕Y轴上的DPI比例。
        /// </summary>
        //public double DpiScaleY => VisualTreeHelper.GetDpi(this).DpiScaleY;

        /// <summary>
        /// Scrcpy是否处于关闭状态，true--关闭状态；false--启动状态。
        /// </summary>
        private bool IsScClosed => _scHost == null || _scHost.ScHandle == IntPtr.Zero;

        /// <summary>
        /// 圆形DragPopup的宽度。
        /// </summary>
        private double PopWd => 50 * ScRatioX;

        /// <summary>
        /// 圆形DragPopup的高度。
        /// </summary>
        private double PopHt => 50 * ScRatioY;

        /// <summary>
        /// Scrcpy的句柄，为IntPtr.Zero时代表Scrcpy没有启动。
        /// </summary>
        public IntPtr ScHandle => _scHost?.ScHandle ?? IntPtr.Zero;

        /// <summary>
        /// Scrcpy当前宽度和默认宽度的比值。
        /// </summary>
        public double ScRatioX => ScActualWidth / _scWidth;

        /// <summary>
        /// Scrcpy当前高度和默认高度的比值。
        /// </summary>
        public double ScRatioY => ScActualHeight / _scHeight;

        /// <summary>
        /// Scrcpy当前的实际宽度，单位是逻辑像素，与DPI无关。
        /// </summary>
        public double ScActualWidth => ActualWidth - ExtraWidth;

        /// <summary>
        /// Scrcpy当前的实际高度，单位是逻辑像素，与DPI无关。
        /// </summary>
        public double ScActualHeight => ActualHeight - ExtraHeight;


    }
}
