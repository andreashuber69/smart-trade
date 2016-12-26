namespace SmartTrade
{
    using System;

    using Android.App;
    using Android.Content;

    internal sealed partial class TradeService
    {
        /// <summary>Sets or cancels an alarm which calls the <see cref="TradeService"/> depending on whether trading
        /// is currently enabled.</summary>
        [BroadcastReceiver(Permission = "RECEIVE_BOOT_COMPLETED")]
        [IntentFilter(new string[] { Intent.ActionBootCompleted })]
        private sealed class SetAlarmReceiver : BroadcastReceiver
        {
            public sealed override void OnReceive(Context context, Intent intent)
            {
                var alarmIntent = PendingIntent.GetService(
                    context, 0, new Intent(context, typeof(TradeService)), PendingIntentFlags.UpdateCurrent);
                var manager = AlarmManager.FromContext(context);
                var earliestNextTradeTime = Java.Lang.JavaSystem.CurrentTimeMillis() + 10 * 1000;
                var nextTradeTime = Math.Max(earliestNextTradeTime, Settings.NextTradeTime);

                if (Settings.IsEnabled)
                {
                    manager.Set(AlarmType.RtcWakeup, nextTradeTime, alarmIntent);
                    this.ShowNotification(context, Resource.String.service_enabled);
                }
                else
                {
                    manager.Cancel(alarmIntent);
                    this.ShowNotification(context, Resource.String.service_disabled);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private void ShowNotification(Context context, int messageId)
            {
                var notificationBuilder = new Notification.Builder(context)
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                    .SetContentText(context.Resources.GetString(messageId));
                var popup = new NotificationPopup(context, notificationBuilder);
            }
        }
    }
}