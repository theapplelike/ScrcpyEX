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


        /// <summary>
        /// 脚本的循环次数。
        /// </summary>
        public int CycleNum
        {
            get { return cycleNum; }
            set
            {
                cycleNum = value;
                RaisePropertyChanged();
            }
        }
        private int cycleNum = 0;

        /// <summary>
        /// 脚本执行后已产生的间隔。
        /// </summary>
        public int Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                RaisePropertyChanged();
            }
        }
        private int interval = 0;


    }
}
