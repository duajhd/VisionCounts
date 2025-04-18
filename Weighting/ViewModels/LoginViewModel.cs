using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Security.Cryptography;
using Weighting.Shared;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using System.Data;
using MaterialDesignThemes.Wpf;

namespace Weighting.ViewModels
{
  public  class LoginViewModel : INotifyPropertyChanged
    {
      public  LoginViewModel() 
        {
            LoginCommand = new RelayCommand(LoginCommandExecute);
            SignUpCommand = new RelayCommand(SignUp);

        }
        public event PropertyChangedEventHandler? PropertyChanged;


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)

        {
           // PropertyChanged.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }
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

        private RelayCommand _loginCommand;
        public RelayCommand LoginCommand
        {
            get { return _loginCommand; }
            set { _loginCommand = value; OnPropertyChanged(); }
        }

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
        public void LoginCommandExecute(object o)
        {
            //  DatabaseHelper dh = new DatabaseHelper("Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\formula.db");

            string message;

            try 
            {
                if (Login(UserName, Password, out message))
                {
                    string connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Permission.db";
                    string sql = $"SELECT A.UserName, B.RoleName FROM Users A INNER JOIN Roles B ON A.RoleId = B.RoleId WHERE A.UserName = '{UserName}'";
                    string sqlpermission = $"SELECT  C.PermissionName FROM Users A INNER JOIN RolePermissions B ON A.RoleId = B.RoleId INNER JOIN permissions C ON B.PermissionId = C.PermissionId WHERE A.UserName = '{UserName}'";
                    using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                    {
                        DataTable dt = db.ExecuteQuery(sql);
                        DataTable per_dt = db.ExecuteQuery(sqlpermission);
                        foreach (DataRow row in dt.Rows)
                        {
                            GlobalViewModelSingleton.Instance.Currentusers = new Users { ID = DataRowHelper.GetValue<int>(row, "UserId", 0), RoleName = DataRowHelper.GetValue<string>(row, "RoleName", null), UserName = DataRowHelper.GetValue<string>(row, "UserName", null) };
                        }
                        //为当前用户赋值权限列表
                        GlobalViewModelSingleton.Instance.Permissions.Clear();
                        foreach (DataRow row in per_dt.Rows)
                        {

                            GlobalViewModelSingleton.Instance.Permissions.Add(DataRowHelper.GetValue<string>(row, "PermissionName", "nullValue"));
                        }




                    }

                    //读取秤台号到IP的映射
                    connectionStr = "Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Devices.db";
                    sql = "SELECT * FROM DeviceList";

                    using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                    {
                        DataTable dt = db.ExecuteQuery(sql);


                        foreach (DataRow row in dt.Rows)
                        {
                            //形成映射
                          
                            GlobalViewModelSingleton.Instance.IPAdressArr[DataRowHelper.GetValue<int>(row, "ScalingID", 0)] = DataRowHelper.GetValue<string>(row, "IP", null);
                            
                        }
                    }

                    //获取一个全局秤台列表
                    GlobalViewModelSingleton.Instance.AllScales.Clear();
                    using (DatabaseHelper db = new DatabaseHelper(connectionStr))
                    {
                        DataTable dt = db.ExecuteQuery(sql);


                        foreach (DataRow row in dt.Rows)
                        {
                            GlobalViewModelSingleton.Instance.AllScales.Add(new Devices
                            {
                                ID = DataRowHelper.GetValue<int>(row, "ID", 0),
                                IP = DataRowHelper.GetValue<string>(row, "IP", null),
                                Port = DataRowHelper.GetValue<int>(row, "Port", 0),
                                MaxWeights = DataRowHelper.GetValue<int>(row, "MaxWeights", 0),
                                Brant = DataRowHelper.GetValue<string>(row, "Brant", null),
                                DateOfManufature = DataRowHelper.GetValue<string>(row, "DateOfManufature", null),
                                DeviceName = DataRowHelper.GetValue<string>(row, "DeviceName", null),
                                ScalingID = DataRowHelper.GetValue<int>(row, "ScalingID", 0),
                            });

                        }
                    }

                 
                  
                   

                    Window w = new MainWindow();
                    w.Show();
                    RequestClose?.Invoke();

                }
            }
            catch(Exception e)
            { 
                MessageBox.Show(e.Message); //后期这部分要改成日志
            }


        }


        //注册
        public void SignUp(object o)
        {
            string message;
            Register(UserName, Password, out message);
            MessageBox.Show(message);


        }
        public bool Login(string username, string password, out string message)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE UserName = @username AND PasswordHash = @password_hash";
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("用户名或密码不能为空!");
                message = "用户名或密码不能为空！";
                return false;
            }
            if (!IsValidUsername(username))
            {
                message = "用户名必须为 2-20位的中文和英文，不能包含数字和特殊字符，请重新输入！";
                return false;
            }

            if (!IsValidPassword(password))
            {
                message = "密码至少8位，必须包含大小写字母、数字和特殊字符，请重新输入！";
                return false;
            }
            string hash = HashPassword(password);

            try
            {
                using (DatabaseHelper db = new DatabaseHelper("Data Source=D:\\Quadrant\\Weighting\\Weighting\\bin\\Debug\\Permission.db"))
                {

                    long count = (long)db.ExecuteScalar(sql, new Dictionary<string, object>
                    {
                        {"@username",username },
                        {"@password_hash", hash}
                    });
                    if (count  == 1)
                    {
                        message = "successfully";

                       
                        return true;
                    }
                    message = "error";
                    return false;

                }

            }
            catch
            {
                message = "error";
                return false;
            }





        }

        // 定义关闭请求事件
        public event Action RequestClose;

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
            return Regex.IsMatch(username, @"^[\u4e00-\u9fa5a-zA-Z]{2,20}$");
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
