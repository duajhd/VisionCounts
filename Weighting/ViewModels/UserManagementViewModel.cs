using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Security.Cryptography;

namespace Weighting.ViewModels
{
   
   
  public  class UserManagementViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

     
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public UserManagementViewModel()
        {
            Items1 = new ObservableCollection<Users>();
            SearchCommand = new RelayCommand(SearchCommandExecute);
            DeleteRowCommand = new RelayCommand(DeleteRowExecute);
            ChangeRowCommand = new RelayCommand(ChangeRowCommandExecute);
            SignUpCommand = new RelayCommand(SignUp);
            AddUserCommand = new RelayCommand(AddUserCommandExecute);


            //读取所有角色名
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Permission.db";
            string sql = "SELECT  * FROM Roles";

            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
               DataTable dt = db.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    //RoleName = DataRowHelper.GetValue<string>(row, "RoleName", null),
                    //        UserName = DataRowHelper.GetValue<string>(row, "UserName", null),
                    //        ID = DataRowHelper.GetValue<int>(row, "UserId", 0)
                  
                    AllRoles.Add(

                          new Roles
                          {
                              RoleName = DataRowHelper.GetValue<string>(row, "RoleName", null),

                              ID = DataRowHelper.GetValue<int>(row, "ID", 0)
                          }
                        );

                }
            }



        }
        //新增用户绑定输入的用户名
        private string _username;
        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        //新增用户绑定密码用
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }


        private string _userNameForSearch;
        public string UserNameForSearch
        {
            get => _userNameForSearch;
            set
            {
                _userNameForSearch = value;
                OnPropertyChanged() ;
            }
        }
        private string _selectedUserName;
        public string SelectedUserName
        {
            get=> _selectedUserName;
            set
            {
                _selectedUserName = value;
              
                    OnPropertyChanged();

            }
        }


     
        public ObservableCollection<Users> Items1 { get; set; }
        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand 
        { get => _searchCommand;

            set
            {
                _searchCommand = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _addUserCommand;
        public RelayCommand AddUserCommand
        {
            get => _addUserCommand;

            set
            {
                _addUserCommand = value;
                OnPropertyChanged();
            }
        }

     
        //删除行
        private RelayCommand _changeRowCommand;
        public RelayCommand ChangeRowCommand
        {
            get => _changeRowCommand;
            set { _changeRowCommand = value; OnPropertyChanged(); }
        }

        public RelayCommand DeleteRowCommand { get; set; }

        private RelayCommand _signUpCommand;
        public RelayCommand SignUpCommand
        {
            get => _signUpCommand;
            set
            {
                _signUpCommand = value;
                OnPropertyChanged();
            }
        }

        //添加用户绑定的角色
        public Roles SelectedRole { get; set; }

        //编辑用户绑定的角色
        public Roles SelectedRoleForEditing { get; set; }
        public ObservableCollection<Roles> AllRoles { get; set; } = new();
        public Dictionary<string, bool> SelectedItemsBinding { get; set; } = new();

        public string SelectedItemsDisplay =>
            string.Join(", ", SelectedItemsBinding.Where(kv => kv.Value).Select(kv => kv.Key));

        public bool IsDropDownOpen { get; set; }

       
        //删除后将不可恢复
        private  void DeleteRowExecute(object obj)
        {
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Permission.db";
            // SQL 删除语句
            string sql = "DELETE FROM Users WHERE UserName = @username";

            MessageBoxResult result = MessageBox.Show(
               "将要删除用户，确定要继续吗？",
               "提示",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question);

          
            if (obj is Users&& result == MessageBoxResult.Yes)
            {
                Users row = (Users)obj;
                using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                {
                    db.ExecuteNonQuery(sql,new Dictionary<string, object>
                    {
                        {"@username",row.UserName}
                    });

                    Items1.Remove(row);
                }
            }

        }
        //用户模块搜索命令执行
        private void SearchCommandExecute(object o)
        {
            
            
            string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Permission.db";
            string sql = $"SELECT A.UserName, B.RoleName FROM Users A INNER JOIN Roles B ON A.RoleId = B.RoleId WHERE A.UserName = '{UserNameForSearch}'";
            if (string.IsNullOrEmpty(UserNameForSearch))
            {
                //不填用户名，查出所有用户
                sql = $"SELECT A.UserId, A.UserName, B.RoleName FROM Users A INNER JOIN Roles B ON A.RoleId = B.RoleId ";
            }
            using (DatabaseHelper db = new DatabaseHelper(connectionStr))
            {
               DataTable dt =  db.ExecuteQuery(sql);
                
                if(dt.Rows.Count == 0)
                {
                    MessageBox.Show("系统中没有该用户");
                    return;
                }
                //执行到这里说明查询成功
                if (Items1.Count > 0) Items1.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    Items1.Add(
                        new Users
                        {
                            
                            RoleName = DataRowHelper.GetValue<string>(row, "RoleName", null),
                            UserName  = DataRowHelper.GetValue<string>(row, "UserName", null),
                            ID = DataRowHelper.GetValue<int>(row, "UserId",0)
                        }
                        );
                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// 修改用户需要知道变更后,选择了哪个用户
        private async void ChangeRowCommandExecute(object obj) 
        {

            //获取选择的用户
            Users row = (Users)obj;
            SelectedUserName = row.UserName;
            var dialog = new Views.ChangeUserDialog();
            //await DialogHost.Show(dialog, "RootDialog");
            await DialogHost.Show(dialog, "RootDialog");

        }

        private async void AddUserCommandExecute(object obj)
        {
            var dialog = new Views.AdduserDialog();
            //await DialogHost.Show(dialog, "RootDialog");
           await DialogHost.Show(dialog, "RootDialog");

           
        }
        //修改用户1.获取当前选择行2.textbox填用户信息3.点击确认修改




        public void SignUp(object o)
        {
            string message;
            if(Register(UserName, Password, out message))
            {
                MessageBox.Show(message);
                UserName = "";
                Password = "";
            }
           


        }
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        // 用户名格式验证：字母 + 数字，6-16位
        public static bool IsValidUsername(string username)
        {
            return Regex.IsMatch(username, @"^[a-zA-Z0-9]{6,16}$");
        }

        // 密码格式验证：包含大写、小写、数字、特殊字符，至少8位
        public static bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
        }

        public bool Register(string username, string password, out string message)
        {
            if (username == null || password == null)
            {
                MessageBox.Show("用户名或密码不能为空!");
                message = "用户名或密码不能为空！";
                return false;
            }
            string sql = "INSERT INTO Users (UserName, PasswordHash,RoleId) VALUES (@username, @password_hash,@roleid)";
            if (!IsValidUsername(username))
            {
                message = "用户名必须为 6-16 位字母和数字组合";
                return false;
            }

            if (!IsValidPassword(password))
            {
                message = "密码至少8位，必须包含大小写字母、数字和特殊字符";
                return false;
            }

            string hash = HashPassword(password);

            try
            {
                using (DatabaseHelper db = new DatabaseHelper("Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Permission.db"))
                {
                    db.ExecuteNonQuery(sql, new Dictionary<string, object>
                    {
                        {"@username",username },
                        {"@password_hash", hash},
                        {"@roleid" ,1}
                    });
                    message = "注册成功";
                    return true;
                }
            }
            catch (SQLiteException ex) when (ex.ErrorCode == (int)SQLiteErrorCode.Constraint)
            {
                message = "用户名已存在";
                return false;
            }
            catch (Exception ex)
            {
                message = $"注册失败: {ex.Message}";
                return false;
            }




        }
    }
}
