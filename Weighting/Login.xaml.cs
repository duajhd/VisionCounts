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
using System.Windows.Shapes;
using Weighting.ViewModels;

namespace Weighting
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        LoginViewModel vm;
        public Login()
        {
            InitializeComponent();
            vm = new LoginViewModel();
            this.DataContext = vm;
            // 注册消息接收器
            //Messenger.Default.Register<CloseWindowMessage>(this, message =>
            //{
            //    Close();
            //});
        }
    }
}
