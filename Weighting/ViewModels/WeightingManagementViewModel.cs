﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Weighting.Shared;

namespace Weighting.ViewModels
{
   

    //获取数据2.将数据写入到
    public class WeightingManagementViewModel : INotifyPropertyChanged,IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private List<DeviceClient> deviceClients  = new List<DeviceClient>();


        private List<DeviceClient> devices = new List<DeviceClient>();
    

        private string filePath = "example.txt";
        private FileStream fs = new FileStream("example.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        private StreamWriter writer;

        
        public WeightingManagementViewModel() 
        {
           
            ConnectionCommand = new RelayCommand(ConnectionCommandExecute);
            GenerateRecordsCommand = new RelayCommand(GenerateRecordsCommandExecute);
            writer = new StreamWriter(fs);

           

        }

        public RelayCommand ConnectionCommand { get; set; }
        public RelayCommand GenerateRecordsCommand { get; set; }

        //初始化连接，来获取数据
        private async  void ConnectionCommandExecute(Object parameter)
        {
          
            if (GlobalViewModelSingleton.Instance.AllScales.Count != 0)
            {
                foreach (Devices item in GlobalViewModelSingleton.Instance.AllScales)
                {
                    deviceClients.Add(new DeviceClient(item.IP,item.Port));
                }

                //绑定数据获取事件
                foreach (DeviceClient item in deviceClients)
                {
                    item.DataReceived += HandleDataReceived;
                    await item.ConnectAsync();
                }
            }
            else
            {
                MessageBox.Show("秤台未初始化");
            }
         
        }
        //生成一条称重记录
      private void GenerateRecordsCommandExecute(object parameter)
        {
            if (GlobalViewModelSingleton.Instance.IPToMeasureResult.Count == 0)
            {
                MessageBox.Show("没有激活的方案");
                return;
            }
            GlobalViewModelSingleton.Instance.CuurentFormula.BatchNumber += 1;
            string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
            string sql = "INSERT INTO MeasureResults( FormulaName, DateOfCreation, Operator, BatchNumber,IsPrint) VALUES( @formulaName, @dateOfCreation, @operator, @batchNumber,@isPrint)";
            string BatchNumber =$"{GlobalViewModelSingleton.Instance.CuurentFormula.FormulaName} /{DateTime.Now.ToString("yyyyMMdd")}-{GlobalViewModelSingleton.Instance.CuurentFormula.BatchNumber}";
            string FormulaName = GlobalViewModelSingleton.Instance.CuurentFormula.FormulaName;
            string Operator = GlobalViewModelSingleton.Instance.Currentusers.UserName;
            string OperatorDateStr = DateTime.Today.ToString("yyyy-MM-dd");

            try
            {
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    db.ExecuteNonQuery(sql, new Dictionary<string, object>
                     {
                        {"@formulaName", FormulaName},
                        {"@dateOfCreation",OperatorDateStr },
                        {"@operator", Operator},
                        {"@batchNumber", BatchNumber},
                        { "@isPrint",0}
                     });
                }

                sql = "INSERT INTO MeasureData(ScalingNum,MaterialName,ActualWeight,FormulaName,BatchNumber) VALUES(@scalingNum,@materialName,@actualWeight,@formulaName,@batchNumber)";
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    foreach (KeyValuePair<string, MeasureResult> item in GlobalViewModelSingleton.Instance.IPToMeasureResult)
                    {
                        MeasureResult measureResult = item.Value;

                        db.ExecuteNonQuery(sql, new Dictionary<string, object>
                     {
                        {"@scalingNum", measureResult.ScalingID},
                        {"@materialName",measureResult.MaterialName },
                        {"@actualWeight", measureResult.Result},
                        {"@formulaName", FormulaName},
                        {"@batchNumber" ,BatchNumber}
                     });
                    }
                }
              

                connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}formula.db";
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    db.ExecuteNonQuery($"UPDATE ProductFormula SET  BatchNumber = @value WHERE Name = '{GlobalViewModelSingleton.Instance.CuurentFormula.FormulaName}'", new Dictionary<string, object>
                    {
                        {  "@value" ,GlobalViewModelSingleton.Instance.CuurentFormula.BatchNumber}
                    });
                }



            } 
            catch(Exception ex)
            {
                MessageBox.Show($"记录生成失败！ERROR:{ex.Message}");
            }

           

        }

      
        //在这个函数里校验数据、获取数据、比较数据、设置状态
        private void HandleDataReceived(object sender, DataReceivedEventArgs e)
        {
            var device = sender as DeviceClient;
            string IP = e.Host;
            
           
            if (device != null)
            {
                writer.WriteLine(BitConverter.ToString(e.ReceivedData)+"--"+e.Host);
                //如果单位是g
                if(e.ReceivedData.Length == 17)
                {
                    if (Validate(e.ReceivedData))
                    {
                        //
                        byte[] valuesPart = new byte[8];
                        Array.Copy(e.ReceivedData, 7, valuesPart, 0, 7);

                        //提取值并转换到10进制
                        float values = parseg(valuesPart);

                        string unit = GlobalViewModelSingleton.Instance.IPToMeasureResult[IP].MaterialUnit;

                        //如果目标秤台是kg，则需要单位转化成kg
                        if(unit == "kg")
                        {
                            GlobalViewModelSingleton.Instance.IPToMeasureResult[IP].Result = values/1000;
                        }
                    }
                }
                //如果单位是kg
                else if(e.ReceivedData.Length == 18)
                {
                    if (Validate(e.ReceivedData))
                    {
                        //
                        byte[] valuesPart = new byte[8];
                        Array.Copy(e.ReceivedData, 7, valuesPart, 0, 7);

                       
                        //提取值并转换到10进制
                        float values = ParseKg(valuesPart);

                        string unit = GlobalViewModelSingleton.Instance.IPToMeasureResult[IP].MaterialUnit;

                        //如果目标秤台是g，则需要单位转化成g
                        if (unit == "g")
                        {
                            GlobalViewModelSingleton.Instance.IPToMeasureResult[IP].Result = values * 1000;
                        }
                    }
                }
                
               
            }//20-20-30-2E-32-38-30-6B-67
        }//32-37-39    30-30-30"53-54-2C-4E-54-2C-2B-20-20-30-2E-30-30-30-6B-67-0D-0A"  "53-54-2C-4E-54-2C-2B-20-20-30-2E-30-30-30-6B-67-0D-0A"
        //g是17位；kg是18字节(取出真实值的过程1.遍历数据部分2.计算小数点距离最后一位，通过移位实现)
        //1.分配好IP 2.定义一个18位字节的数组。3.判断是否单位正确4.
        void IDisposable.Dispose()
        {
            foreach (DeviceClient item in deviceClients)
            {
                item.Disconnect();
            }
            fs.Close();
            fs.Dispose();
            writer = null;

           
        }
        //获取真实数值,单位为kg时
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

                result += lowFourBits * (int)Math.Pow(lowFourBits, buffer.Length - (i+1));
               //0000279  =>279 7-4 = 3

            }

            return (float)result;
        }
        //private float ParseKg(byte[] buffer)
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
            if (dataWithCRC.Length < 17) return false;

            int len = dataWithCRC.Length;

            ushort crcCalculated = ComputeCRC(dataWithCRC, len - 2);

            ushort crcInData = BitConverter.ToUInt16(dataWithCRC, len - 2);

            return crcCalculated == crcInData;
        }
    }
}
