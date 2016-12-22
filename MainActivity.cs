namespace SmartTrade
{
    using System;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Widget;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    internal sealed class MainActivity : Activity
    {
        private Button startServiceButton;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);
            this.startServiceButton = FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            this.startServiceButton.Click += this.OnStartServiceButtonClicked;
            this.UpdateGui();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void OnStartServiceButtonClicked(object sender, EventArgs e)
        {
            Settings.IsRunning = !Settings.IsRunning;
            this.UpdateGui();
            this.SendBroadcast(new Intent(Application.Context, typeof(SetAlarmReceiver)));
        }

        private void UpdateGui() => this.startServiceButton.Text = Resources.GetString(
            Settings.IsRunning ? Resource.String.disable_service : Resource.String.enable_service);
    }
}
