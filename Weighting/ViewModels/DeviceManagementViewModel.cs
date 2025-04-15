using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Weighting.Shared;

namespace Weighting.ViewModels
{
   public class DeviceManagementViewModel : INotifyPropertyChanged
    {


        public DeviceManagementViewModel() 
        {
            
            SearchCommand = new RelayCommand(SearchCommandExecute);

            AddRowCommand = new RelayCommand(AddRowCommandExecute);

            ChangeRowCommand = new RelayCommand(ChangeRowCommandExecute);


            Devicelist = new ObservableCollection<Devices>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RelayCommand SearchCommand { get; set; }

        public RelayCommand AddRowCommand { get; set; }

        public RelayCommand ChangeRowCommand { get; set; }
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
                        });

                    }
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void AddRowCommandExecute(object parameter)
        {

            AddedDevices.DeviceName = "23";
            var dialog = new Views.AddDeviceDialog();
            //await DialogHost.Show(dialog, "RootDialog");
            await DialogHost.Show(dialog, "DeviceManagementDialog");
        }

        private  void ChangeRowCommandExecute(object parameter)
        {
            Devices row = (Devices)parameter;


        }
    }
}
