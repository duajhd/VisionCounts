using System.Windows;
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
            // 订阅关闭请求事件
            vm.RequestClose += CloseWindow;
        }

        private void CloseWindow()
        {
            this.Close(); // 关闭窗口
        }
    }
}
