using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ViewModel
{
    internal class SelectDeviceVM : NotificationObject
    {
        /// <summary>
        /// 说明设备信息的集合。
        /// </summary>
        public ObservableCollection<AdbDeviceInfo> DeviceInfos
        {
            get { return deviceInfos; }
            set
            {
                deviceInfos = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<AdbDeviceInfo> deviceInfos = [];


    }
}
