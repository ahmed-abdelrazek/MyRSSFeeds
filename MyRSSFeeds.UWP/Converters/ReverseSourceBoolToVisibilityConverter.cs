using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyRSSFeeds.Converters
{
    public class ReverseSourceBoolToVisibilityConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ConverterParameterProperty =
            DependencyProperty.RegisterAttached("ConverterParameter",
                typeof(bool), typeof(ReverseSourceBoolToVisibilityConverter), new PropertyMetadata(false));

        public static bool GetConverterParameter(DependencyObject obj) => (bool)obj.GetValue(ConverterParameterProperty);
        public static void SetConverterParameter(DependencyObject obj, bool value) => obj.SetValue(ConverterParameterProperty, value);

        public object ConverterParameter
        {
            get { return GetValue(ConverterParameterProperty); }
            set { SetValue(ConverterParameterProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isChecking = (bool)ConverterParameter;
            var isWorking = (bool)value;

            if (isChecking)
            {
                return Visibility.Collapsed;
            }
            else if (isWorking && isChecking == false)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
