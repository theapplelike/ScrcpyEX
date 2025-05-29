using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

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

        /// <summary>
        /// 删除指定脚本。
        /// </summary>
        /// <param name="info">要删除的脚本。</param>
        public void DelRec(RecInfo info)
        {
            MessageBoxResult result = MessageBox.Show($"是否删除“{info.Name}”脚本？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
            {
                _index = -1;
                PointDt.Clear();
                RecInfos.Remove(info);
                File.Delete(@$"{MV.ScriptPath}\{info.ImgName}");
            }
        }

        /// <summary>
        /// 显示当前脚本的坐标点参数到DataTable中。
        /// </summary>
        /// <param name="info">当前选中的脚本。</param>
        public void SetParam(RecInfo info)
        {
            _index = RecInfos.IndexOf(info);
            CopyToDt(info.Infos);
        }

        /// <summary>
        /// 保存修改的坐标点和脚本数据。
        /// </summary>
        public void Save(RecInfo recInfo, BitmapSource source)
        {
            if (_index != -1)
            {
                CopyToClass(PointDt, recInfo);
                if (source != null)
                {
                    MyGM.SavaImg(recInfo.ImgName, source);
                }
                MyGM.POP("脚本保存成功。");
            }
            else
            {
                MyGM.POP("未指定脚本，不能保存。");
            }
        }

        /// <summary>
        /// 取消修改的PointInfo类数据。
        /// </summary>
        //public void Canel()
        //{
        //    if (_index != -1)
        //    {
        //        CopyToDt(RecInfos[_index].Infos);
        //    }
        //}

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
            RecInfos[_index].IsLimited = recInfo.IsLimited;
            RecInfos[_index].CycleNumber = recInfo.CycleNumber;
            RecInfos[_index].IsTime = recInfo.IsTime;
            RecInfos[_index].CycleInterval = recInfo.CycleInterval;

            ReplaceImg(RecInfos[_index].ImgName, recInfo.ImgName);
            RecInfos[_index].ImgName = recInfo.ImgName;
            RecInfos[_index].CropRect = recInfo.CropRect;
            Console.WriteLine($"保存信息：{recInfo.CropRect}。");
        }

        /// <summary>
        /// 如果有新的图片，删除旧图片。
        /// </summary>
        /// <param name="oldName">旧的图片名。</param>
        /// <param name="newName">新的图片名。</param>
        private void ReplaceImg(string oldName, string newName)
        {
            if (oldName != newName && oldName != "")
            {
                File.Delete(@$"{MV.ScriptPath}\{oldName}");
            }
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
