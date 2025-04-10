using MaterialDesignColors;
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
using System.Windows.Documents;

namespace Weighting.ViewModels
{
    public class PlanManagementViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<SelectableViewModel<PlatformScale>> EdtingScalingData { get; }
        //方案数据源
        public ObservableCollection<MixedMaterial> Items1 { get; set; }

        private string _code;
        public string Code
        {
            get => _code;
            set{
                _code = value;
                OnPropertyChanged();
            }
        }

        public bool? IsAllItems1Selected
        {
            get
            {
                var selected = EdtingScalingData.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count == 1 ? selected.Single() : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    SelectAll(value.Value, EdtingScalingData);
                    OnPropertyChanged();
                }
            }
        }
        private static void SelectAll(bool select, IEnumerable<SelectableViewModel<PlatformScale>> models)
        {
            foreach (var model in models)
            {
                model.IsSelected = select;
            }
        }
        //1.读取
        public  PlanManagementViewModel() 
        {
            Items1 = new ObservableCollection<MixedMaterial>();
            EdtingScalingData = new ObservableCollection<SelectableViewModel<PlatformScale>>();
            Search();
            SearchCommand = new RelayCommand(SearchCommandExecute);

            ChangeRowCommand = new RelayCommand(ChangeRowCommandExecute);
        }

        public RelayCommand SearchCommand { get; set; }

        public RelayCommand ChangeRowCommand { get; set; }

        private void SearchCommandExecute(object obj)
        {
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            string sql = $"SELECT * FROM ProductFormula WHERE Code = '{Code}'";
            if (string.IsNullOrEmpty(Code))
            {
                //不填用户名，查出所有用户
                sql = $"SELECT * FROM ProductFormula";
            }
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                DataTable dt = db.ExecuteQuery(sql);
                // 添加 ID 列
               

                // 给每一行赋值
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["ID"] = i + 1;
                }
                foreach (DataRow row in dt.Rows)
                {
                    Items1.Add(
                        
                        new MixedMaterial
                        {
                            ID = DataRowHelper.GetValue<int>(row, "ID", 0),
                            Code = DataRowHelper.GetValue<string>(row, "Code", null),
                            Creator = DataRowHelper.GetValue<string>(row, "Creator", null),
                        }
                        );

                }
            }
        }

        private async void ChangeRowCommandExecute(object obj)
        {
            var dialog = new Views.ChangeFormulaDialog();
            //await DialogHost.Show(dialog, "RootDialog");
            await DialogHost.Show(dialog, "changeFormulaDialog");
        }
        private void Search()
        {
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            string sql = "SELECT A.*, B.Name FROM PlatformScale A INNER JOIN ProductFormula B ON A.Code = B.Code";
            //4.08改为INNER JOIN
            //if (string.IsNullOrEmpty(Code_search) || string.IsNullOrEmpty(FormulaName_search))
            //{
            //    MessageBox.Show("配方编码或配方名称不能为空!");

            //    return;
            //}
            try
            {
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    DataTable dt = db.ExecuteQuery(sql);

                    foreach (DataRow row in dt.Rows)
                    {

                        EdtingScalingData.Add(new SelectableViewModel<PlatformScale>(new PlatformScale
                        {
                            MaterialName = DataRowHelper.GetValue<string>(row, "MaterialName", null),

                            weights = DataRowHelper.GetValue<float>(row, "weights", 0f),

                            UpperTolerance = DataRowHelper.GetValue<float>(row, "UpperTolerance", 0f),

                            LowerTolerance = DataRowHelper.GetValue(row, "LowerTolerance", 0f)

                        }));


                    }
                    //新增检索绑定IsSelected
                    foreach (var model in EdtingScalingData)
                    {
                        model.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(SelectableViewModel<PlatformScale>.IsSelected))

                                OnPropertyChanged(nameof(IsAllItems1Selected));
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取 DataRow 时发生错误: {ex.Message}");
            }

        }
        //写入数据库签验证数据是否合法
        private bool ValidationValue()
        {

            foreach (SelectableViewModel<PlatformScale> item in EdtingScalingData)
            {
                if (string.IsNullOrEmpty(item.Item.MaterialName)) return false;

                if (string.IsNullOrEmpty(item.Item.ToleranceUnit) || string.IsNullOrEmpty(item.Item.MaterialUnit)) return false;


                if (float.IsNaN(item.Item.UpperTolerance) || float.IsInfinity(item.Item.UpperTolerance) || item.Item.UpperTolerance < 0 || item.Item.UpperTolerance > 100) return false;

                if (float.IsNaN(item.Item.LowerTolerance) || float.IsInfinity(item.Item.LowerTolerance) || item.Item.LowerTolerance > 0 || item.Item.LowerTolerance > 100) return false;

                if (float.IsNaN(item.Item.weights) || float.IsInfinity(item.Item.weights) || item.Item.weights < 0) return false;
            }

            //判断是否相同值的秤台
            return EdtingScalingData.Select(item => item.Item.ScalingID).Distinct().ToList().Count == Items1.Count ? true : false;
        }
    }
}
