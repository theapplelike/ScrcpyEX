using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ScrcpyEX.MyDefinition.MyConverter
{
    /// <summary>
    /// 目录中每一项的字体图标。
    /// </summary>
    internal class FileToIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //如果是文件夹直接返回图标
            if (values[0] is bool v && v) return "\uf12b";

            //非文件夹根据扩展名返回图标
            return (values[1] as string ?? string.Empty).ToLower() switch
            {
                ".jpg" or ".jpeg" or ".png" => "\ueb9f",
                ".zip" or ".rar" or ".7z" => "\uf012",
                ".mp3" or ".wav" => "\ue8d6",
                ".mp4" or ".avi" => "\ue714",
                ".txt" => "\uf000",
                ".pdf" => "\uea90",
                ".gif" => "\uf4a9",
                _ => "\uf56e"
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
