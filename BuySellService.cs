namespace SmartTrade
{
    using Android.App;
    using Android.Content;
    using System.Threading.Tasks;

    /// <summary></summary>
    [Service]
    public class BuySellService : IntentService
    {
        protected sealed override async void OnHandleIntent(Intent intent)
        {
            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText(Resources.GetString(Resource.String.service_started));

            using (new NotificationPopup(this, notificationBuilder))
            {
                await Task.Delay(5000);
            }
        }
    }
}
