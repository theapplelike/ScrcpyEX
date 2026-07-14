using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.WindowsAPI
{
    /// <summary>
    /// 音频设备检测。
    /// </summary>
    internal class AudioDeviceDetector
    {
        /// <summary>
        /// 数据流方向。
        /// </summary>
        private enum EDataFlow
        {
            /// <summary>
            /// 音频呈现流。
            /// </summary>
            eRender = 0,

            /// <summary>
            /// 音频捕获流。
            /// </summary>
            eCapture = 1,

            /// <summary>
            /// 音频呈现或捕获流。
            /// </summary>
            eAll = 2
        }

        /// <summary>
        /// 设备状态。
        /// </summary>
        [Flags]
        private enum DeviceState : uint
        {
            Active = 0x1,
            Disabled = 0x2,
            NotPresent = 0x4,
            Unplugged = 0x8,
            All = 0xF
        }

        /// <summary>
        /// 指示系统已分配给音频终端设备的角色。
        /// </summary>
        private enum ERole
        {
            /// <summary>
            /// 游戏、系统通知声音和语音命令。
            /// </summary>
            eConsole = 0,

            /// <summary>
            /// 音乐、电影、旁白和实时音乐录制。
            /// </summary>
            eMultimedia = 1,

            /// <summary>
            /// 语音通信、与其他人交谈。
            /// </summary>
            eCommunications = 2
        }

        /// <summary>
        /// IMMDeviceEnumerator 接口 GUID。
        /// </summary>
        private static readonly Guid CLSID_MMDeviceEnumerator = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
        //private static readonly Guid IID_IMMDeviceEnumerator = new("A95664D2-9614-4F35-A746-DE8DB63617E6");

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask, out IMMDeviceCollection devices);
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
            int GetDevice(string id, out IMMDevice device);
            int RegisterEndpointNotificationCallback(IntPtr client);
            int UnregisterEndpointNotificationCallback(IntPtr client);
        }

        [ComImport]
        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
            int GetCount(out uint pcDevices);
            int Item(uint nDevice, out IMMDevice device);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, out IntPtr ppInterface);
            int OpenPropertyStore(uint stgmAccess, out IntPtr ppProperties);
            int GetId(out IntPtr ppstrId);
            int GetState(out DeviceState pdwState);
        }

        /// <summary>
        /// 检查系统是否存在至少一个“活跃的”音频渲染（输出）设备。
        /// </summary>
        public static bool HasActiveRenderDevice()
        {
            try
            {
                // 创建 COM 对象
                Type type = Type.GetTypeFromCLSID(CLSID_MMDeviceEnumerator);
                if (type == null) return false;

                object obj = Activator.CreateInstance(type);
                IMMDeviceEnumerator enumerator = (IMMDeviceEnumerator)obj;

                // 枚举活跃的输出设备
                int hr = enumerator.EnumAudioEndpoints(
                    EDataFlow.eRender,
                    DeviceState.Active, 
                    out IMMDeviceCollection collection);

                if (hr < 0 || collection == null) return false;

                collection.GetCount(out uint count);
                return count > 0;
            }
            catch
            {
                // 任何异常（比如系统不支持 Core Audio）都视为没有设备
                return false;
            }
        }


    }
}
