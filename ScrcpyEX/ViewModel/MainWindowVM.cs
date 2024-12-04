using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ViewModel
{
    internal class MainWindowVM : NotificationObject
    {
        /// <summary>
        /// 说明投屏的分辨率、比例。
        /// </summary>
        public string ResolutionDesc
        {
            get { return resolutionDesc; }
            set
            {
                resolutionDesc = value;
                RaisePropertyChanged();
            }
        }
        private string resolutionDesc = "未进行投屏";


    }
}
