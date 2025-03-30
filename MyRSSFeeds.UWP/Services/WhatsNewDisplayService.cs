using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.UWP.Helpers;
using MyRSSFeeds.UWP.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace MyRSSFeeds.UWP.Services
{
    // Show a dialog to users after updating the app to tell them what's changed
    // also update the default UserAgent used with the httpclient
    public static class WhatsNewDisplayService
    {
        private static bool shown = false;
        private static bool isAppUpdated = false;

        internal static async Task ShowIfAppropriateAsync()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            if (int.TryParse($"{version.Major}{version.Minor}{version.Build}{version.Revision}", out int currentVersion))
            {
                int lastVersionMajor = 1;
                int lastVersionMinor = 7;
                int lastVersionBuild = 0;
                int lastVersionRevision = 0;

                if (lastVersionMajor >= version.Major)
                {
                    isAppUpdated = true;
                }
                else if (lastVersionMajor >= version.Major && lastVersionMinor >= version.Minor)
                {
                    isAppUpdated = true;
                }
                else if (lastVersionMajor >= version.Major && lastVersionMinor >= version.Minor && lastVersionBuild >= version.Build)
                {
                    isAppUpdated = true;
                }
                else if (lastVersionMajor >= version.Major && lastVersionMinor >= version.Minor && lastVersionBuild >= version.Build && lastVersionRevision >= version.Revision)
                {
                    isAppUpdated = true;
                }
            }

            shown = await ApplicationData.Current.LocalSettings.ReadAsync<bool>("ShownWhatsNew");

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    if (isAppUpdated && !shown)
                    {
                        var agents = new Core.Services.UserAgentService(LiteDbContext.LiteDb).GetAgentData(x => x.Name == "App Default");
                        var updateAgent = agents.FirstOrDefault();
                        if (updateAgent != null)
                        {
                            updateAgent.AgentString = $"MyRSSFeeds/{SystemInfo.AppVersion} (Windows NT 10.0; {SystemInfo.OperatingSystemArchitecture})";
                            new Core.Services.UserAgentService(LiteDbContext.LiteDb).UpdateAgent(updateAgent);
                        }

                        shown = true;
                        await ApplicationData.Current.LocalSettings.SaveAsync<bool>("ShownWhatsNew", shown);
                        var dialog = new WhatsNewDialog();
                        await dialog.ShowAsync();
                    }
                });
        }
    }
}
