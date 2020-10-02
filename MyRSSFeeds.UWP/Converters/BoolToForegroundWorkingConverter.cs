using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MyRSSFeeds.Converters
{
    /// <summary>
    /// Gets a bool and return a color for the Mark Icon
    /// that tells the user if the source checking working or failed to work
    /// </summary>
    public class BoolToForegroundWorkingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var st = (bool?)value;
            return st is null
                ? new SolidColorBrush(Colors.White)
                : st == true ? new SolidColorBrush(Colors.MediumSeaGreen) : new SolidColorBrush(Colors.DarkRed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
