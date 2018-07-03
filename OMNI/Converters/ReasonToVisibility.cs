using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class ReasonToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.ToString().ToUpper().Contains("N") ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Hidden;
        }
    }
}
