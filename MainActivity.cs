namespace SmartTrade
{
    using System;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Widget;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private static readonly string Tag = typeof(MainActivity).FullName;
        private Button startServiceButton;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public MainActivity()
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);
            this.startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            this.startServiceButton.Click += this.StartServiceButton_Click;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void StartServiceButton_Click(object sender, EventArgs e)
        {
            using (var settings = new Settings())
            {
                settings.IsStarted = !settings.IsStarted;
                this.startServiceButton.Text = settings.IsStarted ? "Stop" : "Start";
                this.SendBroadcast(new Intent(Application.Context, typeof(SetAlarmReceiver)));
            }
        }
    }
}
