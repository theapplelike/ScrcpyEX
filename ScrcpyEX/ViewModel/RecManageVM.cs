using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ViewModel
{
    internal class RecManageVM : NotificationObject
    {
        /// <summary>
        /// 录制操作信息的集合。
        /// </summary>
        public List<RecInfo> RecInfos
        {
            get { return recInfos; }
            set
            {
                recInfos = value;
                RaisePropertyChanged();
            }
        }
        private List<RecInfo> recInfos = [new RecInfo() { Name="123"}];


    }
}
