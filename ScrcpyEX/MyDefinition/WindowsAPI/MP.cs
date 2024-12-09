using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// MouseParameter（鼠标参数）类，定义PostMessage函数的鼠标参数。
    /// 更多鼠标参数参阅：https://learn.microsoft.com/zh-cn/windows/win32/inputdev/mouse-input-notifications。
    /// </summary>
    internal class MP
    {
        public const uint WM_CLOSE = 0x0010;

        //用于PostMessage函数的第二个参数Msg
        public const uint WM_MOUSEMOVE = 0x0200;    //鼠标移动
        public const uint WM_LBUTTONDOWN = 0x0201;  //按下鼠标左键
        public const uint WM_LBUTTONUP = 0x0202;    //抬起鼠标左键
        public const uint WM_LBUTTONDBLCLK = 0x0203;//双击鼠标左键
        public const uint WM_RBUTTONDOWN = 0x0204;  //按下鼠标右键
        public const uint WM_RBUTTONUP = 0x0205;    //抬起鼠标右键
        public const uint WM_RBUTTONDBLCLK = 0x0206;//双击鼠标右键

        //用于PostMessage函数的第三个参数wParam
        public const uint MK_LBUTTON = 0x0001;      //按下了鼠标左键
        public const uint MK_RBUTTON = 0x0002;      //按下了鼠标右键
        public const uint MK_SHIFT = 0x0004;        //按下了 SHIFT 键
        public const uint MK_CONTROL = 0x0008;      //按下了 CTRL 键
        public const uint MK_MBUTTON = 0x0010;      //按下了鼠标中键
        public const uint MK_XBUTTON1 = 0x0020;     //按下了第一个 X 按钮
        public const uint MK_XBUTTON2 = 0x0040;     //按下了第二个 X 按钮
    }
}
