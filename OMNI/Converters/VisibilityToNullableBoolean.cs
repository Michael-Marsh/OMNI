using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OMNI.Converters
{
    public sealed class VisibilityToNullableBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Visibility ? ((Visibility)value) == Visibility.Visible : Binding.DoNothing;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool? ? ((bool?)value) == true ? Visibility.Visible : Visibility.Collapsed : value is bool ? ((bool)value) ? Visibility.Visible : Visibility.Collapsed : Binding.DoNothing;
    }
}
