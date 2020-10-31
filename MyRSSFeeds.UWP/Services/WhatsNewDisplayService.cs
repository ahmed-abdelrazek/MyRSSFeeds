using Microsoft.Toolkit.Uwp.Helpers;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MyRSSFeeds.Services
{
    // Show a dialog to users after updating the app to tell them what's changed
    // also update the default UserAgent used with the httpclient
    public static class WhatsNewDisplayService
    {
        private static bool shown = false;

        internal static async Task ShowIfAppropriateAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    if (SystemInformation.IsAppUpdated && !shown)
                    {
                        var agents = await Core.Services.UserAgentService.GetAgentDataAsync(x => x.Name == "App Default");
                        var updateAgent = agents.FirstOrDefault();
                        if (updateAgent != null)
                        {
                            updateAgent.AgentString = $"MyRSSFeeds/{SystemInfo.AppVersion} (Windows NT 10.0; {SystemInfo.OperatingSystemArchitecture})";
                            await Core.Services.UserAgentService.UpdateAgentAsync(updateAgent);
                        }

                        shown = true;
                        var dialog = new WhatsNewDialog();
                        await dialog.ShowAsync();
                    }
                });
        }
    }
}
