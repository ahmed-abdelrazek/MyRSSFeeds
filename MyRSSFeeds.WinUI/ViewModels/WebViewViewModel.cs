using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using MyRSSFeeds.Contracts.Services;
using MyRSSFeeds.Contracts.ViewModels;

namespace MyRSSFeeds.ViewModels
{
    // TODO WTS: Review best practices and distribution guidelines for apps using WebView2
    // https://docs.microsoft.com/microsoft-edge/webview2/concepts/developer-guide
    // https://docs.microsoft.com/microsoft-edge/webview2/concepts/distribution
    //
    // You can also read more about WebView2 control at
    // https://docs.microsoft.com/microsoft-edge/webview2/get-started/winui
    public class WebViewViewModel : ObservableRecipient, INavigationAware
    {
        // TODO WTS: Set the URI of the page to show by default
        private const string DefaultUrl = "about:blank";
        private Uri _source;
        private string _description;
        private bool _isLoading = true;
        private bool _hasFailures;

        private ICommand _browserBackCommand;
        private ICommand _browserForwardCommand;
        private ICommand _openInBrowserCommand;
        private ICommand _reloadCommand;
        private ICommand _retryCommand;

        public IWebViewService WebViewService { get; }

        public WebView2 WebView2 { get; set; }

        public Uri Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    if (!string.IsNullOrEmpty(_description))
                    {
                        //WebViewService
                    }
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasFailures
        {
            get => _hasFailures;
            set => SetProperty(ref _hasFailures, value);
        }

        public ICommand BrowserBackCommand => _browserBackCommand ?? (_browserBackCommand = new RelayCommand(
            () => WebViewService?.GoBack(), () => WebViewService?.CanGoBack ?? false));

        public ICommand BrowserForwardCommand => _browserForwardCommand ?? (_browserForwardCommand = new RelayCommand(
            () => WebViewService?.GoForward(), () => WebViewService?.CanGoForward ?? false));

        public ICommand ReloadCommand => _reloadCommand ?? (_reloadCommand = new RelayCommand(
            () => WebViewService?.Reload()));

        public ICommand RetryCommand => _retryCommand ?? (_retryCommand = new RelayCommand(OnRetry));

        public ICommand OpenInBrowserCommand => _openInBrowserCommand ?? (_openInBrowserCommand = new RelayCommand(async
            () => await Windows.System.Launcher.LaunchUriAsync(Source)));

        public WebViewViewModel(IWebViewService webViewService)
        {
            WebViewService = webViewService;
        }

        public void OnNavigatedTo(object parameter)
        {
            WebViewService.NavigationCompleted += OnNavigationCompleted;
            Source = new Uri(DefaultUrl);
        }

        public void OnNavigatedFrom()
        {
            WebViewService.UnregisterEvents();
            WebViewService.NavigationCompleted -= OnNavigationCompleted;
        }

        private void OnNavigationCompleted(object sender, CoreWebView2WebErrorStatus webErrorStatus)
        {
            IsLoading = false;
            OnPropertyChanged(nameof(BrowserBackCommand));
            OnPropertyChanged(nameof(BrowserForwardCommand));
            if (webErrorStatus != default)
            {
                // Use `webErrorStatus` to vary the displayed message based on the error reason
                HasFailures = true;
            }
        }

        private void OnRetry()
        {
            HasFailures = false;
            IsLoading = true;
            WebViewService?.Reload();
        }
    }
}
