using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Weighting.Shared;

namespace Weighting.ViewModels
{
    public   class DocumentMagementViewModel : INotifyPropertyChanged,IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DocumentMagementViewModel() 
        {
            ConnectionCommand = new RelayCommand(ConnectionCommandExecute);
            writer = new StreamWriter(fs);
        }

        public RelayCommand ConnectionCommand { get; set; }
        private List<DeviceClient> devices = new List<DeviceClient>();

        private string filePath = "example.txt";
        private FileStream fs = new FileStream("example.txt", FileMode.OpenOrCreate,FileAccess.Write,FileShare.None);
        private StreamWriter writer;
        private async void ConnectionCommandExecute(Object parameter)
        {
            
            devices.Add(new DeviceClient("192.168.0.7", 23));
            foreach (DeviceClient item in devices)
            {
                item.DataReceived += HandleDataReceived;
                await item.ConnectAsync();
            }
              

        
        }

        private void HandleDataReceived(object sender,DataReceivedEventArgs e)
        {
            var device = sender as DeviceClient;

            if (device!=null)
            {
                string line = BitConverter.ToString(e.ReceivedData);
                writer.WriteLine(BitConverter.ToString(e.ReceivedData)+"\n");
            }//20-20-30-2E-32-38-30-6B-67
        }//32-37-39    30-30-30"53-54-2C-4E-54-2C-2B-20-20-30-2E-30-30-30-6B-67-0D-0A"  "53-54-2C-4E-54-2C-2B-20-20-30-2E-30-30-30-6B-67-0D-0A"
        //g是17位；kg是18字节(取出真实值的过程1.遍历数据部分2.计算小数点距离最后一位，通过移位实现)
        //1.分配好IP 2.定义一个18位字节的数组。3.判断是否单位正确4.
        void IDisposable.Dispose()
        {
            foreach (DeviceClient item in devices)
            {
                item.Disconnect();
            }
            writer.Close();
            
            writer.Dispose();
            writer = null;
        }
        //获取真实数值
        private float Parse(byte[] buffer)
        {
            int result = 0;
            //8-14位是数据位下标
            

            int pow = 0;
            for (int i = 0;i< buffer.Length;i++)
            {
                byte lowFourBits = (byte)(buffer[i] & 0x0F); // 取出低四位

                result += lowFourBits*(int)Math.Pow(lowFourBits, buffer.Length - i);
                if (buffer[i] == 0x46)
                {
                    
                    pow = buffer.Length - (i + 1);//7-(3+1)，刚好是小数点的位置
                }
              
            }
            



            return (float)(result * Math.Pow(10, pow));
        }
        //private float Parse(byte[] buffer)
        //{
        //    List<int> digits = new List<int>();
        //    int decimalPos = -1;

        //    for (int i = 0; i < buffer.Length; i++)
        //    {
        //        if (buffer[i] == 0x46)
        //        {
        //            decimalPos = digits.Count; // 小数点前的位数
        //        }
        //        else
        //        {
        //            digits.Add(buffer[i] & 0x0F); // 取低四位数字
        //        }
        //    }

        //    double result = 0;
        //    for (int i = 0; i < digits.Count; i++)
        //    {
        //        result = result * 10 + digits[i];
        //    }

        //    if (decimalPos >= 0)
        //    {
        //        result /= Math.Pow(10, digits.Count - decimalPos);
        //    }

        //    return (float)result;
        //}
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
            if (dataWithCRC.Length < 3) return false;

            int len = dataWithCRC.Length;
            ushort crcCalculated = ComputeCRC(dataWithCRC, len - 2);
            ushort crcInData = BitConverter.ToUInt16(dataWithCRC, len - 2);

            return crcCalculated == crcInData;
        }
    }
}
