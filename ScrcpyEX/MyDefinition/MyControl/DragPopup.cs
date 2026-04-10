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
        /// 显示映射按键字符的边框，固定大小。
        /// </summary>
        private Border _bdChar;

        /// <summary>
        /// 显示映射按键的字符。
        /// </summary>
        private TextBlock _tbkDisplayChar;

        private readonly bool _isLeaveParent;   //是否可以移出逻辑父级
        private Point _lastPoint;               //上一次的鼠标坐标
        private Point _nowPoint;                //当前鼠标的坐标
        private bool _isDragging = false;       //是否处于可拖拽的状态
        private bool _isEdit = false;           //当前是否处于编辑状态
        private IntPtr _lp = IntPtr.Zero;       //记录位置信息的句柄
        private bool _disposed = false;         //是否释放资源

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
        /// <param name="vkCode">当前映射绑定的虚拟键码。</param>
        /// <param name="popIsEdit">Popup是否为编辑状态。</param>
        /// <param name="isLeaveParent">是否可以移出逻辑父级。</param>
        public DragPopup(KeyMapping mapping, bool popIsEdit, double diameter, bool isLeaveParent = false)
        {
            ControlInit(mapping.Char, diameter);
            ControlEventBinding();

            VkCode = mapping.VkCode;
            _isLeaveParent = isLeaveParent;

            //_txt = Child.FindVisualChild<TextBox>();
            //_txt.PreviewTextInput += TextBox_PreviewTextInput;
            //_txt.TextChanged += TextBox_TextChanged;
            //_txt.SetValue(InputMethod.IsInputMethodEnabledProperty, false);//文本框禁用输入法

            //_btn = Child.FindVisualChild<Button>();
            //_btn.Click += Button_Click;

            //_tbkChar = Child.FindVisualChildByName<TextBlock>("tbkChar");

            //MouseDown += Pop_MouseDown;
            //MouseMove += Pop_MouseMove;
            //MouseUp += Pop_MouseUp;

            SetDisplay(popIsEdit);
        }

        /// <summary>
        /// DragPopup中包含的控件进行初始化。
        /// </summary>
        /// <param name="ch">映射的字符。</param>
        /// <param name="diameter">映射Popup的直径。</param>
        private void ControlInit(string ch, double diameter)
        {
            //提供最外层边框
            _bdOuter = new()
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                BorderBrush = Brushes.Black,
                BorderThickness = new(1),
                CornerRadius = new(diameter),
                //Name = "bdOuter",
            };

            //提供装多个控件的容器
            Grid grid = new();

            //提供输入
            _txtInput = new()
            {
                Text = ch,
                Width = 15,
                FontSize = 16,
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
                //Name = "tbkAnchorPoint",
            };

            //提供删除功能
            _btnDelete = new()
            {
                FontSize = 8,
                Content = "X",
                Height = 14,
                Width = 14,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                //Visibility = Visibility.Collapsed,
            };

            //显示映射按键字符的边框，固定大小
            _bdChar = new()
            {
                Width = 20,
                Height = 20,
                Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)),
                BorderBrush = Brushes.Black,
                CornerRadius = new(20),
                //Name = "bdChar",
            };

            //显示映射按键的字符
            _tbkDisplayChar = new()
            {
                Text = ch,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Black,
                FontSize = 16,
                FontFamily = new FontFamily("Consolas"),
                //Name = "tbkDisplayChar",
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
                //让Popup中的TextBlock捕获鼠标，防止移动太快时丢失Popup
                //TextBlock tbk = Child.FindVisualChildByName<TextBlock>("tbkAnchorPoint");
                _isDragging = Mouse.Capture(_tbkAnchorPoint);
                SelectAll(_txtInput);

                //记录当前坐标为上一次的坐标
                _lastPoint = MyGM.GetPosition();
            }
            else
            {
                (double originX, double originY) = MV.MainWin.GetPointInScrcpy();
                _lp = new(((int)originY << 16) + (int)originX);

                //发送鼠标按下消息
                DLL.PostMessage(MV.MainWin.ScHandle, WM.WM_MOUSEMOVE, IntPtr.Zero, _lp);
                DLL.PostMessage(MV.MainWin.ScHandle, WM.WM_LBUTTONDOWN, (IntPtr)WM.MK_LBUTTON, _lp);
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
                //Console.WriteLine("解锁");
            }
            else
            {
                //Console.WriteLine("准备发送");
                if (_lp != IntPtr.Zero)
                {
                    //发送鼠标抬起消息
                    DLL.PostMessage(MV.MainWin.ScHandle, WM.WM_LBUTTONUP, IntPtr.Zero, _lp);
                    //Console.WriteLine("发送抬起事件");
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

                double newX = HorizontalOffset + (_nowPoint.X - _lastPoint.X) / MV.MainWin.DpiScaleX;
                double newY = VerticalOffset + (_nowPoint.Y - _lastPoint.Y) / MV.MainWin.DpiScaleY;

                if (!_isLeaveParent)//是否允许离开父控件
                {
                    LimitToParent(ref newX, ref newY);
                }

                //Console.WriteLine($"##:{newX}+{_pop.Height}<>{_parentRect.Width}。");
                //Console.WriteLine($"上次坐标：（{_lastPoint.X}，{_lastPoint.Y}），" +
                //    $"当前坐标：（{_nowPoint.X}，{_nowPoint.Y}），" +
                //    $"旧位置：（{newX}，{newY}），" +
                //    $"新位置：（{_pop.HorizontalOffset}，{_pop.VerticalOffset}），");

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
        /// 设置Popup的显示状态。
        /// </summary>
        /// <param name="isEdit">Popup是否为编辑状态。</param>
        public void SetDisplay(bool isEdit)
        {
            _isEdit = isEdit;
            if (isEdit)
            {
                //Border border = (Border)Child;
                //border.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
                //border.BorderThickness = new(1);

                //Child.FindVisualChild<TextBox>().Visibility = Visibility.Visible;
                //Child.FindVisualChild<Button>().Visibility = Visibility.Visible;
                //Child.FindVisualChildByName<Border>("borderChar").Visibility = Visibility.Hidden;
                //Child.FindVisualChildByName<TextBlock>("tbkChar").Visibility = Visibility.Hidden;
                //_pop.IsHitTestVisible = true;

                _bdOuter.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
                _bdOuter.BorderThickness = new(1);
                _txtInput.Visibility = Visibility.Visible;
                _btnDelete.Visibility = Visibility.Visible;
                _bdChar.Visibility = Visibility.Hidden;
                _tbkDisplayChar.Visibility = Visibility.Hidden;
            }
            else
            {
                //Border border = (Border)Child;
                //border.Background = Brushes.Transparent;
                //border.BorderThickness = new(0);

                //Child.FindVisualChild<TextBox>().Visibility = Visibility.Hidden;
                //Child.FindVisualChild<Button>().Visibility = Visibility.Hidden;
                //Child.FindVisualChildByName<Border>("borderChar").Visibility = Visibility.Visible;
                //Child.FindVisualChildByName<TextBlock>("tbkChar").Visibility = Visibility.Visible;
                //_pop.IsHitTestVisible = false;

                _bdOuter.Background = Brushes.Transparent;
                _bdOuter.BorderThickness = new(0);
                _txtInput.Visibility = Visibility.Hidden;
                _btnDelete.Visibility = Visibility.Hidden;
                _bdChar.Visibility = Visibility.Visible;
                _tbkDisplayChar.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 设置Popup是否显示。
        /// </summary>
        /// <param name="isOpen">Popup是否可见。</param>
        public void SetVisible(bool isOpen)
        {
            IsOpen = isOpen;
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
