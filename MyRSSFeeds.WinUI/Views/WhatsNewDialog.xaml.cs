using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyRSSFeeds.WinUI.Views
{
    public sealed partial class WhatsNewDialog : ContentDialog
    {
        public WhatsNewDialog()
        {
            // TODO WTS: Update the contents of this dialog every time you release a new version of the app
            if (App.MainWindow.Content is FrameworkElement frameworkElement)
            {
                RequestedTheme = frameworkElement.RequestedTheme;
            }

            InitializeComponent();
        }
    }
}
