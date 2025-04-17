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
        private readonly object _syncRoot = new();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void RaiseReset()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            Notify(nameof(Count));
            Notify("Item[]");
            Notify(nameof(Keys));
            Notify(nameof(Values));
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_syncRoot)
                    return _dictionary[key];
            }
            set
            {
                KeyValuePair<TKey, TValue> oldItem;
                bool existed;

                lock (_syncRoot)
                {
                    existed = _dictionary.TryGetValue(key, out var oldValue);
                    oldItem = new KeyValuePair<TKey, TValue>(key, oldValue);
                    _dictionary[key] = value;
                }

                var newItem = new KeyValuePair<TKey, TValue>(key, value);
                if (existed)
                {
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
                }
                else
                {
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem));
                    Notify(nameof(Count));
                    Notify(nameof(Keys));
                    Notify(nameof(Values));
                }

                Notify("Item[]");
            }
        }

        public ICollection<TKey> Keys
        {
            get { lock (_syncRoot) return _dictionary.Keys.ToList(); }
        }

        public ICollection<TValue> Values
        {
            get { lock (_syncRoot) return _dictionary.Values.ToList(); }
        }

        public int Count
        {
            get { lock (_syncRoot) return _dictionary.Count; }
        }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                _dictionary.Add(key, value);
            }

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
            Notify(nameof(Count));
            Notify("Item[]");
            Notify(nameof(Keys));
            Notify(nameof(Values));
        }

        public bool Remove(TKey key)
        {
            KeyValuePair<TKey, TValue> removedItem;

            lock (_syncRoot)
            {
                if (_dictionary.TryGetValue(key, out var value))
                {
                    if (_dictionary.Remove(key))
                    {
                        removedItem = new KeyValuePair<TKey, TValue>(key, value);
                    }
                    else return false;
                }
                else return false;
            }

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem));
            Notify(nameof(Count));
            Notify("Item[]");
            Notify(nameof(Keys));
            Notify(nameof(Values));
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
                return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
                return _dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            lock (_syncRoot)
            {
                _dictionary.Clear();
            }

            RaiseReset();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_syncRoot)
                return _dictionary.TryGetValue(item.Key, out var val) && EqualityComparer<TValue>.Default.Equals(val, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _dictionary.ToList().GetEnumerator(); // ToList() for snapshot
            }
        }

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
