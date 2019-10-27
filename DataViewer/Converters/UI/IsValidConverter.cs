using System;
using System.Globalization;
using System.Windows.Data;

namespace DataViewer.Converters.UI
{
    class IsValidConverter : IValueConverter
    {
        /// <summary>
        /// Returns null if value is true and 'IndianRed' if value is false.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => System.Convert.ToBoolean(value) ? null : "IndianRed";

        /// <summary>
        /// Returns true for 'IndianRed' and false otherwise.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
