using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScrcpyEX.ViewModel
{
    internal class RecManageVM : NotificationObject
    {
        public RecManageVM()
        {
            //RecInfos = new(MyCfg.Cfg.RecInfos);
            //foreach (RecInfo recInfo in MyCfg.Cfg.RecInfos)
            //{
            //    Console.WriteLine($"///////////，{recInfo.Name}。");
            //    foreach (PointInfo info in recInfo.Infos)
            //    {
            //        Console.WriteLine($"{info.Pt.X},{info.Pt.Y},{info.LastTime}。");
            //    }
            //}
        }

        /// <summary>
        /// 录制操作信息的集合。
        /// </summary>
        public ObservableCollection<RecInfo> RecInfos
        {
            get { return recInfos; }
            set
            {
                recInfos = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<RecInfo> recInfos = [new() { Name="123"}, new() { Name = "223" }];

        /// <summary>
        /// 坐标点的集合。
        /// </summary>
        public ObservableCollection<PointInfo> PtInfos
        {
            get { return ptInfos; }
            set
            {
                ptInfos = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<PointInfo> ptInfos;

        public ICommand DelRecCommand => new RelayCommand(DelRec);
        public ICommand SetParamCommand => new RelayCommand(SetParam);

        /// <summary>
        /// 删除指定记录。
        /// </summary>
        /// <param name="parameter">指定的记录对象。</param>
        private void DelRec(object parameter)
        {
            Console.WriteLine($"类型：{parameter.GetType()}。");
            if (parameter is RecInfo info)
            {
                recInfos.Remove(info);
            }
        }

        /// <summary>
        /// 设置参数。
        /// </summary>
        /// <param name="parameter">坐标点的集合。</param>
        private void SetParam(object parameter)
        {
            Console.WriteLine($"类型：{parameter.GetType()}。");
            if (parameter is RecInfo info)
            {
                PtInfos = new(info.Infos);
            }
        }

        /// <summary>
        /// 保存数据。
        /// </summary>
        public void SaveData()
        {
            MyCfg.Cfg.RecInfos = new(RecInfos);
            MyCfg.Save();
        }


    }
}
