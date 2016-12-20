namespace SmartTrade
{
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
        private NotificationPopup popup;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public sealed override void OnCreate()
        {
            base.OnCreate();
            Log.Info(Tag, "OnCreate: the service is initializing.");
        }

        public sealed override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (this.popup == null)
            {
                Log.Info(Tag, "OnStartCommand: The service is starting.");
                var notificationBuilder = new Notification.Builder(this)
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetContentTitle(Resources.GetString(Resource.String.app_name))
                    .SetContentText(Resources.GetString(Resource.String.notification_text));
                this.popup = new NotificationPopup(this, notificationBuilder);
            }
            else
            {
                Log.Info(Tag, "OnStartCommand: This service has already been started.");
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
            this.popup?.Dispose();
            this.popup = null;
            base.OnDestroy();
        }
    }
}
