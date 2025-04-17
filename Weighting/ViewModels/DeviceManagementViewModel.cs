using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;


namespace Weighting.ViewModels
{
   public class DeviceManagementViewModel : INotifyPropertyChanged
    {


        public DeviceManagementViewModel() 
        {
            
            SearchCommand = new RelayCommand(SearchCommandExecute);

            AddRowCommand = new RelayCommand(AddRowCommandExecute);

            ChangeRowCommand = new RelayCommand(ChangeRowCommandExecute);

            DeleteRowCommand = new RelayCommand(DeleteRowCommandExecute);


            Devicelist = new ObservableCollection<Devices>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _deviceName;
        public string DeviceName
        {
            get => _deviceName;
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    OnPropertyChanged(nameof(DeviceName));
                }
            }
        }

        private string _maxWeight;
        public string MaxWeight
        {
            get => _maxWeight;
            set
            {
                if (_maxWeight != value)
                {
                    _maxWeight = value;
                    OnPropertyChanged(nameof(MaxWeight));
                }
            }
        }

        private string _brand;
        public string Brand
        {
            get => _brand;
            set
            {
                if (_brand != value)
                {
                    _brand = value;
                    OnPropertyChanged(nameof(Brand));
                }
            }
        }

        private string _ipAddress;
        public string IPAddress
        {
            get => _ipAddress;
            set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    OnPropertyChanged(nameof(IPAddress));
                }
            }
        }

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }

        private DateTime _productionDate = DateTime.Now;
        public DateTime ProductionDate
        {
            get => _productionDate;
            set
            {
                if (_productionDate != value)
                {
                    _productionDate = value;
                    OnPropertyChanged(nameof(ProductionDate));
                }
            }
        }

        private int _scalingID;
        public int ScalingID
        {
            get => _scalingID;
            set
            {
                if (_scalingID != value)
                {
                    _scalingID = value;
                    OnPropertyChanged(nameof(ScalingID));
                }
            }
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged(nameof(Unit));
                }
            }
        }
        public IEnumerable<string> ScalingNumsStr => new[] { "1号秤台", "2号秤台", "3号秤台", "4号秤台", "5号秤台", "6号秤台", "7号秤台", "8号秤台", "9号秤台" };

        public IEnumerable<string> Units => new[] { "g", "kg" };
        public RelayCommand SearchCommand { get; set; }

        public RelayCommand AddRowCommand { get; set; }

        public RelayCommand ChangeRowCommand { get; set; }

        public RelayCommand DeleteRowCommand { get; set; }
    //1.如何获取数据?获取数据保存在哪里才能进行比较?。
    //思路1.我要比较重量首先我要获取各个秤台的重量（形成配方》秤台设备列表的映射，通过遍历配方列表获取数据）
    //数据获取比较思路1.设置秤台2.设置通讯3.稳定传输后接收到数据4.写入到一个结果列表5.根据配方进行比较
    public ObservableCollection<Devices> Devicelist { get; set; }

        private Devices _addedDevices = new Devices();
        public Devices AddedDevices
        {
            get => _addedDevices;
            set
            {
                _addedDevices = value;
                OnPropertyChanged();
            }
        }
        private void SearchCommandExecute(object parameter)
        {
            //获取秤台列表

            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Devices.db";
            string sql = "SELECT * FROM DeviceList";
            Devicelist.Clear();
            try
            {
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    DataTable dt = db.ExecuteQuery(sql);

                    foreach (DataRow row in dt.Rows)
                    {
                        Devicelist.Add(new Devices
                        {
                            ID = DataRowHelper.GetValue<int>(row, "ID", 0),
                            IP = DataRowHelper.GetValue<string>(row, "IP", null),
                            Port = DataRowHelper.GetValue<int>(row, "Port", 0),
                            MaxWeights = DataRowHelper.GetValue<int>(row, "MaxWeights", 0),
                            Brant = DataRowHelper.GetValue<string>(row, "Brant", null),
                            DateOfManufature = DataRowHelper.GetValue<string>(row, "DateOfManufature", null),
                            DeviceName = DataRowHelper.GetValue<string>(row, "DeviceName", null),
                            Unit = DataRowHelper.GetValue<string>(row, "Unit", null),
                        });

                    }
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
       private void  DeleteRowCommandExecute(object parameter)
        {
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Devices.db";
            // SQL 删除语句
            string sql = "DELETE FROM DeviceList WHERE ID = @id";

            MessageBoxResult result = MessageBox.Show(
               "将要删除用户，确定要继续吗？",
               "提示",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question);


            if (parameter is Devices && result == MessageBoxResult.Yes)
            {
                Devices row = (Devices)parameter;
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    db.ExecuteNonQuery(sql, new Dictionary<string, object>
                    {
                        {"@id",row.ID}
                    });

                    Devicelist.Remove(row);
                }
            }
        }

        private async void AddRowCommandExecute(object parameter)
        {

           
            var dialog = new Views.AddDeviceDialog();
            //await DialogHost.Show(dialog, "RootDialog");
           var result =  await DialogHost.Show(dialog, "DeviceManagementDialog");
            try
            {
                if (result?.ToString() == "True")
                {
                    string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Devices.db";

                    using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                    {
                        string sql = "INSERT INTO DeviceList( IP, Port, ScalingID, MaxWeights,Brant, DateOfManufature, DeviceName,Unit) VALUES(@iP, @port, @scalingID, @maxWeights,@brant, @dateOfManufature, @deviceName,@unit)";

                        db.ExecuteNonQuery(sql, new Dictionary<string, object>
                        {
                            { "@iP",IPAddress},
                            {"@port",Port},
                            { "@scalingID",ScalingID },
                            { "@maxWeights",MaxWeight},
                            { "@brant",Brand},
                            { "@dateOfManufature",ProductionDate.ToString()}, //在combox写入数据时浪费了很多事件
                            { "@deviceName",ScalingID.ToString() + "号秤台"},
                            {"@unit",Unit}
                        });

                    }
                }
            } catch (Exception ex)
            { 
                MessageBox.Show($"发生{ex.Message}异常，数据添加失败！");
            }
           
        }

        private  void ChangeRowCommandExecute(object parameter)
        {
            Devices row = (Devices)parameter;


        }
    }
}
