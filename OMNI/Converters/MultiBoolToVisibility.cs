using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class MultiBoolToVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (bool v in values)
            {
                if (v) { return Visibility.Visible; }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
