namespace SmartTrade
{
    using System;
    using System.Threading;

    using Android.App;
    using Android.Content;

    internal sealed class NotificationPopup : IDisposable
    {
        private static int lastId = 0;
        private readonly Action cancel;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose() => this.cancel();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal NotificationPopup(Context context, Notification.Builder builder)
        {
            builder
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                .SetContentIntent(PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0))
                .SetAutoCancel(true);

            var manager = NotificationManager.FromContext(context);
            var id = Interlocked.Increment(ref lastId);
            this.cancel = () => manager.Cancel(id);
            manager.Notify(id, builder.Build());
        }
    }
}