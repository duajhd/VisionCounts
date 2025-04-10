using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Weighting.ViewModels;
namespace Weighting.Views
{
    /// <summary>
    /// DeviceManagement.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceManagement : UserControl
    {
        DeviceManagementViewModel vm;
        public DeviceManagement()
        {
            InitializeComponent();
            DeviceManagementViewModel vm = new DeviceManagementViewModel();
            this.DataContext = vm;
        }
    }
}
