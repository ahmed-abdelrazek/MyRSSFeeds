using MyRSSFeeds.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace MyRSSFeeds.WinUI.Views
{
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = App.GetService<ShellViewModel>();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView, KeyboardAccelerators);
        }
    }
}
