using OpenCvSharp.ML;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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

            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null && !_isRegEvent)
            {
                _parentWindow.AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown), true);
                _isRegEvent = true;
            }

            prgLoad.Visibility = Visibility.Visible;
            DirView = CollectionViewSource.GetDefaultView(_dir);
            await NavigateTo(DirService.Root);
            prgLoad.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 自定义清理。
        /// </summary>
        private void MyClose()
        {
            if (_parentWindow != null && _isRegEvent)
            {
                _parentWindow.RemoveHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));
                _isRegEvent = false;
            }
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
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                //gvcTime.Width = double.NaN;
                //gvcSize.Width = double.NaN;
                double nameWd = lstDir.ActualWidth -
                    SystemParameters.VerticalScrollBarWidth -
                    gvcSize.ActualWidth -
                    gvcTime.ActualWidth - 10;

                gvcName.Width = nameWd > 0 ? nameWd : double.NaN;
                //Debug.WriteLine($"info:" +
                //    $"{lstDir.ActualWidth}--" +
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
            string[] segs = DirService.ToFriendly(path).SplitReal(DirService.Sep);
            StringBuilder add = new();
            List<BreadcrumbItem> bis = [];
        
            for (int i = 0; i < segs.Length; i++)
            {
                add.Append($"{(i == 0 ? "" : DirService.Sep)}{segs[i]}");
                bis.Add(new BreadcrumbItem
                {
                    Name = segs[i],
                    FullPath = i == 0 ? GetRootPath(add) : add.ToString(),
                    IsLast = i < segs.Length - 1 ? Visibility.Visible : Visibility.Collapsed
                });
                //Debug.WriteLine($"++++++面包屑累加路径：{bis[i].FullPath}");
            }
        
            Bis = new(bis);
        }

        /// <summary>
        /// 获得根目录的完整路径，末尾会携带路径分隔符。
        /// </summary>
        /// <param name="add">各分段路径的累加。</param>
        /// <returns>根目录的完整路径。</returns>
        private string GetRootPath(StringBuilder add)
        {
            return $"{add}{DirService.Sep}";
        }

        /// <summary>
        /// 加载目标路径下的所有子目录和文件到集合中。
        /// </summary>
        /// <param name="tgtPath">目标目录。</param>
        /// <returns>true--加载成功；false--加载失败，并在界面上显示错误信息。</returns>
        private async Task<bool> LoadDirFile(string tgtPath)
        {
            try
            {
                IEnumerable<FileItem> items = await DirService.GetDirContentAsync(tgtPath);
                _dir.Clear();
                _dir.AddRange(items);
            }
            catch (Exception ex)
            {
                tbkErr.Text = ex.Message;
                tbkErr.Visibility = Visibility.Visible;
                return IsLoadSuccess = false;
            }

            return IsLoadSuccess = true;
        }

        /// <summary>
        /// 创建右键菜单。
        /// </summary>
        /// <param name="clickedFile">右键点击的那一项。</param>
        /// <param name="selectedFiles">所有选中的文件项。</param>
        /// <returns>ContextMenu控件。</returns>
        private ContextMenu CreateMenu(FileItem clickedFile, IList<FileItem> selectedFiles)
        {
            ContextMenu menu = new();
            MenuItem xferItem = new() { Header = "传输文件" };
            MenuItem coverItem = new() { Header = "传输文件（覆盖）" };
            MenuItem deleItem = new() { Header = "删除文件" };
            selectedFiles = [.. selectedFiles.Where(f => !f.IsDirectory)];

            xferItem.Click += (s, e) =>
            {
                // 构造事件参数并引发路由事件
                XferFileEventArgs args = new()
                {
                    ClickedItem = clickedFile,
                    SelectedItems = selectedFiles,
                    RoutedEvent = XferFileRequestedEvent,
                };
                RaiseEvent(args);
            };
            coverItem.Click += (s, e) =>
            {
                XferFileEventArgs args = new()
                {
                    IsCover = true,
                    ClickedItem = clickedFile,
                    SelectedItems = selectedFiles,
                    RoutedEvent = XferFileRequestedEvent,
                };
                RaiseEvent(args);
            };
            deleItem.Click += async (s, e) =>
            {
                prgLoad.Visibility = Visibility.Visible;
                await DirService.DeleteFilesAsync(selectedFiles);
                Reload();
                prgLoad.Visibility = Visibility.Collapsed;
            };

            GenTitle(menu, selectedFiles);
            menu.Items.Add(xferItem);
            menu.Items.Add(coverItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(deleItem);

            return menu;
        }

        /// <summary>
        /// 生成菜单的标题。
        /// </summary>
        /// <param name="menu">菜单控件。</param>
        /// <param name="selectedFiles">所有选中的项。</param>
        private void GenTitle(ContextMenu menu, IList<FileItem> selectedFiles)
        {
            MenuItem headerItem = new()
            {
                Header = selectedFiles.Count == 1
                     ? $"文件: {selectedFiles[0].Name}"
                     : $"已选中 {selectedFiles.Count} 个文件",
                Style = (Style)FindResource("ContextMenuHeaderStyle")
            };
            menu.Items.Add(headerItem);
            menu.Items.Add(new Separator());
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

        /// <summary>
        /// 当前目录是否可以执行传输任务。
        /// </summary>
        /// <returns>true--实际目录，可以传输；false--逻辑目录，不可以传输。</returns>
        public bool IsCanXfer()
        {
            return DirService.IsCanXfer(CurPath);
        }

        /// <summary>
        /// 显示或隐藏文本地址栏。
        /// </summary>
        /// <param name="isShow">是否显示文本地址栏。</param>
        private void SetAddrBarVisib(bool isShow)
        {
            _isEditingPath = isShow;
            txtAddr.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;

            if (isShow)
            {
                txtAddr.Text = CurPath;
                txtAddr.Focus();
                txtAddr.SelectAll();
            }
        }

        /// <summary>
        /// 说明指定元素是否为指定父级的子元素。
        /// </summary>
        /// <param name="parent">父级元素。</param>
        /// <param name="child">子元素。</param>
        /// <returns>true--子元素属于父元素；false--子元素不属于此父元素。</returns>
        private bool IsChildOf(DependencyObject parent, DependencyObject child)
        {
            while (child != null)
            {
                //Debug.WriteLine($"while循环，IsChildOf：{child.GetType()}");
                if (child == parent) return true;
                child = VisualTreeHelper.GetParent(child);
            }
            return false;
        }


    }
}
