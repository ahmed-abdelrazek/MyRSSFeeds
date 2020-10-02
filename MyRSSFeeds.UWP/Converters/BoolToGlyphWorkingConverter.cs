using System;
using Windows.UI.Xaml.Data;

namespace MyRSSFeeds.Converters
{
    /// <summary>
    /// Gets a bool and return a Mark Icon
    /// that tells the user if the source checking working or failed to work
    /// </summary>
    public class BoolToGlyphWorkingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var st = (bool?)value;
            return st is null ? "\xE81C" : st == true ? "\xE73E" : "\xE711";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
