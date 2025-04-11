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


        //用于查询配方
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
        public IEnumerable<string> ScalingNumsStr => new[] { "1号秤台", "2号秤台", "3号秤台", "4号秤台", "5号秤台", "6号秤台", "7号秤台", "8号秤台", "9号秤台" };

        public IEnumerable<string> Units => new[] { "g", "kg" };

        public IEnumerable<string> MaterialNames => new[] { "Binder A", "Binder B", "Binder C", " 粉末（10-1）", "粉末（10 - 2）" };
        public string SelecteScaling { get; set; }
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

        //保存更改后的配方（更改配方只ge更改配方内容）
         private void SaveFormula(object obj)
        {
            //保存数据的验证原则：1.string数据不能为空2.数值要换成整数
            if (string.IsNullOrEmpty(FormulaName)   || string.IsNullOrEmpty(Code))
            {
                MessageBox.Show("配方编码或配方名称不能为空!");
               
                return;
            }

            if (!ValidationValue())
            {
                MessageBox.Show("配料输入数据有误，请重检查并重新输入!");

                return;
            }


            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            string sql = "INSERT INTO PlatformScale( MaterialName, weights, UpperTolerance, LowerTolerance, Code, ScalingName, ScalingNum,MaterialUnit,ToleranceUnit,ScalingID) VALUES( @materialName, @weights, @upperTolerance, @lowerTolerance, @code, @scalingName, @scalingNum,@materialUnit,@toleranceUnit,@scalingID)";
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                db.ExecuteNonQuery("INSERT INTO  ProductFormula(Code,Name) VALUES(@code,@name)",new Dictionary<string, object>
                {
                    {"@code",Code },
                    {"@name", FormulaName}
                });
               

                foreach (var item in Items1)
                {
                    db.ExecuteNonQuery(sql, new Dictionary<string, object>
                {
                    { "@materialName",item.Item.MaterialName},
                    {"@weights",Math.Round(item.Item.weights,2) },
                            { "@upperTolerance", Math.Round(item.Item.UpperTolerance,2) },
                            { "@lowerTolerance",Math.Round(item.Item.LowerTolerance,2)},
                            { "@code",Code},
                            { "@scalingName","test"},
                            { "@scalingNum",item.Item.ScalingName}, //在combox写入数据时浪费了很多事件
                        { "@materialUnit", item.Item.MaterialUnit},
                        {"@toleranceUnit",item.Item.ToleranceUnit },
                        { "@scalingID",item.Item.ScalingID}


                });
                }
             
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
