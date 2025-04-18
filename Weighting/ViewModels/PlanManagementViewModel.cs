using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using Weighting.Shared;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace Weighting.ViewModels
{
    public class PlanManagementViewModel : INotifyPropertyChanged
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
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }


        private bool isStimulated = false;
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
        public PlanManagementViewModel()
        {
            Items1 = new ObservableCollection<MixedMaterial>();
            EdtingScalingData = new ObservableCollection<SelectableViewModel<PlatformScale>>();

            SearchCommand = new RelayCommand(SearchCommandExecute);

            ChangeRowCommand = new RelayCommand(ChangeRowCommandExecute);

            AddRowCommand = new RelayCommand(AddRow);

            DeleteRowCommand = new RelayCommand(DeleteRow);

            StimulateCommand = new RelayCommand(StimulateCommandExceute);

            DelateFormulaCommand = new RelayCommand(DelateFormulaCommandExecute);

        }

        public RelayCommand SearchCommand { get; set; }

        public RelayCommand ChangeRowCommand { get; set; }

        public RelayCommand AddRowCommand { get; set; }

        //删除检出配方的一个配料
        public RelayCommand DeleteRowCommand { get; set; }

        //激活配方
        public RelayCommand StimulateCommand { get; set; }

        //删除配方

        public RelayCommand DelateFormulaCommand { get; set; }

        //删除一个配方，同时删除所有配料
        //20250413,一开始不能删除提示参数不足，后来可以删除却没有级联删除，为什么?
        private void DelateFormulaCommandExecute(object parameter)
        {
            MixedMaterial row = (MixedMaterial)parameter;

            if (!string.IsNullOrEmpty(row.Name))
            {

                string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
                // SQL 删除语句
                string sql = $"DELETE FROM ProductFormula WHERE Name = '{row.Name}'";

                MessageBoxResult result = MessageBox.Show(
                   "将要删除配方，确定要继续吗？",
                   "提示",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Question);


                if (result == MessageBoxResult.Yes)
                {

                    using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                    {
                        db.ExecuteNonQuery(sql);

                        Items1.Remove(row);
                    }
                }
            }
        }
        //使用这个搜索逻辑需确保所有方案均在未激活状态
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
                
                foreach (DataRow row in dt.Rows)
                {
                    Items1.Add(

                        new MixedMaterial
                        {
                            ID = DataRowHelper.GetValue<int>(row, "ID", 0),
                            Code = DataRowHelper.GetValue<string>(row, "Code", null),
                            Name = DataRowHelper.GetValue<string>(row, "Name", null),
                            Creator = DataRowHelper.GetValue<string>(row, "Creator", null),
                            IsStimulated = false
                        }
                        );

                }
            }
        }

        //激活配方
        //客户端配置、应该放在这里
        //激活配方前，首先应该停止采集
        //1.点击激活配方2.读取配料表3.形成IP=>measureresult的映射4.创建客户端5.开始采集并将采集结果存到字典
        private  void StimulateCommandExceute(object parameter)
        {
            GlobalViewModelSingleton.Instance.CuurentFormula.ScalesData.Clear();

            MixedMaterial rowforstimulation = (MixedMaterial)parameter;




            if (string.IsNullOrEmpty(rowforstimulation.Name))
            {
                MessageBox.Show("配方编码或配方名称不能为空!");

                return;
            }

            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db";
            string sql = $"SELECT A.*, B.Name FROM PlatformScale A INNER JOIN ProductFormula B ON A.Name = B.Name  WHERE A.Name = '{rowforstimulation.Name}'";





            //读取新配方
            GlobalViewModelSingleton.Instance.CuurentFormula.FormulaName = rowforstimulation.Name;
            GlobalViewModelSingleton.Instance.CuurentFormula.Code = rowforstimulation.Code;
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
                DataTable dt = db.ExecuteQuery(sql);

                foreach (DataRow row in dt.Rows)
                {

                    GlobalViewModelSingleton.Instance.CuurentFormula.ScalesData.Add(new PlatformScale
                    {
                        ID = DataRowHelper.GetValue<int>(row, "ID", 0),

                        MaterialName = DataRowHelper.GetValue<string>(row, "MaterialName", null),

                        weights = DataRowHelper.GetValue<float>(row, "weights", 0f),

                        UpperTolerance = DataRowHelper.GetValue<float>(row, "UpperTolerance", 0f),

                        LowerTolerance = DataRowHelper.GetValue<float>(row, "LowerTolerance", 0f),

                        MaterialUnit = DataRowHelper.GetValue<string>(row, "MaterialUnit", null),

                        ToleranceUnit = DataRowHelper.GetValue<string>(row, "ToleranceUnit", null),

                        ScalingID = DataRowHelper.GetValue<int>(row, "ScalingID", 0),

                    });


                }


            }
            //生成IP到测量结果的映射（切换配方时，需要重新生成这个映射）
            GlobalViewModelSingleton.Instance.deviceClients.Clear();
            GlobalViewModelSingleton.Instance.IPToMeasureResult.Clear();
            foreach (PlatformScale item1 in GlobalViewModelSingleton.Instance.CuurentFormula.ScalesData)
            {
                foreach (Devices item2 in GlobalViewModelSingleton.Instance.AllScales)
                {
                    if (item1.ScalingID == item2.ScalingID)
                    {
                        GlobalViewModelSingleton.Instance.IPToMeasureResult.Add(item2.IP, new MeasureResult
                        {
                            ID = item1.ScalingID,
                            ScalingName = item1.ScalingName,
                            MaterialName = item1.ScalingName,
                            weights = item1.weights,
                            UpperTolerance = item1.UpperTolerance,
                            LowerTolerance = item1.LowerTolerance,

                            ScalingID = item1.ScalingID,
                            MaterialUnit = item1.MaterialUnit,
                            ToleranceUnit = item1.ToleranceUnit,
                            IsSatisfied = false,
                        });

                        //同步初始化秤台
                        GlobalViewModelSingleton.Instance.deviceClients.Add(new DeviceClient(item2.IP, item2.Port));
                    }
                }
            }

            //连接后立即开始采集
            //foreach (DeviceClient item in GlobalViewModelSingleton.Instance.deviceClients)
            //{

            //    await item.ConnectAsync();
            //}



            //先把所有置为false//数据上下文变了//当前选择的ID是否与本身的ID相同

            //如果有激活的，
            if (isStimulated)
            {
                foreach (MixedMaterial item in Items1)
                {
                    if (item.ID == rowforstimulation.ID)
                    {
                        item.IsStimulated = false;
                    }

                    isStimulated = false;
                }
            }
            else
            {
                foreach (MixedMaterial item in Items1)
                {
                    if (item.ID == rowforstimulation.ID)
                    {
                        item.IsStimulated = true;
                    }

                    isStimulated = true;
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
            if(result?.ToString() == "True")
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

        private void AddRow(object obj)
        {
            var item = new SelectableViewModel<PlatformScale>(new PlatformScale
            {
                MaterialName = "",

                weights = 0.00f,

                UpperTolerance = 0.00f,

                LowerTolerance = -0.00f,

                ScalingNum = "1"


            });

            item.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SelectableViewModel<PlatformScale>.IsSelected))

                    OnPropertyChanged(nameof(IsAllItems1Selected));
            };
            EdtingScalingData.Add(item);
        }

        private void DeleteRow(object obj)
        {

            if (obj != null)
            {
                SelectableViewModel<PlatformScale> p = (SelectableViewModel<PlatformScale>)obj;

                var resultes = EdtingScalingData.FirstOrDefault(item => p.Item.MaterialName == item.Item.MaterialName);
                if (resultes != null)
                {
                    EdtingScalingData.Remove(resultes);
                }
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
            return EdtingScalingData.Select(item => item.Item.ScalingID).Distinct().ToList().Count == EdtingScalingData.Count ? true : false;
        }
    }
}
