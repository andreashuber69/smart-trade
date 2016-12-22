namespace SmartTrade
{
    using Android.App;
    using Android.Content;

    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new string[] { Intent.ActionBootCompleted })]
    public class SetAlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var alarmIntent = PendingIntent.GetService(
                context, 0, new Intent(context, typeof(BuySellService)), PendingIntentFlags.UpdateCurrent);
            var manager = AlarmManager.FromContext(context);

            if (Settings.IsRunning)
            {
                manager.Set(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 10000, alarmIntent);
                this.ShowNotification(context, Resource.String.service_running);
            }
            else
            {
                manager.Cancel(alarmIntent);
                this.ShowNotification(context, Resource.String.service_paused);
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