using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// WindowsMessage（Windows消息常量）类。
    /// </summary>
    internal class WM
    {
        public const uint WM_CLOSE = 0x0010;        //当一个窗口或应用程序要关闭时发送一个信号

        public const uint WM_KEYDOWN = 0x0100;      //按下一个非系统键（按下键时未按下“ALT”键）
        public const uint WM_KEYUP = 0x0101;        //释放一个非系统键
        public const uint WM_SYSKEYDOWN = 0x0104;   //当按住ALT键同时按下其他键时发送此消息给拥有键盘焦点的窗口
        public const uint WM_SYSKEYUP = 0x0105;     //当释放一个键同时按住ALT键时发送此消息给拥有键盘焦点的窗口

        //鼠标相关的消息常量，可用于PostMessage函数的第二个参数Msg
        public const uint WM_MOUSEMOVE = 0x0200;    //鼠标移动
        public const uint WM_LBUTTONDOWN = 0x0201;  //按下鼠标左键
        public const uint WM_LBUTTONUP = 0x0202;    //抬起鼠标左键
        public const uint WM_LBUTTONDBLCLK = 0x0203;//双击鼠标左键
        public const uint WM_RBUTTONDOWN = 0x0204;  //按下鼠标右键
        public const uint WM_RBUTTONUP = 0x0205;    //抬起鼠标右键
        public const uint WM_RBUTTONDBLCLK = 0x0206;//双击鼠标右键

        //参阅：https://learn.microsoft.com/zh-cn/windows/win32/inputdev/mouse-input-notifications。
        //表示鼠标键的组合状态，可用于PostMessage函数的第三个参数wParam
        public const uint MK_LBUTTON = 0x0001;      //按下了鼠标左键
        public const uint MK_RBUTTON = 0x0002;      //按下了鼠标右键
        public const uint MK_SHIFT = 0x0004;        //按下了 SHIFT 键
        public const uint MK_CONTROL = 0x0008;      //按下了 CTRL 键
        public const uint MK_MBUTTON = 0x0010;      //按下了鼠标中键
        public const uint MK_XBUTTON1 = 0x0020;     //按下了第一个 X 按钮
        public const uint MK_XBUTTON2 = 0x0040;     //按下了第二个 X 按钮
    }
}
