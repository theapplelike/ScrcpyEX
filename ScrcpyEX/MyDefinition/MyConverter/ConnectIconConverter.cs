using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Data;

namespace ScrcpyEX.MyDefinition.MyConverter
{
    /// <summary>
    /// 根据ADB连接方式的不同显示不同的图标。
    /// </summary>
    internal class ConnectIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (MyGM.ExtractIpAddress(value.ToString()) != "")
            {
                return "\uee77";
            }
            return "\uecf0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
