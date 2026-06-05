using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ScrcpyEX.MyDefinition.MyClass
{
    internal class MiniTouchService
    {
        /// <summary>
        /// STFService的包名。
        /// </summary>
        private readonly string _pkg = "jp.co.cyberagent.stf";

        private readonly object _syncRoot = new();

        /// <summary>
        /// TCP客户端连接。
        /// </summary>
        private TcpClient _client;

        /// <summary>
        /// 写入流。
        /// </summary>
        private StreamWriter _writer;

        /// <summary>
        /// 读取流。
        /// </summary>
        private StreamReader _reader;

        /// <summary>
        /// 网络流。
        /// </summary>
        private NetworkStream _stream;

        private CancellationTokenSource _reconnectCts;
        private Task _reconnectTask;

        private bool _started;
        private bool _touchDown;
        private bool _disposed;

        //private int _contactId = 0;

        /// <summary>
        /// 推送到设备的路径。
        /// </summary>
        //public string PushPath { get; set; } = "/data/local/tmp/minitouch";

        /// <summary>
        /// 转发消息的端口。
        /// </summary>
        public int ForwardPort { get; set; } = 1111;

        public bool AutoReconnect { get; set; } = true;
        public int ReconnectIntervalMs { get; set; } = 1000;


        public int CenterX { get; set; } = 0;
        public int CenterY { get; set; } = 0;
        public int Radius { get; set; } = 120;

        public bool IsConnected
        {
            get
            {
                lock (_syncRoot)
                {
                    return _client != null && _client.Connected;
                }
            }
        }

        /// <summary>
        /// STFService是否已经启动。
        /// </summary>
        public bool IsStart => _started;

        private readonly System.Timers.Timer _readTimer = new();

        private async void Read_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            _readTimer.Stop();
            //if (_reader != null && _stream.DataAvailable)
            if (_stream.DataAvailable)
            {
                //string line = await _stream.ReadAsync();
                //Debug.WriteLine($"\r\n【Read】：{line}");

                byte[] buffer = new byte[4096];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(data);
            }
            _readTimer.Start();
        }

        #region Public API
        /// <summary>
        /// 启动minitouch服务。
        /// </summary>
        public async Task<bool> StartAsync()
        {
            if (IsStart) return true;

            _readTimer.Elapsed += Read_Tick;
            _readTimer.Interval = 1000;

            //await UninstallStf();
            await EnsureStfAsync("D:/Download/STFService-1.apk");

            //await MyGM.AdbAsync("shell am stopservice jp.co.cyberagent.stf/.Service");
            //await MyGM.AdbAsync("shell dumpsys activity services jp.co.cyberagent.stf");
            //await MyGM.AdbAsync("shell am start-foreground-service --user 0 -a jp.co.cyberagent.stf.ACTION_START -n jp.co.cyberagent.stf/.Service");
            await MyGM.AdbAsync("shell am startservice -n jp.co.cyberagent.stf/.Service --es action minitouch --es socketName minitouch");
            //await MyGM.AdbAsync("shell CLASSPATH=/data/local/tmp/STFService.apk app_process /system/bin jp.co.cyberagent.stf.MinitouchAgent");

            // 2. 赋权并启动
            //await KillMtAsync();
            //await StartMtAsync();

            // 3. adb forward
            await SetupPortForwardAsync();

            // 4. 连接 socket
            bool ok = await ConnectAsync();
            if (!ok)
            {
                Debug.WriteLine($"连接 minitouch 失败。");
                return false;
            }

            //await MyGM.AdbAsync("shell /data/local/tmp/minitouch -h");

            // 5. 启动重连监控
            lock (_syncRoot)
            {
                _started = true;
                if (AutoReconnect && _reconnectTask == null)
                {
                    _reconnectCts = new CancellationTokenSource();
                    _reconnectTask = Task.Run(() => ReconnectLoopAsync(_reconnectCts.Token));
                }
            }

            return true;
        }

        /// <summary>
        /// 断开电脑端连接，但不杀安卓端 minitouch。
        /// </summary>
        public void Disconnect()
        {
            lock (_syncRoot)
            {
                CloseClientInternal();
            }
        }

        /// <summary>
        /// 关闭 minitouch：安卓端停止 + 电脑端清理
        /// </summary>
        public async Task StopAsync()
        {
            lock (_syncRoot)
            {
                _started = false;
            }

            try
            {
                _reconnectCts?.Cancel();
            }
            catch { }

            Disconnect();

            //await KillMtAsync();
            await Task.Delay(10);

            lock (_syncRoot)
            {
                if (_reconnectTask != null)
                {
                    try { _reconnectTask.Wait(1000); } catch { }
                    _reconnectTask = null;
                }

                _reconnectCts?.Dispose();
                _reconnectCts = null;
            }

            Debug.WriteLine($"MiniTouch 已停止。");
        }

        /// <summary>
        /// 发送按下
        /// </summary>
        public void Down(int x, int y, int pressure = 50, int contactId = 0)
        {
            //ExecuteTouchCommand(() =>
            //{
            //    _touchDown = true;
            //    SendLine($"d {contactId} {x} {y} {pressure}");
            //    Commit();
            //});

            _touchDown = true;
            SendLine($"d {contactId} {x} {y} {pressure}");
            SendLine($"c");
            //Commit();
            Debug.WriteLine($"按下");
        }

        /// <summary>
        /// 发送移动
        /// </summary>
        public void Move(int x, int y, int pressure = 50, int contactId = 0)
        {
            //ExecuteTouchCommand(() =>
            //{
            //    SendLine($"m {contactId} {x} {y} {pressure}");
            //    Commit();
            //});

            SendLine($"m {contactId} {x} {y} {pressure}");
            SendLine($"c");
            //Commit();
            Debug.WriteLine($"移动");
        }

        /// <summary>
        /// 发送抬起
        /// </summary>
        public void Up(int x, int y, int pressure = 50, int contactId = 0)
        {
            //ExecuteTouchCommand(() =>
            //{
            //    _touchDown = false;
            //    //SendLine($"u {contactId} {x} {y} {pressure}");
            //    SendLine($"u 0");
            //    Commit();
            //});
            _touchDown = false;
            //SendLine($"u {contactId} {x} {y} {pressure}");
            SendLine($"u 0");
            SendLine($"c");
            //Commit();
            Debug.WriteLine($"抬起");
        }

        /// <summary>
        /// 摇杆：根据方向向量发送触摸
        /// </summary>
        public void UpdateJoystick(double dx, double dy, int? radius = null, int pressure = 50, int contactId = 0)
        {
            int r = radius ?? Radius;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.0001)
            {
                if (_touchDown) Up((int)dx,(int)dy);
                return;
            }

            dx /= len;
            dy /= len;

            int x = CenterX + (int)Math.Round(dx * r);
            int y = CenterY + (int)Math.Round(dy * r);

            if (!_touchDown)
                Down(x, y, pressure, contactId);
            else
                Move(x, y, pressure, contactId);
        }

        /// <summary>
        /// 键盘方向更新（WASD 或 自定义方向键）
        /// </summary>
        public void UpdateJoystickByPressedKeys(bool up, bool down, bool left, bool right, int? radius = null, int pressure = 50, int contactId = 0)
        {
            double dx = 0;
            double dy = 0;

            if (left) dx -= 1;
            if (right) dx += 1;
            if (up) dy -= 1;
            if (down) dy += 1;

            UpdateJoystick(dx, dy, radius, pressure, contactId);
        }

        #endregion

        #region Device Prepare
        /// <summary>
        /// 检查设备上是否存在 STFService，不存在则安装。
        /// </summary>
        /// <param name="localPath">STFService在本地的目录。</param>
        private async Task EnsureStfAsync(string localPath)
        {
            if (!await CheckStf())
            {
                await InstallStf(localPath);
            }
        }

        /// <summary>
        /// 安装STFService。
        /// </summary>
        /// <param name="localPath">STFService在本地的目录。</param>
        /// <returns>STFService是否安装成功。</returns>
        private async Task<bool> InstallStf(string localPath)
        {
            string otp = await MyGM.AdbAsync($"install {localPath}");
            return otp.Contains("Success");
        }

        /// <summary>
        /// 卸载STFService。
        /// </summary>
        /// <returns>STFService是否卸载成功。</returns>
        private async Task<bool> UninstallStf()
        {
            string otp = await MyGM.AdbAsync($"uninstall {_pkg}");
            return otp.Contains("Success");
        }

        /// <summary>
        /// 检查STFService是否安装。
        /// </summary>
        /// <returns>true--已安装；false--未安装。</returns>
        private async Task<bool> CheckStf()
        {
            string otp = await MyGM.AdbAsync($"shell pm list packages {_pkg}");
            return otp.Contains(_pkg);
        }

        /// <summary>
        /// 是否需要推送 minitouch 文件。
        /// </summary>
        /// <param name="path">minitouch路径。ADB命令执行后文件存在，返回Y，否则N。</param>
        /// <returns>true--文件不存在，需要推送；false--存在。</returns>
        //private async Task<bool> IsPushAsync(string path)
        //{
        //    string otp = await MyGM.AdbAsync($"shell [ -f {path} ] && echo Y || echo N");
        //    return otp.Contains("N");
        //}

        /// <summary>
        /// Android 设备后台静默启动 minitouch 服务，且不阻塞当前的 adb shell 会话。
        /// </summary>
        //private async Task StartMtAsync()
        //{
        //    await KillMtAsync();
        //
        //    //给minitouch权限
        //    await MyGM.AdbAsync($"shell chmod 755 {PushPath}");
        //
        //    //>	将标准输出(stdout)重定向到 /dev/null（即丢弃所有正常输出）
        //    //2 > &1 将标准错误(stderr)合并到标准输出，一起丢弃
        //    //& 将命令放到后台运行，shell立即返回，不再等待该命令结束
        //    await MyGM.AdbAsync($"shell {PushPath} > /dev/null 2>&1 &");
        //}

        /// <summary>
        /// 强制杀掉所有名为 minitouch 的进程。
        /// -9 表示向进程发送SIGKILL(信号9)，由内核立即强制终止进程。
        /// </summary>
        //private async Task KillMtAsync()
        //{
        //    await MyGM.AdbAsync("shell pkill -9 minitouch");
        //}

        /// <summary>
        /// 设置端口转发，所有发往电脑 127.0.0.1:port 的TCP数据，
        /// 都会被ADB服务进程通过USB或Wi-Fi连接透明地转发到Android设备上名为minitouch的抽象套接字服务。
        /// <para>
        /// forward	用于设置端口转发规则。
        /// </para>
        /// <para>
        /// tcp:port 让ADB使用TCP协议在本地 127.0.0.1:port 上监听。
        /// </para>
        /// <para>
        /// localabstract:minitouch 转发消息到设备上名为 minitouch 的抽象套接字。
        /// </para>
        /// </summary>
        private async Task SetupPortForwardAsync()
        {
            await MyGM.AdbAsync($"forward tcp:{ForwardPort} localabstract:stfservice");
        }
        #endregion

        #region Socket Connect / Reconnect
        /// <summary>
        /// TcpClient连接到服务端。
        /// </summary>
        /// <returns>是否连接成功。</returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                TcpClient client = new();
                Task connect = client.ConnectAsync("127.0.0.1", ForwardPort);
                Task timeout = Task.Delay(2000);

                Task completed = await Task.WhenAny(connect, timeout);
                if (completed == timeout)
                {
                    client.Close();
                    Debug.WriteLine($"连接stfservice超时");
                    return false;
                }

                _client = client;
                _stream = client.GetStream();
                _stream.ReadTimeout = 5000;
                _writer = new StreamWriter(_stream, Encoding.UTF8)
                {
                    NewLine = "\n",
                    AutoFlush = true,
                };
                _reader = new StreamReader(_stream, Encoding.UTF8);

                // 读取 minitouch 头信息（可选）
                //Task.Run(ReadHandshakeAsync);

                //await ReadHandshakeAsync();
                Debug.WriteLine($"已连接到 stfservice");
                _readTimer.Start();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"连接stfservice异常:{ex.Message}");
                CloseClientInternal();
                return false;
            }
        }

        private async Task ReadHandshakeAsync1()
        {
            // minitouch 第一行通常是版本信息，如 "v 1"
            if (await _reader.ReadLineAsync() == null)
                throw new IOException("minitouch 连接意外关闭");

            // 后面可能有多行，以 '^' 开头，如 "^ 2 10 3840"
            // 一直读到没有更多数据（或读到特定标识，视具体实现而定）
            // 安全做法：读取直到遇到空行或结束
            int i = 0;
            while (true)
            {
                i++;
                string line = await _reader.ReadLineAsync();
                if (line == null) break;       // 流结束
                if (string.IsNullOrWhiteSpace(line)) break; // 空行

                Debug.WriteLine($"{line}\n");
            }
            Debug.WriteLine($"minitouch 握手完成,{i}");
        }

        private async Task ReadHandshakeAsync()
        {
            // minitouch 启动后通常会输出若干行头信息
            // 这里读取几行用于清空缓冲，也便于你后续扩展解析 max-x/max-y
            for (int i = 0; i < 5; i++)
            {
                //if (_stream != null && _stream.DataAvailable)
                {
                    string line = await _reader.ReadLineAsync();
                    //if (line == null)
                    //{
                    //    Debug.WriteLine($"NNNNN");
                    //    continue;
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"\r\n{i}:{line}");
                    //}
                    Debug.WriteLine($"\r\n{i++}:{line}");
                    await Task.Delay(200);
                }
                //else
                //{
                //    await Task.Delay(300);
                //}
            }
        }

        private async Task ReconnectLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_started && !IsConnected)
                    {
                        Debug.WriteLine($"检测到连接断开，尝试重连...");
                        await ConnectAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"重连异常: {ex.Message}");
                }

                try
                {
                    await Task.Delay(ReconnectIntervalMs, token).ConfigureAwait(false);
                }
                catch
                {
                    break;
                }
            }
        }

        #endregion

        #region Touch Protocol

        private void ExecuteTouchCommand(Action action)
        {
            lock (_syncRoot)
            {
                if (_disposed) return;

                if (!IsConnected)
                {
                    if (AutoReconnect)
                    {
                        // 这里不直接阻塞重连，先尝试一次
                        try
                        {
                            ConnectAsync().Wait(1500);
                        }
                        catch { }
                    }

                    if (!IsConnected)
                    {
                        Debug.WriteLine($"未连接 minitouch，触摸命令未发送。");
                        return;
                    }
                }

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"发送触摸命令失败: {ex.Message}");
                    CloseClientInternal();
                }
            }
        }

        private void SendLine(string cmd)
        {
            //if (_writer == null) throw new InvalidOperationException("尚未连接 minitouch");
            //_writer.WriteLine($"{cmd}\n");
            //_writer.Flush();

            byte[] data = Encoding.ASCII.GetBytes(cmd + "\n");
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
            Debug.WriteLine($"发送指令: {cmd}");
        }

        private void Commit()
        {
            //if (_writer == null) throw new InvalidOperationException("尚未连接 minitouch");
            //_writer.WriteLine("c\n");
            //_writer.Flush();

            byte[] data = Encoding.ASCII.GetBytes("c\n");
            _stream.Write(data, 0, data.Length);
        }

        #endregion

        #region Close / Dispose
        /// <summary>
        /// 关闭内部客户端的连接。
        /// </summary>
        private void CloseClientInternal()
        {
            try { _writer?.Flush(); } catch { }

            try { _writer?.Dispose(); } catch { }
            try { _reader?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }

            _writer = null;
            _reader = null;
            _stream = null;
            _client = null;
            _touchDown = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                StopAsync().Wait(2000);
            }
            catch { }

            lock (_syncRoot)
            {
                CloseClientInternal();
            }
        }

        #endregion

    }
}
