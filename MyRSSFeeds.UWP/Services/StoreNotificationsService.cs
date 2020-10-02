using Microsoft.Services.Store.Engagement;
using MyRSSFeeds.Activation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace MyRSSFeeds.Services
{
    internal class StoreNotificationsService : ActivationHandler<ToastNotificationActivatedEventArgs>
    {
        public async Task InitializeAsync()
        {
            try
            {
                var engagementManager = StoreServicesEngagementManager.GetDefault();
                await engagementManager.RegisterNotificationChannelAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // TODO WTS: Channel registration call can fail, please handle exceptions as appropriate to your scenario.
            }
        }

        protected override async Task HandleInternalAsync(ToastNotificationActivatedEventArgs args)
        {
            var toastActivationArgs = args as ToastNotificationActivatedEventArgs;

            var engagementManager = StoreServicesEngagementManager.GetDefault();
            string originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(toastActivationArgs.Argument);

            //// Use the originalArgs variable to access the original arguments passed to the app.

            await Task.CompletedTask;
        }
    }
}
