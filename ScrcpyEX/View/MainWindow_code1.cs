using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        /// <summary>
        /// 设置第三方程序的绑定事件，且触发退出事件。
        /// </summary>
        private void SetProcess()
        {
            _prc.EnableRaisingEvents = true;
            _prc.Exited += Prc_Exited;
            _prc.ErrorDataReceived += Prc_ErrorDataReceived;
            _prc.OutputDataReceived += Prc_OutputDataReceived;
        }

        /// <summary>
        /// 第三方程序退出时，停止读取输出流和错误流，并重置句柄。
        /// </summary>
        private void Prc_Exited(object sender, EventArgs e)
        {
            _prc.CancelOutputRead();
            _prc.CancelErrorRead();
            _scHandle = IntPtr.Zero;
            Console.WriteLine($"scrcpy程序已关闭。");
        }

        /// <summary>
        /// 读取错误流。
        /// </summary>
        private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"〓〓：{e.Data}。");
        }

        /// <summary>
        /// 读取输出流。
        /// </summary>
        private void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"{e.Data}。");
        }

        /// <summary>
        /// 设置计时器，在其中进行第三方程序的大小调整。
        /// </summary>
        private void SetTimer()
        {
            _resizeTimer.Tick += new EventHandler(Resize_Tick);
            _resizeTimer.Interval = TimeSpan.FromSeconds(0.1);

            _popTimer.Tick += new EventHandler(Pop_Tick);
            _popTimer.Interval = TimeSpan.FromSeconds(3);
        }

        /// <summary>
        /// 要使用Thread在线程中调整第三方程序大小，这样不会改变焦点，导致用户调整大小操作被打断。
        /// </summary>
        private void Resize_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("正在timer中修改窗体大小。");

            new Thread(ResizeSC).Start();
            _resizeTimer.Stop();
        }

        private void Pop_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("**************");
            pop.IsOpen = false;
            _popTimer.Stop();
            Console.WriteLine("隐藏pop。");
        }
    }
}
