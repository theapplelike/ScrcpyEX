using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 拖拽移动类。
    /// </summary>
    internal class MyDragMove
    {
        private readonly Popup _pop;
        private Point _lastPoint;           //上一次的鼠标坐标
        private Point _nowPoint;            //当前鼠标的坐标

        private bool _isDragging = false;   //是否处于可拖拽的状态

        /// <summary>
        /// 设置指定的Popup可以进行拖拽。
        /// </summary>
        /// <param name="pop">要添加拖拽功能的Popup。</param>
        /// <param name="isLeaveParent">是否可以移出逻辑父级。</param>
        public MyDragMove(Popup pop, bool isLeaveParent = false)
        {
            _pop = pop;
            _pop.MouseDown += (s, e) => Pop_MouseDown();
            _pop.MouseMove += (s, e) => Pop_MouseMove(isLeaveParent);
            _pop.MouseUp += (s, e) => Pop_MouseUp();

            TextBox txt = MyGM.FindVisualChild<TextBox>(pop.Child);
            txt.PreviewTextInput += TextBox_PreviewTextInput;
            txt.TextChanged += TextBox_TextChanged;

            Button btn = MyGM.FindVisualChild<Button>(pop.Child);
            btn.Click += Button_Click;
        }

        /// <summary>
        /// 鼠标按下时发生的事件，允许移动且记录当前坐标。
        /// 嵌套顺序应该为Popup->Border(提供边框)->TextBox(提供输入框)、TextBlock(提供定位点)。
        /// </summary>
        private void Pop_MouseDown()
        {
            //让Popup中的TextBlock捕获鼠标，防止移动太快时丢失Popup
            TextBlock tbk = MyGM.FindVisualChild<TextBlock>(_pop.Child);
            _isDragging = Mouse.Capture(tbk);

            //记录当前坐标为上一次的坐标
            _lastPoint = MyGM.GetPosition();

            Console.WriteLine($"popup中心点：（{_pop.HorizontalOffset + 25 - 5}，{_pop.VerticalOffset + 25 - 35}）。");
            Console.WriteLine($"当前鼠标坐标：（{_lastPoint.X},{_lastPoint.Y}）,{_isDragging}.");
        }

        /// <summary>
        /// 鼠标移动时发生的事件。
        /// </summary>
        /// <param name="isLeaveParent">是否可以移出逻辑父级。</param>
        private void Pop_MouseMove(bool isLeaveParent)
        {
            if (_isDragging)
            {
                _nowPoint = MyGM.GetPosition();//现在的坐标

                double newX = _pop.HorizontalOffset + (_nowPoint.X - _lastPoint.X) / MV.MainWin.DpiScaleX;
                double newY = _pop.VerticalOffset + (_nowPoint.Y - _lastPoint.Y) / MV.MainWin.DpiScaleY;

                if (!isLeaveParent)//是否允许离开父控件
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
        private void Pop_MouseUp()
        {
            Mouse.Capture(null);
            _isDragging = false;
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
                    e.Handled = true;//如果已经有一个字符，阻止进一步输入
                }
                else
                {
                    if (!Regex.IsMatch(e.Text, "^[a-zA-Z0-9]{1}$"))
                    {
                        e.Handled = true;
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
        /// 删除当前的Popup。
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MV.TempPopups.Remove(_pop);
            _pop.IsOpen = false;
        }

        /// <summary>
        /// 将元素坐标限制在逻辑父级当中，
        /// </summary>
        /// <param name="newX">左边距＜newX＜左边距+父级宽度。</param>
        /// <param name="newY">上边距＜newY＜上边距+父级高度。</param>
        private void LimitToParent(ref double newX, ref double newY)
        {
            double left = MV.MainWin.ExtraWidthLeft;
            double maxLeft = MV.MainWin.ExtraWidthLeft + MV.MainWin.ScActualWidth;
            double top = MV.MainWin.ExtraHeightTop;
            double maxTop = MV.MainWin.ExtraHeightTop + MV.MainWin.ScActualHeight;

            if (newX < left)
            {
                newX = left;
            }
            else if (newX + _pop.Width > maxLeft)
            {
                newX = maxLeft - _pop.Width;
            }
            if (newY < top)
            {
                newY = top;
            }
            else if (newY + _pop.Height > maxTop)
            {
                newY = maxTop - _pop.Height;
            }
        }


    }
}
