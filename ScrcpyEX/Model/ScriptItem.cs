using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 单脚本和组脚本的基类。
    /// </summary>
    public class ScriptItem : NotificationObject
    {
        /// <summary>
        /// 脚本的名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _name = "";

    }
}
