using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Collections.ObjectModel;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using System.Windows;
using System.Printing;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace Weighting.ViewModels
{
 
    public class ScalingManagemengViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<SelectableViewModel<PlatformScale>> Items1 { get; }

        public ObservableCollection<SelectableViewModel<PlatformScale>> EdtingScalingData {  get; } 
        public ScalingManagemengViewModel()

        {
            //初始化数据
            Items1 = CreateData();
            EdtingScalingData = new ObservableCollection<SelectableViewModel<PlatformScale>>();
            foreach (var model in Items1)
            {
                model.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SelectableViewModel<PlatformScale>.IsSelected))

                        OnPropertyChanged(nameof(IsAllItems1Selected));
                };
            }

            AddRowCommand = new RelayCommand(AddRow);
            DeleteRowCommand = new RelayCommand(DeleteRow);
            SaveFormulaCommand = new RelayCommand(SaveFormula);
            SearchCommand = new RelayCommand(Search);

        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //isselect后应该进到这里来1.修改isSelected2.触发更新IsAllItems1Selected
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


        //编辑配方的
        public bool? IsAllItems1Selected_Search
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

        public RelayCommand AddRowCommand { get; set; }
        public RelayCommand DeleteRowCommand { get; set; }

        public RelayCommand SaveFormulaCommand { get; set; }

        public RelayCommand SearchCommand { get; set; }
        private static void SelectAll(bool select, IEnumerable<SelectableViewModel<PlatformScale>> models)
        {
            foreach (var model in models)
            {
                model.IsSelected = select;
            }
        }
        //private static ObservableCollection<PlatformScale> CreateData()
        //{
        //    return new ObservableCollection<PlatformScale>
        //    {
        //        new PlatformScale
        //        {
        //              MaterialName = "stron",

        //              weights = 1.002f,

        //              UpperTolerance = 0.2000f,

        //              LowerTolerance = -0.2000f
        //        }

        //    };




        //}     
        private static ObservableCollection<SelectableViewModel<PlatformScale>> CreateData()
        {
            return new ObservableCollection<SelectableViewModel<PlatformScale>>{
                new SelectableViewModel<PlatformScale>( new PlatformScale
                {
                      MaterialName = "stron",

                      weights = 1.002f,

                      UpperTolerance = 0.2000f,

                      LowerTolerance = -0.2000f
                })

            };
        }
         public IEnumerable<string> ScalingNumsStr => new[] { "1号秤台", "2号秤台", "3号秤台", "4号秤台", "5号秤台", "6号秤台", "7号秤台", "8号秤台", "9号秤台" };

        public IEnumerable<string> Units => new[] { "g","kg"};

        public IEnumerable<string> MaterialNames => new[] { "Binder A", "Binder B", "Binder C", " 粉末（10-1）" , "粉末（10 - 2）" };
        public string SelecteScaling {  get; set; }

        //配方名称和配方编码

        private string _formulaName = null;
        public string FormulaName
        {
            get=> _formulaName;
            set
            {
                _formulaName = value;
                OnPropertyChanged();
            }
        }

        private string _code = null;
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                OnPropertyChanged();
            }
        }


        //配方名称和配方编码(用于搜索)

        private string _formulaName_search = null;
        public string FormulaName_search
        {
            get => _formulaName_search;
            set
            {
                _formulaName_search = value;
                OnPropertyChanged();
            }
        }

        private string _code_search = null;
        public string Code_search
        {
            get => _code_search;
            set
            {
                _code_search = value;
                OnPropertyChanged();
            }
        }
        #region relaycommand
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
            Items1.Add(item);
        }
        
        private void DeleteRow(object obj)
        {
            
            if (obj != null) 
            {
                SelectableViewModel<PlatformScale> p = (SelectableViewModel<PlatformScale>)obj ;

                var resultes = Items1.FirstOrDefault(item => p.Item.MaterialName == item.Item.MaterialName);
                if (resultes != null)
                {
                    Items1.Remove(resultes);
                }
            }

        }

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

        private void Search(object obj)
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

                                OnPropertyChanged(nameof(IsAllItems1Selected_Search));
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"读取 DataRow 时发生错误: {ex.Message}");
            }
          
        }
        #endregion
        //如果新增方案和编辑方案都已经1.激活方案2.

        private bool ValidationValue()
        {

            foreach (SelectableViewModel<PlatformScale> item in Items1)
            {
                if (string.IsNullOrEmpty(item.Item.MaterialName)) return false;

                if (string.IsNullOrEmpty(item.Item.ToleranceUnit) || string.IsNullOrEmpty(item.Item.MaterialUnit)) return false;
               

                if (float.IsNaN(item.Item.UpperTolerance) || float.IsInfinity(item.Item.UpperTolerance) || item.Item.UpperTolerance<0 || item.Item.UpperTolerance>100) return false;
              
                if (float.IsNaN(item.Item.LowerTolerance) || float.IsInfinity(item.Item.LowerTolerance) || item.Item.LowerTolerance > 0 || item.Item.LowerTolerance > 100) return false;
                
                if (float.IsNaN(item.Item.weights) || float.IsInfinity(item.Item.weights) || item.Item.weights < 0) return false;
            }

            //判断是否相同值的秤台
            return Items1.Select(item=> item.Item.ScalingID).Distinct().ToList().Count == Items1.Count?true:false;
        }
    }
}
