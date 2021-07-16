using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyRSSFeeds.Core.Models;

namespace MyRSSFeeds.Views
{
    public sealed partial class SourcesListDetailsControl : UserControl
    {
        public Source ListDetailsMenuItem
        {
            get => GetValue(ListDetailsMenuItemProperty) as Source;
            set => SetValue(ListDetailsMenuItemProperty, value);
        }

        public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(Source), typeof(SourcesListDetailsControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

        public SourcesListDetailsControl()
        {
            InitializeComponent();
        }

        private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SourcesListDetailsControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
