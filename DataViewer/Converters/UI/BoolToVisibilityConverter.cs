using System;
using System.Globalization;
using System.Windows.Data;

namespace DataViewer.Converters.UI
{
    class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Returns 'Visible' if value is true and 'Collapsed' if value is false.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => System.Convert.ToBoolean(value) ? "Visible" : "Collapsed";

        /// <summary>
        /// Returns true for 'Visible' and false otherwise.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => System.Convert.ToBoolean(value.ToString() == "Visible" ? true : false);
    }
}
