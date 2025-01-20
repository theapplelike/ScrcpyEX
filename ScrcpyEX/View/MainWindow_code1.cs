using Newtonsoft.Json.Linq;
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
        /// 当前窗口或Scrcpy是否为活动窗口。
        /// </summary>
        /// <returns>0--窗口拥有焦点；1--Scrcpy拥有焦点；2--窗口或Scrcpy都没有焦点。</returns>
        public int IsActiveWindow()
        {
            if (IsActive)
            {
                return 0;
            }
            else
            {
                IntPtr intPtr = DLL.GetForegroundWindow();
                return intPtr == _scHandle ? 1 : 2;
            }
        }

        /// <summary>
        /// 停止执行脚本的计时器，如果当前正在录制，也会停止录制操作。
        /// </summary>
        private void StopScriptAndRecord()
        {
            if (_clickTimer.IsEnabled)
            {
                _clickTimer.Stop();
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
            if (_scHandle == IntPtr.Zero)
            {
                popMid.Show("未进行投屏，不能进行录制。");
                return;
            }
            if (_clickTimer.IsEnabled)
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
            RecManage rec = new();
            if (rec.ShowDialog() == true)
            {
                if (_scHandle == IntPtr.Zero)
                {
                    popMid.Show("未进行投屏，不能执行脚本。");
                }
                else
                {
                    if (MouseScript.HookID != IntPtr.Zero)
                    {
                        popMid.Show("已开启录制操作，不能执行脚本。");
                        return;
                    }

                    //消除黑边，有黑边时会有偏移
                    RemoveBlack();

                    //如果脚本已执行则关闭
                    if (_clickTimer.IsEnabled)
                    {
                        _clickTimer.Stop();
                        _countTimer.Stop();
                    }

                    bdRec.Visibility = Visibility.Visible;
                    dplTip.Visibility = Visibility.Visible;
                    _vm.CycleNum = 0;

                    _recInfo = rec.TempRecInfo;
                    _clickTimer.Interval = TimeSpan.FromSeconds(_recInfo.CycleInterval);
                    _clickTimer.Start();
                    _countTimer.Start();
                    Click_Tick(_recInfo);//立刻执行一次脚本
                }
            }
        }

        /// <summary>
        /// 在窗体中创建一个可以移动的Popup，Popup为圆形，直径默认为50，会根据比例放大缩小。
        /// 有黑边时会导致Popup的位置有偏移。
        /// </summary>
        private Popup CreatePop(KeyMapping mapping)
        {
            double radius = 50 * ScRatioX;

            Popup pop = new()
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = gShow,//这里需要指明父级
                AllowsTransparency = true,
                Height = radius,
                Width = radius,
                IsOpen = true,
                HorizontalOffset = mapping.OffsetX * ScRatioX,
                VerticalOffset = mapping.OffsetY * ScRatioY,
            };

            Console.WriteLine($"原始左上:({mapping.OffsetX},{mapping.OffsetY}),比例:({ScRatioX},{ScRatioY}),最终左上:({pop.HorizontalOffset},{pop.VerticalOffset})。");

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
            if (desc == "进行映射编辑")
            {
                btnMappingEdit.Content = "保存编辑";
                btnMappingAdd.IsEnabled = true;
            }
            else
            {
                MappingSave();
                btnMappingEdit.Content = "进行映射编辑";
                btnMappingAdd.IsEnabled = false;
            }
        }

        /// <summary>
        /// 添加一个映射Popup到界面上。
        /// </summary>
        private void MappingAdd()
        {
            MV.TempPopups.Add(CreatePop(new KeyMapping()));
        }

        /// <summary>
        /// 保存界面上存在的映射Popup。
        /// </summary>
        private void MappingSave()
        {
            MyCfg.Cfg.KeyMappings.Clear();

            foreach (var pop in MV.TempPopups)
            {
                string key = MyGM.FindVisualChild<TextBox>(pop.Child).Text;
                double offsetX = pop.HorizontalOffset / ScRatioX;
                double offsetY = pop.VerticalOffset / ScRatioY;

                MyCfg.Cfg.KeyMappings.Add(new KeyMapping(key, offsetX, offsetY));
                Console.WriteLine($"映射内容：{key}，保存偏移：（{offsetX}，{offsetY}）。");
            }

            Console.WriteLine($"已有映射：{MyCfg.Cfg.KeyMappings.Count}。");
        }

        /// <summary>
        /// 创建已保存的所有映射Popup，并保存到临时Popup列表，保存前会清空列表。
        /// </summary>
        private void CreateSavedPop()
        {
            MV.TempPopups.Clear();

            foreach (KeyMapping mapping in MyCfg.Cfg.KeyMappings)
            {
                MV.TempPopups.Add(CreatePop(mapping));
            }
        }

        /// <summary>
        /// 键盘输入映射为点击操作。
        /// </summary>
        /// <param name="key">捕获的键盘字符。</param>
        public void KeyboardMapping11(string key)
        {
            int focus = IsActiveWindow();
            if (focus != 2)
            {
                if (focus == 1)
                {
                    Activate();//将窗口带到前台
                }
                DoKeyMapping(key, ScRatioX);
            }
            else
            {
                Console.WriteLine("不为活动窗口。");
            }
        }

        /// <summary>
        /// 如果键入的字符在映射列表中，则映射为单击操作。
        /// </summary>
        /// <param name="key">捕获的键盘字符。</param>
        /// <param name="ratio">Scrcpy当前宽度和默认宽度的比值。</param>
        private void DoKeyMapping(string key, double ratio)
        {
            //如果包含数字
            if (Regex.IsMatch(key, @"[0-9]"))
            {
                key = Regex.Replace(key, @"[A-Za-z]", "");
            }
            Console.WriteLine($"映射字符：{key}。");//应只剩字母或数字

            //bool result = MyCfg.Cfg.KeyMappings.Any(a => a.Key == key);
            //if (result)
            //{
            //    Console.WriteLine("列表中存在项。");
            //}
            //else
            //{
            //    Console.WriteLine("不存在。");
            //}

            List<KeyMapping> mappings = MyCfg.Cfg.KeyMappings.Where(a => a.Key == key).ToList();
            //if (mappings.Count > 0)
            //{
            //    Console.WriteLine("列表中存在项。");
            //}


            foreach (KeyMapping m in mappings)
            {
                //根据现在的比例计算半径
                double radius = 50 * ratio / 2;
                
                //获得圆心坐标
                double centreX = m.OffsetX * ratio + radius;
                double centreY = m.OffsetY * ratio + radius;

                //获得对应的安卓坐标
                double x = centreX / ScActualWidth * 2312;
                double y = centreY / ScActualHeight * 1080;

                Console.WriteLine($"圆心点：（{centreX}，{centreY}），安卓点：（{x}，{y}）。");
                Test1(x, y);
            }
        }


    }
}
