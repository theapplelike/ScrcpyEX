using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static ScrcpyEX.MyDefinition.MyEnum;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 组脚本，包含多个脚本信息。
    /// </summary>
    public class ScriptGroup : ScriptItem, ICloneable
    {
        /// <summary>
        /// 脚本信息列表。
        /// </summary>
        public ObservableCollection<ScriptInfo> Scripts { get; set; }

        /// <summary>
        /// 组脚本执行时的衔接模式。
        /// </summary>
        public LinkMode LinkMode { get; set; } = LinkMode.InTurn;

        /// <summary>
        /// true--其中的每个脚本执行后计数加一；false--完整执行所有脚本后计数加一。
        /// </summary>
        public bool IsAloneAdd { get; set; } = false;

        /// <summary>
        /// 组脚本的默认构造函数。
        /// </summary>
        public ScriptGroup() { }

        object ICloneable.Clone() => Clone(); //显式接口实现

        /// <summary>
        /// 公共强类型方法，创建当前实例副本的新对象。
        /// </summary>
        /// <returns>返回此实例副本的新对象。</returns>
        public ScriptGroup Clone() => new()
        {
            Name = Name,
            Scripts = [.. Scripts.Select(s => s.Clone())],
        };


    }
}
