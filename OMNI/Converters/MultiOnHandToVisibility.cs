using System;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public sealed class MultiOnHandToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var onHandCount = value == null ? 0 : System.Convert.ToInt32(value);
            return onHandCount > 1 ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
