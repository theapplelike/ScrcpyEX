using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    /// <summary>
    /// 扫描局域网中开放ADB调试的设备，包括电脑所在网络和电脑的热点网络。
    /// </summary>
    public class AdbDeviceScanner
    {
        /// <summary>
        /// adb的端口。
        /// </summary>
        private const int _adbPort = 5555;

        /// <summary>
        /// 每次检测任务的超时时间。
        /// </summary>
        private const int _timeoutMs = 2000;

        /// <summary>
        /// 最大并发扫描数量。
        /// </summary>
        private const int _maxConcurrentScans = 600;

        /// <summary>
        /// 扫描局域网中开放ADB端口的设备。
        /// </summary>
        /// <param name="startRange">IP范围起始。</param>
        /// <param name="endRange">IP范围结束。</param>
        /// <returns>发现的设备IP地址列表。</returns>
        public async Task<List<string>> ScanNetworkForAdbDevicesAsync(int startRange = 1, int endRange = 254)
        {
            List<string> devices = [];
            List<Task> tasks = [];
            List<string> baseIps = GetNetworkSegment();
            SemaphoreSlim semaphore = new(_maxConcurrentScans);

            foreach (string baseIp in baseIps)
            {
                Debug.WriteLine($"基地址：{baseIp}");
                for (int i = startRange; i <= endRange; i++)
                {
                    string ip = $"{baseIp}.{i}";
                    await semaphore.WaitAsync();

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (await IsPortOpenAsync(ip, _adbPort, _timeoutMs))
                            {
                                lock (devices)
                                {
                                    devices.Add(ip);
                                }
                                Debug.WriteLine($"发现ADB设备: {ip}");
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
            return devices;
        }

        /// <summary>
        /// 检查指定IP和端口是否开放。
        /// </summary>
        private async Task<bool> IsPortOpenAsync(string ip, int port, int timeoutMs)
        {
            try
            {
                using TcpClient tcpClient = new();
                Task connectTask = tcpClient.ConnectAsync(ip, port);
                Task timeoutTask = Task.Delay(timeoutMs);
                Task completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask && tcpClient.Connected)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // 连接失败，端口可能关闭或其他网络问题
                Debug.WriteLine($"检查 {ip}:{port} 失败: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// 获得本地电脑的网段（开启热点后给连接设备的网段）和已连接WIFI的网段（所在局域网的网段）。
        /// </summary>
        /// <returns>只包含网段的列表，子网掩码255.255.255.0，即IP地址的前24位。</returns>
        public List<string> GetNetworkSegment()
        {
            List<string> addresses = [];

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 筛选活动状态的WiFi接口
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    Debug.WriteLine($"网络接口名称: {ni.Name}");

                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    foreach (UnicastIPAddressInformation ipInfo in ipProps.UnicastAddresses)
                    {
                        // 筛选IPv4地址且非本地回环
                        if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(ipInfo.Address))
                        {
                            IPAddress ip = ipInfo.Address;
                            IPAddress subnet = ipInfo.IPv4Mask;

                            // 计算网段（网络地址）
                            byte[] ipBytes = ip.GetAddressBytes();
                            addresses.Add(string.Join(".", ipBytes.Take(3).Select(b => b.ToString())));
                        }
                    }
                }
            }
            return addresses;
        }


    }
}