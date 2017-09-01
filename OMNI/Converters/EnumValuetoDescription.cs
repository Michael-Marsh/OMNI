using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace OMNI.Converters
{
    public sealed class EnumValuetoDescription : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var type = value.GetType();
            if (!type.IsEnum)
            {
                return null;
            }
            var field = type.GetField(value.ToString());
            var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), true)
                            .Cast<DescriptionAttribute>()
                            .FirstOrDefault();
            return attr != null ? attr.Description : field.Name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
