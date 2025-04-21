using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Collections.ObjectModel;
using System.Data;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer.Discovery;
using Zebra.Sdk.Printer;
using System.Runtime.InteropServices;

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
            SearchCommand = new RelayCommand(SearchCommandExecute);

            PrintCommand = new RelayCommand(PrintCommandExecute);
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
        public ObservableCollection<Record> Items1 { get; set; }
        public RelayCommand SearchCommand { get; set; }

        public RelayCommand PrintCommand { get; set; }
        private void SearchCommandExecute(object parameter)
        {
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Devices.db";
            string sql = $"SELECT * FROM MeasureResults  WHERE  = '{FormulaName}'";
            if (string.IsNullOrEmpty(FormulaName))
            {
                //不填用户名，查出所有用户
                sql = $"SELECT * FROM MeasureResults ";
            }
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                DataTable dt = db.ExecuteQuery(sql);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("未查找到该配方");
                    return;
                }
                //执行到这里说明查询成功
                if (Items1.Count > 0) Items1.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    Items1.Add(
                        new Record
                        {
                            FormulaName = DataRowHelper.GetValue<string>(row, "FormulaName", null),
                            DateOfCreation = DataRowHelper.GetValue<string>(row, "DateOfCreation", null),
                            Operator = DataRowHelper.GetValue<string>(row, "Operator", null),
                            BatchNumber = DataRowHelper.GetValue<string>(row, "BatchNumber",null),
                            IsPrint = DataRowHelper.GetValue<int>(row, "IsPrint", 0)
                        }
                        );

                }
            }
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
            string zpl = @"^XA^PW240^LL160^FO70,20^BQN,2,6^FDLA,https://your-url.com/abc123^FS^XZ";
            string printerName = "ZDesigner ZD888-203dpi ZPL";  // 替换为你的打印机名

            bool result = SendStringToPrinter(printerName, zpl);
            Console.WriteLine(result ? "打印成功" : "打印失败");
        }
        private string ZPLCommand = $"";
     





    }
}
