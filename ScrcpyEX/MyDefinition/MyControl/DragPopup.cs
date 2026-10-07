using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
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
        /// 当前Popup对应的按键信息映射类。
        /// </summary>
        private readonly KeyMapping _mapping;

        /// <summary>
        /// Popup中控件的X轴缩放比例。
        /// </summary>
        private readonly double _rx;

        /// <summary>
        /// Popup中控件的Y轴缩放比例。
        /// </summary>
        private readonly double _ry;

        /// <summary>
        /// Popup初始是否为编辑状态。
        /// </summary>
        private readonly bool _isInitEdit;

        /// <summary>
        /// 是否可以移出逻辑父级。
        /// </summary>
        private readonly bool _isLeaveParent;

        /// <summary>
        /// 上一次的鼠标坐标。
        /// </summary>
        //private Point _lastPoint;

        /// <summary>
        /// 当前鼠标的坐标。
        /// </summary>
        //private Point _nowPoint;

        /// <summary>
        /// 鼠标按下时，鼠标位置相对 popup 左上角的偏移（在 PlacementTarget 坐标系中）。
        /// </summary>
        private Point _grabOffset;

        /// <summary>
        /// 是否处于可拖拽的状态。
        /// </summary>
        private bool _isDragging = false;

        /// <summary>
        /// 当前是否处于编辑状态。
        /// </summary>
        private bool _isEdit = false;

        /// <summary>
        /// 50%透明度的白色。
        /// </summary>
        private readonly Brush _argb50 = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));

        /// <summary>
        /// 每隔一段时间检测鼠标是否离开，鼠标如果离开显示Popup。
        /// </summary>
        private readonly DispatcherTimer _showTimer = new();

        /// <summary>
        /// 是否已释放资源。
        /// </summary>
        private bool _disposed = false;

        #region 吸附相关字段
        /// <summary>
        /// 吸附触发阈值（像素）。
        /// </summary>
        private const double SnapThreshold = 6.0;

        /// <summary>
        /// 吸附线所在图层（挂在 PlacementTarget 上）。
        /// </summary>
        private Canvas _snapLayer = null;

        /// <summary>
        /// 垂直吸附线。
        /// </summary>
        private Line _vSnapLine = null;

        /// <summary>
        /// 水平吸附线。
        /// </summary>
        private Line _hSnapLine = null;

        /// <summary>
        /// 吸附线颜色。
        /// </summary>
        private readonly Brush _snapBrush = Brushes.Blue;

        /// <summary>
        /// 吸附图层的标识（防止重复创建）。
        /// </summary>
        private const string SnapLayerTag = "DragPopup.SnapLayer";
        #endregion

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
        /// <param name="rx">其中控件的X轴缩放比例。</param>
        /// <param name="ry">其中控件的Y轴缩放比例。</param>
        /// <param name="isInitEdit">Popup初始是否为编辑状态。</param>
        /// <param name="isLeaveParent">是否可以移出逻辑父级。</param>
        public DragPopup(KeyMapping mapping, double rx, double ry, bool isInitEdit, bool isLeaveParent = false)
        {
            _mapping = mapping;
            _rx = rx;
            _ry = ry;
            _isInitEdit = isInitEdit;
            _isLeaveParent = isLeaveParent;

            VkCode = mapping.VkCode;
            Loaded += Popup_Loaded;
            MouseEnter += Popup_MouseEnter;
        }

        /// <summary>
        /// 控件加载完成时发生。
        /// </summary>
        private void Popup_Loaded(object sender, RoutedEventArgs e)
        {
            ControlInit(_rx, _ry, 0.3);
            ControlEventBinding();
            EnsureSnapLayer();
            SetEditingStatus(_isInitEdit);
        }

        /// <summary>
        /// DragPopup中包含的控件进行初始化。
        /// </summary>
        /// <param name="rx">Popup中控件的X轴缩放比例。</param>
        /// <param name="ry">Popup中控件的Y轴缩放比例。</param>
        /// <param name="limit">缩放比例的上下限。</param>
        private void ControlInit(double rx, double ry, double limit)
        {
            //提供最外层边框
            _bdOuter = new()
            {
                Background = _argb50,
                BorderBrush = Brushes.Black,
                BorderThickness = new(1),
                CornerRadius = new(Width),
            };

            //提供装多个控件的容器
            Grid grid = new();

            //提供输入
            _txtInput = new()
            {
                Text = _mapping.Char,
                Width = MyGM.RatioClamp(15 , rx, limit),
                FontSize = MyGM.RatioClamp(16, ry, limit),
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
                Width = MyGM.RatioClamp(14, rx, 0.5),
                Height = MyGM.RatioClamp(14, ry, 0.5),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new(0, -2, -2, 0),
                //Visibility = Visibility.Collapsed,
            };

            //显示映射按键字符的边框
            _bdChar = new()
            {
                Width = MyGM.RatioClamp(20, rx, limit),
                Height = MyGM.RatioClamp(20, ry, limit),
                Background = _argb50,
                BorderBrush = Brushes.Black,
                CornerRadius = new(20),
            };

            //显示映射按键的字符
            _tbkDisplayChar = new()
            {
                Text = _mapping.Char,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Black,
                FontSize = MyGM.RatioClamp(16, rx, limit),
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

            _showTimer.Tick += Pop_Tick;
            _showTimer.Interval = TimeSpan.FromSeconds(1);

            MouseDown += Pop_MouseDown;
            MouseUp += Pop_MouseUp;
            MouseMove += Pop_MouseMove;
        }

        /// <summary>
        /// 鼠标按下时发生的事件，处于编辑状态时允许移动且记录当前坐标。
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
                //_lastPoint = MyGM.GetPosnW();

                // 记录鼠标相对 popup 左上角的偏移
                Point mouseInTarget = Mouse.GetPosition(PlacementTarget);
                _grabOffset = new Point( mouseInTarget.X - HorizontalOffset, mouseInTarget.Y - VerticalOffset);
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
                HideAllSnapLines();
            }
        }
        
        /// <summary>
        /// 鼠标移动时发生的事件。
        /// </summary>
        private void Pop_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _isEdit)
            {
                //_nowPoint = MyGM.GetPosnW();//现在的坐标
                //double newX = HorizontalOffset + (_nowPoint.X - _lastPoint.X) / MV.DpiScaleX;
                //double newY = VerticalOffset + (_nowPoint.Y - _lastPoint.Y) / MV.DpiScaleY;

                // 直接拿到鼠标相对 PlacementTarget 的绝对坐标
                Point mouseInTarget = Mouse.GetPosition(PlacementTarget);

                // popup 目标位置 = 鼠标位置 − 抓取偏移
                double newX = mouseInTarget.X - _grabOffset.X;
                double newY = mouseInTarget.Y - _grabOffset.Y;

                if (!_isLeaveParent)//是否允许离开父控件
                {
                    LimitToParent(ref newX, ref newY);
                }

                ApplySnap(ref newX, ref newY);

                HorizontalOffset = newX;
                VerticalOffset = newY;
                //_lastPoint = _nowPoint;
            }
        }

        /// <summary>
        /// 键盘的输入会触发此事件，只允许输入一个字符，且只能为指定字符。
        /// </summary>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox txt)
            {
                if (txt.Text.Length >= 1)
                {
                    if (IsAllTextSelected(txt))
                    {
                        VkCode = CharToVirtualKey(e.Text[0]);
                        _tbkDisplayChar.Text = e.Text;
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
            if (sender is TextBox txt)
            {
                if (txt.Text.Length > 1)
                {
                    txt.Text = txt.Text.Substring(0, 1);//如果长度超过一，只保留第一个
                    txt.CaretIndex = 1;
                }
                else if (txt.Text.Length == 1)
                {
                    if (char.IsLower(txt.Text[0]))
                    {
                        txt.Text = txt.Text.ToUpper();
                        txt.CaretIndex = 1;
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
        /// <param name="ch">需要转化的字符。</param>
        /// <returns>Keys枚举。</returns>
        private Keys CharToVirtualKey(char ch)
        {
            short result = DLL.VkKeyScan(ch);

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
        /// 鼠标进入Popup时让其中的控件不可见，鼠标点击事件可以直接发给Scrcpy。
        /// 但Popup不能使用IsOpen=false隐藏，否则后续不能检测Popup内的控件。
        /// </summary>
        private void Popup_MouseEnter(object sender, MouseEventArgs e)
        {
            _bdChar.Background = Brushes.Transparent;
            _tbkDisplayChar.Visibility = Visibility.Collapsed;
            _showTimer.Start();
        }

        /// <summary>
        /// Popup定时检测鼠标位置，不和鼠标重合则显示。
        /// </summary>
        private void Pop_Tick(object sender, EventArgs e)
        {
            if (!IsMouseOverPopup(this))
            {
                _showTimer.Stop();

                _bdChar.Background = _argb50;
                _tbkDisplayChar.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 鼠标是否在Popup中指定控件的上方。
        /// </summary>
        /// <param name="popup">顶级Popup控件。</param>
        /// <returns>true--鼠标在控件上方；false--鼠标在其他地方。</returns>
        private bool IsMouseOverPopup(Popup popup)
        {
            if (!popup.IsOpen)
            {
                return false;
            }

            //获得指定子元素的屏幕坐标和尺寸来比对
            Point topLeft = _bdChar.PointToScreen(new Point(0, 0));
            Size size = new(_bdChar.ActualWidth * MV.DpiScaleX, _bdChar.ActualHeight * MV.DpiScaleY);
            Rect charRect = new(topLeft, size);

            return charRect.Contains(MyGM.GetPosnW());
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

        #region 吸附相关方法
        /// <summary>
        /// 在 PlacementTarget 上准备一个不会影响布局、不参与命中的 Canvas 作为吸附线图层。
        /// </summary>
        private void EnsureSnapLayer()
        {
            if (PlacementTarget is not Panel panel) return;

            _snapLayer = panel.Children.OfType<Canvas>().FirstOrDefault(c => Equals(c.Tag, SnapLayerTag));

            if (_snapLayer == null)
            {
                _snapLayer = new Canvas
                {
                    Tag = SnapLayerTag,
                    IsHitTestVisible = false,
                    Background = null,
                };
                // 放到最上层
                Panel.SetZIndex(_snapLayer, 5);
                panel.Children.Add(_snapLayer);
            }

            _vSnapLine = new Line
            {
                Stroke = _snapBrush,
                StrokeThickness = 1,
                SnapsToDevicePixels = true,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
            };
            _hSnapLine = new Line
            {
                Stroke = _snapBrush,
                StrokeThickness = 1,
                SnapsToDevicePixels = true,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
            };

            _snapLayer.Children.Add(_vSnapLine);
            _snapLayer.Children.Add(_hSnapLine);
        }

        /// <summary>
        /// 对给定的 popup 位置进行吸附修正，并显示/隐藏吸附线。
        /// </summary>
        private void ApplySnap(ref double newX, ref double newY)
        {
            // 根据左、右、上、下边缘分组
            List<double> leftTargets = [];
            List<double> rightTargets = [];
            List<double> topTargets = [];
            List<double> bottomTargets = [];

            foreach (DragPopup other in MV.DragPopups)
            {
                if (ReferenceEquals(other, this)) continue;

                double l = other.HorizontalOffset;
                double t = other.VerticalOffset;
                double w = other.Width;
                double h = other.Height;

                leftTargets.Add(l);          // 其他 popup 的左边缘
                rightTargets.Add(l + w);     // 其他 popup 的右边缘
                topTargets.Add(t);           // 其他 popup 的上边缘
                bottomTargets.Add(t + h);    // 其他 popup 的下边缘
            }

            TrySnapOnAxis(true, leftTargets, rightTargets, ref newX);
            TrySnapOnAxis(false, topTargets, bottomTargets, ref newY);
        }

        /// <summary>
        /// 计算是否需要吸附，吸附时改变当前方向的位置并显示吸附线。
        /// </summary>
        /// <param name="isXAxis">是否X轴方向。</param>
        /// <param name="targets1">起点边的目标数组。</param>
        /// <param name="targets2">终点边的目标数组。</param>
        /// <param name="cur">当前方向上的位置。</param>
        private void TrySnapOnAxis(bool isXAxis, List<double> targets1, List<double> targets2, ref double cur)
        {
            double bestDist = SnapThreshold;
            double bestTarget = double.NaN;
            double bestDelta = 0;
            double offset = isXAxis ? Width : Height;

            // 计算第一个边缘
            foreach (double tx in targets1)
            {
                double d = Math.Abs(cur - tx);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestTarget = tx;
                    bestDelta = tx - cur;
                }
            }

            // 计算第二个边缘
            foreach (double tx in targets2)
            {
                double d = Math.Abs(cur + offset - tx);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestTarget = tx;
                    bestDelta = tx - (cur + offset);
                }
            }

            if (bestDist < SnapThreshold)
            {
                cur += bestDelta;
                if (isXAxis) ShowVSnapLine(bestTarget);
                else ShowHSnapLine(bestTarget);
            }
            else
            {
                if (isXAxis) HideVSnapLine();
                else HideHSnapLine();
            }
        }

        /// <summary>
        /// 在指定X轴位置显示垂直吸附线。
        /// </summary>
        /// <param name="x">X轴位置。</param>
        private void ShowVSnapLine(double x)
        {
            double h = _snapLayer.ActualHeight > 0 ? _snapLayer.ActualHeight : _snapLayer.Height;
            _vSnapLine.X1 = x;
            _vSnapLine.X2 = x;
            _vSnapLine.Y1 = 0;
            _vSnapLine.Y2 = h;
            _vSnapLine.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 在指定Y轴位置显示水平吸附线。
        /// </summary>
        /// <param name="y">Y轴位置。</param>
        private void ShowHSnapLine(double y)
        {
            double w = _snapLayer.ActualWidth > 0 ? _snapLayer.ActualWidth : _snapLayer.Width;
            _hSnapLine.X1 = 0;
            _hSnapLine.X2 = w;
            _hSnapLine.Y1 = y;
            _hSnapLine.Y2 = y;
            _hSnapLine.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 隐藏垂直吸附线。
        /// </summary>
        private void HideVSnapLine() => _vSnapLine.Visibility = Visibility.Collapsed;

        /// <summary>
        /// 隐藏水平吸附线。
        /// </summary>
        private void HideHSnapLine() => _hSnapLine.Visibility = Visibility.Collapsed;

        /// <summary>
        /// 隐藏所有吸附线。
        /// </summary>
        private void HideAllSnapLines()
        {
            HideVSnapLine();
            HideHSnapLine();
        }
        #endregion

        /// <summary>
        /// 设置Popup的编辑状态。
        /// </summary>
        /// <param name="isEdit">Popup是否为编辑状态。</param>
        public void SetEditingStatus(bool isEdit)
        {
            _isEdit = isEdit;
            if (isEdit)
            {
                _bdOuter.Background = _argb50;
                _bdOuter.BorderThickness = new(1);
                _txtInput.Visibility = Visibility.Visible;
                _btnDelete.Visibility = Visibility.Visible;
                _bdChar.Visibility = Visibility.Collapsed;
                _tbkDisplayChar.Visibility = Visibility.Collapsed;
            }
            else
            {
                _bdOuter.Background = Brushes.Transparent;
                _bdOuter.BorderThickness = new(0);
                _txtInput.Visibility = Visibility.Collapsed;
                _btnDelete.Visibility = Visibility.Collapsed;
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
                _showTimer.Tick -= Pop_Tick;

                Loaded -= Popup_Loaded;
                MouseEnter -= Popup_MouseEnter;
                MouseDown -= Pop_MouseDown;
                MouseMove -= Pop_MouseMove;
                MouseUp -= Pop_MouseUp;
            }
            
            _disposed = true;
        }

        ~DragPopup() => Dispose(false);


    }
}
