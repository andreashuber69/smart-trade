namespace SmartTrade
{
    using Android.App;
    using Android.Content;

    internal sealed partial class TradeService
    {
        /// <summary>Sets or cancels an alarm which calls the <see cref="TradeService"/> depending on whether trading
        /// is currently enabled.</summary>
        [BroadcastReceiver(Permission = "RECEIVE_BOOT_COMPLETED")]
        [IntentFilter(new string[] { Intent.ActionBootCompleted })]
        private sealed class BootCompletedReceiver : BroadcastReceiver
        {
            public sealed override void OnReceive(Context context, Intent intent)
            {
                ScheduleTrade();
                var id = TradeService.IsEnabled ? Resource.String.service_enabled : Resource.String.service_disabled;
                ShowNotification(context, id);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private static void ShowNotification(Context context, int messageId)
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