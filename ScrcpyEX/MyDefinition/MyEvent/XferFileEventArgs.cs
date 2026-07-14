using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.MyDefinition.MyEvent
{
    /// <summary>
    /// 包含要传输文件的信息，Transfer->Xfer。
    /// </summary>
    internal class XferFileEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 如果已存在文件，是否覆盖。
        /// </summary>
        public bool IsCover { get; set; } = false;

        /// <summary>
        /// 右键点击的那一项。
        /// </summary>
        public FileItem ClickedItem { get; set; }

        /// <summary>
        /// 所有选中的文件项。
        /// </summary>
        public IList<FileItem> SelectedItems { get; set; }

        /// <summary>
        /// 构造传输文件事件的原始信息。
        /// </summary>
        public XferFileEventArgs()
        {
            
        }

        /// <summary>
        /// 构造传输文件事件的原始信息。
        /// </summary>
        /// <param name="routedEvent">路由事件。</param>
        /// <param name="item">当前选择目录/文件的信息。</param>
        //public XferFileEventArgs(RoutedEvent routedEvent, FileItem item) : base(routedEvent)
        //{
        //    SelectedItem = item;
        //}
    }
}
