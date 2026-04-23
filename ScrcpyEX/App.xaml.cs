using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ScrcpyEX
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private readonly string path = @".\main\Local\ErrorLog";

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
            HandleException(e.Exception, "UI线程异常");
            e.Handled = true; //设置为ture可以防止程序终止
            MyGM.ShwError($"UI线程异常：{e.Exception}");
        }

        /// <summary>
        /// 捕获非UI线程异常（包括Task），无法阻止应用程序终止。
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, "非UI线程异常");
            MyGM.ShwError($"非UI线程异常：{e.ExceptionObject as Exception}");
        }

        /// <summary>
        /// 捕获未观察到的Task异常，在.NET 4.0中，未观察的Task异常默认会导致应用终止，在.NET 4.5+，行为改为仅记录日志。
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception, "未观察到的Task异常");
            e.SetObserved(); //标记未观察到的任务异常已被开发者显式处理，防止程序终止
            MyGM.ShwError($"未观察到的Task异常：{e.Exception}");
        }

        /// <summary>
        /// 处理全局异常。
        /// </summary>
        /// <param name="ex">异常对象。</param>
        /// <param name="processEvent">处理异常的事件名称。</param>
        private void HandleException(Exception ex, string processEvent)
        {
            StringBuilder sb = new();
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
            sb.AppendLine("【处理事件】：" + processEvent);
            if (ex != null)
            {
                sb.AppendLine("【异常类型】：" + ex.GetType().Name);
                sb.AppendLine("【异常信息】：" + ex.Message);
                sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
            }
            else
            {
                sb.AppendLine("【未处理异常】：" + ex.ToString());
            }
            sb.AppendLine("****************************--------****************************");
            Directory.CreateDirectory(path);

            //第二个参数为true表示追加数据到该文件中；为false表示覆盖该文件。如果指定的文件不存在，该参数无效，且构造函数将创建一个新文件。
            using StreamWriter sw = new(path + @"\ErrorLog.txt", true, Encoding.UTF8);
            sw.WriteLine(sb);
        }

    }
}
