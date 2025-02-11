using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScrcpyEX.MyDefinition.MyClass
{
    internal class ADBSocketCommunicator
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public Task ConnectAsync(string host = "localhost", int port = 5555)
        {
            _client = new TcpClient(host, port);
            _stream = _client.GetStream();
            return Task.CompletedTask;
        }

        public async Task<string> SendCommandAsync(string command)
        {
            // 发送ADB命令
            byte[] commandBytes = Encoding.ASCII.GetBytes(command + "\n");
            await _stream.WriteAsync(commandBytes, 0, commandBytes.Length);

            // 读取响应
            StringBuilder response = new StringBuilder();
            do
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
            } while (true);

            return response.ToString();
        }

        public void Disconnect()
        {
            _stream.Dispose();
            _client.Dispose();
        }


    }
}
