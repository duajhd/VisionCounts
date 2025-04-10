using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Weighting.Views;



using System.Text.RegularExpressions;
using System.Data.SQLite;
namespace Weighting
{
    public static class DataRowHelper
    {
        public static T GetValue<T>(DataRow row, string columnName, T defaultValue = default)
        {
            try
            {
                if (!row.Table.Columns.Contains(columnName)) // 检查字段是否存在
                    return defaultValue;

                if (row[columnName] == DBNull.Value) // 检查是否为 DBNull
                    return defaultValue;

                return (T)Convert.ChangeType(row[columnName], typeof(T)); // 类型转换
            }
            catch
            {
                return defaultValue; // 转换失败时返回默认值
            }
        }
    }
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
    public  class WeightingViewModel:INotifyPropertyChanged
    {
        public WeightingViewModel() 
        {
            //ReadDataBaseCommand = new RelayCommand(ReadDataBase);
            //SignUpCommand = new RelayCommand(SignUp);
        }

       

       



       
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName=null) 
        
        { 

        }
       

       

        // SHA256 加密
     
       

    }


}
