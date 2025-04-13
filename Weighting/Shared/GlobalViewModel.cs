using System;
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
        private List<string> _permission = new List<string> { "PlanManagement", "FormulaManagement" ,"UserManagement", "DeviceManagementt" };
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
        private List<Devices> _devicelist = new List<Devices>();
        public List<Devices> Devicelist
        {
            get => _devicelist;
            set
            {
                _devicelist = value;
                OnPropertyChanged();
            }
        }
    }



   
    public class GlobalViewModelSingleton
    {
        private static readonly GlobalViewModel instance = new GlobalViewModel();

        public static GlobalViewModel Instance => instance;
    }

   
}
