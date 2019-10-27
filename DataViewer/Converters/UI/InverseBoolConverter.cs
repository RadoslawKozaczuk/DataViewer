using System;
using System.Globalization;
using System.Windows.Data;

namespace DataViewer.Converters.UI
{
    /// <summary>
    /// This converter simply neglects the given value.
    /// </summary>
    class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !System.Convert.ToBoolean(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => !System.Convert.ToBoolean(value);
    }
}
