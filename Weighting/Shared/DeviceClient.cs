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
         

            // 启动异步读取任务
            _ = StartReceivingAsync();
        }
        private static string ByteArrayToHexString(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            return string.Join("-", Array.ConvertAll(bytes, b => b.ToString("X2")));
        }

        private async Task StartReceivingAsync()
        {
            byte[] buffer = new byte[18];

            float lowValue = 0.0f;
            float upValue = 0.0f;
            float standard = 0.0f;
            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // 连接关闭

                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    // 触发事件并传递设备信息
                    //或许不需要发出事件，直接写到Dictionary即可
                    //如果单位是g  && Validate(receivedData)
                    if (receivedData.Length == 17 )
                    {
                      
                            //
                            byte[] valuesPart = new byte[7];
                            Array.Copy(receivedData, 7, valuesPart, 0, 7);
                           //提取值并转换到10进制
                            float values = ParseValue(valuesPart);

                            string unit = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].MaterialUnit;

                            //如果目标秤台是kg，则需要单位转化成kg
                            if (unit == "kg")
                            {
                                 values = values / 1000;
                            }
                        GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].Result = values;
                        standard = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].weights;
                        lowValue = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].LowerTolerance+ standard;
                         upValue = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].UpperTolerance+standard;
                         
                        if ((values > lowValue) && (values < upValue))
                        {
                            GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].IsSatisfied = true;
                        }
                        else
                        {
                            GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].IsSatisfied = false;
                        }
                    }
                    //如果单位是kg
                    else if (receivedData.Length == 18)
                    {
                       
                            //
                            byte[] valuesPart = new byte[7];
                            Array.Copy(receivedData, 7, valuesPart, 0, 7);

                            //提取值并转换到10进制
                            float values = ParseValue(valuesPart);

                            string unit = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].MaterialUnit;

                            //如果目标秤台是g，则需要单位转化成g
                            if (unit == "g")
                            {
                                 values = values * 1000;
                            }
                          GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].Result = values;
                          standard = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].weights;
                          lowValue = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].LowerTolerance + standard;
                          upValue = GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].UpperTolerance + standard;

                          if ((values > lowValue) && (values < upValue))
                         {
                             GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].IsSatisfied = true;
                          }
                          else
                          {
                            GlobalViewModelSingleton.Instance.IPToMeasureResult[_host].IsSatisfied = false;
                          }
                    }


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


        private float ParseKg(byte[] buffer)
        {
            int result = 0;
            //8-14位是数据位下标


            int pow = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                byte lowFourBits = (byte)(buffer[i] & 0x0F); // 取出低四位

                result += lowFourBits * (int)Math.Pow(lowFourBits, buffer.Length - i);
                if (buffer[i] == 0x46)
                {

                    pow = buffer.Length - (i + 1);//7-(3+1)，刚好是小数点的位置
                }

            }




            return (float)(result * Math.Pow(10, pow));
        }

        private float parseg(byte[] buffer)
        {
            int result = 0;
            //8-14位是数据位下标

            for (int i = 0; i < buffer.Length; i++)
            {
                byte lowFourBits = (byte)(buffer[i] & 0x0F); // 取出低四位

                result += lowFourBits * (int)Math.Pow(lowFourBits, buffer.Length - (i + 1));
                //0000279  =>279 7-4 = 3

            }

            return (float)result;
        }
        private float ParseValue(byte[] buffer)
        {
            List<int> digits = new List<int>();
            int decimalPos = -1;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0x2e)
                {
                    decimalPos = digits.Count; // 小数点前的位数
                }
                else
                {
                    digits.Add(buffer[i] & 0x0F); // 取低四位数字
                }
            }

            double result = 0;
            for (int i = 0; i < digits.Count; i++)
            {
                result = result * 10 + digits[i];
            }

            if (decimalPos >= 0)
            {
                result /= Math.Pow(10, digits.Count - decimalPos);
            }

            return (float)result;
        }
        //计算CRC
        public static ushort ComputeCRC(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb)
                        crc ^= 0xA001;
                }
            }
            return crc;
        }

        // CRC校验，传入从串口读进来的数据
        public static bool Validate(byte[] dataWithCRC)
        {
            if (dataWithCRC.Length < 17) return false;

            int len = dataWithCRC.Length;

            ushort crcCalculated = ComputeCRC(dataWithCRC, len - 2);

            ushort crcInData = BitConverter.ToUInt16(dataWithCRC, len - 2);

            return crcCalculated == crcInData;
        }
    }
}
