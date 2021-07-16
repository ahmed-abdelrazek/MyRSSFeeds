using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using MyRSSFeeds.ViewModels;

namespace MyRSSFeeds.Views
{
    public sealed partial class SourcesListPage : Page
    {
        public SourcesListPageViewModel ViewModel { get; }

        public SourcesListPage()
        {
            ViewModel = Ioc.Default.GetService<SourcesListPageViewModel>();
            InitializeComponent();
        }

        private void OnViewStateChanged(object sender, ListDetailsViewState e)
        {
            if (e == ListDetailsViewState.Both)
            {
                //ViewModel.EnsureItemSelected();
            }
        }
    }
}
