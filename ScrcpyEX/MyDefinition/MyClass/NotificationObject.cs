using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition
{
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 引发属性的更新。
        /// </summary>
        /// <param name="propertyName">方法调用方的方法名或属性名。</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性为新的值。
        /// </summary>
        /// <typeparam name="T">属性的类型。</typeparam>
        /// <param name="field">和属性关联的字段。</param>
        /// <param name="value">新的值。</param>
        /// <param name="propertyName">属性的名称。</param>
        /// <returns>true--值设置成功；false--新值和旧值一样，不会通知客户端。</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性为新的值，新值和旧值一样时不会通知客户端。
        /// </summary>
        /// <typeparam name="T">属性的类型。</typeparam>
        /// <param name="field">和属性关联的字段。</param>
        /// <param name="value">新的值。</param>
        /// <param name="callback">属性修改时触发的委托。</param>
        /// <param name="propertyName">属性的名称。</param>
        protected void SetProperty<T>(ref T field, T value, Action callback, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            field = value;
            RaisePropertyChanged(propertyName);
            callback?.Invoke();
        }
    }
}
