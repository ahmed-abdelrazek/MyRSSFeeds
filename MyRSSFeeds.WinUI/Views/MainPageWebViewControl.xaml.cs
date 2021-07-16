using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.ViewModels;
using System;
using System.Text;

namespace MyRSSFeeds.Views
{
    // To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/
    public sealed partial class MainPageWebViewControl : UserControl
    {
        public string Description
        {
            get => GetValue(DescriptionProperty) as string;
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(SourcesListDetailsControl), new PropertyMetadata(null, OnRssDetailsMenuItemPropertyChanged));

        public WebViewViewModel ViewModel { get; }

        public MainPageWebViewControl()
        {
            ViewModel = Ioc.Default.GetService<WebViewViewModel>();
            InitializeComponent();
            InitializeCoreWebView();
            ViewModel.WebViewService.Initialize(webView);
            ViewModel.WebView2 = webView;
        }

        private async void InitializeCoreWebView()
        {
            await webView.EnsureCoreWebView2Async();
        }

        private static void OnRssDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MainPageWebViewControl;
            if (control.DataContext is RSS data)
            {
                control.ViewModel.IsLoading = true;

                StringBuilder authors = new("");

                foreach (var a in data.Authors)
                {
                    authors.Append($"{a.Username}&#59; ");
                }
                if (Application.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    control.ViewModel.Description = $"<!doctype html> <html> <head> <title>{{data.PostTitle}}</title> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> <style> .container {{ display: grid; grid-template-columns: 1fr; grid-template-rows: repeat(2, auto) 100%; gap: 10px 5px; grid-auto-flow: row; grid-template-areas: \"TitleArea\" \"InfoArea\" \"DetailsArea\"; }} .TitleArea {{ grid-area: TitleArea; }} .InfoArea {{ display: grid; grid-template-columns: repeat(3, max-content); grid-template-rows: 100%; gap: 0px 10px; grid-auto-flow: row; grid-template-areas: \"Site Date Author\"; grid-area: InfoArea; }} .Site {{ grid-area: Site; }} .Date {{ grid-area: Date; }} .Author {{ grid-area: Author; }} .DetailsArea {{ grid-area: DetailsArea; }} html, body, .container {{ height: 100%; margin: 5px; }} h1, h2, h3, h4, h5, h6, p {{ height: 100%; margin: 0; }} a {{ outline: none; text-decoration: none; color: #00008B; padding: 2px 1px 0; }} a:link {{ color: #00008B; }} a:visited {{ color: #00003B; }} a:focus, a:hover {{ border-bottom: 1px solid; }} </style> </head> <body> <div class=\"container\"> <div class=\"TitleArea\"><h2><a href=\"{data.LaunchURL.OriginalString}\">{data.PostTitle}</a></h2></div> <div class=\"InfoArea\"> <div class=\"Site\"><h4><a href=\"{data.PostSource.BaseUrl.OriginalString}\">{data.PostSource.SiteTitle}</a></h4></div> <div class=\"Date\"><h4>{data.CreatedAtLocalTime}</h4></div> <div class=\"Author\"><h4>{authors}</h4></div> </div> <div class=\"DetailsArea\"><p>{data.Description}</p></div> </div> </body> </html>";
                }
                else
                {
                    control.ViewModel.Description = $"<!doctype html> <html> <head> <title>{{data.PostTitle}}</title> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> <style> .container {{ display: grid; grid-template-columns: 1fr; grid-template-rows: repeat(2, auto) 100%; gap: 10px 5px; grid-auto-flow: row; grid-template-areas: \"TitleArea\" \"InfoArea\" \"DetailsArea\"; }} .TitleArea {{ grid-area: TitleArea; }} .InfoArea {{ display: grid; grid-template-columns: repeat(3, max-content); grid-template-rows: 100%; gap: 0px 10px; grid-auto-flow: row; grid-template-areas: \"Site Date Author\"; grid-area: InfoArea; }} .Site {{ grid-area: Site; }} .Date {{ grid-area: Date; }} .Author {{ grid-area: Author; }} .DetailsArea {{ grid-area: DetailsArea; }} html, body, .container {{ height: 100%; margin: 5px; background: black; }} h1, h2, h3, h4, h5, h6, p {{ height: 100%; margin: 0; color: #ADD8E6; }} a {{ outline: none; text-decoration: none; color: #728FCE; padding: 2px 1px 0; }} a:link {{ color: #728FCE; }} a:visited {{ color: #191970; }} a:focus, a:hover {{ border-bottom: 1px solid; }} </style> </head> <body> <div class=\"container\"> <div class=\"TitleArea\"><h2><a href=\"{data.LaunchURL.OriginalString}\">{data.PostTitle}</a></h2></div> <div class=\"InfoArea\"> <div class=\"Site\"><h4><a href=\"{data.PostSource.BaseUrl.OriginalString}\">{data.PostSource.SiteTitle}</a></h4></div> <div class=\"Date\"><h4>{data.CreatedAtLocalTime}</h4></div> <div class=\"Author\"><h4>{authors}</h4></div> </div> <div class=\"DetailsArea\"><p>{data.Description}</p></div> </div> </body> </html>";
                }
                control.ViewModel.Source = data.LaunchURL;
                control.ViewModel.WebView2.NavigateToString(control.ViewModel.Description);
                control.ViewModel.IsLoading = false;
            }
        }
    }
}
