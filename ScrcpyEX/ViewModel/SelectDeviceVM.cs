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
        public ObservableCollection<string> Infos
        {
            get { return infos; }
            set
            {
                infos = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<string> infos = ["1", "2"];

        /// <summary>
        /// 按钮显示的图标。
        /// </summary>
        public string BtnChar
        {
            get { return btnChar; }
            set
            {
                btnChar = value;
                RaisePropertyChanged();
            }
        }
        private string btnChar = "\uecf0";


    }
}
