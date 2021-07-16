using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.UI.Xaml.Controls;

using MyRSSFeeds.ViewModels;
using System;

namespace MyRSSFeeds.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            ViewModel = Ioc.Default.GetService<MainViewModel>();
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(new Progress<int>(percent => ViewModel.ProgressCurrent = percent), ViewModel.TokenSource.Token);
        }
    }
}
