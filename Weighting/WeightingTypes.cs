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
    public class PlatformScale
    {
        //数据库中的ID
        public int ID { get; set; }
        //秤台名
        public string ScalingName { get; set; }
        //物料名

        public string MaterialName { get; set; }

        //标准重量
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


    //绑定查询界面用
    public class MixedMaterial
    {
        public int ID { get; set; }
        public string Code {  get; set; }
        public string Creator {  get; set; }

        public string Name {  get; set; }

       
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

        public string DateOfManufature { get; set; }

        public string DeviceName { get; set; }
    }

    //测量结果，通过继承PlatformScale 两层遍历形成IP>MeasureResult的映射，login生成一次，后续使用
    //遍历秤台和
    public class MeasureResult: PlatformScale
    {
        public MeasureResult() { }
       
        //设备名
        
        public float result { get; set; }

        //秤台单位kg/g
        public string Unit { get; set; }

        //是否满足
        public bool IsSatisfied { get; set; }


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
