﻿using MyRSSFeeds.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyRSSFeeds.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }
    }
}
