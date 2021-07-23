using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using MyRSSFeeds.Contracts.Services;
using MyRSSFeeds.Contracts.ViewModels;
using MyRSSFeeds.Core.Models;
using System;
using System.Text;
using System.Windows.Input;

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
        private const string DefaultUrl = "about:blank";
        private Uri _source;
        private RSS _selectedRssItem;
        private bool _isLoading = true;
        private bool _hasFailures;

        private readonly IThemeSelectorService _themeSelectorService;

        public ElementTheme ElementTheme { get; set; }

        private ICommand _browserBackCommand;
        private ICommand _browserForwardCommand;
        private ICommand _openInBrowserCommand;
        private ICommand _reloadCommand;
        private ICommand _retryCommand;

        public IWebViewService WebViewService { get; }

        public Uri Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public RSS SelectedRssItem
        {
            get => _selectedRssItem;
            set
            {
                if (SetProperty(ref _selectedRssItem, value))
                {
                    if (_selectedRssItem is not null)
                    {
                        IsLoading = true;

                        Source = _selectedRssItem.LaunchURL;

                        StringBuilder authors = new("");
                        StringBuilder webpage = new("");

                        foreach (var a in _selectedRssItem.Authors)
                        {
                            authors.Append($"{a.Username}&#59; ");
                        }
                        if (ElementTheme == ElementTheme.Light || (ElementTheme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Light))
                        {
                            webpage.Append($"<!doctype html> <html> <head> <title>{{data.PostTitle}}</title> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> <style> .container {{ display: grid; grid-template-columns: 1fr; grid-template-rows: repeat(2, auto) 100%; gap: 10px 5px; grid-auto-flow: row; grid-template-areas: \"TitleArea\" \"InfoArea\" \"DetailsArea\"; }} .TitleArea {{ grid-area: TitleArea; }} .InfoArea {{ display: grid; grid-template-columns: repeat(3, max-content); grid-template-rows: 100%; gap: 0px 10px; grid-auto-flow: row; grid-template-areas: \"Site Date Author\"; grid-area: InfoArea; }} .Site {{ grid-area: Site; }} .Date {{ grid-area: Date; }} .Author {{ grid-area: Author; }} .DetailsArea {{ grid-area: DetailsArea; }} html, body, .container {{ height: 100%; margin: 5px; }} h1, h2, h3, h4, h5, h6, p {{ height: auto; margin: 0; }} a {{ outline: none; text-decoration: none; color: #00008B; padding: 2px 1px 0; }} a:link {{ color: #00008B; }} a:visited {{ color: #00003B; }} a:focus, a:hover {{ border-bottom: 1px solid; }} </style> </head> <body> <div class=\"container\"> <div class=\"TitleArea\"><h2><a href=\"{_selectedRssItem.LaunchURL.OriginalString}\">{_selectedRssItem.PostTitle}</a></h2></div> <div class=\"InfoArea\"> <div class=\"Site\"><h4><a href=\"{_selectedRssItem.PostSource.BaseUrl.OriginalString}\">{_selectedRssItem.PostSource.SiteTitle}</a></h4></div> <div class=\"Date\"><h4>{_selectedRssItem.CreatedAtLocalTime}</h4></div> <div class=\"Author\"><h4>{authors}</h4></div> </div> <div class=\"DetailsArea\"><p>{_selectedRssItem.Description}</p></div> </div> </body> </html>");
                        }
                        else
                        {
                            webpage.Append($"<!doctype html> <html> <head> <title>{{data.PostTitle}}</title> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> <style> .container {{ display: grid; grid-template-columns: 1fr; grid-template-rows: repeat(2, auto) 100%; gap: 10px 5px; grid-auto-flow: row; grid-template-areas: \"TitleArea\" \"InfoArea\" \"DetailsArea\"; }} .TitleArea {{ grid-area: TitleArea; }} .InfoArea {{ display: grid; grid-template-columns: repeat(3, max-content); grid-template-rows: 100%; gap: 0px 10px; grid-auto-flow: row; grid-template-areas: \"Site Date Author\"; grid-area: InfoArea; }} .Site {{ grid-area: Site; }} .Date {{ grid-area: Date; }} .Author {{ grid-area: Author; }} .DetailsArea {{ grid-area: DetailsArea; }} html, body, .container {{ height: 100%; margin: 5px; background: black; }} h1, h2, h3, h4, h5, h6, p {{ height: auto; margin: 0; color: #ADD8E6; }} a {{ outline: none; text-decoration: none; color: #728FCE; padding: 2px 1px 0; }} a:link {{ color: #728FCE; }} a:visited {{ color: #191970; }} a:focus, a:hover {{ border-bottom: 1px solid; }} </style> </head> <body> <div class=\"container\"> <div class=\"TitleArea\"><h2><a href=\"{_selectedRssItem.LaunchURL.OriginalString}\">{_selectedRssItem.PostTitle}</a></h2></div> <div class=\"InfoArea\"> <div class=\"Site\"><h4><a href=\"{_selectedRssItem.PostSource.BaseUrl.OriginalString}\">{_selectedRssItem.PostSource.SiteTitle}</a></h4></div> <div class=\"Date\"><h4>{_selectedRssItem.CreatedAtLocalTime}</h4></div> <div class=\"Author\"><h4>{authors}</h4></div> </div> <div class=\"DetailsArea\"><p>{_selectedRssItem.Description}</p></div> </div> </body> </html>");
                        }
                        WebViewService.NavigateToString(webpage.ToString());
                        IsLoading = false;
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

        public WebViewViewModel(IWebViewService webViewService, IThemeSelectorService themeSelectorService)
        {
            WebViewService = webViewService;
            _themeSelectorService = themeSelectorService;
            ElementTheme = _themeSelectorService.Theme;
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
