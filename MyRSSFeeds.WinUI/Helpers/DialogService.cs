using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyRSSFeeds.WinUI.Helpers
{
    // Replaces UWP MessageDialog which requires HWND interop and is deprecated in desktop apps.
    // Only one ContentDialog can be open per window in WinUI 3 (a second ShowAsync crashes the
    // app), so all dialogs are serialized through a semaphore like MessageDialog used to queue.
    public static class DialogService
    {
        private static readonly SemaphoreSlim _oneDialogAtATime = new SemaphoreSlim(1, 1);

        public static async Task ShowAsync(string message, string title = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok".GetLocalized()
            };

            await ShowDialogAsync(dialog);
        }

        public static async Task<bool> ShowYesNoAsync(string message, string yesText, string noText, string title = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = yesText,
                CloseButtonText = noText,
                DefaultButton = ContentDialogButton.Close
            };

            return await ShowDialogAsync(dialog) == ContentDialogResult.Primary;
        }

        public static async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
        {
            await _oneDialogAtATime.WaitAsync();
            try
            {
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
                return await dialog.ShowAsync();
            }
            finally
            {
                _oneDialogAtATime.Release();
            }
        }
    }
}
