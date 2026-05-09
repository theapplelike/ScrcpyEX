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
        public string Resolution
        {
            get => _resolution;
            set => SetProperty(ref _resolution, value);
        }
        private string _resolution = "未进行投屏";

        /// <summary>
        /// 说明程序当前的缩放比例。
        /// </summary>
        public string Scaling
        {
            get => _scaling;
            set => SetProperty(ref _scaling, value);
        }
        private string _scaling = "未进行投屏";

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

        /// <summary>
        /// 用于图像脚本计时器的自适应间隔，单位是毫秒，最小为5000豪秒。
        /// </summary>
        public double ScriptInterval
        {
            get { return _scriptInterval; }
            set
            {
                SetProperty(ref _scriptInterval, value < 5000 ? 5000 : value);
            }
        }
        private double _scriptInterval = 5000;


    }
}
