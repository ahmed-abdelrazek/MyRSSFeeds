using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MyRSSFeeds.Activation;
using MyRSSFeeds.Contracts.Services;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.Core.Services.Interfaces;
using MyRSSFeeds.Helpers;
using MyRSSFeeds.Services;
using MyRSSFeeds.ViewModels;
using MyRSSFeeds.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace MyRSSFeeds
{
    public partial class App : Application
    {
        public static Window MainWindow { get; set; } = new Window { Title = "AppDisplayName".GetLocalized() };

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            Ioc.Default.ConfigureServices(ConfigureServices());
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/windows/winui/api/microsoft.ui.xaml.unhandledexceptioneventargs
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);

            await InitializeAsync();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IRSSDataService, RSSDataService>();
            services.AddSingleton<ISourceDataService, SourceDataService>();
            services.AddSingleton<IUserAgentService, UserAgentService>();

            // Views and ViewModels
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<SourcesListPageViewModel>();
            services.AddTransient<SourcesListPage>();
            services.AddTransient<WebViewViewModel>();
            services.AddTransient<WebViewPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            return services.BuildServiceProvider();
        }

        private async Task InitializeAsync()
        {
            // if user is running the app for the first time then set the feed list limit to 1000
            if (SystemInformation.Instance.IsFirstRun)
            {
                await ApplicationData.Current.LocalSettings.SaveAsync("FeedsLimit", 1000);
                await ApplicationData.Current.LocalSettings.SaveAsync("WaitAfterLastCheck", 120);
            }

            // Get App Version and Operating System Architecture to use with "user agent".
            SystemInfo.OperatingSystemArchitecture = SystemInformation.Instance.OperatingSystemArchitecture.ToString();
            SystemInfo.AppVersion = $"{SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}.{SystemInformation.Instance.ApplicationVersion.Revision}";

            //sets the Database Path its connection string and the database itself
            string dbName = "LiteDbMRF.db";
            var dbFile = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbName);

            Core.Data.LiteDbContext.DbConnectionString = new LiteDB.ConnectionString { Filename = dbFile, Connection = LiteDB.ConnectionType.Shared };
            Core.Data.LiteDbContext.InitializeDatabase();

            //await ThemeSelectorService.InitializeAsync().ConfigureAwait(false);
        }
    }
}
