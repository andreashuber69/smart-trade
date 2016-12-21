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
            var manager = NotificationManager.FromContext(context);
            var id = Interlocked.Increment(ref lastId);
            this.cancel = () => manager.Cancel(id);
            manager.Notify(id, builder.Build());
        }
    }
}