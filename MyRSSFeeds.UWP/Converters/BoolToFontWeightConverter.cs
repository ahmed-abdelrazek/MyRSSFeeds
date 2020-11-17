using System;
using Windows.UI.Xaml.Data;

namespace MyRSSFeeds.UWP.Converters
{
    /// <summary>
    /// Gets a bool an return a font weight for Feed list titles
    /// depend on if the user opened the post or not
    /// </summary>
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool read && read == false
                ? Windows.UI.Text.FontWeights.Bold
                : (object)Windows.UI.Text.FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
