using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;

using Microsoft.UI.Xaml.Controls;

using MyRSSFeeds.ViewModels;

namespace MyRSSFeeds.Views
{
    public sealed partial class SourcesListDetailsPage : Page
    {
        public SourcesListDetailsViewModel ViewModel { get; }

        public SourcesListDetailsPage()
        {
            ViewModel = Ioc.Default.GetService<SourcesListDetailsViewModel>();
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
