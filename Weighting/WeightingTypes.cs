using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weighting
{
    class WeightingTypes
    {
    }

    public class Users
    {
       public  int ID {  get; set; }
       
       public string UserName { get; set; }
        public string RoleName { get; set; }

        public int RoleId { get; set; }
    }
    //秤台的结构体
    public struct Scaling
    {
        public string ScalingName { get; set; }

        public MaterialType materialType { get; set; }

        public float MaximunWeight { get; set; }

        //上公差
        public float UpperTolerance { get; set; }

        public float LowerTolerance { get; set; }

        //使用单位
        public string Units {  get; set; }
        //1.激活配方2.获取秤台数据3.计算结果是否亮绿灯
        //获取的各秤重量(有序的)和当前的配方
    }

    //单个成分描述:
    public class PlatformScale
    {
        //秤台名
        public string ScalingName { get; set; }
        //物料名

        public string MaterialName { get; set; }

        public float weights { get; set; }

        //上公差
        public float UpperTolerance { get; set; }

        public float LowerTolerance { get; set; }

        //使用秤台号
        public string ScalingNum { get; set; }

        public int ScalingID {  get; set; }

        //配料单位
        public string MaterialUnit {  get; set; }

        //公差单位
        public string ToleranceUnit { get; set; }

    }

    public class MixedMaterial
    {
        public int ID { get; set; }
        public string Code {  get; set; }
        public string Creator {  get; set; }
    }
    public class SelectableViewModel<T> : INotifyPropertyChanged
    {
        // 数据项
        private T _item;
        public T Item 
        { get => _item;
            set 
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            } 
        }

        // 是否选中
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        // 构造函数
        public SelectableViewModel(T item, bool isSelected = false)
        {
            Item = item;
            IsSelected = isSelected;
        }

        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Formula
    {
         Formula() { }

        //配方编码
        string Code;

        string FormulaName;

        List<PlatformScale> ScalesData;


    }

    public enum MaterialType
    {
        //粘合剂
        Adhesive,
        //粉末
        Powder
    }



}
