////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;

    using Android.App;
    using Android.OS;
    using Android.Widget;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    internal sealed class MainActivity : Activity
    {
        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);
            this.startServiceButton = this.FindViewById<Button>(Resource.Id.start_timestamp_service_button);
            this.startServiceButton.Click += this.OnStartServiceButtonClicked;
            this.UpdateGui();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Button startServiceButton;

        private void OnStartServiceButtonClicked(object sender, EventArgs e)
        {
            TradeService.IsEnabled = !TradeService.IsEnabled;
            this.UpdateGui();
        }

        private void UpdateGui() => this.startServiceButton.Text = this.Resources.GetString(
            TradeService.IsEnabled ? Resource.String.disable_service : Resource.String.enable_service);
    }
}
