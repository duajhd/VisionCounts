﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Weighting.Shared
{
    public class GlobalViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _globalVariable;
        public string GlobalVariable
        {
            get => _globalVariable;
            set
            {
                _globalVariable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GlobalVariable)));
            }
        }

        //当前登录用户的权限列表
        private List<string> _permission = new List<string> { "PlanManagement", "FormulaManagement", "UserManagement", "DeviceManagementt" , "DocumentManagement" };
        public List<string> Permissions
        {
            get => _permission;
            set
            {
                _permission = value;
                OnPropertyChanged();
            }
        }
        //编辑用户时，选择的当前用户
        private Users _currentusers;
        public Users Currentusers
        {
            get => _currentusers;
            set
            {
                _currentusers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GlobalVariable)));
            }
        }

        //当前使用的方案
        public Formula CuurentFormula = new();


        //通过遍历DeviceList生成这个,IP到测量结果的映射
        public Dictionary<string, MeasureResult> IPToMeasureResult = new Dictionary<string, MeasureResult>();

        //最大IP地址数也是最大秤台数，最大支持35个秤台
        public static int IPNum = 35;
        public string[] IPAdressArr = new string[IPNum];
        //直接用ScalingID做下标，用下标读取IP
        //public Dictionary<int,string> ScalingIDToIP 
    }



   
    public class GlobalViewModelSingleton
    {
        private static readonly GlobalViewModel instance = new GlobalViewModel();

        public static GlobalViewModel Instance => instance;
    }

   
}
