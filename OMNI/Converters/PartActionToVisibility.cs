using OMNI.Enumerations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class PartActionToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((CMMSPartAction)value == CMMSPartAction.New && parameter == null) || (CMMSPartAction)value == CMMSPartAction.Open && parameter != null 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Visibility.Visible;
    }
}
