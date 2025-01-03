using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.ViewModel
{
    internal class SetupVM : NotificationObject
    {
        /// <summary>
        /// 对全局配置类的引用。
        /// </summary>
        public MyCfg Cfg
        {
            get { return cfg; }
            set
            {
                cfg = value;
                RaisePropertyChanged();
            }
        }
        private MyCfg cfg;

        /// <summary>
        /// 绑定第一个菜单的显示属性，需要双向绑定。
        /// </summary>
        public Visibility VisibleStp1
        {
            get { return visibleStp1; }
            set
            {
                visibleStp1 = value;
                CharBtn1 = visibleStp1 == Visibility.Visible ? "\ue70e" : "\ue70d";
                RaisePropertyChanged();
            }
        }
        private Visibility visibleStp1;

        /// <summary>
        /// 第一个菜单按钮右边的字符，通过第一个菜单的显示属性控制。
        /// </summary>
        public string CharBtn1
        {
            get { return charBtn1; }
            set
            {
                charBtn1 = value;
                RaisePropertyChanged();
            }
        }
        private string charBtn1 = "\ue70e";


    }
}
