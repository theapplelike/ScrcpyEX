using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ScrcpyEX.MyDefinition.MyControl
{
    /// <summary>
    /// 右下角带有角标的按钮，鼠标悬停时显示角标。
    /// </summary>
    internal class IconButton : Button
    {
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        /// <summary>
        /// 主图标文本依赖属性。
        /// </summary>
        public static readonly DependencyProperty MainIconProperty =
            DependencyProperty.Register(nameof(MainIcon), typeof(string), typeof(IconButton));

        /// <summary>
        /// 角标文本依赖属性。
        /// </summary>
        public static readonly DependencyProperty CornerIconProperty =
            DependencyProperty.Register(nameof(CornerIcon), typeof(string), typeof(IconButton));

        /// <summary>
        /// 主要图标。
        /// </summary>
        public string MainIcon
        {
            get => (string)GetValue(MainIconProperty);
            set => SetValue(MainIconProperty, value);
        }

        /// <summary>
        /// 右下角的角标。
        /// </summary>
        public string CornerIcon
        {
            get => (string)GetValue(CornerIconProperty);
            set => SetValue(CornerIconProperty, value);
        }
    }
}
