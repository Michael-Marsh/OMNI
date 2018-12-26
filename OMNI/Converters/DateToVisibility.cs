using System;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class DateToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            parameter = parameter ?? string.Empty;
            if (string.IsNullOrEmpty(parameter.ToString()) || parameter.ToString() != "i")
            {
                return value == null || (DateTime)value == DateTime.MinValue ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return value == null || (DateTime)value != DateTime.MinValue ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Collapsed;
        }
    }
}
