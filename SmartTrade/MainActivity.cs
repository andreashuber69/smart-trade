////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System.ComponentModel;
    using Android.App;
    using Android.OS;
    using Android.Widget;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    internal sealed class MainActivity : Activity
    {
        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            TradeService.Settings.PropertyChanged += this.OnSettingsPropertyChanged;
            this.SetContentView(Resource.Layout.Main);
            this.enableDisableServiceButton = GetEnableDisableServiceButton(this);
            this.customerIdEditText = GetCustomerIdEditText(this);
            this.apiKeyEditText = GetApiKeyEditText(this);
            this.apiSecretEditText = GetApiSecretEditText(this);

            this.EnableDisableCredentialInput();
        }

        protected sealed override void OnDestroy()
        {
            TradeService.Settings.PropertyChanged -= this.OnSettingsPropertyChanged;
            base.OnDestroy();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static EditText GetCustomerIdEditText(Activity activity)
        {
            var result = activity.FindViewById<EditText>(Resource.Id.customer_id);
            result.Text =
                TradeService.Settings.CustomerId == 0 ? string.Empty : TradeService.Settings.CustomerId.ToString();

            result.TextChanged +=
                (s, e) =>
                {
                    int customerId;
                    TradeService.Settings.CustomerId = int.TryParse(result.Text, out customerId) ? customerId : 0;
                };

            return result;
        }

        private static EditText GetApiKeyEditText(Activity activity)
        {
            var result = activity.FindViewById<EditText>(Resource.Id.api_key);
            result.Text = TradeService.Settings.ApiKey;
            result.TextChanged += (s, e) => TradeService.Settings.ApiKey = result.Text;
            return result;
        }

        private static EditText GetApiSecretEditText(Activity activity)
        {
            var result = activity.FindViewById<EditText>(Resource.Id.api_secret);
            result.Text = TradeService.Settings.ApiSecret;
            result.TextChanged += (s, e) => TradeService.Settings.ApiSecret = result.Text;
            return result;
        }

        private static ToggleButton GetEnableDisableServiceButton(Activity activity)
        {
            var result = activity.FindViewById<ToggleButton>(Resource.Id.enable_disable_service_button);
            result.Checked = TradeService.IsEnabled;
            result.Click += (s, e) => TradeService.IsEnabled = result.Checked;
            return result;
        }

        private EditText customerIdEditText;
        private EditText apiKeyEditText;
        private EditText apiSecretEditText;
        private ToggleButton enableDisableServiceButton;

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISettings.NextTradeTime))
            {
                this.EnableDisableCredentialInput();
            }
        }

        private void EnableDisableCredentialInput()
        {
            this.customerIdEditText.Enabled = this.apiKeyEditText.Enabled = this.apiSecretEditText.Enabled =
                !TradeService.IsEnabled;
        }
    }
}
