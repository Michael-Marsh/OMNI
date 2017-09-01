using System;
using System.Windows;
using System.Windows.Data;

namespace OMNI.QMS.Converters
{
    public class QIRIDNumbertoVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (parameter != null && value == null)
            {
                return Visibility.Visible;
            }
            else if (parameter != null && (int)value > 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
