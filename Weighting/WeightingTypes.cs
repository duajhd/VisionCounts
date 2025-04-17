using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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

     //   public MaterialType materialType { get; set; }

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
    //public class PlatformScale
    //{
    //    //数据库中的ID
    //    public int ID { get; set; }
    //    //秤台名
    //    public string ScalingName { get; set; }
    //    //物料名

    //    public string MaterialName { get; set; }

    //    //标准重量
    //    public float weights { get; set; }

    //    //上公差
    //    public float UpperTolerance { get; set; }

    //    public float LowerTolerance { get; set; }

    //    //使用秤台号
    //    public string ScalingNum { get; set; }

    //    public int ScalingID {  get; set; }

    //    //配料单位
    //    public string MaterialUnit {  get; set; }

    //    //公差单位
    //    public string ToleranceUnit { get; set; }

    //}
    public class PlatformScale : INotifyPropertyChanged
    {
        // 数据库中的ID
        private int _id;
        public int ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        // 秤台名
        private string _scalingName;
        public string ScalingName
        {
            get => _scalingName;
            set
            {
                if (_scalingName != value)
                {
                    _scalingName = value;
                    OnPropertyChanged(nameof(ScalingName));
                }
            }
        }

        // 物料名
        private string _materialName;
        public string MaterialName
        {
            get => _materialName;
            set
            {
                if (_materialName != value)
                {
                    _materialName = value;
                    OnPropertyChanged(nameof(MaterialName));
                }
            }
        }

        // 标准重量
        private float _weights;
        public float weights
        {
            get => _weights;
            set
            {
                if (_weights != value)
                {
                    _weights = value;
                    OnPropertyChanged(nameof(weights));
                }
            }
        }

        // 上公差
        private float _upperTolerance;
        public float UpperTolerance
        {
            get => _upperTolerance;
            set
            {
                if (_upperTolerance != value)
                {
                    _upperTolerance = value;
                    OnPropertyChanged(nameof(UpperTolerance));
                }
            }
        }

        // 下公差
        private float _lowerTolerance;
        public float LowerTolerance
        {
            get => _lowerTolerance;
            set
            {
                if (_lowerTolerance != value)
                {
                    _lowerTolerance = value;
                    OnPropertyChanged(nameof(LowerTolerance));
                }
            }
        }

        // 使用秤台号
        private string _scalingNum;
        public string ScalingNum
        {
            get => _scalingNum;
            set
            {
                if (_scalingNum != value)
                {
                    _scalingNum = value;
                    OnPropertyChanged(nameof(ScalingNum));
                }
            }
        }

        // 秤台ID
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

        // 配料单位
        private string _materialUnit;
        public string MaterialUnit
        {
            get => _materialUnit;
            set
            {
                if (_materialUnit != value)
                {
                    _materialUnit = value;
                    OnPropertyChanged(nameof(MaterialUnit));
                }
            }
        }

        // 公差单位
        private string _toleranceUnit;
        public string ToleranceUnit
        {
            get => _toleranceUnit;
            set
            {
                if (_toleranceUnit != value)
                {
                    _toleranceUnit = value;
                    OnPropertyChanged(nameof(ToleranceUnit));
                }
            }
        }

        // 实现 INotifyPropertyChanged 接口
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //绑定查询界面用
    public class MixedMaterial
    {
        public int ID { get; set; }
        public string Code {  get; set; }
        public string Creator {  get; set; }

        public string Name {  get; set; }

        public bool isStimulated {  get; set; }

       
    }

    public class Roles
    {
        public int ID { get; set; }
        public string RoleName { get; set; }
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

    public class ScalingResult: PlatformScale
    {
        //秤台单位
        public string ScalingUnit { get; set; }

        //称重的结果数据
        public float ScalingData { get; set; }
    }

    public class Formula
    {
        public Formula() {


            ScalesData = new List<PlatformScale>();



        }

        //配方编码
        public string Code;

        public string FormulaName;

        public List<PlatformScale> ScalesData;


    }


    public class Devices
    {
      public   int ID { get; set; }
        public string  IP { get; set; }
        public int Port { get; set; }

        public int ScalingID { get; set; }

        public int MaxWeights { get; set; }

        public string Brant { get; set; }

        public  string Unit {  get; set; }
        public string DateOfManufature { get; set; }

        public string DeviceName { get; set; }
    }

    //测量结果，通过继承PlatformScale 两层遍历形成IP>MeasureResult的映射，login生成一次，后续使用
    //遍历秤台和
    public class MeasureResult: PlatformScale
    {
        public MeasureResult() { }

        //设备名

        private float _result;
        public float Result 
        { 
            get => _result; 
            set 
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
         }

        //秤台单位kg/g
        private string _unit;
        public string Unit { get => _unit; 
            set 
            { 
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
         }

        //是否满足
        private bool _isSatisfied;
        public bool IsSatisfied
        {
            get => _isSatisfied;
            set
            {
                _isSatisfied = value;
                OnPropertyChanged(nameof(IsSatisfied));
            }
        }


    }
    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] ReceivedData { get; }
        public string Host { get; }
        public int Port { get; }

        public DataReceivedEventArgs(byte[] receivedData, string host, int port)
        {
            ReceivedData = receivedData;
            Host = host;
            Port = port;
        }
    }



}
