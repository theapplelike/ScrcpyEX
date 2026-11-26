using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ScrcpyEX
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            GlobalException();
        }

        /// <summary>
        /// 添加全局异常的处理。
        /// </summary>
        private void GlobalException()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// 捕获UI线程未处理异常。
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; //设置为ture可以防止程序终止
            MyGM.RecordException(e.Exception, "UI线程异常");
        }

        /// <summary>
        /// 捕获非UI线程异常（包括Task），无法阻止应用程序终止。
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MyGM.RecordException(e.ExceptionObject as Exception, "非UI线程异常");
        }

        /// <summary>
        /// 捕获未观察到的Task异常，在.NET 4.0中，未观察的Task异常默认会导致应用终止，在.NET 4.5+，行为改为仅记录日志。
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved(); //标记未观察到的任务异常已被开发者显式处理，防止程序终止
            MyGM.RecordException(e.Exception, "未观察到的Task异常");
        }


    }
}
