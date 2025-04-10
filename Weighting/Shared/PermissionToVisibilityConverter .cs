using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
