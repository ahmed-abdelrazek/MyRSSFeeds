using MyRSSFeeds.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyRSSFeeds.Views
{
    public sealed partial class SourcesViewPage : Page
    {
        public SourcesViewModel ViewModel { get; } = new SourcesViewModel();

        public SourcesViewPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadDataAsync().ConfigureAwait(true);
        }
    }
}
