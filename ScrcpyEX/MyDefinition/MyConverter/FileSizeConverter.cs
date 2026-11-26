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
        /// <param name="value">文件的原始大小，单位Bit。Binding.Path 指向的那个值。</param>
        /// <param name="targetType">目标属性的类型，即你想返回的值最终要赋给什么类型。</param>
        /// <param name="parameter">full--结果附上精确字节数；emptyDir--目录返回空字符串。</param>
        /// <param name="culture">区域信息，用于格式化。</param>
        /// <returns>合适的单位大小。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not long size)
            {
                if (parameter is string s1 && s1 == "emptyDir") return "";
                else return "非文件类型";
            }
            if (size == 0) return "0 B";

            int order = 0;
            double len = size;
            while (len >= 1024 && order < _units.Length - 1)
            {
                order++;
                len /= 1024;
            }
            string human = $"{len:0.##} {_units[order]}";

            // 传入 "full" 时，附上精确字节数
            if (parameter is string s && s == "full") return $"{human} ({size:N0} 字节)";
            return human;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
