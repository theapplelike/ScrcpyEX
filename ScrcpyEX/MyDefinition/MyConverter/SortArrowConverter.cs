using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ScrcpyEX.MyDefinition.MyConverter
{
    /// <summary>
    /// 在排序列的右边显示一个箭头表示升序、降序、无序。
    /// </summary>
    internal class SortArrowConverter : IMultiValueConverter
    {
        private static readonly Geometry _ascGeometry = Geometry.Parse("M0,4 L4,0 L8,4 Z");
        private static readonly Geometry _descGeometry = Geometry.Parse("M0,0 L4,4 L8,0 Z");

        /// <summary>
        /// 根据条件返回表示表示顺序的箭头。
        /// </summary>
        /// <param name="values">
        /// <para>
        /// values[0]: 当前列的 Tag (string)。
        /// </para>
        /// <para>
        /// values[1]: ViewModel.SortColumn 属性，当前排序的列。
        /// </para>
        /// <para>
        /// values[2]: ViewModel.SortDirection 属性，当前的排序方向。
        /// </para>
        /// </param>
        /// <returns>一个Geometry类型的几何图形，不需要排序则返回null。</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string columnName = values[0] as string;
            string SortColumn = values[1] as string;
            ListSortDirection? direction = values[2] as ListSortDirection?;

            if (string.IsNullOrEmpty(columnName) || columnName != SortColumn || !direction.HasValue)
            {
                return null; // 不显示箭头
            }

            return direction == ListSortDirection.Ascending ? _ascGeometry : _descGeometry;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
