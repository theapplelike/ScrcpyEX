using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition.Interface;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.MyEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ScrcpyEX.MyDefinition.MyControl
{
    public partial class FileExplorer//var
    {
        /// <summary>
        /// 历史记录列表。
        /// </summary>
        private readonly List<string> _history = [];

        /// <summary>
        /// 当前的历史记录索引。
        /// </summary>
        private int _curIndex = -1;

        /// <summary>
        /// 目录的集合。
        /// </summary>
        private readonly RangeOC<FileItem> _dir = [];

        /// <summary>
        /// 当前的路径。
        /// </summary>
        public string CurPath { get; set; }

        /// <summary>
        /// 是否可以发送文件。
        /// </summary>
        public bool IsCanSend { get; set; }

        /// <summary>
        /// 排序命令。
        /// </summary>
        public ICommand SortCmd => new RelayCommand(ApplySort);

        /// <summary>
        /// 现在排序的行。
        /// </summary>
        public string SortColumn
        {
            get => (string)GetValue(SortColumnProperty);
            set => SetValue(SortColumnProperty, value);
        }
        public static readonly DependencyProperty SortColumnProperty = RegProp<string>(nameof(SortColumn));

        /// <summary>
        /// 排序的方向。
        /// </summary>
        public ListSortDirection? SortDirection
        {
            get => (ListSortDirection?)GetValue(SortDirectionProperty);
            set => SetValue(SortDirectionProperty, value);
        }
        public static readonly DependencyProperty SortDirectionProperty = RegProp<ListSortDirection?>(nameof(SortDirection));

        /// <summary>
        /// 导航项的集合。
        /// </summary>
        public ObservableCollection<BreadcrumbItem> Bis
        {
            get => (ObservableCollection<BreadcrumbItem>)GetValue(BisProperty);
            set => SetValue(BisProperty, value);
        }
        public static readonly DependencyProperty BisProperty = RegProp<ObservableCollection<BreadcrumbItem>>(nameof(Bis));

        /// <summary>
        /// 目录的视图，用于UI排序。
        /// </summary>
        public ICollectionView DirView
        {
            get => (ICollectionView)GetValue(DirViewProperty);
            private set => SetValue(DirViewProperty, value);
        }
        public static readonly DependencyProperty DirViewProperty = RegProp<ICollectionView>(nameof(DirView));

        /// <summary>
        /// 封装和目录相关的操作。
        /// </summary>
        public IDirectoryService DirService
        {
            get => (IDirectoryService)GetValue(DirServiceProperty);
            set => SetValue(DirServiceProperty, value);
        }
        public static readonly DependencyProperty DirServiceProperty = RegProp<IDirectoryService>(nameof(DirService));

        /// <summary>
        /// 是否显示导航到根目录按钮。
        /// </summary>
        public Visibility ShowGoRoot
        {
            get => (Visibility)GetValue(ShowGoRootProperty);
            set => SetValue(ShowGoRootProperty, value);
        }
        public static readonly DependencyProperty ShowGoRootProperty = DependencyProperty.Register(
            nameof(ShowGoRoot),
            typeof(Visibility),
            typeof(FileExplorer),
            new PropertyMetadata(Visibility.Visible, (d, e) =>
            {
                FileExplorer control = (FileExplorer)d;
                Visibility val = (Visibility)e.NewValue;

                control.scv.Margin = val == Visibility.Visible ? new() : new(0, 0, 15, 0);
            }
        ));

        /// <summary>
        /// 注册 传输文件请求 冒泡路由事件。
        /// </summary>
        public static readonly RoutedEvent XferFileRequestedEvent = EventManager.RegisterRoutedEvent(
            "XferFileRequested",          // 事件名称
            RoutingStrategy.Bubble,           // 冒泡策略
            typeof(RoutedEventHandler),       // 处理器类型（可改为更具体的委托）
            typeof(FileExplorer)
        );            

        /// <summary>
        /// CLR 事件包装器（方便外部 += 订阅）。
        /// </summary>
        public event RoutedEventHandler XferFileRequested
        {
            add => AddHandler(XferFileRequestedEvent, value);
            remove => RemoveHandler(XferFileRequestedEvent, value);
        }


    }
}
