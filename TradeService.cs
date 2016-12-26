namespace SmartTrade
{
    using System;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;

    /// <summary>Buys or sells according to the configured schedule.</summary>
    /// <remarks>Reschedules itself after each buy/sell attempt.</remarks>
    [Service]
    internal sealed partial class TradeService : IntentService
    {
        internal static bool IsEnabled
        {
            get { return Settings.NextTradeTime > 0; }
            set { ScheduleTrade(value ? Java.Lang.JavaSystem.CurrentTimeMillis() : 0); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void ScheduleTrade(long time)
        {
            Settings.NextTradeTime = time;
            ScheduleTrade();
        }

        private static void ScheduleTrade()
        {
            var context = Application.Context;
            var manager = AlarmManager.FromContext(context);
            var alarmIntent = PendingIntent.GetService(
                context, 0, new Intent(context, typeof(TradeService)), PendingIntentFlags.UpdateCurrent);

            if (Settings.NextTradeTime > 0)
            {
                var earliestNextTradeTime = Java.Lang.JavaSystem.CurrentTimeMillis() + 10 * 1000;
                var nextTradeTime = Math.Max(earliestNextTradeTime, Settings.NextTradeTime);
                manager.Set(AlarmType.RtcWakeup, nextTradeTime, alarmIntent);
            }
            else
            {
                manager.Cancel(alarmIntent);
            }
        }
    }
}
