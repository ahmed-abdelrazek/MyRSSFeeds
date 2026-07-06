using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.WinUI.Extensions;
using MyRSSFeeds.WinUI.Services;
using MyRSSFeeds.WinUI.ViewModels;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MyRSSFeeds.WinUI
{
    public sealed partial class App : Application
    {
        public static Window MainWindow { get; private set; }

        private static System.IServiceProvider _services;

        public static T GetService<T>() where T : class => _services.GetRequiredService<T>();

        private static System.IServiceProvider ConfigureServices(LiteDatabase db)
        {
            return new ServiceCollection()
                .AddSingleton(db)
                .AddSingleton<UserAgentService>()
                .AddSingleton<RSSDataService>()
                .AddSingleton<SourceDataService>()
                .AddSingleton<RssRequest>()
                .AddTransient<ShellViewModel>()
                .AddTransient<MainViewModel>()
                .AddTransient<SourcesViewModel>()
                .AddTransient<SettingsViewModel>()
                .BuildServiceProvider();
        }

        public App()
        {
            InitializeComponent();

            // XamlParseException and friends carry almost no detail in WinUI 3;
            // log crashes to LocalState\unhandled.log so they are diagnosable
            UnhandledException += (s, e) => LogCrash(e.Message, e.Exception);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                // Get App Version and Operating System Architecture to use with "user agent".
                // Must be set before InitializeDatabase so first-run seeding builds a
                // real "App Default" agent string instead of an empty one
                var version = Package.Current.Id.Version;
                SystemInfo.OperatingSystemArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
                SystemInfo.AppVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

                //sets the Database Path its connection string and the database itself
                Core.Data.LiteDbContext.DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LiteDbMRF.db");
                var db = Core.Data.LiteDbContext.InitializeDatabase();
                _services = ConfigureServices(db);

                // if user is running the app for the first time then set the feed list limit to 1000
                if (Core.Data.LiteDbContext.IsFirstRun)
                {
                    await ApplicationData.Current.LocalSettings.SaveAsync("FeedsLimit", 1000);
                    await ApplicationData.Current.LocalSettings.SaveAsync("WaitAfterLastCheck", 120);
                }

                await ThemeSelectorService.InitializeAsync();

                // MainWindow.xaml hosts ShellPage (and the custom title bar);
                // constructing the window wires up NavigationService.Frame
                MainWindow = new MainWindow();

                // the container never disposes instance registrations
                MainWindow.Closed += (s, e) => db.Dispose();

                NavigationService.Navigate(Core.Data.LiteDbContext.IsFirstRun
                    ? typeof(Views.SourcesViewPage)
                    : typeof(Views.MainPage));

                await ThemeSelectorService.SetRequestedThemeAsync();

                MainWindow.Activate();
            }
            catch (System.Exception ex)
            {
                LogCrash("OnLaunched failed", ex);
                throw;
            }
        }

        private static void LogCrash(string message, System.Exception exception)
        {
            try
            {
                File.AppendAllText(
                    Path.Combine(ApplicationData.Current.LocalFolder.Path, "unhandled.log"),
                    $"[{System.DateTime.Now:O}] {message}\r\n{exception}\r\n\r\n");
            }
            catch { }
        }
    }
}
