using System;
using System.Globalization;
using System.Windows.Data;

namespace OMNI.Converters
{
    public class DateGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _date = System.Convert.ToDateTime(value).Date;
            return _date == DateTime.Today
                ? "Today"
                : _date == DateTime.Today.AddDays(-1)
                    ? "Yesterday"
                    : _date == DateTime.Today.AddDays(-2)
                        ? DateTime.Today.AddDays(-2).DayOfWeek.ToString()
                        : _date == DateTime.Today.AddDays(-3)
                            ? DateTime.Today.AddDays(-3).DayOfWeek.ToString()
                            : _date == DateTime.Today.AddDays(-4)
                                ? DateTime.Today.AddDays(-4).DayOfWeek.ToString()
                                : _date == DateTime.Today.AddDays(-5)
                                    ? DateTime.Today.AddDays(-5).DayOfWeek.ToString()
                                    : _date.Month == DateTime.Today.Month && _date.Year == DateTime.Today.Year
                                        ? "This Month"
                                        : _date.Month == DateTime.Today.AddMonths(-1).Month && _date.Year == DateTime.Today.Year
                                            ? "Last Month"
                                            : "Older";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
