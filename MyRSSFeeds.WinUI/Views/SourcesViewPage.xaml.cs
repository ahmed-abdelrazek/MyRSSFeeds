using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.WinUI.ViewModels;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MyRSSFeeds.WinUI.Views
{
    public sealed partial class SourcesViewPage : Page
    {
        public SourcesViewModel ViewModel { get; } = App.GetService<SourcesViewModel>();

        public SourcesViewPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadDataAsync(new Progress<int>(percent => ViewModel.ProgressCurrent = percent), ViewModel.TokenSource.Token).FireAndGet();
        }
    }
}
