using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.WinUI.Extensions;
using MyRSSFeeds.WinUI.Views;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MyRSSFeeds.WinUI.Services
{
    // Show a dialog to users after updating the app to tell them what's changed
    // also update the default UserAgent used with the httpclient
    public static class WhatsNewDisplayService
    {
        internal static async Task ShowIfAppropriateAsync()
        {
            PackageVersion version = Package.Current.Id.Version;
            string currentVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            string lastVersionSeen = await ApplicationData.Current.LocalSettings.ReadAsync<string>("LastVersionSeen");

            if (string.IsNullOrEmpty(lastVersionSeen))
            {
                // fresh install (or first run after this setting was introduced):
                // nothing changed for this user, just remember where they start
                await ApplicationData.Current.LocalSettings.SaveAsync("LastVersionSeen", currentVersion);
                return;
            }

            if (lastVersionSeen == currentVersion)
            {
                return;
            }

            // the app was updated - stamp the new version on the default user agent
            var userAgentService = App.GetService<UserAgentService>();
            var updateAgent = userAgentService.GetAgentData(x => x.Name == "App Default").FirstOrDefault();
            if (updateAgent != null)
            {
                updateAgent.AgentString = $"MyRSSFeeds/{SystemInfo.AppVersion} (Windows NT 10.0; {SystemInfo.OperatingSystemArchitecture})";
                userAgentService.UpdateAgent(updateAgent);
            }

            // saved before showing so the dialog can't reappear within the session
            // (this runs on every MainPage load) or if it is dismissed by a crash
            await ApplicationData.Current.LocalSettings.SaveAsync("LastVersionSeen", currentVersion);

            var dialog = new WhatsNewDialog();
            await DialogService.ShowDialogAsync(dialog);
        }
    }
}
