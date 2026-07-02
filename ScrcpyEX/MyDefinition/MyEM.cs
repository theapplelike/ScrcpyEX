using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ScrcpyEX.MyDefinition
{
    /// <summary>
    /// 自定义扩展方法（Extension Method）类。
    /// </summary>
    internal static class MyEM
    {
        /// <summary>
        /// 在指定元素的Visual树中查找类型匹配的子元素，Popup要使用Child属性调用此方法。
        /// </summary>
        /// <typeparam name="T">要查找的子元素类型，比如Button。</typeparam>
        /// <param name="obj">指定的父级元素。</param>
        /// <returns>返回在指定的父级元素中查找到的第一个类型匹配的子元素，可以查找到孙级元素，如果没找到返回null。</returns>
        public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
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
        /// 在指定元素的Visual树中查找类型匹配且指定名称的子元素。
        /// </summary>
        /// <typeparam name="T">要查找的子元素类型，比如Button。</typeparam>
        /// <param name="obj">指定的父级元素。</param>
        /// <param name="name">要查找的子元素的名称。</param>
        /// <returns>返回在指定的父级元素中查找到的第一个类型匹配的子元素，可以查找到孙级元素，如果没找到返回null。</returns>
        public static T FindVisualChildByName<T>(this DependencyObject obj, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T t && (child as FrameworkElement)?.Name == name)
                {
                    return t;
                }

                T childOfChild = FindVisualChildByName<T>(child, name);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得元素的第一个指定父元素，未查找到时会继续向上查找祖级元素。
        /// </summary>
        /// <typeparam name="T">指定的父元素类型。</typeparam>
        /// <param name="obj">要查找父元素的元素。</param>
        /// <returns>返回查找到的第一个指定父元素，未查到时返回null。</returns>
        public static T FindVisualParent<T>(this DependencyObject obj) where T : FrameworkElement
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
        /// 根据分隔符拆分原始字符串，不会包含空数组元素。
        /// </summary>
        /// <param name="org">原始字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <returns>分隔后的字符串数组，不会包含空数组元素。</returns>
        public static string[] SplitReal(this string org, string separator)
        {
            return org.Split([separator], StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 根据分隔符拆分原始字符串，不会包含空数组元素。
        /// </summary>
        /// <param name="org">原始字符串。</param>
        /// <param name="separator">分隔符。</param>
        /// <returns>分隔后的字符串数组，不会包含空数组元素。</returns>
        public static string[] SplitReal(this string org, char separator)
        {
            return org.Split([separator], StringSplitOptions.RemoveEmptyEntries);
        }


    }
}
