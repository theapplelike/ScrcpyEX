using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.ExtensionMethod.MyAttribute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code
    {
        private void Init()
        {
            prc.Exited += Prc_Exited;
            prc.ErrorDataReceived += Prc_ErrorDataReceived;
            prc.OutputDataReceived += Prc_OutputDataReceived;
            ReadCfg();

        }

        /// <summary>
        /// 读取配置的值给UI界面。
        /// </summary>
        private void ReadCfg()
        {
            //txtIP.Text = MyCfg.Cfg.IP;
            //chkStayAwake.Checked = MyCfg.Cfg.StayAwake;
            //chkOffScreen.Checked = MyCfg.Cfg.OffScreen;
            //chkShowTouches.Checked = MyCfg.Cfg.ShowTouches;
            //chkDeviceSleep.Checked = MyCfg.Cfg.DeviceSleep;
        }

        /// <summary>
        /// 将UI界面的值保存到配置当中。
        /// </summary>
        private void SaveCfg()
        {
            //MyCfg.Cfg.IP = txtIP.Text;
            //MyCfg.Cfg.StayAwake = chkStayAwake.Checked;
            //MyCfg.Cfg.OffScreen = chkOffScreen.Checked;
            //MyCfg.Cfg.ShowTouches = chkShowTouches.Checked;
            //MyCfg.Cfg.DeviceSleep = chkDeviceSleep.Checked;

            //MyCfg.Cfg.IP = txtIP.Text;
            MyCfg.Cfg.StayAwake = true;
            MyCfg.Cfg.OffScreen = false;
            MyCfg.Cfg.ShowTouches = true;
            MyCfg.Cfg.DeviceSleep = false;
            MyCfg.Save();
        }

        /// <summary>
        /// 获得指定属性（Property）的特性（Attribute），如果没有特性（Attribute）则返回null。
        /// </summary>
        /// <param name="Prop">属性（Property）的名称。</param>
        private string GetArg(string Prop)
        {
            var info = typeof(MyCfg).GetProperty(Prop).GetCustomAttribute<ArgumentsAttribute>();
            return info?.Arguments ?? null;
        }

        /// <summary>
        /// 获得指定值是否可以转化为bool，不能返回false，如果可以返回其bool值。
        /// </summary>
        private bool GetVal(object val)
        {
            return val is bool result ? result : false;
        }

        /// <summary>
        /// 获得所有用户选择的命令行参数。
        /// </summary>
        private string GetAllArguments()
        {
            SaveCfg();
            StringBuilder sb = new();

            foreach (PropertyInfo prop in typeof(MyCfg).GetProperties())
            {
                string str = GetArg(prop.Name);
                if (str != null && GetVal(prop.GetValue(MyCfg.Cfg)))
                {
                    sb.Append($"{str} ");
                }
                //Console.WriteLine($"属性{prop.Name}：{prop.GetValue(MyCfg.Cfg)}，{GetArg(prop.Name)}。");
            }

            scrcpyName = DateTime.Now.ToString("HHmmss") + "mook";
            sb.Append($"--window-title={scrcpyName} ");
            //Console.WriteLine($"命令行：{sb}。");
            return sb.ToString();
        }

        /// <summary>
        /// 获得scrcpy的句柄，并将scrcpy放到panel中。
        /// </summary>
        /// <returns>true--嵌入成功；false嵌入失败。</returns>
        private bool EmbeddedForm()
        {
            foreach (var p in Process.GetProcesses())
            {
                //Console.WriteLine($"进入foreach：{p.MainWindowTitle}。");
                if (p.MainWindowTitle.Contains(scrcpyName))
                {
                    //ShowWindow(p.MainWindowHandle, 0);
                    Console.WriteLine($"程序信息：{p.MainWindowHandle},{p.MainWindowTitle}。");
                    IntPtr pnl = pnlShow.Handle;
                    IntPtr h = ((HwndSource)PresentationSource.FromDependencyObject(gShow)).Handle;
                    IntPtr formh = ((HwndSource)PresentationSource.FromVisual(this)).Handle;

                    scrcpyHandle = p.MainWindowHandle;

                    HideTitleBar(scrcpyHandle);
                    SetParent(p.MainWindowHandle, pnl);
                    ShowWindow(p.MainWindowHandle, 3);

                    Console.WriteLine($"父级的句柄：{pnl}，{pnlShow.Width}，{pnlShow.Height}。");
                    Console.WriteLine($"窗体的句柄：{h},{formh}。");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 隐藏指定窗口的标题栏。通过位运算移除WS_CAPTION样式，从而隐藏标题栏。然后设置为新的窗口样式。
        /// </summary>
        /// <param name="handle">窗口的句柄。</param>
        private void HideTitleBar(IntPtr handle)
        {
            int style = GetWindowLong(handle, GWL_STYLE);

            //删除WS_CAPTION样式以隐藏标题栏  
            style = style & ~WS_CAPTION;

            //删除WS_THICKFRAME样式以隐藏窗体可以调整大小的边框
            style = style & ~WS_THICKFRAME;

            //设置为新的窗口样式
            SetWindowLong(handle, GWL_STYLE, style);
        }


    }
}
