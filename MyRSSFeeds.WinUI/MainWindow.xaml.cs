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

            // Packaged WinUI 3 windows don't pick up the exe or manifest icon
            // for the title bar/taskbar; it has to be set on the AppWindow,
            // and the default title bar hides it unless told to show it.
            var icoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "MyRSSFeeds.ico");
            AppWindow.SetIcon(icoPath);
            AppWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.ShowIconAndSystemMenu;
            Helpers.WindowIconHelper.SetIcon(this, icoPath);
        }
    }
}
