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
        /// 说明文本。
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
        /// 封装进度环控件。
        /// </summary>
        public ProgressRing()
        {
            InitializeComponent();
        }


    }
}
