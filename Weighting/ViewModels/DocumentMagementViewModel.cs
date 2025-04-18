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
using System.Collections.ObjectModel;
using System.Data;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace Weighting.ViewModels
{
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
            SearchCommand = new RelayCommand(SearchCommandExecute);

            PrintCommand = new RelayCommand(PrintCommandExecute);
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
                            BatchNumber = DataRowHelper.GetValue<string>(row, "BatchNumber",null)
                        }
                        );

                }
            }
        }
        private void PrintCommandExecute(object parameter)
        {

        }
        private string ZPLCommand = $"";
     





    }
}
