////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
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
            this.enabledisableServiceButton =
                this.FindViewById<ToggleButton>(Resource.Id.enable_disable_service_button);
            this.enabledisableServiceButton.Checked = TradeService.IsEnabled;
            this.enabledisableServiceButton.Click +=
                (s, e) => TradeService.IsEnabled = this.enabledisableServiceButton.Checked;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ToggleButton enabledisableServiceButton;
    }
}
