using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace MyRSSFeeds.WinUI.Helpers
{
    // Replaces UWP MessageDialog which requires HWND interop and is deprecated in desktop apps.
    public static class DialogService
    {
        public static async Task ShowAsync(string message, string title = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        public static async Task<bool> ShowYesNoAsync(string message, string yesText, string noText, string title = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = yesText,
                CloseButtonText = noText,
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            return await dialog.ShowAsync() == ContentDialogResult.Primary;
        }
    }
}
