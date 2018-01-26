using System;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class RequiredWidthCutoff : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = 0;
            int.TryParse(value.ToString(), out result);
            return value.ToString().Equals("NaN") ? true : result > 70;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
