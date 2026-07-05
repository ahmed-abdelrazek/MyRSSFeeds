using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.WinUI.ViewModels;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MyRSSFeeds.WinUI.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            ViewModel.Initialize(webView);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadDataAsync(new Progress<int>(percent => ViewModel.ProgressCurrent = percent), ViewModel.TokenSource.Token).FireAndGet();
        }
    }
}
