using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// ProgressRing.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressRing : UserControl
    {
        /// <summary>
        /// 进度环的类型，1--三点，2-三边，3-风扇。
        /// </summary>
        public int RingType
        {
            get => (int)GetValue(RingTypeProperty);
            set => SetValue(RingTypeProperty, value);
        }
        public static readonly DependencyProperty RingTypeProperty = DependencyProperty.Register(
            nameof(RingType), 
            typeof(int), 
            typeof(ProgressRing), 
            new PropertyMetadata(1, (d, e) =>
            {
                ProgressRing ctrl = (ProgressRing)d;
                switch ((int)e.NewValue)
                {
                    case 1: ctrl.elp.StrokeDashArray = [2, 8]; ctrl.elp.StrokeThickness = 4; break;
                    case 2: ctrl.elp.StrokeDashArray = [4, 6]; ctrl.elp.StrokeThickness = 4; break;
                    case 3: ctrl.elp.StrokeDashArray = [1, 1]; ctrl.elp.StrokeThickness = 10; break;
                }
            })
        );

        /// <summary>
        /// 主要说明文本。
        /// </summary>
        public string Desc
        {
            get => (string)GetValue(DescProperty);
            set => SetValue(DescProperty, value);
        }
        public static readonly DependencyProperty DescProperty = DependencyProperty.Register(
            nameof(Desc), typeof(string), typeof(ProgressRing),
            new PropertyMetadata("加载中...", (d, e) => ((ProgressRing)d).tbkDesc.Text = (string)e.NewValue));

        /// <summary>
        /// 消息队列。
        /// </summary>
        private readonly Queue<string> _msg = new();

        /// <summary>
        /// 旋转动画的引用。
        /// </summary>
        private Storyboard _anim = null;

        /// <summary>
        /// 封装进度环控件。
        /// </summary>
        public ProgressRing()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _anim = (Storyboard)elp.FindResource("RotateAnim");
            if (Visibility == Visibility.Visible)
            {
                StartSpin();
            }
        }

        /// <summary>
        /// 整个控件显示时开始动画，隐藏时停止动画。
        /// </summary>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_anim == null) return; //尚未加载
            if ((bool)e.NewValue)
            {
                StartSpin();
            }
            else
            {
                StopSpin();
            }
        }

        /// <summary>
        /// 增加一条次要说明消息，上限为3。
        /// </summary>
        /// <param name="msg">说明消息。</param>
        public void AddMsg(string msg)
        {
            _msg.Enqueue(msg);
            if (_msg.Count > 3) _msg.Dequeue();

            string[] ary = [.. _msg];
            tbkTxt1.Text = ary.Length > 0 ? ary[0] : null;
            tbkTxt2.Text = ary.Length > 1 ? ary[1] : null;
            tbkTxt3.Text = ary.Length > 2 ? ary[2] : null;
        }

        /// <summary>
        /// 使用主要消息初始化控件，会清空次要消息队列。
        /// </summary>
        /// <param name="desc">主要说明文本。</param>
        public void Init(string desc)
        {
            _msg.Clear();
            tbkDesc.Text = desc;
            tbkTxt1.Text = "";
            tbkTxt2.Text = "";
            tbkTxt3.Text = "";
        }

        /// <summary>
        /// 开始旋转动画。
        /// </summary>
        public void StartSpin() => _anim?.Begin(elp, true);

        /// <summary>
        /// 停止旋转动画。
        /// </summary>
        public void StopSpin() => _anim?.Stop(elp);


    }
}
