using System;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public sealed class IntToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null || (int)value == 0 ? Visibility.Hidden : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Hidden;
        }
    }
}
