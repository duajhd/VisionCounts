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
        private string _name;
        public string Name
        {
            get => _name;
            set{
                _name = value;
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
        public MixedMaterial SelectedFormulaName { get; set; }

        
        //1.读取
        public  PlanManagementViewModel() 
        {
            Items1 = new ObservableCollection<MixedMaterial>();
            EdtingScalingData = new ObservableCollection<SelectableViewModel<PlatformScale>>();
           
            SearchCommand = new RelayCommand(SearchCommandExecute);

            ChangeRowCommand = new RelayCommand(ChangeRowCommandExecute);
        }

        public RelayCommand SearchCommand { get; set; }

        public RelayCommand ChangeRowCommand { get; set; }

        private void SearchCommandExecute(object obj)
        {
            Items1.Clear();
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            string sql = $"SELECT * FROM ProductFormula WHERE Name = '{Name}'";
            if (string.IsNullOrEmpty(Name))
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
                            Name = DataRowHelper.GetValue<string>(row, "Name", null),
                            Creator = DataRowHelper.GetValue<string>(row, "Creator", null),
                        }
                        );

                }
            }
        }

        private async void ChangeRowCommandExecute(object obj)
        {
            MixedMaterial row = (MixedMaterial)obj;

            SelectedFormulaName = row;


            if (string.IsNullOrEmpty(row.Name))
            {
                MessageBox.Show("配方编码或配方名称不能为空!");

                return;
            }
            Search(row.Name);
            var dialog = new Views.ChangeFormulaDialog();
          
             //await DialogHost.Show(dialog, "RootDialog");
           var result =   await DialogHost.Show(dialog, "changeFormulaDialog");
            if(result.ToString() == "True")
            {
                ChangeFormula();
            }
        }
        //修改配方时根据选定的配方名，从数据库读取配料列表
        private void Search(string Name)
        {
            EdtingScalingData.Clear();


            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            string sql = $"SELECT A.*, B.Name FROM PlatformScale A INNER JOIN ProductFormula B ON A.Name = B.Name  WHERE A.Name = '{Name}'";
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
                            ID = DataRowHelper.GetValue<int>(row, "ID",0),

                            MaterialName = DataRowHelper.GetValue<string>(row, "MaterialName", null),

                            weights = DataRowHelper.GetValue<float>(row, "weights", 0f),

                            UpperTolerance = DataRowHelper.GetValue<float>(row, "UpperTolerance", 0f),

                            LowerTolerance = DataRowHelper.GetValue<float>(row, "LowerTolerance", 0f),

                            MaterialUnit = DataRowHelper.GetValue<string>(row, "MaterialUnit", null),

                            ToleranceUnit = DataRowHelper.GetValue<string>(row, "ToleranceUnit", null),

                            ScalingID = DataRowHelper.GetValue<int>(row, "ScalingID", 0),

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


        private void ChangeFormula()
        {
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            try
            {

                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    foreach (SelectableViewModel<PlatformScale> item in EdtingScalingData)
                    {
                        string sql = $"UPDATE PlatformScale SET  MaterialName=@materialName, weights=@weights, UpperTolerance=@upperTolerance, LowerTolerance=@lowerTolerance,  ScalingName=@scalingName, ScalingNum=@scalingNum,MaterialUnit=@materialUnit,ToleranceUnit=@toleranceUnit,ScalingID=@scalingID WHERE ID = '{item.Item.ID}'";

                        db.ExecuteNonQuery(sql, new Dictionary<string, object>
                        {
                            { "@materialName",item.Item.MaterialName},
                            {"@weights",Math.Round(item.Item.weights,2) },
                                    { "@upperTolerance", Math.Round(item.Item.UpperTolerance,2) },
                                    { "@lowerTolerance",Math.Round(item.Item.LowerTolerance,2)},
                           
                                    { "@scalingName","test"},
                                    { "@scalingNum",item.Item.ScalingName}, //在combox写入数据时浪费了很多事件
                                { "@materialUnit", item.Item.MaterialUnit},
                                {"@toleranceUnit",item.Item.ToleranceUnit },
                                { "@scalingID",item.Item.ScalingID}


                        });
                    }
                }
            }
            catch
            {

            }
        }
        //保存更改后的配方（更改配方只ge更改配方内容）
      
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
