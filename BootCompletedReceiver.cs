namespace SmartTrade
{
    using Android.App;
    using Android.Content;

    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new string[] { Intent.ActionBootCompleted })]
    public class BootCompletedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();

            var notificationBuilder = new Notification.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                .SetContentText(context.Resources.GetString(Resource.String.notification_text));

            var popup = new NotificationPopup(context, notificationBuilder);
        }
    }
}