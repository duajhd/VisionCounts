using System.Windows;
namespace Weighting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WeightingViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            vm = new WeightingViewModel();
            this.DataContext = vm;
        }
    }
}