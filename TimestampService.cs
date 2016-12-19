namespace SmartTrade
{
    using System;
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

        private UtcTimestamper timestamper;
        private Handler handler;
        private Action runnable;
        private bool isStarted;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public sealed override void OnCreate()
        {
            base.OnCreate();
            Log.Info(Tag, "OnCreate: the service is initializing.");

            this.timestamper = new UtcTimestamper();
            this.handler = new Handler();

            // This Action is only for demonstration purposes.
            this.runnable = new Action(() =>
                            {
                                if (this.timestamper != null)
                                {
                                    Log.Debug(Tag, this.timestamper.GetFormattedTimestamp());
                                    handler.PostDelayed(this.runnable, DelayBetweenLogMessages);
                                }
                            });
        }

        public sealed override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (this.isStarted)
            {
                Log.Info(Tag, "OnStartCommand: This service has already been started.");
            }
            else
            {
                Log.Info(Tag, "OnStartCommand: The service is starting.");
                DispatchNotificationThatServiceIsRunning();
                this.handler.PostDelayed(runnable, DelayBetweenLogMessages);
                this.isStarted = true;
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
            Log.Debug(Tag, GetFormattedTimestamp());
            Log.Info(Tag, "OnDestroy: The started service is shutting down.");

            // Stop the handler.
            handler.RemoveCallbacks(runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(NotificationId);

            timestamper = null;
            isStarted = false;
            base.OnDestroy();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>This method will return a formatted timestamp to the client.</summary>
        /// <returns>A string that details what time the service started and how long it has been running.</returns>
        private string GetFormattedTimestamp() => timestamper?.GetFormattedTimestamp();

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
