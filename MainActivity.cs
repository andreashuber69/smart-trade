namespace SmartTrade
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Widget;
    using Android.Util;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private static readonly string Tag = typeof(MainActivity).FullName;
        private const string ServiceStartedKey = "has_service_been_started";

        private Button stopServiceButton;
        private Button startServiceButton;
        private Intent serviceToStart;
        private bool isStarted;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            if (savedInstanceState != null)
            {
                isStarted = savedInstanceState.GetBoolean(ServiceStartedKey, false);
            }

            serviceToStart = new Intent(this, typeof(TimestampService));

            stopServiceButton = FindViewById<Button>(Resource.Id.stop_timestamp_service_button);
            startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);

            if (isStarted)
            {
                stopServiceButton.Click += StopServiceButton_Click;
                stopServiceButton.Enabled = true;
                startServiceButton.Enabled = false;
            }
            else
            {
                startServiceButton.Click += StartServiceButton_Click;
                startServiceButton.Enabled = true;
                stopServiceButton.Enabled = false;
            }
        }

        protected sealed override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(ServiceStartedKey, isStarted);
            base.OnSaveInstanceState(outState);
        }

        protected sealed override void OnDestroy()
        {
            Log.Info(Tag, "Activity is being destroyed; stop the service.");

            StopService(serviceToStart);
            base.OnDestroy();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void StopServiceButton_Click(object sender, System.EventArgs e)
        {
            stopServiceButton.Click -= StopServiceButton_Click;
            stopServiceButton.Enabled = false;

            Log.Info(Tag, "User requested that the service be stopped.");
            StopService(serviceToStart);
            isStarted = false;

            startServiceButton.Click += StartServiceButton_Click;
            startServiceButton.Enabled = true;
        }

        private void StartServiceButton_Click(object sender, System.EventArgs e)
        {
            this.ScheduleAlarm();
            startServiceButton.Enabled = false;
            startServiceButton.Click -= StartServiceButton_Click;

            StartService(serviceToStart);
            Log.Info(Tag, "User requested that the service be started.");

            isStarted = true;
            stopServiceButton.Click += StopServiceButton_Click;

            stopServiceButton.Enabled = true;
        }

        private void ScheduleAlarm()
        {
            var intent = new Intent(Application.Context, typeof(AlarmReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
            var manager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            manager.Set(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 10000, pendingIntent);
        }
    }
}
