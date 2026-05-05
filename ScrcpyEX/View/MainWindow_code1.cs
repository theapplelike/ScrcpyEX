using Newtonsoft.Json.Linq;
using OpenCvSharp.ML;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyControl;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        /// <summary>
        /// 获得鼠标相对于Scrcpy左上角的坐标，
        /// X轴需要额外减去：左额外距离*DPI比例-左非工作区距离*DPI比例，
        /// Y轴需要额外减去：上额外距离*DPI比例。
        /// </summary>
        /// <returns>鼠标相对于Scrcpy左上角的坐标，以元组表示（X，Y）。</returns>
        public (double, double) GetPointRelativeToScrcpy()
        {
            Point pt1 = PointToScreen(new Point(0, 0)); //获得程序左上角相对于屏幕的位置，获得的值与DPI相关
            Point pt2 = MyGM.GetPosnW();                //获取鼠标相对于屏幕的位置

            double x = pt2.X - pt1.X - ExtraWidthLeft * MV.DpiScaleX + SystemParameters.WindowNonClientFrameThickness.Left * MV.DpiScaleX;
            double y = pt2.Y - pt1.Y - ExtraHeightTop * MV.DpiScaleY;

            //Debug.WriteLine($"程序相对于屏幕：({pt1.X}, {pt1.Y})，{SystemParameters.WindowNonClientFrameThickness.Left}。");
            //Debug.WriteLine($"鼠标相对于屏幕：({pt2.X}, {pt2.Y})。");
            //Debug.WriteLine($"Scrcpy相对于：({x}, {y})。");

            return (x, y);
        }

        /// <summary>
        /// 获得鼠标在Scrcpy中的坐标，计算最大宽度和高度时需要乘以DPI比例。
        /// </summary>
        /// <returns>如果鼠标在Scrcpy中，返回鼠标在Scrcpy中的坐标；否则返回null。</returns>
        public Point? GetPointInScrcpy()
        {
            //判定是否点击在Scrcpy上，如果上层有Popup等窗口遮挡则返回null
            IntPtr hWndUnderMouse = DLL.WindowFromPoint(System.Windows.Forms.Cursor.Position);
            if (hWndUnderMouse != ScHandle)
            {
                //Debug.WriteLine($"点击在非Scrcpy窗口");
                return null;
            }

            (double x, double y) = GetPointRelativeToScrcpy();
            double maxW = ScActualWidth * MV.DpiScaleX;
            double maxH = ScActualHeight * MV.DpiScaleY;
            
            //最后实际保存的坐标要以默认尺寸为准，需要除以当前尺寸和默认尺寸的比例，
            //这样可以让用户在任意尺寸下录制的脚本在任意尺寸下都可以正确播放
            double actuclX = x / ScRatioX;
            double actuclY = y / ScRatioY;
            //Debug.WriteLine($"鼠标在程序中坐标：({x}, {y})；Scrcpy尺寸：({maxW}, {maxH})，和初始尺寸的比例：（{ScRatioX},{ScRatioY}），实际保存点：（{actuclX},{actuclY}）");
            
            if (x > 0 && x < maxW && y > 0 && y < maxH)
            {
                //Console.WriteLine($"YES，鼠标在Scrcpy中。");
                //Console.WriteLine($"////////////////////。\n");
                return new Point(actuclX, actuclY);
            }
            else
            {
                return null;//边缘位置无效，比如0
            }
        }

        /// <summary>
        /// 停止执行脚本的计时器，如果当前正在录制，也会停止录制操作。
        /// </summary>
        private void StopScriptAndRecord()
        {
            if (_countTimer.Enabled)
            {
                _scriptTimer.Stop();
                _countTimer.Stop();
                bdRec.Visibility = Visibility.Collapsed;
                dplTip.Visibility = Visibility.Collapsed;
            }
            if (MouseScript.HookID != IntPtr.Zero)
            {
                StopRecord();
            }
        }

        /// <summary>
        /// 停止执行脚本的计时器，如果当前正在录制，也会停止录制操作，子线程调用此方法。
        /// </summary>
        //private void ThreadStopScriptAndRecord()
        //{
        //    Dispatcher.BeginInvoke(() =>
        //    {
        //        StopScriptAndRecord();
        //    });
        //
        //    Dispatcher.BeginInvoke(StopScriptAndRecord);
        //}

        /// <summary>
        /// 注册鼠标钩子进行录制脚本操作，如果已注册鼠标钩子，则注销鼠标钩子并保存录制的脚本。
        /// </summary>
        private void StartRecord()
        {
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能进行录制。");
                return;
            }
            if (_countTimer.Enabled)
            {
                popMid.Show("正在执行脚本，不能进行录制。");
                return;
            }

            if (MouseScript.HookID == IntPtr.Zero)
            {
                if (MouseScript.RegisterHook())
                {
                    RemoveScBlack();
                    HideKeyMapping();
                    bdRec.Visibility = Visibility.Visible;
                    btnImgCompare.Visibility = Visibility.Collapsed;
                    popMid.Show("开启脚本录制。");
                }
                else
                {
                    popMid.Show("鼠标钩子注册失败，未能启动录制。");
                }
            }
            else
            {
                StopRecord();
            }
        }

        /// <summary>
        /// 停止录制操作。
        /// </summary>
        private void StopRecord()
        {
            MouseScript.CancelHook();
            ShowKeyMapping();
            bdRec.Visibility = Visibility.Hidden;
            popMid.Show("已停止录制操作。");
        }

        /// <summary>
        /// 打开录制管理界面，界面返回值为true时执行脚本。
        /// </summary>
        private void OpenRecordManage()
        {
            RecManage manage = new(ScHandle);
            if (manage.ShowDialog() != true)
            {
                return;
            }
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能执行脚本。");
                return;
            }
            if (MouseScript.HookID != IntPtr.Zero)
            {
                popMid.Show("已开启录制操作，不能执行脚本。");
                return;
            }

            List<ScriptInfo> infos = manage.GetSelectScript();
            
            if (infos[0].IsTime)
            {
                ScriptBefore(infos);
                TimedScript();
            }
            else
            {
                if (infos[0].ImgName != "")
                {
                    ScriptBefore(infos);
                    ImageScript();
                }
                else
                {
                    popMid.Show("未截取图片，不能执行图片脚本。");
                }
            }
            

            //if (manage.CurrScript.IsTime)
            //{
            //    ScriptBefore(manage.CurrScript);
            //    TimedScript();
            //}
            //else
            //{
            //    if (manage.CurrScript.ImgName != "")
            //    {
            //        ScriptBefore(manage.CurrScript);
            //        ImageScript();
            //    }
            //    else
            //    {
            //        popMid.Show("未截取图片，不能执行图片脚本。");
            //    }
            //}
        }

        /// <summary>
        /// 脚本执行前要执行的操作。
        /// </summary>
        /// <param name="recInfo">要执行的脚本信息。</param>
        //private void ScriptBefore(ScriptInfo recInfo)
        //{
        //    //消除黑边，有黑边时会有偏移
        //    RemoveScBlack();
        //
        //    //UI和变量初始化
        //    _vm.Running = 0;
        //    _vm.CycleNum = 0;
        //    _recInfo = recInfo;
        //    _stopwatch = Stopwatch.StartNew();
        //    bdRec.Visibility = Visibility.Visible;
        //    dplTip.Visibility = Visibility.Visible;
        //    btnImgCompare.Visibility = Visibility.Visible;
        //
        //    //如果脚本已执行则关闭
        //    if (_countTimer.Enabled)
        //    {
        //        _scriptTimer.Stop();
        //        _countTimer.Stop();
        //    }
        //
        //    //移除计时脚本和图像脚本，避免重复注册
        //    _scriptTimer.Elapsed -= _timedHandler;
        //    _scriptTimer.Elapsed -= _imageHandler;
        //}

        private void ScriptBefore(List<ScriptInfo> infos)
        {
            //消除黑边，有黑边时会有偏移
            RemoveScBlack();

            //UI和变量初始化
            _vm.Running = 0;
            _vm.CycleNum = 0;
            _scriptInfos = infos;
            _stopwatch = Stopwatch.StartNew();
            bdRec.Visibility = Visibility.Visible;
            dplTip.Visibility = Visibility.Visible;
            btnImgCompare.Visibility = Visibility.Visible;

            //如果脚本已执行则关闭
            if (_countTimer.Enabled)
            {
                _scriptTimer.Stop();
                _countTimer.Stop();
            }

            //移除计时脚本和图像脚本，避免重复注册
            _scriptTimer.Elapsed -= _timedHandler;
            _scriptTimer.Elapsed -= _imageHandler;
        }

        /// <summary>
        /// 注册计时脚本并执行。
        /// </summary>
        //private async void TimedScript()
        //{
        //    _scriptTimer.Elapsed += _timedHandler;
        //    _scriptTimer.Interval = _recInfo.CycleInterval * 1000;
        //    await TimedScript_Tick(_recInfo);//立刻执行一次脚本
        //
        //    _scriptTimer.Start();
        //    _countTimer.Start();
        //}
        private async void TimedScript()
        {
            _scriptTimer.Elapsed += _timedHandler;
            _scriptTimer.Interval = _scriptInfos[0].CycleInterval * 1000;
            _scriptTimer.Start();
            _countTimer.Start();

            await TimedScript_Tick(_scriptInfos);//立刻执行一次脚本
        }

        /// <summary>
        /// 注册图像脚本并执行。
        /// </summary>
        private void ImageScript()
        {
            _si = 0;
            _scriptTimer.Elapsed += _imageHandler;
            _scriptTimer.Interval = 10;//第一次执行的间隔
            _scriptTimer.Start();
            _countTimer.Start();
            //ImageScript_Tick(_recInfo);//立刻执行一次脚本
        }

        /// <summary>
        /// 在窗体中创建一个可以移动的圆形Popup，作为按键映射，此控件宽高与DPI相关。
        /// 有黑边时会导致Popup的位置有偏移。
        /// </summary>
        /// <param name="mapping">按键映射信息类。</param>
        /// <param name="isInitEdit">Popup初始是否为编辑状态。</param>
        /// <param name="isInitVisible">Popup初始是否可见。</param>
        /// <returns>DragPopup对象。</returns>
        private DragPopup CreatePop(KeyMapping mapping, bool isInitEdit, bool isInitVisible = true)
        {
            DragPopup pop = new(mapping, ScRatioX, isInitEdit)
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = gShow,    //这里需要指明父级
                AllowsTransparency = true,
                Height = PopupDiameter,
                Width = PopupDiameter,
                IsOpen = isInitVisible,
                HorizontalOffset = mapping.OffsetX * ScRatioX,
                VerticalOffset = mapping.OffsetY * ScRatioY,
            };

            gShow.Children.Add(pop);//这里的父级要和pop.PlacementTarget属性一致
            //Debug.WriteLine($"映射Popup原始左上:({mapping.OffsetX},{mapping.OffsetY}),比例:({ScRatioX},{ScRatioY}),最终左上:({pop.HorizontalOffset},{pop.VerticalOffset})。");

            return pop;
        }

        /// <summary>
        /// 进行按键映射的编辑，或保存编辑。
        /// </summary>
        /// <param name="desc">说明当前状态的字符串。</param>
        private void MappingEdit(string desc)
        {
            if (IsScClosed)
            {
                popMid.Show("未进行投屏，不能进行映射编辑。");
                return;
            }

            if (desc == "映射编辑")
            {
                foreach (DragPopup drag in MV.DragPopups)
                {
                    drag.SetEditingStatus(true);
                }

                btnMappingEdit.Content = "保存修改";
                btnMappingBack.IsEnabled = true;
                btnMappingAdd.IsEnabled = true;
            }
            else
            {
                MappingSave();
                MappingEditCompleted();
            }
        }

        /// <summary>
        /// 不保存按键映射编辑的内容。
        /// </summary>
        private void MappingCancel()
        {
            CreateSavedPop();
            MappingEditCompleted();
        }

        /// <summary>
        /// 添加一个映射Popup到界面上。
        /// </summary>
        private void MappingAdd()
        {
            MV.DragPopups.Add(CreatePop(new KeyMapping(ScActualWidth, ScActualHeight, PopupDiameter), true));
        }

        /// <summary>
        /// 保存界面上存在的映射Popup。
        /// </summary>
        private void MappingSave()
        {
            MyCfg.Cfg.KeyMappings.Clear();

            foreach (DragPopup drag in MV.DragPopups)
            {
                Keys vkCode = drag.VkCode;
                string ch = drag.Char;
                double offsetX = drag.OffsetX / ScRatioX;
                double offsetY = drag.OffsetY / ScRatioY;
                drag.SetEditingStatus(false);

                MyCfg.Cfg.KeyMappings.Add(new KeyMapping(ch, vkCode, offsetX, offsetY));
                Debug.WriteLine($"映射字符：{ch}，虚拟键：{vkCode}/{(int)vkCode}，保存偏移:（{offsetX:F2}，{offsetY:F2}）。");
            }

            Debug.WriteLine($"已有映射数量：{MyCfg.Cfg.KeyMappings.Count}。");
        }

        /// <summary>
        /// 映射按键编辑完后恢复UI状态。
        /// </summary>
        private void MappingEditCompleted()
        {
            btnMappingEdit.Content = "映射编辑";
            btnMappingBack.IsEnabled = false;
            btnMappingAdd.IsEnabled = false;
        }

        /// <summary>
        /// 创建配置中所有的键盘映射Popup，并保存到列表，保存前会清理列表。
        /// </summary>
        /// <param name="isInitEdit">Popup初始是否为编辑状态。</param>
        /// <param name="isInitVisible">Popup初始是否可见。</param>
        private void CreateSavedPop(bool isInitEdit = false, bool isInitVisible = true)
        {
            foreach (DragPopup drag in MV.DragPopups)
            {
                drag.Dispose();
                drag.IsOpen = false;
            }
            MV.DragPopups.Clear();

            foreach (KeyMapping mapping in MyCfg.Cfg.KeyMappings)
            {
                MV.DragPopups.Add(CreatePop(mapping, isInitEdit, isInitVisible));
            }
        }

        /// <summary>
        /// 隐藏所有的按键映射。
        /// </summary>
        private void HideKeyMapping()
        {
            foreach (DragPopup drag in MV.DragPopups)
            {
                drag.IsOpen = false;
            }
        }

        /// <summary>
        /// 显示所有的按键映射。
        /// </summary>
        private void ShowKeyMapping()
        {
            foreach (DragPopup drag in MV.DragPopups)
            {
                drag.IsOpen = true;
            }
        }

        /// <summary>
        /// 键盘输入映射为点击操作。
        /// </summary>
        /// <param name="vkCode">捕获的虚拟键码。</param>
        public void KeyboardMapping(Keys vkCode)
        {
            //Debug.WriteLine($"窗口是否活动：{IsActive}，是否有焦点：{IsFocused}");
            //Debug.WriteLine($"主句柄：{DLL.GetForegroundWindow() == _mainHandle}，子句柄：{DLL.GetForegroundWindow() == ScHandle}");
            if (IsActive && ScHandle != IntPtr.Zero)
            {
                //if (key =="Q")//按下Q使用鼠标控制方向
                //{
                //    PressMouse();
                //}
                
                DoKeyMapping(vkCode, ScRatioX, ScHandle);
            }
        }

        /// <summary>
        /// 使用按键操作方向不够顺滑，所以让鼠标持续性按下来操作方向，但是使用鼠标操作方向时，
        /// 如果按下键盘会中断鼠标按下的状态，目前没有更好的办法实现模拟器一样的操控。
        /// </summary>
        /// <param name="isDown">当前按键是否为按下状态。</param>
        //private void PressMouse(bool isDown = true)
        //{
        //    if (isDown)
        //    {
        //        //if (_isPressMouse)
        //        //{
        //        //    _isPressMouse = false;
        //        //    //DLL.mouse_event(0x04, 0, 0, 0, 0);//鼠标抬起
        //        //    //DLL.PostMessage(_scHandle, WM.WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        //        //}
        //        //else
        //        //{
        //        //    _isPressMouse = true;
        //        //    //DLL.mouse_event(0x02, 0, 0, 0, 0);//鼠标按下
        //        //    //DLL.PostMessage(_scHandle, WM.WM_LBUTTONDOWN, (IntPtr)WM.MK_LBUTTON, IntPtr.Zero);
        //        //}
        //        DLL.mouse_event(0x02, 0, 0, 0, 0);//鼠标按下
        //    }
        //    //Console.WriteLine($"开启鼠标移动,{_isPressMouse}。");
        //}

        /// <summary>
        /// 如果键入的字符在映射列表中，则映射为单击操作。
        /// </summary>
        /// <param name="vkCode">捕获的虚拟键码。</param>
        /// <param name="ratio">Scrcpy当前宽度和默认宽度的比值。</param>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        private void DoKeyMapping(Keys vkCode, double ratio, IntPtr scHandle)
        {
            List<KeyMapping> mappings = MyCfg.Cfg.KeyMappings.Where(a => a.VkCode == vkCode).ToList();

            if (mappings.Count <= 0)
            {
                return;
            }
            else
            {
                if (popSize.IsOpen) { popSize.IsOpen = false; }
                if (popFunc.IsOpen) { popFunc.IsOpen = false; }
            }

            foreach (KeyMapping m in mappings)
            {
                //根据现在的比例计算半径
                double radius = PopupDiameter * MV.DpiScaleX / 2;
                
                //获得圆心坐标
                int centreX = (int)(m.OffsetX * ratio * MV.DpiScaleX + radius);
                int centreY = (int)(m.OffsetY * ratio * MV.DpiScaleY + radius);

                IntPtr lp = new((centreY << 16) + centreX);
                DLL.PostMessage(scHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, lp);
                DLL.PostMessage(scHandle, WM.WM_LBUTTONDOWN, (IntPtr)MK.LBUTTON, lp);
                DLL.PostMessage(scHandle, WM.WM_LBUTTONUP, IntPtr.Zero, lp);
                
                //如果标志位为true，则让鼠标左键处于按下状态
                //if (_isPressMouse)
                //{
                //    DLL.mouse_event(0x02, 0, 0, 0, 0);//按下
                //    //Console.WriteLine("继续让鼠标左键处于按下状态。");
                //}

                //ADB_Click(centreX, centreY);
                //Console.WriteLine($"圆心点:({centreX},{centreY})。");
            }

            //逻辑鼠标回到物理鼠标的位置
            (double originX, double originY) = GetPointRelativeToScrcpy();
            IntPtr lp1 = new(((int)originY << 16) + (int)originX);
            DLL.PostMessage(scHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, lp1);
        }

        /// <summary>
        /// Scrcpy窗口获得焦点，Scrcpy有焦点时才能触发安卓的输入法。
        /// </summary>
        private void ScGotFocus(object parameter)
        {
            IntPtr intPtr = DLL.SetFocus(ScHandle);
            //bool a = DLL.SetForegroundWindow(ScHandle);
            Debug.WriteLine($"让Scrcpy获得焦点，{intPtr}");
        }


    }
}
