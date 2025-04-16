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
using System.Collections.Specialized;
using System.Collections;
namespace Weighting
{

    //响应式Dictionary<>
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
                                                   INotifyCollectionChanged,
                                                   INotifyPropertyChanged
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyCountChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));

        private void NotifyIndexerChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));

        private void NotifyKeysChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));

        private void NotifyValuesChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));

        private void RaiseReset()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            NotifyCountChanged();
            NotifyIndexerChanged();
            NotifyKeysChanged();
            NotifyValuesChanged();
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                var existing = _dictionary.TryGetValue(key, out var oldValue);
                _dictionary[key] = value;
                if (existing)
                {
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace,
                            new KeyValuePair<TKey, TValue>(key, value),
                            new KeyValuePair<TKey, TValue>(key, oldValue)));
                }
                else
                {
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new KeyValuePair<TKey, TValue>(key, value)));
                    NotifyCountChanged();
                    NotifyKeysChanged();
                    NotifyValuesChanged();
                }
                NotifyIndexerChanged();
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new KeyValuePair<TKey, TValue>(key, value)));
            NotifyCountChanged();
            NotifyIndexerChanged();
            NotifyKeysChanged();
            NotifyValuesChanged();
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var value) && _dictionary.Remove(key))
            {
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        new KeyValuePair<TKey, TValue>(key, value)));
                NotifyCountChanged();
                NotifyIndexerChanged();
                NotifyKeysChanged();
                NotifyValuesChanged();
                return true;
            }
            return false;
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Clear()
        {
            _dictionary.Clear();
            RaiseReset();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.ContainsKey(item.Key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
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
