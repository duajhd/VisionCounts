using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Weighting.Shared
{
    public class PermissionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 获取参数（期望的权限名称）
            string requiredPermission = parameter as string;

            // 获取权限列表
            List<string> permissions = value as List<string>;

            // 判断权限是否存在
            if (permissions != null && permissions.Contains(requiredPermission))
            {
                return System.Windows.Visibility.Visible; // 有权限时显示
            }
            return System.Windows.Visibility.Collapsed; // 没有权限时隐藏
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 不需要反向转换
        }
    }

    public class StringToIntegerConverter : IValueConverter
    {
        // 将绑定值从源转换为目标（显示文本 -> 整数）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && int.TryParse(text.Replace("号秤台", ""), out int result))
            {
                return result;
            }
            return 0; // 默认值
        }

        // 将绑定值从目标转换为源（整数 -> 显示文本）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int id)
            {
                return $"{id}号秤台";
            }
            return null;
        }
        

       

    }

    public class ScaleNameToNumberConverter : IValueConverter
    {
        // 文本 => 数字（界面到后台）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && str.EndsWith("号秤台"))
            {
                if (int.TryParse(str.Replace("号秤台", ""), out int result))
                    return result;
            }
            return Binding.DoNothing;
        }

        // 数字 => 文本（后台到界面）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int num)
                return $"{num}号秤台";
            return null;
        }
    }

    public class RoleToIdConverter : IValueConverter
    {
        // 从源到目标的转换（例如：将 "Administrator" 转换为 1）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string roleName)
            {
                return roleName switch
                {
                    "Administrator" => 1,
                    "User" => 2,
                    _ => 0 // 默认值
                };
            }
            return 0; // 如果值不是字符串，返回默认值
        }

        // 从目标到源的转换（例如：将 1 转换回 "Administrator"）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int roleId)
            {
                return roleId switch
                {
                    1 => "Administrator",
                    2 => "User",
                    _ => string.Empty // 默认值
                };
            }
            return string.Empty; // 如果值不是整数，返回空字符串
        }
    }

    //bool =》已激活
    public class ActivationConverter : IValueConverter
    {
        // 从源到目标的转换（Source -> Target）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查输入值是否为布尔类型
            if (value is bool boolValue)
            {
                return boolValue ? "断开" : "激活";
            }
            return "未知"; // 如果值不是布尔类型，返回默认值
        }

        // 从目标到源的转换（Target -> Source，通常不需要实现）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 如果不需要双向绑定，可以抛出异常
        }
    }

    public class PrintStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "未知状态";

            int status;
            if (int.TryParse(value.ToString(), out status))
            {
                return status == 1 ? "已打印" : "未打印";
            }

            return "未知状态";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (str == "已打印")
                    return 1;
                if (str == "未打印")
                    return 0;
            }

            return Binding.DoNothing;
        }
    }


    public class gUnitVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string paramStr)
            {
                if (paramStr.Equals("kg", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Collapsed;
                }
                if (paramStr.Equals("g", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class kgUnitVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string paramStr)
            {

                if (paramStr.Equals("g", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Collapsed;
                }
                if (paramStr.Equals("kg", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
