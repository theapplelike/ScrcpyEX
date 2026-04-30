using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 组脚本，包含多个脚本信息。
    /// </summary>
    internal class ScriptGroup : ScriptItem, ICloneable
    {
        /// <summary>
        /// 脚本信息列表。
        /// </summary>
        public ObservableCollection<ScriptInfo> Scripts { get; set; }

        /// <summary>
        /// 组脚本的默认构造函数。
        /// </summary>
        public ScriptGroup() { }

        public object Clone()
        {
            return new ScriptGroup
            {
                Name = Name,
                Scripts = [.. Scripts.Select(s => (ScriptInfo)s.Clone())]
            };
        }


    }
}
