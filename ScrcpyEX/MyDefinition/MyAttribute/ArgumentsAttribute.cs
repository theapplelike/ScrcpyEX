using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ExtensionMethod.MyAttribute
{
    /// <summary>
    /// 自定义的命令行参数属性。
    /// </summary>
    internal class CommandLineAttribute : Attribute
    {
        /// <summary>
        /// 命令行参数。
        /// </summary>
        public string Arguments { get; set; }

        public CommandLineAttribute(string Arguments)
        {
            this.Arguments = Arguments;
        }
    }
}
