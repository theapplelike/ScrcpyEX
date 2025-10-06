using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code
    {
        /// <summary>
        /// 自定义初始化。
        /// </summary>
        private void MyInit()
        {
            MV.MainWin = this;
            MV.Pop = pop;
            _mainHandle = new WindowInteropHelper(this).Handle;
            bdRec.Visibility = Visibility.Hidden;
            dplTip.Visibility = Visibility.Collapsed;

            SetControlEvent();
            SetTimer();
            ReadCfg();
            SetUISize(true);
            InputModeSwitcher.SwitchToEnglishMode(_mainHandle);

            //快捷键
            InputBindings.Add(new KeyBinding(new RelayCommand(ScGotFocus), Key.F1, ModifierKeys.None));
        }

        /// <summary>
        /// 如果不使用上一次的尺寸位置，则设置为默认初始大小，
        /// 宽=scrcpy宽+左右边距，高=scrcpy高+上下边距+标题栏高。
        /// </summary>
        /// <param name="isUseLastSize">是否使用上一次的大小和位置。</param>
        private void SetUISize(bool isUseLastSize = false)
        {
            if (isUseLastSize && MyCfg.Cfg.Size.IsRecord)
            {
                Width = MyCfg.Cfg.Size.Width;
                Height = MyCfg.Cfg.Size.Height;
                Left = MyCfg.Cfg.Size.Left;
                Top = MyCfg.Cfg.Size.Top;
            }
            else
            {
                Width = _scWidth + ExtraWidth;
                Height = _scHeight + ExtraHeight;
                Left = (SystemParameters.WorkArea.Width - ActualWidth) / 2;
                Top = (SystemParameters.WorkArea.Height - ActualHeight) / 2;
            }
            //Console.WriteLine($"窗体尺寸：{Width},{Height}。");
        }

        /// <summary>
        /// 读取配置的值给UI界面。
        /// </summary>
        private void ReadCfg()
        {
            rdoLoc1.IsChecked = MyCfg.Cfg.Locate1;
            rdoLoc2.IsChecked = MyCfg.Cfg.Locate2;
            rdoLoc3.IsChecked = MyCfg.Cfg.Locate3;
            rdoLoc4.IsChecked = MyCfg.Cfg.Locate4;
        }

        /// <summary>
        /// 将UI界面的值保存到配置当中。
        /// </summary>
        private void SaveCfg()
        {
            MyCfg.Cfg.Locate1 = (bool)rdoLoc1.IsChecked;
            MyCfg.Cfg.Locate2 = (bool)rdoLoc2.IsChecked;
            MyCfg.Cfg.Locate3 = (bool)rdoLoc3.IsChecked;
            MyCfg.Cfg.Locate4 = (bool)rdoLoc4.IsChecked;

            //窗体在正常状态下才记录其尺寸位置信息
            if (WindowState == WindowState.Normal)
            {
                MyCfg.Cfg.Size.Width = Width;
                MyCfg.Cfg.Size.Height = Height;
                MyCfg.Cfg.Size.Left = Left;
                MyCfg.Cfg.Size.Top = Top;
                MyCfg.Cfg.Size.IsRecord = true;
            }
            MyCfg.Save();
        }

        /// <summary>
        /// 保存数据、清理使用的系统资源。
        /// </summary>
        private void MyClose()
        {
            SaveCfg();
            CloseOther();
        }

        /// <summary>
        /// 关闭其他一些程序使用的资源。
        /// </summary>
        private void CloseOther()
        {
            if (MouseScript.HookID != IntPtr.Zero)
            {
                MouseScript.CancelHook();
            }
        }

        /// <summary>
        /// 启动Scrcpy。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async void StartScrcpy(bool isWired)
        {
            if (IsClosed)
            {
                btnWired.IsEnabled = btnWirel.IsEnabled = false;
                CheckScrcpy();
                await ScrcpyInit(isWired);
                btnWired.IsEnabled = btnWirel.IsEnabled = true;
            }
        }

        /// <summary>
        /// 检查scrcpy是否已经运行，如果运行则停止。
        /// </summary>
        private void CheckScrcpy()
        {
            // 查询指定名称的进程（无需包含.exe扩展名）
            Process[] processes = Process.GetProcessesByName("scrcpy");

            if (processes.Length > 0)
            {
                //Console.WriteLine($"进程ID: {processes[0].Id}，路径：{processes[0].MainModule?.FileName}");
                processes[0].Kill();
            }
        }

        /// <summary>
        /// Scrcpy启动完成后，初始化Scrcpy相关的所有功能。
        /// </summary>
        /// <param name="isWired">是否使用有线形式启动。</param>
        private async Task ScrcpyInit(bool isWired)
        {
            _scrcpyHost = new ScrcpyHost();
            bdShow.Child = await _scrcpyHost.StartScrcpyAsync(isWired);

            CreateSavedPop();
            Activate();
            KeyboardHook.RegisterHook();
        }

        /// <summary>
        /// 最大化或者还原窗口。
        /// </summary>
        private void Maximize()
        {
            double wd = SystemParameters.WorkArea.Width;
            double ht = SystemParameters.WorkArea.Height;

            if (Width >= wd && Height >= ht)
            {
                LoadTempSize(_tempSize);
                btnMaximize.Content = "\ue922";
            }
            else
            {
                SaveTempSize();
                Width = wd; 
                Height = ht;
                Left = Top = 0;
                btnMaximize.Content = "\ue923";
            }
        }

        /// <summary>
        /// 保存当前窗体的尺寸和位置到临时变量中。
        /// </summary>
        private void SaveTempSize() => _tempSize = new(Width, Height, Left, Top);

        /// <summary>
        /// 应用临时变量中的窗体尺寸和位置。
        /// </summary>
        private void LoadTempSize(FormSize size)
        {
            if (size == null)
            {
                SetUISize();
                Locate();
                return;
            }

            Width = size.Width;
            Height = size.Height;
            Left = size.Left;
            Top = size.Top;
        }

        /// <summary>
        /// 将窗体定位到指定位置，当把Left设置为0时，左侧可能不会完全贴合，会留下一点空白，
        /// 这是窗口边框，其大小取决于当前的Windows主题，左侧要贴合要减去非工作区的左侧宽度。
        /// </summary>
        private void Locate()
        {
            if ((bool)rdoLoc1.IsChecked)
            {
                //Left = 0 - SystemParameters.WindowNonClientFrameThickness.Left;
                Left = 0;
                Top = 0;
            }
            else if ((bool)rdoLoc2.IsChecked)
            {
                Left = SystemParameters.WorkArea.Width - Width - 3;//完全贴合右边会导致截图脚本有偏移
                Top = 0;
            }
            else if ((bool)rdoLoc3.IsChecked)
            {
                Left = 0 - SystemParameters.WindowNonClientFrameThickness.Left;
                Top = SystemParameters.WorkArea.Height - Height;
            }
            else if ((bool)rdoLoc4.IsChecked)
            {
                Left = SystemParameters.WorkArea.Width - Width;
                Top = SystemParameters.WorkArea.Height - Height;
            }
        }

        /// <summary>
        /// 窗体宽度修改为屏幕的四分之一并移动定位，高度根据15:7调整。
        /// </summary>
        private void QuarterMove()
        {
            Width = SystemParameters.WorkArea.Width / 2;
            Height = (Width - ExtraWidth) * 1.0 / 15 * 7 + ExtraHeight;
            Locate();
            Console.WriteLine($"宽高：{Width},{Height}。");
        }

        /// <summary>
        /// Process.Kill()执行后，会调用Exited事件，前提是EnableRaisingEvents要设置为true。
        /// </summary>
        private void CloseScrcpy()
        {
            if (!IsClosed)
            {
                _scrcpyHost.DisposeScrcpy();
                bdShow.Child = null;
                StopScriptAndRecord();
                Console.WriteLine($"手动结束Scrcpy程序。");
            }
        }

        /// <summary>
        /// 消除投屏的黑边。
        /// </summary>
        private void RemoveBlack()
        {
            if (IsClosed)
            {
                popMid.Show("未进行投屏，不能进行调整。");
                return;
            }

            int scWidth = (int)ScActualWidth;
            int scHeight = (int)ScActualHeight;
            int max = GetGreatestCommonDivisor(scWidth, scHeight);

            int xProportion = scWidth / max;
            int yProportion = scHeight / max;
            Console.WriteLine($"消除黑边,当前Scrcpy的比例：({ScActualWidth / max}，{ScActualHeight / max})，期望比例:({_horizontalRatio},{_verticalRatio})。");
            Console.WriteLine($"当前Scrcpy的宽高：({ScActualWidth},{ScActualHeight})，\n和原始大小的比例：{ScRatioX}，{ScRatioY}");

            //计算通分后的分子，如果标准分子大，应以宽为准，调整高度；反之调整宽度
            int currentNumerator = xProportion * _verticalRatio;
            int standardNumerator = yProportion * _horizontalRatio;
            if (currentNumerator > standardNumerator)
            {
                Width = ScActualHeight / _verticalRatio * _horizontalRatio + ExtraWidth;
            }
            else if (currentNumerator < standardNumerator)
            {
                Height = ScActualWidth / _horizontalRatio * _verticalRatio + ExtraHeight;
            }
        }

        /// <summary>
        /// 缩小指定步长的尺寸，以15:7为标准。
        /// </summary>
        /// <param name="step">步长，默认为8。</param>
        private void ReduceSize(int step = 8)
        {
            if (IsClosed)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }
            if (ScRatioX <= 0.25 || ScRatioY <= 0.25)
            {
                popMid.Show("不能再继续缩小。");
                return;
            }

            Width -= _horizontalRatio * step;
            Height -= _verticalRatio * step;
            Locate();
            RemoveBlack();
        }

        /// <summary>
        /// 放大指定步长的尺寸，以15:7为标准。
        /// </summary>
        /// <param name="step">步长，默认为8。</param>
        private void AmplifySize(int step = 8)
        {
            if (IsClosed)
            {
                popMid.Show("未进行投屏，不能缩放尺寸。");
                return;
            }
            if (ScRatioX >= 1.5)
            {
                popMid.Show("不能再继续放大。");
                return;
            }
            else if (Width + _horizontalRatio * step > SystemParameters.WorkArea.Width)
            {
                Width = SystemParameters.WorkArea.Width;
            }
            else
            {
                Width += _horizontalRatio * step;
            }

            if (ScRatioY >= 1.5)
            {
                //Y轴不能再放大
            }
            else if (Height + _verticalRatio * step > SystemParameters.WorkArea.Height)
            {
                Height = SystemParameters.WorkArea.Height;
            }
            else
            {
                Height += _verticalRatio * step;
            }

            Locate();
            RemoveBlack();
        }

        /// <summary>
        /// 从最小化恢复时可能导致Scrcpy窗口向左偏移，所以恢复时刷新Scrcpy窗口。
        /// </summary>
        private void RefreshScrcpy()
        {
            switch (WindowState)
            {
                case WindowState.Minimized:
                    //Debug.WriteLine("窗口已最小化");
                    break;
                case WindowState.Normal:
                    //Debug.WriteLine("窗口已恢复正常状态");
                    _scrcpyHost.RefreshScrcpy();
                    break;
                case WindowState.Maximized:
                    //Debug.WriteLine("窗口已最大化");
                    break;
            }
        }

    }
}
