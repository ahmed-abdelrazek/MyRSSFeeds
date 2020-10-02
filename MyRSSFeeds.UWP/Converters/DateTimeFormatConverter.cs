using System;
using Windows.UI.Xaml.Data;

namespace MyRSSFeeds.Converters
{
    /// <summary>
    /// Gets a datetime from the viewmodel format it and return it as string with the new format
    /// gets formated datatime string from the view and return it as datetime to the viewmodel
    /// </summary>
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is DateTime dt && parameter != null ? dt.ToString(parameter.ToString()) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value != null ? DateTime.Parse(value.ToString()) : (object)default(DateTime);
        }
    }
}
