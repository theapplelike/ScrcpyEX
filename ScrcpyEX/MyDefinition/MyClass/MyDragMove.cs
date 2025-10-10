using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Keys = System.Windows.Forms.Keys;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 拖拽移动类，用于键盘映射的Popup。
    /// </summary>
    internal class MyDragMove : IDisposable
    {
        private readonly Popup _pop;
        private readonly Button _btn;
        private readonly TextBox _txt;
        private readonly bool _isLeaveParent;//是否可以移出逻辑父级

        private Point _lastPoint;           //上一次的鼠标坐标
        private Point _nowPoint;            //当前鼠标的坐标
        private bool _isDragging = false;   //是否处于可拖拽的状态
        private bool _isEdit = false;       //当前是否处于编辑状态
        private IntPtr _lp = IntPtr.Zero;   //记录位置信息的句柄
        private bool _disposed = false;     //是否释放资源

        /// <summary>
        /// 当前映射绑定的字符。
        /// </summary>
        public string Char => _txt.Text;

        /// <summary>
        /// 当前映射绑定的虚拟键码。
        /// </summary>
        public Keys VkCode { get; set; } = Keys.None;

        /// <summary>
        /// 当前映射的X轴偏移。
        /// </summary>
        public double OffsetX => _pop.HorizontalOffset;

        /// <summary>
        /// 当前映射的Y轴偏移。
        /// </summary>
        public double OffsetY => _pop.VerticalOffset;

        /// <summary>
        /// 设置指定的Popup可以进行拖拽。
        /// </summary>
        /// <param name="pop">要添加拖拽功能的Popup。</param>
        /// <param name="isLeaveParent">是否可以移出逻辑父级。</param>
        public MyDragMove(Popup pop, Keys vkCode, bool popIsOpen, bool isLeaveParent = false)
        {
            VkCode = vkCode;

            _pop = pop;
            _isLeaveParent = isLeaveParent;

            _txt = pop.Child.FindVisualChild<TextBox>();
            _txt.PreviewTextInput += TextBox_PreviewTextInput;
            _txt.TextChanged += TextBox_TextChanged;
            _txt.SetValue(InputMethod.IsInputMethodEnabledProperty, false);//文本框禁用输入法

            _btn = pop.Child.FindVisualChild<Button>();
            _btn.Click += Button_Click;

            _pop.MouseDown += Pop_MouseDown;
            _pop.MouseMove += Pop_MouseMove;
            _pop.MouseUp += Pop_MouseUp;

            SetDisplay(popIsOpen);
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
                TextBlock tbk = _pop.Child.FindVisualChildByName<TextBlock>("tbk");
                _isDragging = Mouse.Capture(tbk);
                GotFocus(_txt);

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
            double radius = 50 / 2;//根据现在的比例计算半径，还需要乘以Scrcpy当前宽度和默认宽度的比值，这里默认这个比值为1，这个值仅用于输出调试内容，所以没有传入
            Console.WriteLine($"popup中心点：（{_pop.HorizontalOffset + radius}，{_pop.VerticalOffset + radius}），左上角：（{_pop.HorizontalOffset}，{_pop.VerticalOffset}）。");
            Console.WriteLine($"记录的最后鼠标坐标：（{_lastPoint.X},{_lastPoint.Y}）,{_isDragging}.");
        }
        
        /// <summary>
        /// 鼠标移动时发生的事件。
        /// </summary>
        private void Pop_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _isEdit)
            {
                _nowPoint = MyGM.GetPosition();//现在的坐标

                double newX = _pop.HorizontalOffset + (_nowPoint.X - _lastPoint.X) / MV.MainWin.DpiScaleX;
                double newY = _pop.VerticalOffset + (_nowPoint.Y - _lastPoint.Y) / MV.MainWin.DpiScaleY;

                if (!_isLeaveParent)//是否允许离开父控件
                {
                    LimitToParent(ref newX, ref newY);
                }

                //Console.WriteLine($"##:{newX}+{_pop.Height}<>{_parentRect.Width}。");
                //Console.WriteLine($"上次坐标：（{_lastPoint.X}，{_lastPoint.Y}），" +
                //    $"当前坐标：（{_nowPoint.X}，{_nowPoint.Y}），" +
                //    $"旧位置：（{newX}，{newY}），" +
                //    $"新位置：（{_pop.HorizontalOffset}，{_pop.VerticalOffset}），");

                _pop.HorizontalOffset = newX;
                _pop.VerticalOffset = newY;
                _lastPoint = _nowPoint;
            }
        }

        /// <summary>
        /// 鼠标抬起发生的事件，不允许移动，取消元素对鼠标的捕获。
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
        /// 键盘的输入会触发此事件，只允许输入一个字符，且只能为字母数字。
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
                        //Console.WriteLine($"转化为：{CharToVirtualKey(e.Text[0])}");
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
                        Console.WriteLine($"转化为：{CharToVirtualKey(e.Text[0])}");
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
        private void GotFocus(TextBox txt)
        {
            txt.Focus();
            txt.SelectAll();
        }

        /// <summary>
        /// 检查文本框是否选中了所有的文本。
        /// </summary>
        /// <param name="txt">指定的文本框。</param>
        /// <returns>true--选中了所有文本；false--未选中所有文本，文本框中没有任何内容时始终返回false。</returns>
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MV.DragMoves.Remove(this);
            _pop.IsOpen = false;
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
            else if (newX + _pop.Width > maxLeft)
            {
                newX = maxLeft - _pop.Width;
            }
            if (newY < 0)
            {
                newY = 0;
            }
            else if (newY + _pop.Height > maxTop)
            {
                newY = maxTop - _pop.Height;
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
                Border border = (Border)_pop.Child;
                border.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
                border.BorderThickness = new(1);

                _pop.Child.FindVisualChild<TextBox>().Visibility = Visibility.Visible;
                _pop.Child.FindVisualChild<Button>().Visibility = Visibility.Visible;
                _pop.Child.FindVisualChildByName<Border>("borderChar").Visibility = Visibility.Hidden;
                _pop.Child.FindVisualChildByName<TextBlock>("tbkChar").Visibility = Visibility.Hidden;
                //_pop.IsHitTestVisible = true;
            }
            else
            {
                Border border = (Border)_pop.Child;
                border.Background = Brushes.Transparent;
                border.BorderThickness = new(0);

                _pop.Child.FindVisualChild<TextBox>().Visibility = Visibility.Hidden;
                _pop.Child.FindVisualChild<Button>().Visibility = Visibility.Hidden;
                _pop.Child.FindVisualChildByName<Border>("borderChar").Visibility = Visibility.Visible;
                _pop.Child.FindVisualChildByName<TextBlock>("tbkChar").Visibility = Visibility.Visible;
                //_pop.IsHitTestVisible = false;
            }
        }

        /// <summary>
        /// 设置Popup是否显示。
        /// </summary>
        /// <param name="isOpen">Popup是否可见。</param>
        public void SetVisible(bool isOpen)
        {
            _pop.IsOpen = isOpen;
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
                _txt.PreviewTextInput -= TextBox_PreviewTextInput;
                _txt.TextChanged -= TextBox_TextChanged;

                _btn.Click -= Button_Click;

                _pop.MouseDown -= Pop_MouseDown;
                _pop.MouseMove -= Pop_MouseMove;
                _pop.MouseUp -= Pop_MouseUp;
            }

            _disposed = true;
        }

        ~MyDragMove() => Dispose(false);


    }
}
