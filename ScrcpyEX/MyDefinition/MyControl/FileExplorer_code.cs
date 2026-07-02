using OpenCvSharp.ML;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition.MyControl
{
    public partial class FileExplorer//code
    {
        /// <summary>
        /// 自定义初始化。
        /// </summary>
        private async void MyInit()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            DirView = CollectionViewSource.GetDefaultView(_dir);
            await NavigateTo(DirService.Root);
        }

        /// <summary>
        /// 让指定的列进行排序，如果是同一列按照顺序：升序、降序、无序。
        /// </summary>
        /// <param name="para">需要排序的列。</param>
        private void ApplySort(object para)
        {
            if (SortColumn == para.ToString())
            {
                SortDirection = SortDirection switch
                {
                    ListSortDirection.Ascending => ListSortDirection.Descending,
                    ListSortDirection.Descending => null,
                    _ => ListSortDirection.Ascending,
                };
            }
            else
            {
                SortColumn = para.ToString();
                SortDirection = ListSortDirection.Ascending;
            }

            using (DirView.DeferRefresh())
            {
                DirView.SortDescriptions.Clear();
                if (SortDirection.HasValue)
                {
                    DirView.SortDescriptions.Add(new SortDescription(SortColumn, SortDirection.Value));
                }
            }
            Debug.WriteLine($"排序的列：{para}");
        }

        /// <summary>
        /// 删除当前索引+1后的所有历史记录，然后添加当前路径到历史记录。
        /// </summary>
        /// <param name="path">要添加的路径。</param>
        public void AddHistory(string path)
        {
            if (_curIndex + 1 >= 0)
            {
                for (int i = _history.Count - 1; i > _curIndex; i--)
                {
                    _history.RemoveAt(i);
                }
            }

            _history.Add(path);
            _curIndex = _history.Count - 1;
        }

        /// <summary>
        /// 导航到指定的目录。
        /// </summary>
        /// <param name="tgtPath">目标目录。</param>
        /// <param name="isRecord">是否将此目录添加到历史记录。</param>
        public async Task NavigateTo(string tgtPath, bool isRecord = false)
        {
            _curPath = tgtPath;

            //string otp = await MyGM.AdbAsync($"shell ls -la {tgtPath}");
            //ParseDir(otp, tgtPath.TrimEnd('/'));

            //Dir = new(await DirService.GetDirectoryContentAsync(tgtPath));

            var items = await DirService.GetDirContentAsync(tgtPath);
            _dir.Clear();
            _dir.AddRange(items);

            //_dir = new ObservableCollection<FileItem>(items);
            //DirView = CollectionViewSource.GetDefaultView(_dir);


            SetColWidth();
            UpdateBis(tgtPath);

            if (isRecord) AddHistory(tgtPath);
            SetNaviUI();

            Debug.WriteLine($"目标目录(子项：{_dir.Count})：{tgtPath}");
            Debug.WriteLine($"历史记录(索引：{_curIndex})：");
            foreach (string st in _history)
            {
                Debug.WriteLine($"{st}");
            }
        }

        /// <summary>
        /// 设置每一列的宽度。
        /// </summary>
        private void SetColWidth()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                gvcTime.Width = double.NaN;
                gvcSize.Width = double.NaN;
                double nameWd = lstAndroid.ActualWidth -
                    SystemParameters.VerticalScrollBarWidth -
                    gvcSize.ActualWidth -
                    gvcTime.ActualWidth - 10;

                gvcName.Width = nameWd > 0 ? nameWd : double.NaN;
                //System.Diagnostics.Debug.WriteLine($"info:" +
                //    $"{lstAndroid.ActualWidth}--" +
                //    $"{gvcName.ActualWidth}--{gvcSize.ActualWidth}--{gvcTime.ActualWidth}");
            }));
        }

        /// <summary>
        /// 设置导航UI的状态。
        /// </summary>
        private void SetNaviUI()
        {
            if (DirService.IsRoot(_curPath))
            {
                btnGoUp.IsEnabled = false;
            }
            else
            {
                btnGoUp.IsEnabled = true;
            }

            if (_curIndex >= 0)
            {
                btnBack.IsEnabled = true;
            }
            else
            {
                btnBack.IsEnabled = false;
            }

            if (_curIndex < _history.Count - 1)
            {
                btnFwrd.IsEnabled = true;
            }
            else
            {
                btnFwrd.IsEnabled = false;
            }
            //Debug.WriteLine($"当前的路径：{_curPath}");
        }

        /// <summary>
        /// 更新面包屑导航项的集合。
        /// </summary>
        /// <param name="path">完整的路径。</param>
        public void UpdateBis(string path)
        {
            string cum = "";
            string[] segs = path.SplitReal(DirService.Sep);
            List<BreadcrumbItem> bis = [];
        
            for (int i = 0; i < segs.Length; i++)
            {
                cum += $"{(i == 0 ? "" : DirService.Sep)}{segs[i]}";
                bis.Add(new BreadcrumbItem
                {
                    Name = segs[i],
                    FullPath = i == 0 ? $"{cum}{DirService.Sep}" : cum,//根目录的完整路径后需要携带路径分隔符
                    IsLast = i < segs.Length - 1 ? Visibility.Visible : Visibility.Collapsed
                });
                //Debug.WriteLine($"++++++完整路径：{cum}");
            }
        
            Bis = new(bis);
        }

        /// <summary>
        /// 注册属性。
        /// </summary>
        private static DependencyProperty RegProp<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(FileExplorer));
        }


    }
}
