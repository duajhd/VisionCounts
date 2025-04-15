using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Weighting.Shared;

namespace Weighting.ViewModels
{
    public   class DocumentMagementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DocumentMagementViewModel() 
        {
            ConnectionCommand = new RelayCommand(ConnectionCommandExecute);
            
        }

        public RelayCommand ConnectionCommand { get; set; }
      
        private async void ConnectionCommandExecute(Object parameter)
        {
            
          
              

        
        }

       
    }
}
