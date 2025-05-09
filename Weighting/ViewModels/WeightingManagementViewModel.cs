using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Weighting.Shared;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;


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
        public bool pushERP(string[] weightRecord)
        {
            //K3CloudApi client = new K3CloudApi("https://erp.quadrant.cn/k3cloud/");
           // K3CloudApiClient client = new K3CloudApiClient("https://erp.quadrant.cn/k3cloud/");
            //K3CloudApi client = new K3CloudApi("http://115.236.169.2:8181/k3cloud");     zjkjhl0144.
            try
            {
                //659193fec0e84d
                K3CloudApiClient client = new K3CloudApiClient("http://10.11.18.24/K3Cloud/");
                var loginResult = client.ValidateLogin("6819b5bd88b930", "ERP-开发", "147258Zj!@#", 2052);
                //var loginResult = client.Login("65cb1ca55b2f44", "ERPAPI", "Quadrant2023!#@", 2052);// 正：639691765153c9 659c049e433c9e      测试："650a55d6cf9b51", "ERP-开发", "zj@123456789"           
                JObject jsonObject = JObject.Parse((string)loginResult);
                
                if (1 == (int)jsonObject["LoginResultType"] )
                {
                    //用于记录结果
                    StringBuilder Info = new StringBuilder();
                    //业务对象标识
                    string formId = "VNVC_HLPH";
                    //请求参数，要求为json字符串
                    string jsonString = string.Format(@"
                {{
                    ""NeedUpDateFields"": [],
                    ""NeedReturnFields"": [],
                    ""IsDeleteEntry"": ""true"",
                    ""SubSystemId"": """",
                    ""IsVerifyBaseDataField"": ""false"",
                    ""IsEntryBatchFill"": ""true"",
                    ""ValidateFlag"": ""true"",
                    ""NumberSearch"": ""true"",
                    ""IsAutoAdjustField"": ""false"",
                    ""InterationFlags"": """",
                    ""IgnoreInterationFlag"": """",
                    ""IsControlPrecision"": ""false"",
                    ""ValidateRepeatJson"": ""false"",
                    ""IsAutoSubmitAndAudit"": ""true"",
                    ""Model"": {{
                        ""FID"": 0,
                        ""FNumber"": ""{0}"",
                        ""FName"": ""{1}"",
                        ""F_VNVC_OrgId"": {{
                            ""FNumber"": ""2060""
                        }},
                        ""F_VNVC_CreateDate"": ""{2}"",
                        ""F_VNVC_User"": ""{3}"",
                        ""F_VNVC_Entity"": [
                            {{
                                ""FEntryID"": 0,
                                ""F_VNVC_FAMC"": ""{1}"",
                                ""F_VNVC_PCH"": ""{0}"",
                                ""F_VNVC_ZZL"": ""{4}"",
                                ""F_VNVC_PF1"": ""{5}"",
                                ""F_VNVC_PF2"": ""{6}"",
                                ""F_VNVC_PF3"": ""{7}"",
                                ""F_VNVC_PF4"": ""{8}"",
                                ""F_VNVC_PF5"": ""{9}"",
                                ""F_VNVC_PF6"": ""{10}"",
                                ""F_VNVC_PF7"": ""{11}"",
                                ""F_VNVC_PF8"": ""{12}"",
                                ""F_VNVC_CZ"": ""{13}"",
                                ""F_VNVC_BZ"": ""{14}""
                            }}
                        ]
                    }}
                }}", weightRecord[3], weightRecord[2], weightRecord[0], weightRecord[1], weightRecord[4], weightRecord[5], weightRecord[6], weightRecord[7], weightRecord[8], weightRecord[9], weightRecord[10], weightRecord[11], weightRecord[12], weightRecord[13], weightRecord[14]);


                    //调用接口
                    var resultJson = client.Save(formId, jsonString);

                     jsonObject = JObject.Parse((string)resultJson);
                    bool isSuccess = (bool)jsonObject["Result"]["ResponseStatus"]["IsSuccess"];
                    if (isSuccess)
                    {
                        MessageBox.Show("成功同步一条记录到ERP");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("同步记录到ERP失败!");

                    }
                }
                else
                {
                    MessageBox.Show("ERP登录校验未通过!");

                }

            }
            catch (Exception e)
            {
                MessageBox.Show("ERP同步异常!" + e);

            }
            return false;
        }
        //生成一条称重记录
        private void GenerateRecordsCommandExecute(object parameter)
        {
            if (GlobalViewModelSingleton.Instance.IPToMeasureResult.Count == 0)
            {
                MessageBox.Show("没有激活的方案");
                return;
            }

            int SatisfiedSacleCounts = GlobalViewModelSingleton.Instance.IPToMeasureResult.Count(pair=>pair.Value.IsSatisfied==false);
            //if (SatisfiedSacleCounts!=0)
            //{
            //    MessageBox.Show("有秤台未达标准重量！全部达标后才可生成");
            //    return;
            //}
            //批次当天切换方案不会清零，而且可以继承别的方案的批号；过24点后自动清理   还是得从数据库获取一个批号
            GlobalViewModelSingleton.Instance.CuurentFormula.BatchNumber = GetTodayBatchNum();
            
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

                //保存称重记录
                string[] weightRecord = new string[20];
                float totalWeights = 0.0f; //总重量，单位是kg
                weightRecord[0] = DateTime.Now.ToString("yyyy-MM-dd");
                weightRecord[1] = GlobalViewModelSingleton.Instance.CuurentFormula.Creator;
                weightRecord[2] = GlobalViewModelSingleton.Instance.CuurentFormula.FormulaName;
                weightRecord[3] = $"{GlobalViewModelSingleton.Instance.CuurentFormula.FormulaName} /{DateTime.Now.ToString("yyyyMMdd")}-{GlobalViewModelSingleton.Instance.CuurentFormula.BatchNumber}";
                int index = 5;
                foreach (MeasureResult results in GlobalViewModelSingleton.Instance.IPToMeasureResult.Values)
                {
                    // detailRecord.Weight + "" + detailRecord.Unit;
                    weightRecord[index] = $"{results.weights}{results.MaterialUnit}";
                    index += 1;
                    //计算总重量，单位是kg
                    if (results.MaterialUnit == "g")
                    {
                        totalWeights += (results.Result) / 1000;
                    }
                    else
                    {
                        totalWeights += results.Result;
                    }
                }
                weightRecord[4] = totalWeights.ToString();
                bool res = pushERP(weightRecord);
              

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


        private int GetTodayBatchNum()
        {
            string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
              string sql = $"SELECT BatchNumber FROM MeasureResults WHERE DATE(DateOfCreation) = '{DateTime.Now.ToString("yyyy-MM-dd")}'";
           // string sql = $"SELECT BatchNumber FROM MeasureResults WHERE DATE(DateOfCreation) = '2025-05-10'";
            List<string> batchnums = new List<string>();
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                DataTable dt = db.ExecuteQuery(sql);
                

              foreach (DataRow row in dt.Rows)
              {
                    batchnums.Add(DataRowHelper.GetValue<string>(row, "BatchNumber", null));
              }
            }
            int? max = batchnums
            .Select(s =>
            {
                var match = Regex.Match(s, @"-(\d+)$");
                return match.Success ? (int?)int.Parse(match.Groups[1].Value) : null;
            })
            .Where(x => x.HasValue)
            .Max();
            if (max>0&& max!=0)
            {
                return max.Value+1;
            }
            else{
                return 1;
            }
        }
    }
}
