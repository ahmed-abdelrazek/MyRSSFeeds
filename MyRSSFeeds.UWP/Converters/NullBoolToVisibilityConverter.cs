using System;
using Windows.UI.Xaml.Data;

namespace MyRSSFeeds.Converters
{
    /// <summary>
    /// Gets null or true and return it as Visibility.Collapsed
    /// </summary>
    public class NullBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var st = (bool?)value;
            return st == null || st == true
                ? Windows.UI.Xaml.Visibility.Collapsed
                : (object)Windows.UI.Xaml.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
