using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.ViewModels;

namespace MyRSSFeeds.Views
{
    // To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/
    public sealed partial class MainPageWebViewControl : UserControl
    {
        public RSS SelectedItem
        {
            get => GetValue(SelectedItemProperty) as RSS;
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(RSS), typeof(MainPageWebViewControl), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

        public WebViewViewModel ViewModel { get; }

        public MainPageWebViewControl()
        {
            ViewModel = Ioc.Default.GetService<WebViewViewModel>();
            InitializeComponent();
            ViewModel.WebViewService.Initialize(webView);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MainPageWebViewControl;
            if (control.DataContext is RSS data)
            {
                control.ViewModel.SelectedRssItem = data;
            }
        }
    }
}
