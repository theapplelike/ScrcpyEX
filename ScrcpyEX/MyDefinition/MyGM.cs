using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
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
        public static void POP(string text)
        {
            (MV.Pop.Child as TextBlock).Text = text;
            
            MV.Pop.IsOpen = false;
            MV.Pop.IsOpen = true;
            MV.PopTimer.Start();
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


    }
}
