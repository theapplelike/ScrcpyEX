using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ScrcpyEX.ViewModel
{
    internal class RecManageVM : NotificationObject
    {
        /// <summary>
        /// 说明当前正在编辑的坐标点实体类的索引。
        /// </summary>
        private int _index = -1;

        public RecManageVM()
        {
            PointDt.Columns.Add("LastTime", typeof(double));
            PointDt.Columns.Add("XAxis", typeof(double));
            PointDt.Columns.Add("YAxis", typeof(double));
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
        private ObservableCollection<RecInfo> recInfos = [new() { Name="1"}, new() { Name = "2" }];

        /// <summary>
        /// 坐标点的集合。
        /// </summary>
        public DataTable PointDt
        {
            get { return pointDt; }
            set
            {
                pointDt = value;
                RaisePropertyChanged();
            }
        }
        private DataTable pointDt = new();

        public ICommand DelRecCommand => new RelayCommand(DelRec);

        /// <summary>
        /// 删除指定记录。
        /// </summary>
        /// <param name="parameter">指定的记录对象。</param>
        private void DelRec(object parameter)
        {
            if (parameter is RecInfo info)
            {
                MessageBoxResult result = MessageBox.Show($"是否删除“{info.Name}”脚本？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                {
                    RecInfos.Remove(info);
                }
            }
        }

        /// <summary>
        /// 显示坐标点参数到DataTable中。
        /// </summary>
        /// <param name="parameter">当前选中的脚本。</param>
        public void SetParam(RecInfo info)
        {
            _index = RecInfos.IndexOf(info);
            CopyToDt(info.Infos);
        }

        /// <summary>
        /// 保存修改的坐标点和脚本数据。
        /// </summary>
        public void Save(RecInfo recInfo)
        {
            if (_index != -1)
            {
                CopyToClass(PointDt, recInfo);
            }
        }

        /// <summary>
        /// 取消修改的PointInfo类数据。
        /// </summary>
        public void Canel()
        {
            if (_index != -1)
            {
                CopyToDt(RecInfos[_index].Infos);
            }
        }

        /// <summary>
        /// 将实体类的值复制给DataTable。
        /// </summary>
        /// <param name="infos">坐标点信息列表。</param>
        private void CopyToDt(List<PointInfo> infos)
        {
            PointDt.Clear();
            foreach (PointInfo point in infos)
            {
                PointDt.Rows.Add(point.LastTime, point.Pt.X, point.Pt.Y);
            }
        }

        /// <summary>
        /// 将DataTable的值复制给实体类。
        /// </summary>
        /// <param name="dt">代表坐标点集合的DataTable。</param>
        /// <param name="recInfo">界面上的关于录制操作的信息。</param>
        private void CopyToClass(DataTable dt, RecInfo recInfo)
        {
            List<PointInfo> tempInfo = [];
            foreach (DataRow dr in dt.Rows)
            {
                double r1 = double.Parse(dr["XAxis"].ToString());
                double r2 = double.Parse(dr["YAxis"].ToString());
                double r3 = double.Parse(dr["LastTime"].ToString());

                tempInfo.Add(new PointInfo(new Point(r1, r2), r3));
            }
            RecInfos[_index].Infos = tempInfo;
            RecInfos[_index].Limited = recInfo.Limited;
            RecInfos[_index].HandStop = recInfo.HandStop;
            RecInfos[_index].CycleNumber = recInfo.CycleNumber;
            RecInfos[_index].CycleInterval = recInfo.CycleInterval;
        }

        /// <summary>
        /// 保存被修改的数据。
        /// </summary>
        public void SaveData()
        {
            MyCfg.Cfg.RecInfos = new(RecInfos);
            MyCfg.Save();
        }


    }
}
