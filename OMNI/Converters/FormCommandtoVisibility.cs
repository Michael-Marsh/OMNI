using OMNI.Enumerations;
using System;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class FormCommandtoVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? (FormCommand)value == FormCommand.Search ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
