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
namespace Weighting.ViewModels
{
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
    public  class SignUpViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<SelectableViewModel<PlatformScale>> Items1 { get; }
        public SignUpViewModel() 
        
        {
            Items1 = CreateData();

            foreach (var model in Items1)
            {
                model.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(SelectableViewModel<PlatformScale>.IsSelected))

                        OnPropertyChanged(nameof(IsAllItems1Selected));
                };
            }

        }
      
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
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


    }
}
