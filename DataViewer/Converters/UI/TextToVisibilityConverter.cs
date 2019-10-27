using System;
using System.Globalization;
using System.Windows.Data;

namespace DataViewer.Converters.UI
{
    /// <summary>
    /// Converts the text to 'Collapsed' if it is null or empty or to 'Visible' if it is not.
    /// </summary>
    class TextToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts the text to 'Collapsed' if it is null or empty or to 'Visible' if it is not.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null || string.IsNullOrWhiteSpace(value.ToString()) ? "Collapsed" : "Visible";

        /// <summary>
        /// Returns empty string
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException("Not implemented yet");
    }
}
