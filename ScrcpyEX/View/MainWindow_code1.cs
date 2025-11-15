using Newtonsoft.Json.Linq;
using OpenCvSharp.ML;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
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
        /// 获得鼠标在Scrcpy中的坐标，
        /// X轴需要额外减去：左额外距离*DPI比例-左非工作区距离*DPI比例，
        /// Y轴需要额外减去：上额外距离*DPI比例。
        /// </summary>
        /// <returns>鼠标在Scrcpy中的坐标，以元组表示（X，Y）。</returns>
        public (double, double) GetPointInScrcpy()
        {
            Point pt1 = PointToScreen(new Point(0, 0)); //获得程序左上角相对于屏幕的位置，获得的值与DPI相关
            Point pt2 = MyGM.GetPosition();             //获取鼠标相对于屏幕的位置

            double x = pt2.X - pt1.X - ExtraWidthLeft * DpiScaleX + SystemParameters.WindowNonClientFrameThickness.Left * DpiScaleX;
            double y = pt2.Y - pt1.Y - ExtraHeightTop * DpiScaleY;

            //Console.WriteLine($"程序相对于屏幕%：({pt1.X}, {pt1.Y})，{SystemParameters.WindowNonClientFrameThickness.Left}。");
            //Console.WriteLine($"鼠标相对于屏幕%：({pt2.X}, {pt2.Y})。");

            return (x, y);
        }

        /// <summary>
        /// 获得鼠标在Scrcpy中的坐标，计算最大宽度和高度时需要乘以DPI比例。
        /// </summary>
        /// <returns>如果鼠标在Scrcpy中，返回鼠标在Scrcpy中的坐标；否则返回null。</returns>
        public Point? GetMouseInScrcpy()
        {
            (double x, double y) = GetPointInScrcpy();
            double maxW = ScActualWidth * DpiScaleX;
            double maxH = ScActualHeight * DpiScaleY;
            
            //最后实际保存的坐标要以默认尺寸为准，需要除以当前尺寸和默认尺寸的比例，
            //这样可以让用户在任意尺寸下录制的脚本在任意尺寸下都可以正确播放
            double actuclX = x / ScRatioX;
            double actuclY = y / ScRatioY;
            //Console.WriteLine($"鼠标在程序中坐标%：({x}, {y})；scrcpy尺寸%：({maxW}, {maxH})，和初始尺寸的比例：（{ScRatioX},{ScRatioY}），实际保存点：（{actuclX},{actuclY}）");
            
            if (x > 0 && x < maxW && y > 0 && y < maxH)
            {
                //Console.WriteLine($"YES，鼠标在scrcpy中。");
                //Console.WriteLine($"////////////////////。\n");
                return new Point(actuclX, actuclY);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 停止执行脚本的计时器，如果当前正在录制，也会停止录制操作。
        /// </summary>
        private void StopScriptAndRecord()
        {
            if (_countTimer.IsEnabled)
            {
                _scriptTimer.Stop();
                _countTimer.Stop();
                bdRec.Visibility = Visibility.Hidden;
                dplTip.Visibility = Visibility.Collapsed;
            }
            if (MouseScript.HookID != IntPtr.Zero)
            {
                StopRecord();
            }
        }

        /// <summary>
        /// 注册鼠标钩子进行录制脚本操作，如果已注册鼠标钩子，则注销鼠标钩子并保存录制的脚本。
        /// </summary>
        private void StartRecord()
        {
            if (IsClosed)
            {
                popMid.Show("未进行投屏，不能进行录制。");
                return;
            }
            if (_countTimer.IsEnabled)
            {
                popMid.Show("正在执行脚本，不能进行录制。");
                return;
            }

            if (MouseScript.HookID == IntPtr.Zero)
            {
                if (MouseScript.RegisterHook())
                {
                    RemoveBlack();
                    bdRec.Visibility = Visibility.Visible;
                    btnImgCompare.Visibility = Visibility.Collapsed;
                    popMid.Show("已开启录制操作，录制时会自动消除黑边。");
                }
                else
                {
                    popMid.Show("钩子注册失败，未能启动录制。");
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
            bdRec.Visibility = Visibility.Hidden;
            popMid.Show("已停止录制操作。");
        }

        /// <summary>
        /// 打开录制管理界面，界面返回值为true时执行脚本。
        /// </summary>
        private void OpenRecordManage()
        {
            RecManage manage = new();
            if (manage.ShowDialog() != true)
            {
                return;
            }
            
            if (IsClosed)
            {
                popMid.Show("未进行投屏，不能执行脚本。");
                return;
            }
            
            if (MouseScript.HookID != IntPtr.Zero)
            {
                popMid.Show("已开启录制操作，不能执行脚本。");
                return;
            }
                    
            if (manage.TempRecInfo.IsTime)
            {
                ScriptBefore(manage.TempRecInfo);
                TimedScript();
            }
            else
            {
                if (manage.TempRecInfo.ImgName != "")
                {
                    ScriptBefore(manage.TempRecInfo);
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
        /// <param name="recInfo">要执行的脚本信息。</param>
        private void ScriptBefore(RecInfo recInfo)
        {
            //消除黑边，有黑边时会有偏移
            RemoveBlack();

            //UI和变量初始化
            _vm.Running = 0;
            _vm.CycleNum = 0;
            _recInfo = recInfo;
            _stopwatch = Stopwatch.StartNew();
            bdRec.Visibility = Visibility.Visible;
            dplTip.Visibility = Visibility.Visible;
            btnImgCompare.Visibility = Visibility.Visible;

            //如果脚本已执行则关闭
            if (_countTimer.IsEnabled)
            {
                _scriptTimer.Stop();
                _countTimer.Stop();
            }

            //移除计时脚本和图像脚本，避免重复注册
            _scriptTimer.Tick -= _timedHandler;
            _scriptTimer.Tick -= _imageHandler;
        }

        /// <summary>
        /// 注册计时脚本并执行。
        /// </summary>
        private void TimedScript()
        {
            _scriptTimer.Tick += _timedHandler;
            _scriptTimer.Interval = TimeSpan.FromSeconds(_recInfo.CycleInterval);
            _scriptTimer.Start();
            _countTimer.Start();
            TimedScript_Tick(_recInfo);//立刻执行一次脚本
        }

        /// <summary>
        /// 注册图像脚本并执行。
        /// </summary>
        private void ImageScript()
        {
            _scriptTimer.Tick += _imageHandler;
            _scriptTimer.Interval = TimeSpan.FromSeconds(5);
            _scriptTimer.Start();
            _countTimer.Start();
            ImageScript_Tick(_recInfo);//立刻执行一次脚本
        }

        /// <summary>
        /// 在窗体中创建一个可以移动的Popup，Popup为圆形，直径默认为50，会根据比例放大缩小。
        /// 有黑边时会导致Popup的位置有偏移。
        /// </summary>
        /// <param name="mapping">按键映射类。</param>
        /// <param name="popIsEdit">Popup是否为编辑状态。</param>
        /// <param name="popIsVisible">Popup是否可见。</param>
        /// <returns>MyDragMove对象，其中封装了Popup的所有事件方法。</returns>
        private MyDragMove CreatePop(KeyMapping mapping, bool popIsEdit, bool popIsVisible = true)
        {
            double radius = 50 * ScRatioX;

            Popup pop = new()
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = gShow,//这里需要指明父级
                //PlacementTarget = gMain,//这里需要指明父级
                AllowsTransparency = true,
                Height = radius,
                Width = radius,
                IsOpen = popIsVisible,
                HorizontalOffset = mapping.OffsetX * ScRatioX,
                VerticalOffset = mapping.OffsetY * ScRatioY,
            };

            //Console.WriteLine($"映射Popup原始左上:({mapping.OffsetX},{mapping.OffsetY}),比例:({ScRatioX},{ScRatioY}),最终左上:({pop.HorizontalOffset},{pop.VerticalOffset})。");

            //提供边框
            Border border = new()
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                BorderBrush = Brushes.Black,
                BorderThickness = new(1),
                CornerRadius = new(radius),
                Name = "border",
            };

            //提供装多个控件的容器
            Grid grid = new();

            //提供输入
            TextBox txt = new()
            {
                Text = mapping.Char,
                Width = 15,
                FontSize = 16,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                //Visibility = Visibility.Collapsed,
            };

            //提供定位点
            TextBlock tbk = new()
            {
                Width = 1,
                Height = 1,
                //Background = Brushes.Red,
                Name = "tbk",
            };

            //提供删除功能
            Button btn = new()
            {
                FontSize = 8,
                Content = "X",
                Height = 14,
                Width = 14,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                //Visibility = Visibility.Collapsed,
            };

            //说明的字符的边框
            Border borderChar = new()
            {
                Width = 20,
                Height = 20,
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                BorderBrush = Brushes.Black,
                CornerRadius = new(20),
                Name = "borderChar",
            };

            //提供说明字符的TextBlock
            TextBlock tbkChar = new()
            {
                Text = mapping.Char,
                Name = "tbkChar",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Black,
                FontSize = 16,
                FontFamily = new FontFamily("Consolas"),
            };

            //确定层次关系
            borderChar.Child = tbkChar;
            grid.Children.Add(txt);
            grid.Children.Add(tbk);
            grid.Children.Add(btn);
            grid.Children.Add(borderChar);
            border.Child = grid;
            pop.Child = border;
            gShow.Children.Add(pop);//这里的父级要和pop.PlacementTarget属性一致

            return new MyDragMove(pop, mapping.VkCode, popIsEdit);
        }

        /// <summary>
        /// 进行按键映射的编辑，或保存编辑。
        /// </summary>
        /// <param name="desc">说明当前状态的字符串。</param>
        private void MappingEdit(string desc)
        {
            if (IsClosed)
            {
                popMid.Show("未进行投屏，不能进行映射编辑。");
                return;
            }

            if (desc == "映射编辑")
            {
                foreach (MyDragMove drag in MV.DragMoves)
                {
                    drag.SetDisplay(true);
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
            MV.DragMoves.Add(CreatePop(new KeyMapping(), true));
        }

        /// <summary>
        /// 保存界面上存在的映射Popup。
        /// </summary>
        private void MappingSave()
        {
            MyCfg.Cfg.KeyMappings.Clear();

            foreach (MyDragMove drag in MV.DragMoves)
            {
                string ch = drag.Char;
                Keys vkCode = drag.VkCode;
                double offsetX = drag.OffsetX / ScRatioX;
                double offsetY = drag.OffsetY / ScRatioY;
                drag.SetDisplay(false);

                MyCfg.Cfg.KeyMappings.Add(new KeyMapping(ch, vkCode, offsetX, offsetY));
                Console.WriteLine($"映射字符：{ch}，虚拟键：{vkCode}/{(int)vkCode}，保存偏移:（{offsetX:F2}，{offsetY:F2}）。");
            }

            Console.WriteLine($"已有映射数量：{MyCfg.Cfg.KeyMappings.Count}。");
        }

        /// <summary>
        /// 映射按钮编辑完后调用此方法，无论是否保存。
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
        /// <param name="popIsEdit">Popup是否为编辑状态。</param>
        private void CreateSavedPop(bool popIsEdit = false, bool popIsVisible = true)
        {
            foreach (MyDragMove drag in MV.DragMoves)
            {
                drag.Dispose();
                drag.SetVisible(false);
            }
            MV.DragMoves.Clear();

            foreach (KeyMapping mapping in MyCfg.Cfg.KeyMappings)
            {
                MV.DragMoves.Add(CreatePop(mapping, popIsEdit, popIsVisible));
            }
        }

        /// <summary>
        /// 隐藏所有的按键映射。
        /// </summary>
        private void HideKeyMapping()
        {
            foreach (MyDragMove drag in MV.DragMoves)
            {
                drag.SetVisible(false);
            }
        }

        /// <summary>
        /// 显示所有的按键映射。
        /// </summary>
        private void ShowKeyMapping()
        {
            foreach (MyDragMove drag in MV.DragMoves)
            {
                drag.SetVisible(true);
            }
        }

        /// <summary>
        /// 键盘输入映射为点击操作。
        /// </summary>
        /// <param name="vkCode">捕获的虚拟键码。</param>
        public void KeyboardMapping(Keys vkCode)
        {
            //Console.WriteLine($"窗口是否活动：{IsActive}，是否有焦点：{IsFocused}");
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
            //让字符只剩字母或数字
            //if (Regex.IsMatch(key, @"[0-9]"))
            //{
            //    key = Regex.Replace(key, @"[A-Za-z]", "");
            //}

            List<KeyMapping> mappings = MyCfg.Cfg.KeyMappings.Where(a => a.VkCode == vkCode).ToList();
            foreach (KeyMapping m in mappings)
            {
                //根据现在的比例计算半径
                double radius = 50 * ratio * DpiScaleX / 2;
                
                //获得圆心坐标
                int centreX = (int)(m.OffsetX * ratio * DpiScaleX + radius);
                int centreY = (int)(m.OffsetY * ratio * DpiScaleY + radius);

                IntPtr lp = new((centreY << 16) + centreX);
                DLL.PostMessage(scHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, lp);
                DLL.PostMessage(scHandle, WM.WM_LBUTTONDOWN, (IntPtr)WM.MK_LBUTTON, lp);
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
            (double originX, double originY) = GetPointInScrcpy();
            IntPtr lp1 = new(((int)originY << 16) + (int)originX);
            DLL.PostMessage(scHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, lp1);
        }

        /// <summary>
        /// Scrcpy窗口获得焦点，Scrcpy有焦点时才能触发安卓的输入法。
        /// </summary>
        private void ScGotFocus(object parameter)
        {
            DLL.SetFocus(ScHandle);
        }


    }
}
