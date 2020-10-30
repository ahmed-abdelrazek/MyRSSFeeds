using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyRSSFeeds.Converters
{
    public class ReverseSourceBoolToVisibilityConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty IsCheckingProperty =
            DependencyProperty.RegisterAttached("IsChecking",
                typeof(bool), typeof(ReverseSourceBoolToVisibilityConverter), new PropertyMetadata(false));

        public static bool GetIsChecking(DependencyObject obj) => (bool)obj.GetValue(IsCheckingProperty);
        public static void SetIsChecking(DependencyObject obj, bool value) => obj.SetValue(IsCheckingProperty, value);

        public object IsChecking
        {
            get { return GetValue(IsCheckingProperty); }
            set { SetValue(IsCheckingProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isChecking = (bool)IsChecking;
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
