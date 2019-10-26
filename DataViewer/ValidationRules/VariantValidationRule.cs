using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataViewer.ValidationRules
{
    class VariantValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string name = (value as BindingGroup).Items[0] as string;
            return string.IsNullOrWhiteSpace(name) 
                ? new ValidationResult(false, "Variant name cannot be empty.") 
                : ValidationResult.ValidResult;
        }
    }
}
