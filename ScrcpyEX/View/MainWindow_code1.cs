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
        /// 获得鼠标相对于SC左上角的坐标，SC未启动时返回的是相对于屏幕左上角的坐标。
        /// </summary>
        /// <returns>鼠标相对于SC左上角的坐标，以元组表示（x，y）。</returns>
        public (double, double) GetPointRelativeToScrcpy()
        {
            DLL.GetCursorPos(out DLL.POINT scPt);
            DLL.ScreenToClient(ScHandle, ref scPt);

            return (scPt.X, scPt.Y);
        }

        /// <summary>
        /// 获得鼠标在SC中的坐标。
        /// </summary>
        /// <returns>返回鼠标在SC中的坐标；如果鼠标不在SC中，或上层有Popup等窗口遮挡则返回null。</returns>
        public Point? GetPointInScrcpy()
        {
            IntPtr hWnd = DLL.WindowFromPoint(MyGM.GetPosnD());
            if (hWnd != ScHandle)
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
            if (MRS_Hook.HookID != IntPtr.Zero)
            {
                StopRecord();
            }
        }

        /// <summary>
        /// 进行截图。
        /// </summary>
        private void DoScreenshot()
        {
            Screenshot screen = new(ScHandle);
            if (screen.ShowDialog() == true)
            {
                _cropImg = screen.Crop;
                _cropRect = screen.CropRect;
            }
        }

        /// <summary>
        /// 注册鼠标钩子进行录制脚本操作，如果已注册鼠标钩子，则注销鼠标钩子并保存录制的脚本。
        /// </summary>
        private void ScriptRecordSwitch()
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

            if (MRS_Hook.HookID == IntPtr.Zero)
            {
                if (MRS_Hook.RegisterHook())
                {
                    RemoveScBlack();
                    HideKeyMapping();
                    bdRec.Visibility = Visibility.Visible;
                    dplImgShot.Visibility = Visibility.Visible;
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
            MRS_Hook.CancelHook(_cropImg, _cropRect);
            ShowKeyMapping();
            bdRec.Visibility = Visibility.Collapsed;
            dplImgShot.Visibility = Visibility.Collapsed;
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
            if (MRS_Hook.HookID != IntPtr.Zero)
            {
                popMid.Show("已开启录制操作，不能执行脚本。");
                return;
            }

            //List<ScriptInfo> infos = manage.GetSelectScript();
            ScriptItem item = manage.GetSelectScript();
            List<ScriptInfo> infos = MyGM.GetScriptList(item);

            if (infos[0].IsTime)
            {
                ScriptBefore(item, infos);
                TimedScript();
            }
            else
            {
                if (infos[0].ImgName != "")
                {
                    ScriptBefore(item, infos);
                    ImageScript();
                }
                else
                {
                    popMid.Show("未截取图片，不能执行图片脚本。");
                }
            }
        }

        /// <summary>
        /// 脚本执行前要执行的操作。
        /// </summary>
        /// <param name="infos">要执行的脚本信息。</param>
        private void ScriptBefore(ScriptItem item, List<ScriptInfo> infos)
        {
            //消除黑边，有黑边时会有偏移
            RemoveScBlack();

            //UI和变量初始化
            _vm.Running = 0;
            _vm.CurCycleNum = 0;
            _scriptItem = item;
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
            _siOrder = 0;
            _siSuccess = 0;
            _scriptTimer.Elapsed += _imageHandler;
            _scriptTimer.Interval = 100;//第一次执行的间隔
            _scriptTimer.Start();
            _countTimer.Start();
        }

        /// <summary>
        /// 在窗体中创建一个可以移动的圆形Popup，作为按键映射，此控件宽高与DPI相关。
        /// 有黑边时会导致Popup的位置有偏移。
        /// </summary>
        /// <param name="mapping">按键映射信息类。</param>
        /// <param name="isInitEdit">Popup初始是否为编辑状态。</param>
        /// <param name="isInitVisible">Popup初始是否可见。</param>
        /// <param name="isNew">是否为新创建的Popup。</param>
        /// <returns>DragPopup对象。</returns>
        private DragPopup CreatePop(KeyMapping mapping, bool isInitEdit, bool isInitVisible, bool isNew = false)
        {
            DragPopup pop = new(mapping, ScRatioX, ScRatioY, isInitEdit)
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = gShow,    //这里需要指明父级
                AllowsTransparency = true,
                Width = PopWd,
                Height = PopHt,
                IsOpen = isInitVisible,
                HorizontalOffset = mapping.OffsetX * (isNew ? 1 : ScRatioX),
                VerticalOffset = mapping.OffsetY * (isNew ? 1 : ScRatioY),
            };
            gShow.Children.Add(pop);//这里的父级要和pop.PlacementTarget属性一致
            
            return pop;
        }

        /// <summary>
        /// 进行按键映射的编辑，或保存编辑。
        /// </summary>
        /// <param name="desc">说明当前状态的字符串。</param>
        private void MappingEdit(string desc)
        {
            //if (IsScClosed)
            //{
            //    popMid.Show("未进行投屏，不能进行映射编辑。");
            //    return;
            //}

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
            MV.DragPopups.Add(CreatePop(
                new KeyMapping(ScActualWidth, ScActualHeight, PopWd, PopHt),
                true, true, true));
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

            HashSet<int> hs = [.. MyCfg.Cfg.KeyMappings.Select(k => (int)k.VkCode)];
            KMC_Hook.InitUserHotkeys(hs);
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
            //if (key =="Q")//按下Q使用鼠标控制方向
            //{
            //    PressMouse();
            //}
                
            DoKeyMapping(vkCode, ScHandle);
        }

        /// <summary>
        /// 触发快捷键。
        /// </summary>
        /// <param name="vkCode">虚拟键码。</param>
        public async void ShortcutKey(Keys vkCode)
        {
            switch (vkCode)
            {
                case Keys.F1: StartScrcpy(true); break;
                case Keys.F2: StartScrcpy(false); break;
                case Keys.F3: CloseScrcpy(); break;
                case Keys.F4: ScriptRecordSwitch(); break;
                case Keys.F5: OpenRecordManage(); break;
                case Keys.F6: new Setup().ShowDialog(); break;
                case Keys.F7: OpenMenu(btnSizeOpt, popSize); break;
                case Keys.F8: OpenMenu(btnFuncOpt, popFunc); break;
                case Keys.F9: await OpenRndis(); break;
                case Keys.F10: new DataSync().ShowDialog(); break;
                case Keys.F11: ScFocusSwitch(); break;
                case Keys.F12: BtnTest_Click(null, null); break;
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
        //        //}
        //        //else
        //        //{
        //        //    _isPressMouse = true;
        //        //    //DLL.mouse_event(0x02, 0, 0, 0, 0);//鼠标按下
        //        //}
        //        DLL.mouse_event(0x02, 0, 0, 0, 0);//鼠标按下
        //    }
        //    //Console.WriteLine($"开启鼠标移动,{_isPressMouse}。");
        //}

        /// <summary>
        /// 如果键入的字符在映射列表中，则映射为单击操作。
        /// </summary>
        /// <param name="vkCode">捕获的虚拟键码。</param>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        private void DoKeyMapping(Keys vkCode, IntPtr scHandle)
        {
            List<KeyMapping> mappings = [.. MyCfg.Cfg.KeyMappings.Where(k => k.VkCode == vkCode)];

            if (mappings.Count <= 0)
            {
                return;
            }
            else
            {
                popSize.IsOpen = false;
                popFunc.IsOpen = false;
            }

            foreach (KeyMapping mp in mappings)
            {
                //根据现在的比例计算左上角到圆心的偏移
                double cx = PopWd * MV.DpiScaleX / 2;
                double cy = PopHt * MV.DpiScaleY / 2;
                
                //获得圆心坐标
                int centreX = (int)(mp.OffsetX * ScRatioX * MV.DpiScaleX + cx);
                int centreY = (int)(mp.OffsetY * ScRatioY * MV.DpiScaleY + cy);

                SendClick(scHandle, centreX, centreY);

                //如果标志位为true，则让鼠标左键处于按下状态
                //if (_isPressMouse)
                //{
                //    DLL.mouse_event(0x02, 0, 0, 0, 0);//按下
                //    //Console.WriteLine("继续让鼠标左键处于按下状态。");
                //}
            }

            //逻辑鼠标回到物理鼠标的位置
            (double originX, double originY) = GetPointRelativeToScrcpy();
            IntPtr lp1 = new(((int)originY << 16) + (int)originX);
            DLL.PostMessage(scHandle, WM.MOUSEMOVE, IntPtr.Zero, lp1);
        }

        /// <summary>
        /// 向目标句柄窗口发送一次完整的鼠标点击操作，按下和抬起。
        /// PostMessage发送鼠标消息时，通过第四个参数指定鼠标在窗口中的位置，高16位是Y轴，低16位是X轴。
        /// </summary>
        /// <param name="scHandle">目标句柄窗口。</param>
        /// <param name="x">相对于目标窗口的X轴坐标。</param>
        /// <param name="y">相对于目标窗口的Y轴坐标。</param>
        private void SendClick(IntPtr scHandle, int x, int y)
        {
            IntPtr lp = new((y << 16) + x);
            DLL.PostMessage(scHandle, WM.MOUSEMOVE, IntPtr.Zero, lp);
            DLL.PostMessage(scHandle, WM.LBUTTONDOWN, (IntPtr)MK.LBUTTON, lp);
            DLL.PostMessage(scHandle, WM.LBUTTONUP, IntPtr.Zero, lp);
        }

        /// <summary>
        /// SC无输入焦点，则获得输入焦点；SC有输入焦点，主程序回收焦。
        /// SC有输入焦点时才能触发安卓的输入法，且电脑输入法需要是英文模式。
        /// </summary>
        private void ScFocusSwitch()
        {
            if (ScHandle == IntPtr.Zero) return;
            if (_vm.IsScHasFouce)
            {
                bool isOk = btnFouce.Focus();
                Debug.WriteLine($"F11，主程序获得焦点，{isOk}");
            }
            else
            {
                IntPtr hwnd = DLL.SetFocus(ScHandle);
                Debug.WriteLine($"F11，让Scrcpy获得焦点，{hwnd}");
            }
        }


    }
}
