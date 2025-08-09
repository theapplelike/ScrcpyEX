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
            get => _resolutionDesc;
            set => SetProperty(ref _resolutionDesc, value);
        }
        private string _resolutionDesc = "未进行投屏";

        /// <summary>
        /// 脚本的循环次数。
        /// </summary>
        public int CycleNum
        {
            get => _cycleNum;
            set => SetProperty(ref _cycleNum, value);
        }
        private int _cycleNum = 0;

        /// <summary>
        /// 单次脚本执行后已产生的时间，单位是秒。
        /// </summary>
        public int Running
        {
            get => _running;
            set => SetProperty(ref _running, value);
        }
        private int _running = 0;


    }
}
