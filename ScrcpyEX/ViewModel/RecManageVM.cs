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
        /// 临时存储正在编辑的坐标点信息列表。
        /// </summary>
        private List<PointInfo> _tempInfo = [];

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
        private ObservableCollection<RecInfo> recInfos = [new() { Name="123"}, new() { Name = "223" }];

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
        public ICommand SetParamCommand => new RelayCommand(SetParam);
        public ICommand SaveCommand => new RelayCommand(Save);
        public ICommand CanelCommand => new RelayCommand(Canel);

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
        /// 设置参数。
        /// </summary>
        /// <param name="parameter">坐标点的集合。</param>
        private void SetParam(object parameter)
        {
            if (parameter is RecInfo info)
            {
                _index = RecInfos.IndexOf(info);
                _tempInfo = info.Infos;
                CopyToDt(info.Infos);
            }
        }

        /// <summary>
        /// 保存修改的PointInfo类数据。
        /// </summary>
        private void Save(object parameter)
        {
            if (_index != -1)
            {
                CopyToClass(PointDt);
            }  
        }

        /// <summary>
        /// 取消修改的PointInfo类数据。
        /// </summary>
        private void Canel(object parameter)
        {
            CopyToDt(_tempInfo);
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
        private void CopyToClass(DataTable dt)
        {
            _tempInfo.Clear();
            foreach (DataRow dr in dt.Rows)
            {
                double r1 = double.Parse(dr["XAxis"].ToString());
                double r2 = double.Parse(dr["YAxis"].ToString());
                double r3 = double.Parse(dr["LastTime"].ToString());

                _tempInfo.Add(new PointInfo(new Point(r1, r2), r3));
                RecInfos[_index].Infos = _tempInfo;
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
