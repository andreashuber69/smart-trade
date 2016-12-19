namespace SmartTrade
{
    using System.Threading.Tasks;
    using Android.App;
    using Android.Util;
    using Android.Content;
    using Android.OS;

    /// <summary>
    /// This is a sample started service. When the service is started, it will log a string that details how long 
    /// the service has been running (using Android.Util.Log). This service displays a notification in the notification
    /// tray while the service is active.
    /// </summary>
    [Service]
    public class TimestampService : Service
    {
        private static readonly string Tag = typeof(TimestampService).FullName;
        private const int DelayBetweenLogMessages = 5000; // milliseconds
        private const int NotificationId = 10000;

        private bool isStarted;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public sealed override void OnCreate()
        {
            base.OnCreate();
            Log.Info(Tag, "OnCreate: the service is initializing.");
        }

        public sealed override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (this.isStarted)
            {
                Log.Info(Tag, "OnStartCommand: This service has already been started.");
            }
            else
            {
                this.isStarted = true;
                Log.Info(Tag, "OnStartCommand: The service is starting.");
                DispatchNotificationThatServiceIsRunning();
                new Handler().Post(this.ServiceActivity);
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.NotSticky;
        }

        public sealed override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }

        public sealed override void OnDestroy()
        {
            // We need to shut things down.
            Log.Info(Tag, "OnDestroy: The started service is shutting down.");

            // Remove the notification from the status bar.
            ((NotificationManager)GetSystemService(NotificationService)).Cancel(NotificationId);

            isStarted = false;
            base.OnDestroy();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void ServiceActivity()
        {
            var timestamper = new UtcTimestamper();

            while (this.isStarted)
            {
                Log.Debug(Tag, timestamper.GetFormattedTimestamp());
                await Task.Delay(DelayBetweenLogMessages);
            }
        }

        private void DispatchNotificationThatServiceIsRunning()
        {
            var notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText(Resources.GetString(Resource.String.notification_text));

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(NotificationId, notificationBuilder.Build());
        }
    }
}
