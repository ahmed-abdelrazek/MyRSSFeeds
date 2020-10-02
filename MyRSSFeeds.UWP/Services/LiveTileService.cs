using MyRSSFeeds.Activation;
using MyRSSFeeds.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace MyRSSFeeds.Services
{
    internal partial class LiveTileService : ActivationHandler<LaunchActivatedEventArgs>
    {
        private const string QueueEnabledKey = "LiveTileNotificationQueueEnabled";

        public async Task EnableQueueAsync()
        {
            var queueEnabled = await ApplicationData.Current.LocalSettings.ReadAsync<bool>(QueueEnabledKey);
            if (!queueEnabled)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                await ApplicationData.Current.LocalSettings.SaveAsync(QueueEnabledKey, true);
            }
        }

        public void UpdateTile(TileNotification notification)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
            catch (Exception ex)
            {
                // TODO WTS: Updating LiveTile can fail in rare conditions, please handle exceptions as appropriate to your scenario.
                Debug.WriteLine(ex);
            }
        }

        public async Task<bool> PinSecondaryTileAsync(SecondaryTile tile, bool allowDuplicity = false)
        {
            try
            {
                return (!await IsAlreadyPinnedAsync(tile) || allowDuplicity) && await tile.RequestCreateAsync();
            }
            catch (Exception ex)
            {
                // TODO WTS: Adding SecondaryTile can fail in rare conditions, please handle exceptions as appropriate to your scenario.
                Debug.WriteLine(ex);
                return false;
            }
        }

        private async Task<bool> IsAlreadyPinnedAsync(SecondaryTile tile)
        {
            var secondaryTiles = await SecondaryTile.FindAllAsync();
            return secondaryTiles.Any(t => t.Arguments == tile.Arguments);
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            // If app is launched from a SecondaryTile, tile arguments property is contained in args.Arguments
            // var secondaryTileArguments = args.Arguments;

            // If app is launched from a LiveTile notification update, TileContent arguments property is contained in args.TileActivatedInfo.RecentlyShownNotifications
            // var tileUpdatesArguments = args.TileActivatedInfo.RecentlyShownNotifications;
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return LaunchFromSecondaryTile(args) || LaunchFromLiveTileUpdate(args);
        }

        private bool LaunchFromSecondaryTile(LaunchActivatedEventArgs args)
        {
            // If app is launched from a SecondaryTile, tile arguments property is contained in args.Arguments
            // TODO WTS: Implement your own logic to determine if you can handle the SecondaryTile activation
            return false;
        }

        private bool LaunchFromLiveTileUpdate(LaunchActivatedEventArgs args)
        {
            // If app is launched from a LiveTile notification update, TileContent arguments property is contained in args.TileActivatedInfo.RecentlyShownNotifications
            // TODO WTS: Implement your own logic to determine if you can handle the LiveTile notification update activation
            return false;
        }
    }
}
