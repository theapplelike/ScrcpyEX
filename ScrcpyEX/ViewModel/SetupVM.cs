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
        /// 绑定第一个菜单的Tag属性，null为折叠状态，非null为展开状态，需要双向绑定。
        /// </summary>
        public object Tag1
        {
            get { return tag1; }
            set
            {
                tag1 = value;
                CharBtn1 = tag1 != null ? "\ue70e" : "\ue70d";
                RaisePropertyChanged();
            }
        }
        private object tag1;

        /// <summary>
        /// 第一个菜单按钮右边的字符，通过第一个菜单的指定属性控制。
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
        private string charBtn1 = "\ue70d";

        /// <summary>
        /// 绑定第二个菜单的Tag属性，null为折叠状态，非null为展开状态，需要双向绑定。
        /// </summary>
        public object Tag2
        {
            get { return tag2; }
            set
            {
                tag2 = value;
                CharBtn2 = tag2 != null ? "\ue70e" : "\ue70d";
                RaisePropertyChanged();
            }
        }
        private object tag2;

        /// <summary>
        /// 第二个菜单按钮右边的字符，通过第二个菜单的指定属性控制。
        /// </summary>
        public string CharBtn2
        {
            get { return charBtn2; }
            set
            {
                charBtn2 = value;
                RaisePropertyChanged();
            }
        }
        private string charBtn2 = "\ue70d";


    }
}
