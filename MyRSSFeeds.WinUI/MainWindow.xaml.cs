using Microsoft.UI.Xaml;
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

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }
    }
}
