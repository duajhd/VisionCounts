using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using QRCoder;
using System.Windows;
using Weighting.Shared;
using System.Collections.ObjectModel;
using System.Data;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;
using Zebra.Sdk.Printer;
using System.Runtime.InteropServices;
using System.Windows.Ink;
using Microsoft.Xaml.Behaviors.Media;
using System.Xml.Linq;
using MaterialDesignThemes.Wpf;
using System.Drawing.Printing;
using System.Drawing;
using SkiaSharp;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Drawing.Text;
using static System.Net.Mime.MediaTypeNames;
using Windows.Networking;
using System.IO;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
namespace Weighting.ViewModels
{
   
    public   class DocumentMagementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendStringToPrinter(string printerName, string zplCommand)
        {
            IntPtr pBytes;
            int dwCount = zplCommand.Length;
            pBytes = Marshal.StringToCoTaskMemAnsi(zplCommand);

            var docInfo = new DOCINFOA
            {
                pDocName = "ZPL Document",
                pDataType = "RAW"
            };

            bool success = false;
            if (OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, docInfo))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        success = WritePrinter(hPrinter, pBytes, dwCount, out _);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            Marshal.FreeCoTaskMem(pBytes);
            return success;
        }
        public DocumentMagementViewModel() 
        {
            Items1 = new ObservableCollection<Record>();

            DetailedLists = new ObservableCollection<MeasureData>();

            SearchCommand = new RelayCommand(SearchCommandExecute);

            AllPrintCommand = new RelayCommand(PrintCommandExecute);

            SinglePrintCommand = new RelayCommand(SingleCommandExecute);

            DetailedInformationCommand = new RelayCommand(DetailedInformationCommandExecute);
            PrintStates = new ObservableCollection<PrintState> 
            {
                new PrintState { id = 1, printstate = "未打印" },
                new PrintState { id = 2, printstate = "已打印" },
            
            };
            SelectedItem = PrintStates[1]; // 默认选中第二个项
        }

        //
        private string _formulaName;
        public string FormulaName
        {
            get => _formulaName;
            set 
            { 
                _formulaName = value;
                OnPropertyChanged();
            }
        }
        //绑定批次号
        private string _batchNumber;
        public string BatchNumber
        {
            get => _batchNumber;
            set
            {
                _batchNumber = value; 
                OnPropertyChanged();
            }
        }
        private DateTime _creationDate = DateTime.Now;
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PrintState> PrintStates { get; set; }
        private PrintState _selectedItem;
        public PrintState SelectedItem
        {
            get => _selectedItem;
            set => _selectedItem = value;
        }
        //
        public bool? IsAllItems1Selected
        {
            get
            {
                var selected = Items1.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count == 1 ? selected.Single() : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    SelectAll(value.Value, Items1);
                    OnPropertyChanged();
                }
            }
        }

        private static void SelectAll(bool select, IEnumerable<Record> models)
        {
            foreach (var model in models)
            {
                model.IsSelected = select;
            }
        }
        //需要打印的记录
        public ObservableCollection<Record> Items1 { get; set; }


        //
        public ObservableCollection<MeasureData> DetailedLists { get; set; }
        public RelayCommand SearchCommand { get; set; }

        public RelayCommand AllPrintCommand { get; set; }

        public RelayCommand SinglePrintCommand { get; set; }

        public RelayCommand DetailedInformationCommand { get; set; }

        private async void DetailedInformationCommandExecute(object parameter)
        {
            DetailedLists.Clear();
            Record record = (Record)parameter;

            string  formulaName = record.FormulaName;
            string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
            string sql = $"SELECT * FROM MeasureData WHERE FormulaName = '{formulaName}'"; 

            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                DataTable dt = db.ExecuteQuery(sql);

                foreach (DataRow row in dt.Rows)
                {
                    DetailedLists.Add(

                        new MeasureData
                        {
                            MaterialName = DataRowHelper.GetValue<string>(row, "MaterialName", null),
                            ActualWeight = DataRowHelper.GetValue<float>(row, "ActualWeight", 0.0f),
                            ScalingNum = DataRowHelper.GetValue<int>(row, "ScalingNum", 0),
                        }
                        );

                }
            }
            var dialog = new Views.DetailedInformationDialog();

            //await DialogHost.Show(dialog, "RootDialog");RootDialog
            await DialogHost.Show(dialog, "DetailedInDialog");
        }
        private void SingleCommandExecute(object parameter)
        {
            Record row = (Record) parameter;
          
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(row.BatchNumber, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap image = qrCode.GetGraphic(3);

            Bitmap qrImage = qrCode.GetGraphic(3);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Near; // 水平对齐方式（左对齐）
            format.Trimming = StringTrimming.Word;

            
            // 打印二维码
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (sender, g) =>
            {


                int height = 37;
                System.Drawing.Font font = new System.Drawing.Font("黑体", 9f);
                Brush brush = new SolidBrush(Color.Black);
                g.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                int interval = 15;
                int pointX = 5;
                Rectangle destRect = new Rectangle(190, 30, image.Width, image.Height);
                g.Graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                height += 6;
                RectangleF layoutRectangle = new RectangleF(pointX, height, 180f, 85f);
                g.Graphics.DrawString("配方名称:" + row.FormulaName, font, brush, layoutRectangle, format);

                height += interval;
                /*layoutRectangle = new RectangleF(pointX, height, 230f, 85f);
                g.Graphics.DrawString("混料批号:" + asset.BatchNumber, font, brush, layoutRectangle);*/

                string text = "混料批号:" + row.BatchNumber;

                // 创建布局矩形，包括位置和大小
                layoutRectangle = new RectangleF(pointX, height, 180f, 85f);

                // 使用DrawString方法绘制文本，传递StringFormat以控制换行
                g.Graphics.DrawString(text, font, brush, layoutRectangle, format);

                height += interval + 10;
                layoutRectangle = new RectangleF(pointX, height, 180f, 85f);

                string ouputFormat = "yyyy年MM月dd日 HH时mm分";
                //DateTime dateTime = DateTime.ParseExact(createTime,inputFormat,CultureInfo.InvariantCulture);
                string createTime = row.DateOfCreation;
                g.Graphics.DrawString("称重时间:" + createTime, font, brush, layoutRectangle, format);

                height += interval + 10;
                layoutRectangle = new RectangleF(pointX, height, 180f, 85f);
                g.Graphics.DrawString("操作人:" + row.DateOfCreation, font, brush, layoutRectangle, format);
               
            };
            // 创建最终图像（白底）
          





            // 创建PrintPreviewDialog对象，并将PrintDocument对象关联到预览对话框
            /*     PrintPreviewDialog previewDialog = new PrintPreviewDialog();
                 previewDialog.Document = pd;

                 // 显示打印预览对话框
                 previewDialog.ShowDialog();*/


            pd.Print(); // 开始打印*/
     

        }
        //方案名或批次号为空查出所有记录2.有一个为空，根据其中一个查询3.
            private void SearchCommandExecute(object parameter)
        {
            string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
            Items1.Clear();

            string sql = $"SELECT * FROM MeasureResults ";
            if (string.IsNullOrEmpty(FormulaName)&& string.IsNullOrEmpty(BatchNumber))
            {
                //全为空，查出所有结果
                sql = $"SELECT * FROM MeasureResults WHERE DATE(DateOfCreation) > '{CreationDate.ToString("yyyy-MM-dd")}'";
            }else if (string.IsNullOrEmpty(FormulaName) && !string.IsNullOrEmpty(BatchNumber))
            {//方案名为空且批次号不为空
                sql = $"SELECT * FROM MeasureResults  WHERE  BatchNumber = '{BatchNumber}' AND DATE(DateOfCreation) > '{CreationDate.ToString("yyyy-MM-dd")}'";
            }else if (!string.IsNullOrEmpty(FormulaName)&& string.IsNullOrEmpty(BatchNumber))
            {
                //方案名不为空且批次号为空
                sql = $"SELECT * FROM MeasureResults  WHERE  FormulaName = '{FormulaName}' AND DATE(DateOfCreation) > '{CreationDate.ToString("yyyy-MM-dd")}'";
            }else if(!string.IsNullOrEmpty(FormulaName) && !string.IsNullOrEmpty(BatchNumber))
            {
                sql = $"SELECT * FROM MeasureResults  WHERE  FormulaName = '{FormulaName}' AND BatchNumber = '{BatchNumber}'  DATE(DateOfCreation) > '{CreationDate.ToString("yyyy-MM-dd")}'";
            }
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                DataTable dt = db.ExecuteQuery(sql);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("未查找到符合条件的的配方");
                    return;
                }
                //执行到这里说明查询成功
                if (Items1.Count > 0)
                {
                    Items1.Clear();
                }
                foreach (DataRow row in dt.Rows)
                {
                    Items1.Add(
                        new Record
                        {
                            FormulaName = DataRowHelper.GetValue<string>(row, "FormulaName", null),
                            DateOfCreation = DataRowHelper.GetValue<string>(row, "DateOfCreation", null),
                            Operator = DataRowHelper.GetValue<string>(row, "Operator", null),
                            BatchNumber = DataRowHelper.GetValue<string>(row, "BatchNumber", null),
                            IsPrint = DataRowHelper.GetValue<int>(row, "IsPrint", 0)
                        }
                        );

                }
                foreach (var model in Items1)
                {
                    model.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(SelectableViewModel<PlatformScale>.IsSelected))

                            OnPropertyChanged(nameof(IsAllItems1Selected));
                    };
                }
            }
        }
        public bool pushERP(string[] weightRecord)
        {
            //K3CloudApi client = new K3CloudApi("https://erp.quadrant.cn/k3cloud/");
            K3CloudApiClient client = new K3CloudApiClient("https://erp.quadrant.cn/k3cloud/");
            //K3CloudApi client = new K3CloudApi("http://115.236.169.2:8181/k3cloud");     zjkjhl0144.
            try
            {
                //659193fec0e84d
                var loginResult = client.Login("65cb1ca55b2f44", "ERPAPI", "Quadrant2023!#@", 2052);// 正：639691765153c9 659c049e433c9e      测试："650a55d6cf9b51", "ERP-开发", "zj@123456789"           
                if (loginResult)
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

                    JObject jsonObject = JObject.Parse((string)resultJson);
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
        private void PrintCommandExecute(object parameter)
        {
            //try
            //{
            //    // 寻找 USB 打印机
            //    Connection usbConnection = UsbDiscoverer.GetZebraUsbPrinters()[0].GetConnection();

            //    usbConnection.Open();

            //    ZebraPrinter printer = ZebraPrinterFactory.GetInstance(usbConnection);

            //    string zpl = "^XA^FO50,50^A0N,50,50^FDHello Zebra!^FS^XZ";
            //    usbConnection.Write(zpl);

            //    usbConnection.Close();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("打印失败: " + ex.Message);
            //}
            
          
           // Console.WriteLine(result ? "打印成功" : "打印失败");

            foreach (Record item in Items1)
            {
                string zpl = $"^XA^PW240^LL160^FO70,20^BQN,2,6^FDLA,{item.BatchNumber}^FS^XZ";
                // = "^XA^WDE:*.*^XZ";
               // zpl = "^XA^SEE:GB18030.DAT^FS^CWZ,E:SIMSUN.FNT^CI26^JMA^LL200^PW680^MD10^RP2^PON^LRN^LH0,0^FO20,20^AZN,72,72^FD123ABC^FS^PQ1^XZ";
                string printerName = "ZDesigner 888-DT";  // 替换为你的打印机名
                if (item.IsSelected == true)
                {
                    bool result = SendStringToPrinter(printerName, zpl);
                }
               
            }
        }
        private string ZPLCommand = $"";
     





    }
}
