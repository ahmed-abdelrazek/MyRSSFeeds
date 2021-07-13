using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyRSSFeeds.Core.Models;

namespace MyRSSFeeds.Views
{
    public sealed partial class SourcesListDetailsDetailControl : UserControl
    {
        public Source ListDetailsMenuItem
        {
            get => GetValue(ListDetailsMenuItemProperty) as Source;
            set => SetValue(ListDetailsMenuItemProperty, value);
        }

        public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(Source), typeof(SourcesListDetailsDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

        public SourcesListDetailsDetailControl()
        {
            InitializeComponent();
        }

        private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SourcesListDetailsDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
