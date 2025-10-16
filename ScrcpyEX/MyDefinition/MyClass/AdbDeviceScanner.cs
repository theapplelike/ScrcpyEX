using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class AdbDeviceScanner
{
    private const int AdbPort = 5555;
    private const int TimeoutMs = 2000;
    private const int MaxConcurrentScans = 500;

    /// <summary>
    /// 扫描局域网中开放ADB端口的设备
    /// </summary>
    /// <param name="startRange">IP范围起始</param>
    /// <param name="endRange">IP范围结束</param>
    /// <returns>发现的设备IP地址列表</returns>
    public async Task<List<string>> ScanNetworkForAdbDevicesAsync(int startRange = 1, int endRange = 254)
    {
        List<string> devices = [];
        List<Task> tasks = [];
        List<string> baseIps = GetNetworkSegment();
        SemaphoreSlim semaphore = new(MaxConcurrentScans);

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
                        if (await IsPortOpenAsync(ip, AdbPort, TimeoutMs))
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
    /// <returns>包含网段的列表，子网掩码255.255.255.0。</returns>
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

    /// <summary>
    /// 获得本地电脑的网段（开启热点后给连接设备的网段）和已连接WIFI的网段（所在局域网的网段）。
    /// </summary>
    /// <returns>包含网段的列表。</returns>
    public List<IPAddress> GetCompleteNetworkSegment()
    {
        List<IPAddress> addresses = [];

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
                        byte[] subnetBytes = subnet.GetAddressBytes();
                        byte[] networkBytes = new byte[3];

                        for (int i = 0; i < 4; i++)
                        {
                            networkBytes[i] = (byte)(ipBytes[i] & subnetBytes[i]);
                        }
                        IPAddress network = new(networkBytes);
                        addresses.Add(network);

                        Debug.WriteLine($"IP地址: {ip}，子网掩码: {subnet}");
                        Debug.WriteLine($"网段: {network}/{GetCidrMask(subnetBytes)}");
                        Debug.WriteLine(new string('-', 30));
                    }
                }
            }
        }
        return addresses;
    }

    // 计算CIDR表示法的子网掩码（如255.255.255.0 → /24）
    private int GetCidrMask(byte[] maskBytes)
    {
        int cidr = 0;

        for (int i = 0; i < maskBytes.Length; i++)
        //foreach (byte b in maskBytes)
        {
            if (maskBytes[i] == 255) cidr += 8;
            else if (maskBytes[i] > 0)
            {
                // 计算连续1的位数（如254=11111110 → 7位）
                while (maskBytes[i] > 0)
                {
                    if ((maskBytes[i] & 0x80) == 0x80) cidr++;
                    else break;
                    maskBytes[i] <<= 1;
                }
                return cidr;
            }
            else break;
        }
        return cidr;
    }

    /// <summary>
    /// 获取本地网络IP段
    /// </summary>
    public string GetLocalNetworkSegment()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        Console.WriteLine($"主机名：{Dns.GetHostName()}");

        foreach (var ip in host.AddressList)
        {
            Console.WriteLine($"ip地址列表：{ip}。");
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var ipParts = ip.ToString().Split('.');
                if (ipParts.Length == 4)
                {
                    return $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";
                }
            }
        }
        return "192.168.1"; // 默认回退
    }
}
