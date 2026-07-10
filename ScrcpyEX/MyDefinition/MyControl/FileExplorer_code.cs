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

            prgLoad.Visibility = Visibility.Visible;
            DirView = CollectionViewSource.GetDefaultView(_dir);
            await NavigateTo(DirService.Root);
            prgLoad.Visibility = Visibility.Collapsed;
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
            //Debug.WriteLine($"排序的列：{para}");
        }

        /// <summary>
        /// 删除当前索引+1后的所有历史记录，然后添加当前路径到历史记录。
        /// </summary>
        /// <param name="path">要添加的路径。</param>
        private void AddHistory(string path)
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
        private async Task NavigateTo(string tgtPath, bool isRecord = false)
        {
            CurPath = tgtPath;

            if (!await LoadDirFile(tgtPath)) return;
            UpdateBis(tgtPath);

            if (isRecord) AddHistory(tgtPath);
            SetNaviUI();
            SetColWidth();

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
                //gvcTime.Width = double.NaN;
                //gvcSize.Width = double.NaN;
                double nameWd = lstAndroid.ActualWidth -
                    SystemParameters.VerticalScrollBarWidth -
                    gvcSize.ActualWidth -
                    gvcTime.ActualWidth - 10;

                gvcName.Width = nameWd > 0 ? nameWd : double.NaN;
                //Debug.WriteLine($"info:" +
                //    $"{lstAndroid.ActualWidth}--" +
                //    $"{gvcName.ActualWidth}--{gvcSize.ActualWidth}--{gvcTime.ActualWidth}");
            }));
        }

        /// <summary>
        /// 设置导航UI的状态。
        /// </summary>
        private void SetNaviUI()
        {
            btnBack.IsEnabled = _curIndex >= 0;
            btnFwrd.IsEnabled = _curIndex < _history.Count - 1;
            btnGoUp.IsEnabled = !DirService.IsRoot(CurPath);
        }

        /// <summary>
        /// 更新面包屑导航项的集合。
        /// </summary>
        /// <param name="path">完整的路径。</param>
        private void UpdateBis(string path)
        {
            string[] segs = path.SplitReal(DirService.Sep);
            StringBuilder cum = new();
            List<BreadcrumbItem> bis = [];
        
            for (int i = 0; i < segs.Length; i++)
            {
                cum.Append($"{(i == 0 ? "" : DirService.Sep)}{segs[i]}");
                bis.Add(new BreadcrumbItem
                {
                    Name = segs[i],
                    FullPath = i == 0 ? $"{cum}{DirService.Sep}" : cum.ToString(),//根目录的完整路径后需要携带路径分隔符
                    IsLast = i < segs.Length - 1 ? Visibility.Visible : Visibility.Collapsed
                });
                //Debug.WriteLine($"++++++完整路径：{bis[i].FullPath}");
            }
        
            Bis = new(bis);
        }

        /// <summary>
        /// 加载目标路径下的所有子目录和文件到集合中。
        /// </summary>
        /// <param name="tgtPath">目标目录。</param>
        /// <returns>true加载成功；false加载失败，并在界面上显示错误信息。</returns>
        private async Task<bool> LoadDirFile(string tgtPath)
        {
            try
            {
                var items = await DirService.GetDirContentAsync(tgtPath);
                _dir.Clear();
                _dir.AddRange(items);
            }
            catch (Exception ex)
            {
                tbkErr.Text = ex.Message;
                tbkErr.Visibility = Visibility.Visible;
                return IsCanSend = false;
            }

            return IsCanSend = true;
        }

        /// <summary>
        /// 注册属性。
        /// </summary>
        private static DependencyProperty RegProp<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(FileExplorer));
        }

        /// <summary>
        /// 重新加载当前的目录。
        /// </summary>
        public async void Reload()
        {
            await NavigateTo(CurPath);
        }


    }
}
