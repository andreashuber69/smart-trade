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
        private Button startServiceButton;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);
            this.startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            this.startServiceButton.Click += this.StartServiceButton_Click;
        }

        protected sealed override void OnDestroy()
        {
            Log.Info(Tag, "Activity is being destroyed; stop the service.");
            base.OnDestroy();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void StartServiceButton_Click(object sender, System.EventArgs e)
        {
            this.SendBroadcast(new Intent(Application.Context, typeof(BootCompletedReceiver)));
            Log.Info(Tag, "User requested that the service be started.");
        }
    }
}
