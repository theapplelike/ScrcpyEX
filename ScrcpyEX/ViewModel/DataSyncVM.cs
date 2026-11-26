using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.Interface;
using ScrcpyEX.MyDefinition.MyClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.ViewModel
{
    internal class DataSyncVM : NotificationObject
    {
        /// <summary>
        /// ADB目录服务。
        /// </summary>
        public IDirectoryService AdbService { get; } = new AdbDirectoryService();

        /// <summary>
        /// 本地目录的服务。
        /// </summary>
        public IDirectoryService LclService { get; } = new LocalDirectoryService();


    }
}
