using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ExtensionMethod.MyAttribute
{
    internal class ArgumentsAttribute : Attribute
    {
        /// <summary>
        /// 命令行参数。
        /// </summary>
        public string Arguments { get; set; }

        public ArgumentsAttribute(string Arguments)
        {
            this.Arguments = Arguments;
        }
    }
}
