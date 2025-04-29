using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ScrcpyEX.ExtensionMethod;
using System.Windows.Media;
using System.Windows;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义通用方法（Generic Method）类。
    /// </summary>
    internal class MyGM
    {
        /// <summary>
        /// 在鼠标位置显示Popup提示，重复执行时可以显示到最新位置。
        /// </summary>
        /// <param name="text">要显示的文本。</param>
        /// <param name="time">Popup控件显示的时间，单位s。</param>
        public static void POP(string text, int time = 3)
        {
            MV.Pop.Show(text, time);
        }

        /// <summary>
        /// 在指定元素的Visual树中查找类型匹配的子元素。
        /// </summary>
        /// <typeparam name="T">要查找的子元素类型，比如Button。</typeparam>
        /// <param name="obj">指定的父级元素。</param>
        /// <returns>返回在指定的父级元素中查找到的第一个类型匹配的子元素，可以查找到孙级元素，如果没找到返回null。</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t)
                {
                    return t;
                }

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 指定元素下Tag属性相同的Button设置一样的背景色。
        /// </summary>
        /// <param name="obj">指定的父级元素。</param>
        /// <param name="brush">要设置的颜色。</param>
        /// <param name="tag">指定的tag值。</param>
        public static void SetSameColor(DependencyObject obj, Brush brush, object tag)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is Button btn && btn.Tag != null)
                {
                    if (btn.Tag.ToString() == tag.ToString())
                    {
                        btn.Background = brush;
                    }
                }
            }
        }

        /// <summary>
        /// 获得元素的第一个指定父元素，未查找到时会继续向上查找祖级元素。
        /// </summary>
        /// <typeparam name="T">指定的父元素类型。</typeparam>
        /// <param name="obj">要查找父元素的元素。</param>
        /// <returns>返回查找到的第一个指定父元素，未查到时返回null。</returns>
        public static T GetParentObject<T>(DependencyObject obj) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T t)
                {
                    return t;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// 获得光标的当前坐标，以屏幕为基准，且此值已乘以了Dpi比例。
        /// </summary>
        /// <returns>一个System.Windows.Point类型的值。</returns>
        public static Point GetPosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Cursor.Position;
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// 如果min≤value≤max，则为value。value＜min，则为min。max＜value，则为max。
        /// </summary>
        /// <param name="value">要判定范围的值。</param>
        /// <param name="min">结果的下限。</param>
        /// <param name="max">结果的上限。</param>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }


    }
}
