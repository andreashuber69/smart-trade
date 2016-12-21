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
            var pendingIntent = PendingIntent.GetService(
                context, 0, new Intent(context, typeof(BuySellService)), PendingIntentFlags.CancelCurrent);
            AlarmManager.FromContext(context).Set(
                AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 10000, pendingIntent);

            var notificationBuilder = new Notification.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                .SetContentText(context.Resources.GetString(Resource.String.alarms_scheduled));
            var popup = new NotificationPopup(context, notificationBuilder);
        }
    }
}