using Microsoft.UI.Xaml;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.WinUI.Helpers;
using MyRSSFeeds.WinUI.Services;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MyRSSFeeds.WinUI
{
    public sealed partial class App : Application
    {
        public static Window MainWindow { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            //sets the Database Path its connection string and the database itself
            Core.Data.LiteDbContext.DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LiteDbMRF.db");
            Core.Data.LiteDbContext.InitializeDatabase();

            // Get App Version and Operating System Architecture to use with "user agent".
            var version = Package.Current.Id.Version;
            SystemInfo.OperatingSystemArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
            SystemInfo.AppVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            // if user is running the app for the first time then set the feed list limit to 1000
            if (Core.Data.LiteDbContext.IsFirstRun)
            {
                await ApplicationData.Current.LocalSettings.SaveAsync("FeedsLimit", 1000);
                await ApplicationData.Current.LocalSettings.SaveAsync("WaitAfterLastCheck", 120);
            }

            await ThemeSelectorService.InitializeAsync();

            MainWindow = new MainWindow();
            MainWindow.Content = new Views.ShellPage();

            NavigationService.Navigate(Core.Data.LiteDbContext.IsFirstRun
                ? typeof(Views.SourcesViewPage)
                : typeof(Views.MainPage));

            await ThemeSelectorService.SetRequestedThemeAsync();

            MainWindow.Activate();
        }
    }
}
