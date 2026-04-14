using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// 可拖拽、可映射按键的Popup。
    /// </summary>
    internal class DragPopup : Popup, IDisposable
    {
        /// <summary>
        /// 提供最外层边框。
        /// </summary>
        private Border _bdOuter;

        /// <summary>
        /// 提供输入。
        /// </summary>
        private TextBox _txtInput;

        /// <summary>
        /// 提供锚定点。
        /// </summary>
        private TextBlock _tbkAnchorPoint;

        /// <summary>
        /// 提供删除功能。
        /// </summary>
        private Button _btnDelete;

        /// <summary>
        /// 显示映射按键字符的边框。
        /// </summary>
        private Border _bdChar;

        /// <summary>
        /// 显示映射按键的字符。
        /// </summary>
        private TextBlock _tbkDisplayChar;

        /// <summary>
        /// 是否可以移出逻辑父级。
        /// </summary>
        private readonly bool _isLeaveParent;

        /// <summary>
        /// 上一次的鼠标坐标。
        /// </summary>
        private Point _lastPoint;

        /// <summary>
        /// 当前鼠标的坐标。
        /// </summary>
        private Point _nowPoint;

        /// <summary>
        /// 是否处于可拖拽的状态。
        /// </summary>
        private bool _isDragging = false;

        /// <summary>
        /// 当前是否处于编辑状态。
        /// </summary>
        private bool _isEdit = false;

        /// <summary>
        /// 记录位置信息的句柄，用于PostMessage函数。
        /// </summary>
        private IntPtr _lp = IntPtr.Zero;

        /// <summary>
        /// 是否已释放资源。
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 当前映射绑定的字符。
        /// </summary>
        public string Char => _txtInput.Text;

        /// <summary>
        /// 当前映射绑定的虚拟键码。
        /// </summary>
        public Keys VkCode { get; set; } = Keys.None;

        /// <summary>
        /// 当前映射的X轴偏移。
        /// </summary>
        public double OffsetX => HorizontalOffset;

        /// <summary>
        /// 当前映射的Y轴偏移。
        /// </summary>
        public double OffsetY => VerticalOffset;

        /// <summary>
        /// 可拖拽、可映射按键的Popup。
        /// </summary>
        /// <param name="mapping">当前Popup对应的按键信息映射类。</param>
        /// <param name="ratio">其中控件的缩放比例。</param>
        /// <param name="isInitEdit">Popup初始是否为编辑状态。</param>
        /// <param name="isLeaveParent">是否可以移出逻辑父级。</param>
        public DragPopup(KeyMapping mapping, double ratio, bool isInitEdit, bool isLeaveParent = false)
        {
            VkCode = mapping.VkCode;
            _isLeaveParent = isLeaveParent;

            ControlInit(mapping.Char, ratio);
            ControlEventBinding();
            SetEditingStatus(isInitEdit);
        }

        /// <summary>
        /// DragPopup中包含的控件进行初始化。
        /// </summary>
        /// <param name="ch">映射的字符。</param>
        /// <param name="ratio">映射Popup的直径。</param>
        private void ControlInit(string ch, double ratio)
        {
            //提供最外层边框
            _bdOuter = new()
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                BorderBrush = Brushes.Black,
                BorderThickness = new(1),
                CornerRadius = new(50 * ratio),
            };

            //提供装多个控件的容器
            Grid grid = new();

            //提供输入
            _txtInput = new()
            {
                Text = ch,
                //Width = 15,
                Width = MyGM.RatioClamp(15 , ratio, 0.2),
                //FontSize = 16,
                FontSize = MyGM.RatioClamp(16, ratio, 0.2),
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                //Visibility = Visibility.Collapsed,
            };

            //提供锚定点
            _tbkAnchorPoint = new()
            {
                Width = 1,
                Height = 1,
                //Background = Brushes.Red,
            };

            //提供删除功能
            _btnDelete = new()
            {
                FontSize = 8,
                Content = "X",
                //Height = 14,
                //Width = 14,
                Height = MyGM.RatioClamp(14, ratio, 0.2),
                Width = MyGM.RatioClamp(14, ratio, 0.2),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                //Visibility = Visibility.Collapsed,
            };

            //显示映射按键字符的边框
            _bdChar = new()
            {
                //Width = 20,
                //Height = 20,
                Height = MyGM.RatioClamp(20, ratio, 0.2),
                Width = MyGM.RatioClamp(20, ratio, 0.2),
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                BorderBrush = Brushes.Black,
                CornerRadius = new(20),
            };

            //显示映射按键的字符
            _tbkDisplayChar = new()
            {
                Text = ch,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Black,
                //FontSize = 16,
                FontSize = MyGM.RatioClamp(16, ratio, 0.2),
                FontFamily = new FontFamily("Consolas"),
            };

            //确定层次关系
            _bdChar.Child = _tbkDisplayChar;
            grid.Children.Add(_txtInput);
            grid.Children.Add(_tbkAnchorPoint);
            grid.Children.Add(_btnDelete);
            grid.Children.Add(_bdChar);
            _bdOuter.Child = grid;
            Child = _bdOuter;
        }

        /// <summary>
        /// DragPopup中相关控件进行事件绑定。
        /// </summary>
        private void ControlEventBinding()
        {
            _txtInput.PreviewTextInput += TextBox_PreviewTextInput;
            _txtInput.TextChanged += TextBox_TextChanged;
            _txtInput.SetValue(InputMethod.IsInputMethodEnabledProperty, false);//文本框禁用输入法

            _btnDelete.Click += BtnDelete_Click;

            MouseDown += Pop_MouseDown;
            MouseUp += Pop_MouseUp;
            MouseMove += Pop_MouseMove;
        }

        /// <summary>
        /// 鼠标按下时发生的事件，处于编辑状态时允许移动且记录当前坐标，不可编辑时应将点击操作发给Scrcpy。
        /// 嵌套顺序应该为Popup->Border(提供边框)->TextBox(提供输入框)、TextBlock(提供定位点)。
        /// </summary>
        private void Pop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isEdit)
            {
                //让Popup中的锚定点捕获鼠标，防止移动太快时丢失Popup
                _isDragging = Mouse.Capture(_tbkAnchorPoint);
                SelectAll(_txtInput);

                //记录当前坐标为上一次的坐标
                _lastPoint = MyGM.GetPosition();
            }
            else
            {
                (double originX, double originY) = MV.MainWin.GetPointRelativeToScrcpy();
                _lp = new(((int)originY << 16) + (int)originX);

                //发送鼠标按下消息
                DLL.PostMessage(MV.MainWin.ScHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, _lp);
                DLL.PostMessage(MV.MainWin.ScHandle, WM.WM_LBUTTONDOWN, (IntPtr)MK.MK_LBUTTON, _lp);
            }

            //double radius = 50 * MV.MainWin.ScRatioX / 2;
            //double radius = 50 / 2;//根据现在的比例计算半径，还需要乘以Scrcpy当前宽度和默认宽度的比值，这里默认这个比值为1，这个值仅用于输出调试内容，所以没有传入
            //Debug.WriteLine($"popup中心点：（{_pop.HorizontalOffset + radius}，{_pop.VerticalOffset + radius}），左上角：（{_pop.HorizontalOffset}，{_pop.VerticalOffset}）。");
            //Debug.WriteLine($"记录的最后鼠标坐标：（{_lastPoint.X},{_lastPoint.Y}）,{_isDragging}.");
        }

        /// <summary>
        /// 鼠标抬起发生的事件，停止跟随鼠标移动。
        /// </summary>
        private void Pop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isEdit)
            {
                Mouse.Capture(null);
                _isDragging = false;
            }
            else
            {
                if (_lp != IntPtr.Zero)
                {
                    //发送鼠标抬起消息
                    DLL.PostMessage(MV.MainWin.ScHandle, WM.WM_LBUTTONUP, IntPtr.Zero, _lp);
                }
            }
        }

        /// <summary>
        /// 鼠标移动时发生的事件。
        /// </summary>
        private void Pop_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _isEdit)
            {
                _nowPoint = MyGM.GetPosition();//现在的坐标

                double newX = HorizontalOffset + (_nowPoint.X - _lastPoint.X) / MV.DpiScaleX;
                double newY = VerticalOffset + (_nowPoint.Y - _lastPoint.Y) / MV.DpiScaleY;

                if (!_isLeaveParent)//是否允许离开父控件
                {
                    LimitToParent(ref newX, ref newY);
                }

                //Debug.WriteLine(
                //    $"上次坐标：（{_lastPoint.X}，{_lastPoint.Y}），" +
                //    $"当前坐标：（{_nowPoint.X}，{_nowPoint.Y}），" +
                //    $"旧位置：（{newX}，{newY}），" +
                //    $"新位置：（{HorizontalOffset}，{VerticalOffset}），");

                HorizontalOffset = newX;
                VerticalOffset = newY;
                _lastPoint = _nowPoint;
            }
        }

        /// <summary>
        /// 键盘的输入会触发此事件，只允许输入一个字符，且只能为指定字符。
        /// </summary>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length >= 1)
                {
                    if (IsAllTextSelected(textBox))
                    {
                        VkCode = CharToVirtualKey(e.Text[0]);
                        _tbkDisplayChar.Text = e.Text;
                        //Console.WriteLine($"长度>=1：{CharToVirtualKey(e.Text[0])}");
                    }
                    else
                    {
                        e.Handled = true;//如果已经有一个字符，阻止进一步输入
                    }
                }
                else
                {
                    if (Regex.IsMatch(e.Text, @"^[a-zA-Z0-9,./;\[\]']{1}$"))
                    {
                        VkCode = CharToVirtualKey(e.Text[0]);
                        _tbkDisplayChar.Text = e.Text.ToUpper();
                        //Console.WriteLine($"长度<1：{CharToVirtualKey(e.Text[0])}");
                    }
                    else
                    {
                        e.Handled = true;//如果不符合正则表达式，阻止输入
                    }
                }
            }
        }

        /// <summary>
        /// 文本框文本的任何更改都会触发此事件，处理粘贴时多余的文本、小写转大写。
        /// </summary>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text.Length > 1)
                {
                    textBox.Text = textBox.Text.Substring(0, 1);//如果长度超过一，只保留第一个
                    textBox.CaretIndex = 1;
                }
                else if (textBox.Text.Length == 1)
                {
                    if (char.IsLower(textBox.Text[0]))
                    {
                        textBox.Text = textBox.Text.ToUpper();
                        textBox.CaretIndex = 1;
                    }
                }
            }
        }

        /// <summary>
        /// 文本框获得焦点，选择所有文本。
        /// </summary>
        private void SelectAll(TextBox txt)
        {
            txt.Focus();
            txt.SelectAll();
        }

        /// <summary>
        /// 检查文本框是否选中了所有的文本。
        /// </summary>
        /// <param name="txt">指定的文本框。</param>
        /// <returns>true--选中了所有文本；false--未选中所有文本。文本框中没有任何内容时始终返回false。</returns>
        private bool IsAllTextSelected(TextBox txt)
        {
            return txt.SelectionStart == 0 && txt.SelectionLength == txt.Text.Length;
        }

        /// <summary>
        /// 将字符转换为虚拟键码。
        /// </summary>
        /// <param name="character">需要转化的字符。</param>
        /// <returns>Keys枚举。</returns>
        private Keys CharToVirtualKey(char character)
        {
            short result = DLL.VkKeyScan(character);

            if (result != -1)
            {
                int vkCode = result & 0xFF;//低字节是虚拟键码
                return (Keys)vkCode;
            }

            return Keys.None;
        }

        /// <summary>
        /// 删除当前的Popup。
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            MV.DragPopups.Remove(this);
            IsOpen = false;
            Dispose();
        }

        /// <summary>
        /// 将元素坐标限制在逻辑父级当中，
        /// </summary>
        /// <param name="newX">左边距＜newX＜左边距+父级宽度。</param>
        /// <param name="newY">上边距＜newY＜上边距+父级高度。</param>
        private void LimitToParent(ref double newX, ref double newY)
        {
            double maxLeft = MV.MainWin.ScActualWidth;
            double maxTop = MV.MainWin.ScActualHeight;

            if (newX < 0)
            {
                newX = 0;
            }
            else if (newX + Width > maxLeft)
            {
                newX = maxLeft - Width;
            }
            if (newY < 0)
            {
                newY = 0;
            }
            else if (newY + Height > maxTop)
            {
                newY = maxTop - Height;
            }
        }

        /// <summary>
        /// 设置Popup的编辑状态。
        /// </summary>
        /// <param name="isEdit">Popup是否为编辑状态。</param>
        public void SetEditingStatus(bool isEdit)
        {
            _isEdit = isEdit;
            if (isEdit)
            {
                _bdOuter.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
                _bdOuter.BorderThickness = new(1);
                _txtInput.Visibility = Visibility.Visible;
                _btnDelete.Visibility = Visibility.Visible;
                _bdChar.Visibility = Visibility.Hidden;
                _tbkDisplayChar.Visibility = Visibility.Hidden;
            }
            else
            {
                _bdOuter.Background = Brushes.Transparent;
                _bdOuter.BorderThickness = new(0);
                _txtInput.Visibility = Visibility.Hidden;
                _btnDelete.Visibility = Visibility.Hidden;
                _bdChar.Visibility = Visibility.Visible;
                _tbkDisplayChar.Visibility = Visibility.Visible;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            
            if (disposing)
            {
                // 取消所有事件订阅
                _txtInput.PreviewTextInput -= TextBox_PreviewTextInput;
                _txtInput.TextChanged -= TextBox_TextChanged;
            
                _btnDelete.Click -= BtnDelete_Click;
            
                MouseDown -= Pop_MouseDown;
                MouseMove -= Pop_MouseMove;
                MouseUp -= Pop_MouseUp;
            }
            
            _disposed = true;
        }

        ~DragPopup() => Dispose(false);
    }
}
