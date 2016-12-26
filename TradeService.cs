namespace SmartTrade
{
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;

    /// <summary>Buys or sells according to the configured schedule.</summary>
    /// <remarks>Reschedules itself after each buy/sell attempt.</remarks>
    [Service]
    internal sealed partial class TradeService : IntentService
    {
        protected sealed override async void OnHandleIntent(Intent intent)
        {
            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText(Resources.GetString(Resource.String.service_buying));

            using (new NotificationPopup(this, notificationBuilder))
            {
                await Task.Delay(5000);
            }
        }
    }
}
