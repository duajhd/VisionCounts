using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Weighting.Shared
{
    public class DeviceClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _host;
        private int _port;

        // 定义事件
        //数据来时候是根据IP来的；所以要把数据写入到对应IP的对象Dictionary<ip,测量结果>；配方是只有秤台号比较时需要根据秤台号确定比较对象，需要《秤台号，IP》；秤台号，IP可以根据数据库读取
        //
        //今天需要做的工作就是，ping通IP地址
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public DeviceClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();
            Console.WriteLine($"Connected to {_host}:{_port}");

            // 启动异步读取任务
            _ = StartReceivingAsync();
        }

        private async Task StartReceivingAsync()
        {
            byte[] buffer = new byte[18];
            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // 连接关闭

                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    // 触发事件并传递设备信息
                    OnDataReceived(new DataReceivedEventArgs(receivedData, _host, _port));
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data from {_host}:{_port}: {ex.Message}");
            }
        }

        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            Console.WriteLine($"Disconnected from {_host}:{_port}");
        }
    }
}
