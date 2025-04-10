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
    /// DocumentMagement.xaml 的交互逻辑
    /// </summary>
    public partial class DocumentMagement : UserControl
    {
        DocumentMagementViewModel vm;
        public DocumentMagement()
        {
            InitializeComponent();
            DocumentMagementViewModel vm = new DocumentMagementViewModel();
            this.DataContext = vm;
        }
    }
}
