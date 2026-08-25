using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 管理 Scrcpy 进程的作业对象，确保宿主程序退出（包括崩溃）时自动终止子进程。
    /// 每个宿主程序实例应创建独立的 ScrcpyJob 实例，避免多开时互相影响。
    /// </summary>
    public sealed class ScrcpyJob : IDisposable
    {
        /// <summary>
        /// 作业对象的句柄。
        /// </summary>
        private IntPtr _jobHandle;

        /// <summary>
        /// 是否已经清理。
        /// </summary>
        private bool _disposed = false;

        public ScrcpyJob()
        {
            _jobHandle = CreateJobObject(IntPtr.Zero, null);
            if (_jobHandle == IntPtr.Zero)
                throw new Win32Exception($"创建作业对象失败，错误码：{Marshal.GetLastWin32Error()}");

            JOBOBJECT_BASIC_LIMIT_INFORMATION basicInfo = new()
            {
                LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            };

            JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new()
            {
                BasicLimitInformation = basicInfo
            };

            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr infoPtr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(extendedInfo, infoPtr, false);
                if (!SetInformationJobObject(_jobHandle, JobObjectInfoType.ExtendedLimitInformation, infoPtr, (uint)length))
                    throw new Win32Exception($"设置作业对象属性失败，错误码：{Marshal.GetLastWin32Error()}");
            }
            finally
            {
                Marshal.FreeHGlobal(infoPtr);
            }
        }

        /// <summary>
        /// 通过进程 ID 添加进程到作业。
        /// </summary>
        public void AddProcess(int processId)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ScrcpyJob));

            IntPtr hProcess = OpenProcess(PROCESS_SET_QUOTA | PROCESS_TERMINATE, false, (uint)processId);
            if (hProcess == IntPtr.Zero)
                throw new Win32Exception($"根据ID获取句柄失败，错误码：{Marshal.GetLastWin32Error()}");

            try
            {
                if (!AssignProcessToJobObject(_jobHandle, hProcess))
                    throw new Win32Exception($"进程分配给作业对象时失败，错误码：{Marshal.GetLastWin32Error()}");
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_jobHandle != IntPtr.Zero)
                {
                    CloseHandle(_jobHandle);
                    _jobHandle = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 创建作业对象（Job Object），作业对象允许将一组进程作为一个单元进行管理，
        /// 可以对这些进程实施资源限制、安全限制，并在作业对象关闭时终止所有关联进程。
        /// </summary>
        /// <param name="lpJobAttributes">
        /// <para>指向 SECURITY_ATTRIBUTES 结构的指针，决定返回的句柄是否可被子进程继承，以及作业对象的安全描述符。</para>
        /// <para>如果为 NULL，句柄不可继承，且作业对象具有默认安全描述符。</para>
        /// </param>
        /// <param name="lpName">
        /// <para>作业对象的名称（字符串）。名称可用于在其他进程中通过 OpenJobObject 打开同一个作业对象。</para>
        /// <para>如果为 NULL，则创建一个匿名作业对象，只能通过返回的句柄访问。</para>
        /// </param>
        /// <returns>成功--返回作业对象的句柄；失败--返回 NULL，可通过 GetLastError() 获取错误码。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        /// <summary>
        /// 设置作业对象属性。为一个作业对象指定各种限制、通知和 UI 限制，从而统一控制该作业内所有进程的行为。
        /// </summary>
        /// <param name="hJob">作业对象的句柄，由 CreateJobObject 或 OpenJobObject 返回。</param>
        /// <param name="infoType">一个枚举值，指定要设置的信息类别，更多参考 Microsoft 文档。。</param>
        /// <param name="lpJobObjectInfo">指向包含新设置信息的结构体的指针。该结构体的类型必须与 infoType 指定的类别匹配。</param>
        /// <param name="cbJobObjectInfoLength">lpJobObjectInformation 指向的结构体的大小（以字节为单位）。</param>
        /// <returns>成功--返回True；失败--返回False，调用 GetLastError() 获取具体错误码。</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        /// <summary>
        /// 将进程分配给现有作业对象。一旦进程被分配到某个作业，
        /// 该进程及其创建的子进程（除非特别指定脱离）都会受到该作业的限制和管理。
        /// </summary>
        /// <param name="hJob">作业对象的句柄，由 CreateJobObject 或 OpenJobObject 返回。</param>
        /// <param name="hProcess">
        /// <para>要加入作业的进程句柄。该句柄必须具有以下权限：</para>
        /// <para>PROCESS_SET_QUOTA (0x0100)、PROCESS_TERMINATE (0x0001)</para>
        /// <para>通常通过 OpenProcess 函数请求这些权限获得。</para>
        /// </param>
        /// <returns>成功--返回非零值；失败--返回 0，调用 GetLastError() 获取错误码。</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        /// <summary>
        /// OpenProcess 根据进程 ID 获取一个有效的进程句柄，该句柄带有调用者请求的访问权限。
        /// 它常用于在仅有进程 ID 的情况下，对目标进程执行需要特定权限的操作（如终止进程、查询信息、修改内存等）。
        /// </summary>
        /// <param name="dwDesiredAccess">指定对进程对象的访问权限。可以是多个权限的组合。</param>
        /// <param name="bInheritHandle">若为 TRUE，返回的句柄可由子进程继承。</param>
        /// <param name="dwProcessId">目标进程的标识符（PID）。</param>
        /// <returns>成功--返回具有指定权限的进程句柄；失败--返回 NULL，可调用 GetLastError() 获取错误码。</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        /// <summary>
        ///  用于关闭一个打开的内核对象句柄，递减该对象的引用计数。当对象的最后一个句柄被关闭时，系统会销毁该内核对象并释放其占用的资源。
        /// </summary>
        /// <param name="hObject">一个有效的内核对象句柄。该句柄可以指向进程、线程、文件、事件、互斥体、作业对象、管道等。</param>
        /// <returns>成功--返回非零值；失败--返回 0，调用 GetLastError() 获取具体错误码。</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// 用于标识要传递给 SetInformationJobObject / QueryInformationJobObject 的作业对象信息类别。
        /// </summary>
        private enum JobObjectInfoType
        {
            /// <summary>
            /// 作用是告诉 Windows 函数：传入的数据结构体是 JOBOBJECT_EXTENDED_LIMIT_INFORMATION。
            /// </summary>
            ExtendedLimitInformation = 9
        }

        /// <summary>
        /// 作业句柄关闭时终止所有进程。
        /// </summary>
        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

        /// <summary>
        /// 允许设置进程的内存配额（工作集大小）以及修改某些资源限制。
        /// </summary>
        private const uint PROCESS_SET_QUOTA = 0x0100;

        /// <summary>
        /// 允许使用 TerminateProcess 函数强制终止目标进程。
        /// </summary>
        private const uint PROCESS_TERMINATE = 0x0001;

        /// <summary>
        /// 用于设置或查询作业对象基本限制的结构体。
        /// 与 SetInformationJobObject 和 QueryInformationJobObject 函数配合使用，可以控制作业内所有进程的资源使用和行为。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        /// <summary>
        /// 用于设置或查询扩展限制信息的核心数据结构。它是对基本限制结构 JOBOBJECT_BASIC_LIMIT_INFORMATION 的扩充，
        /// 允许更精细地控制作业内进程的资源使用，并提供了内存限制和 I/O 统计能力。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }


    }
}
