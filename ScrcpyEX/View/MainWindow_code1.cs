using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code1
    {
        private void Prc_Exited(object sender, EventArgs e)
        {
            prc.CancelOutputRead();
            prc.CancelErrorRead();
            scrcpyHandle = (IntPtr)(-1);
            Console.WriteLine($"scrcpy程序退出。");
        }

        private void Prc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"Error：{e.Data}。");
        }

        private void Prc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"{e.Data}。");
        }
    }
}
