using ScrcpyEX.ExtensionMethod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ScrcpyEX.MyDefinition.MyConverter
{
    /// <summary>
    /// 文件大小转化为合适的单位大小。
    /// </summary>
    internal class FileSizeConverter : IValueConverter
    {
        private static readonly string[] _units = ["B", "KB", "MB", "GB", "TB"];

        /// <summary>
        /// 文件大小转化为合适的单位大小。
        /// </summary>
        /// <param name="value">文件的原始大小，单位Bit。</param>
        /// <returns>合适的单位大小。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size)
            {
                if (size == 0) return "0 B";

                int order = 0;
                double len = size;
                while (len >= 1024 && order < _units.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {_units[order]}";
            }

            return ""; // 目录没有大小
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
