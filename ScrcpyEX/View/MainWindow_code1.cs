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

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        /// <summary>
        /// 获得鼠标在scrcpy中的位坐标，计算需要额外减去Point(左额外距离*Dpi比例-左非工作区距离*Dpi比例，上额外距离*Dpi比例)。
        /// 计算最大宽度和高度时需要乘以Dpi比例，Point的参数已经乘了Dpi比例，其他参数没有。
        /// </summary>
        /// <returns>如果鼠标在scrcpy中，返回鼠标在scrcpy中的坐标，否则返回null。</returns>
        public Point? GetMouseInScrcpy()
        {
            Point pt1 = PointToScreen(new Point(0, 0));//获得程序左上角相对于屏幕的位置，获得的值与DPI相关
            Point pt2 = MyGM.GetPosition();//获取鼠标相对于屏幕的位置

            double x = pt2.X - pt1.X - ExtraWidthLeft * DpiScaleX + SystemParameters.WindowNonClientFrameThickness.Left * DpiScaleX;
            double y = pt2.Y - pt1.Y - ExtraHeightTop * DpiScaleY;
            double maxW = ScActualWidth * DpiScaleX;
            double maxH = ScActualHeight * DpiScaleY;
            
            //最后实际保存的坐标要以默认尺寸为准，需要除以当前尺寸和默认尺寸的比例，
            //这样可以让用户在任意尺寸下录制的脚本在任意尺寸下都可以正确播放
            double actuclX = x / ScRatioX;
            double actuclY = y / ScRatioY;

            //Console.WriteLine($"程序相对于屏幕%：({pt1.X}, {pt1.Y})，{SystemParameters.WindowNonClientFrameThickness.Left}。");
            //Console.WriteLine($"鼠标相对于屏幕%：({pt2.X}, {pt2.Y})。");
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
        /// <param name="popIsOpen">创建的Popup是否可见。</param>
        /// <returns>创建的Popup对象。</returns>
        private Popup CreatePop(KeyMapping mapping, bool popIsOpen)
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
                IsOpen = popIsOpen,
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
            };

            //提供装多个控件的容器
            Grid grid = new();

            //提供输入
            TextBox txt = new()
            {
                Text = mapping.Key,
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
                //Background= Brushes.Red,
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

            //确定层次关系
            grid.Children.Add(txt);
            grid.Children.Add(tbk);
            grid.Children.Add(btn);
            border.Child = grid;
            pop.Child = border;
            gShow.Children.Add(pop);//这里的父级要和pop.PlacementTarget属性一致

            //设置Popup可以移动
            _ = new MyDragMove(pop);
            return pop;
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
                foreach (Popup pop in MV.TempPopups)
                {
                    pop.IsOpen = true;
                }

                btnMappingEdit.Content = "保存修改";
                btnMappingBack.IsEnabled = true;
                btnMappingAdd.IsEnabled = true;
            }
            else
            {
                MappingSave();
                MappingEditCompleted(true);
            }
        }

        /// <summary>
        /// 不保存按键映射编辑的内容。
        /// </summary>
        private void MappingCancel(List<KeyMapping> mappings)
        {
            foreach (Popup pop in MV.TempPopups)
            {
                pop.IsOpen = false;
            }

            CreateSavedPop();
            MappingEditCompleted(false);
        }

        /// <summary>
        /// 添加一个映射Popup到界面上。
        /// </summary>
        private void MappingAdd()
        {
            MV.TempPopups.Add(CreatePop(new KeyMapping(), true));
        }

        /// <summary>
        /// 保存界面上存在的映射Popup。
        /// </summary>
        private void MappingSave()
        {
            MyCfg.Cfg.KeyMappings.Clear();

            foreach (Popup pop in MV.TempPopups)
            {
                string key = pop.Child.FindVisualChild<TextBox>().Text;
                double offsetX = pop.HorizontalOffset / ScRatioX;
                double offsetY = pop.VerticalOffset / ScRatioY;
                pop.IsOpen = false;

                MyCfg.Cfg.KeyMappings.Add(new KeyMapping(key, offsetX, offsetY));
                Console.WriteLine($"映射内容：{key}，保存偏移：（{offsetX}，{offsetY}）。");
            }

            Console.WriteLine($"已有映射：{MyCfg.Cfg.KeyMappings.Count}。");
        }

        /// <summary>
        /// 映射按钮编辑完后调用此方法，无论是否保存。
        /// </summary>
        /// <param name="isHidden">所有的Popup是否已经隐藏。</param>
        private void MappingEditCompleted(bool isHidden)
        {
            btnMappingEdit.Content = "映射编辑";
            btnMappingBack.IsEnabled = false;
            btnMappingAdd.IsEnabled = false;
        }

        /// <summary>
        /// 创建配置中所有映射Popup，并保存到临时Popup列表，保存前会清空列表。
        /// </summary>
        private void CreateSavedPop()
        {
            MV.TempPopups.Clear();

            foreach (KeyMapping mapping in MyCfg.Cfg.KeyMappings)
            {
                MV.TempPopups.Add(CreatePop(mapping, false));
            }
        }

        /// <summary>
        /// 键盘输入映射为点击操作。
        /// </summary>
        /// <param name="key">捕获的键盘字符。</param>
        public void KeyboardMapping(string key)
        {
            if (IsActive && ScHandle != IntPtr.Zero)
            {
                //if (key =="Q")//按下Q使用鼠标控制方向
                //{
                //    PressMouse();
                //}

                DoKeyMapping(key, ScRatioX, ScHandle);
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
        /// <param name="key">捕获的键盘字符。</param>
        /// <param name="ratio">Scrcpy当前宽度和默认宽度的比值。</param>
        private void DoKeyMapping(string key, double ratio, IntPtr scHandle)
        {
            //让字符只剩字母或数字
            if (Regex.IsMatch(key, @"[0-9]"))
            {
                key = Regex.Replace(key, @"[A-Za-z]", "");
            }

            List<KeyMapping> mappings = MyCfg.Cfg.KeyMappings.Where(a => a.Key == key).ToList();
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
        }

        


    }
}
