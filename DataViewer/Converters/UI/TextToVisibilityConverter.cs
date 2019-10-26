using System;
using System.Globalization;
using System.Windows.Data;

namespace DataViewer.Converters.UI
{
    class TextToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
            => value == null || string.IsNullOrWhiteSpace(value.ToString()) ? "Collapsed" : "Visible";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => "";
    }
}
