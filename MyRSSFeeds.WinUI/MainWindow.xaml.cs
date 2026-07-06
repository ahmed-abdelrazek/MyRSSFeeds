using Microsoft.UI.Xaml;
using MyRSSFeeds.WinUI.Services;
using System;
using System.IO;

namespace MyRSSFeeds.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "My RSS Feeds";

            // Taskbar and Alt-Tab icon; the title bar one is XAML (AppTitleBar)
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "MyRSSFeeds.ico"));

            // Required in code - there is no XAML equivalent for either call
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // The back button lives in the title bar (the NavigationView's own is
            // collapsed since its top row wasted vertical space above the content)
            NavigationService.Navigated += (s, e) => AppTitleBar.IsBackButtonEnabled = NavigationService.CanGoBack;
            NavigationService.OnCurrentPageCanGoBackChanged += (s, canGoBack) => AppTitleBar.IsBackButtonEnabled = NavigationService.CanGoBack || canGoBack;
        }

        private void AppTitleBar_BackRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
        {
            NavigationService.GoBack();
        }
    }
}
