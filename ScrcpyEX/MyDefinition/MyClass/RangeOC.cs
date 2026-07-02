using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 扩展动态数据集合，增加范围增加方法，防止批量操作时频繁刷新UI。
    /// </summary>
    /// <typeparam name="T">集合的类型。</typeparam>
    internal class RangeOC<T> : ObservableCollection<T>
    {
        /// <summary>
        /// 是否抑制事件通知。
        /// </summary>
        private bool _isNotification;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // 如果正在批量操作，则不触发事件
            if (!_isNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        /// <summary>
        /// 将指定的集合元素添加 <see cref="RangeOC{T}"/> 末尾。
        /// </summary>
        /// <param name="items">指定的集合。</param>
        /// <exception cref="ArgumentNullException">传入的集合为null。</exception>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            List<T> list = [.. items];
            if (list.Count == 0) return;

            // 开始批量操作，抑制事件
            _isNotification = true;

            try
            {
                // 直接操作内部集合
                foreach (T item in list)
                {
                    Items.Add(item);
                }
            }
            finally
            {
                _isNotification = false;
            }

            // 批量完成后，手动触发一次 Reset 事件，通知 UI 全部更新
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


    }
}
