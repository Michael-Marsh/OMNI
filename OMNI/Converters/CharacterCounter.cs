using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace OMNI.Converters
{
    /// <summary>
    /// Character Counter for TextBoxes, must pass the TextBox object in the converter paramater as {x:Reference [Name of TextBox]} for correct results
    /// </summary>
    public sealed class CharacterCounter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                return ((TextBox)parameter).MaxLength - value.ToString().Length;
            }
            return ((TextBox)parameter).MaxLength;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
