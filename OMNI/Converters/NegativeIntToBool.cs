using System;
using System.Windows.Data;

namespace OMNI.Converters
{
    public sealed class NegativeIntToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && int.TryParse(value.ToString(), out int i))
            {
                return i >= 0 ? false : true;
            }
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
