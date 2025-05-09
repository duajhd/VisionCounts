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
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Management;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data.Common;
namespace Weighting.ViewModels
{
    public class RawPrinterHelper
    {
        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA pDocInfo);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

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

        public static bool SendStringToPrinter(string printerName, string zplCommand)
        {
            IntPtr pBytes = IntPtr.Zero;
            int dwCount = zplCommand.Length;

            var docInfo = new DOCINFOA
            {
                pDocName = "ZPL Document",
                pDataType = "RAW"
            };

            bool success = false;
            if (OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                pBytes = Marshal.StringToCoTaskMemAnsi(zplCommand);

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
                Marshal.FreeCoTaskMem(pBytes);
            }
            return success;
        }
    }
    public   class DocumentMagementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                new PrintState { id = 0, printstate = "未打印" },
                new PrintState { id = 1, printstate = "已打印" },
            
            };
            SelectedItem = PrintStates[0]; // 默认选中第二个项
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

            string batchNumber = record.BatchNumber;
            string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
            string sql = $"SELECT * FROM MeasureData WHERE BatchNumber = '{batchNumber}'"; 

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
        public static string GenerateZplWithAutoLineBreak(List<string> lines, int labelWidthDots, int fontHeightDots = 30)
        {
            var sb = new StringBuilder();
           

            int startX = 50;
            int currentY = 110;
            int lineSpacing = fontHeightDots + 10;
            int approxCharWidth = fontHeightDots; // 估算字符宽度
            int maxCharsPerLine = labelWidthDots / 2 / approxCharWidth; // 最多打印半张纸宽

            foreach (var line in lines)
            {
                var brokenLines = BreakTextIntoLines(line, maxCharsPerLine);
                foreach (var part in brokenLines)
                {
                    sb.AppendLine($"^FO{startX},{currentY}^A1N,{fontHeightDots},{fontHeightDots}^FD{part}^FS");
                    currentY += lineSpacing;
                }
            }

          
            return sb.ToString();
        }

        private static List<string> BreakTextIntoLines(string text, int maxChars)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < text.Length; i += maxChars)
            {
                int len = Math.Min(maxChars, text.Length - i);
                lines.Add(text.Substring(i, len));
            }
            return lines;
        }
        private void SingleCommandExecute(object parameter)
        {
           
            Record row = (Record) parameter;

            // 查找 USB 打印机
            UsbDiscoverer usbDiscoverer = new UsbDiscoverer();
            DiscoveredUsbPrinter usbPrinter = null;

            foreach (DiscoveredUsbPrinter zebraprinter in UsbDiscoverer.GetZebraUsbPrinters(new ZebraPrinterFilter()))
            {
                Console.WriteLine("找到打印机：" + zebraprinter.ToString());
                usbPrinter = zebraprinter;
                break;
            }

            if (usbPrinter == null)
            {
                MessageBox.Show("未找到 USB 打印机");
                return;
            }

            // 使用 USB 连接打印机
            UsbConnection usbConnection = new UsbConnection(usbPrinter.Address);

            usbConnection.Open();

            var sb = new System.Text.StringBuilder();
            var lines = new List<string>();
            sb.AppendLine("^XA^CW1,E:SIMSUN.TTF^SEE:GB18030.DAT^CI28"); // 开始标签
            lines.Add($"配方名称：{row.FormulaName}");
            lines.Add($"混料批号：{row.BatchNumber}");
            lines.Add($"称重时间：{DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分")}");
            lines.Add($"操作人：{GlobalViewModelSingleton.Instance.Currentusers.UserName}");
           

            sb.AppendLine(GenerateZplWithAutoLineBreak(lines,850));
            sb.AppendLine($"^FO370,110^BQN,2,10 ^FDLA,{row.BatchNumber}");
            sb.AppendLine("^XZ"); // 结束标签

            // 关键：将 ZPL 字符串编码为 UTF-8 字节流
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(sb.ToString());
            usbConnection.Write(utf8Bytes); // 直接写入字节数据




            usbConnection.Close();

          
            string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
            string sql = $"UPDATE  MeasureResults SET IsPrint=@isPrint WHERE BatchNumber = '{row.BatchNumber}'";
            //$"UPDATE Users SET  UserName=@userName,RoleId=@roleId WHERE UserId = '{row.ID}'";
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                db.ExecuteNonQuery(sql, new Dictionary<string, object>
                        {
                            { "@isPrint",1}
                        }); ;
            }
           
     

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
                sql = $"SELECT * FROM MeasureResults WHERE DATE(DateOfCreation) >= '{CreationDate.ToString("yyyy-MM-dd")}' AND IsPrint = {SelectedItem.id}";
            }else if (string.IsNullOrEmpty(FormulaName) && !string.IsNullOrEmpty(BatchNumber))
            {//方案名为空且批次号不为空
                sql = $"SELECT * FROM MeasureResults  WHERE  BatchNumber = '{BatchNumber}' AND DATE(DateOfCreation) >= '{CreationDate.ToString("yyyy-MM-dd")}' AND IsPrint = {SelectedItem.id}";
            }else if (!string.IsNullOrEmpty(FormulaName)&& string.IsNullOrEmpty(BatchNumber))
            {
                //方案名不为空且批次号为空
                sql = $"SELECT * FROM MeasureResults  WHERE  FormulaName = '{FormulaName}' AND DATE(DateOfCreation) >= '{CreationDate.ToString("yyyy-MM-dd")}' AND IsPrint = {SelectedItem.id}";
            }else if(!string.IsNullOrEmpty(FormulaName) && !string.IsNullOrEmpty(BatchNumber))
            {
                sql = $"SELECT * FROM MeasureResults  WHERE  FormulaName = '{FormulaName}' AND BatchNumber = '{BatchNumber}' AND DATE(DateOfCreation) >= '{CreationDate.ToString("yyyy-MM-dd")}' AND IsPrint = {SelectedItem.id}";
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
     
        private void PrintCommandExecute(object parameter)
        {


            try
            {


                // 查找 USB 打印机
                UsbDiscoverer usbDiscoverer = new UsbDiscoverer();
                DiscoveredUsbPrinter usbPrinter = null;

                foreach (DiscoveredUsbPrinter zebraprinter in UsbDiscoverer.GetZebraUsbPrinters(new ZebraPrinterFilter()))
                {
                    Console.WriteLine("找到打印机：" + zebraprinter.ToString());
                    usbPrinter = zebraprinter;
                    break;
                }

                if (usbPrinter == null)
                {
                    MessageBox.Show("未找到 USB 打印机");
                    return;
                }

                // 使用 USB 连接打印机
                UsbConnection usbConnection = new UsbConnection(usbPrinter.Address);

                usbConnection.Open();

                foreach (Record item in Items1)
                {
                    if (item.IsSelected)
                    {


                        var sb = new System.Text.StringBuilder();
                        var lines = new List<string>();
                        sb.AppendLine("^XA^CW1,E:SIMSUN.TTF^SEE:GB18030.DAT^CI28"); // 开始标签
                        lines.Add($"配方名称：{item.FormulaName}");
                        lines.Add($"混料批号：{item.BatchNumber}");
                        lines.Add($"称重时间：{DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分")}");
                        lines.Add($"操作人：{GlobalViewModelSingleton.Instance.Currentusers.UserName}");


                        sb.AppendLine(GenerateZplWithAutoLineBreak(lines, 850));
                        sb.AppendLine($"^FO370,110^BQN,2,10 ^FDLA,{item.BatchNumber}");
                        sb.AppendLine("^XZ"); // 结束标签

                        // 关键：将 ZPL 字符串编码为 UTF-8 字节流
                        byte[] utf8Bytes = Encoding.UTF8.GetBytes(sb.ToString());
                        usbConnection.Write(utf8Bytes); // 直接写入字节数据
                      
                        // pd.Print(); // 开始打印*/
                        item.IsPrint = 1;
                        string connectionStr = $"Data Source={GlobalViewModelSingleton.Instance.CurrentDirectory}Devices.db";
                        string sql = $"UPDATE  MeasureResults SET IsPrint=@isPrint WHERE BatchNumber = '{item.BatchNumber}'";
                        //$"UPDATE Users SET  UserName=@userName,RoleId=@roleId WHERE UserId = '{row.ID}'";
                        using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                        {
                            db.ExecuteNonQuery(sql, new Dictionary<string, object>
                        {
                            { "@isPrint",1}
                        }); ;
                        }
                    }


                }
                usbConnection.Close();



            }
            catch (Exception ex)
            {
                Console.WriteLine("打印失败：" + ex.Message);
               
            }
        
           

           
        }
        //检查是否有打印机连接成功
        public bool isConnectPrint()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
            ManagementObjectCollection printers = searcher.Get();

            foreach (ManagementObject printer in printers)
            {
                if (printer != null)
                {
                    return true;
                }
            }

            return false;
        }






    }
}
