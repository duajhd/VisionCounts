using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Controls;

namespace Weighting.Shared
{
 

    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // 检查值是否为 null 或空字符串
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "该字段不能为空！");
            }

            // 如果验证通过，返回成功结果
            return ValidationResult.ValidResult;
        }
    }
}
