////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System.ComponentModel;
    using System.Globalization;
    using Android.App;
    using Android.OS;
    using Android.Widget;

    using static System.Globalization.CultureInfo;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon")]
    internal sealed class MainActivity : Activity
    {
        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.service.Settings.PropertyChanged += this.OnSettingsPropertyChanged;
            this.SetContentView(Resource.Layout.Main);
            this.enableDisableServiceButton = this.GetEnableDisableServiceButton(this);
            this.customerIdEditText = this.GetCustomerIdEditText(this);
            this.apiKeyEditText = this.GetApiKeyEditText(this);
            this.apiSecretEditText = this.GetApiSecretEditText(this);

            this.EnableDisableCredentialInput();
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.service.Settings.PropertyChanged -= this.OnSettingsPropertyChanged;
                this.service.Dispose();
            }

            base.Dispose(disposing);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly BtcEurTradeService service = new BtcEurTradeService();
        private EditText customerIdEditText;
        private EditText apiKeyEditText;
        private EditText apiSecretEditText;
        private ToggleButton enableDisableServiceButton;

        private EditText GetCustomerIdEditText(Activity activity)
        {
            var result = activity.FindViewById<EditText>(Resource.Id.customer_id);
            result.Text = this.service.Settings.CustomerId == 0 ?
                string.Empty : this.service.Settings.CustomerId.ToString(InvariantCulture);

            result.TextChanged +=
                (s, e) =>
                {
                    int customerId;
                    this.service.Settings.CustomerId =
                        int.TryParse(result.Text, NumberStyles.None, InvariantCulture, out customerId) ? customerId : 0;
                };

            return result;
        }

        private EditText GetApiKeyEditText(Activity activity)
        {
            var result = activity.FindViewById<EditText>(Resource.Id.api_key);
            result.Text = this.service.Settings.ApiKey;
            result.TextChanged += (s, e) => this.service.Settings.ApiKey = result.Text;
            return result;
        }

        private EditText GetApiSecretEditText(Activity activity)
        {
            var result = activity.FindViewById<EditText>(Resource.Id.api_secret);
            result.Text = this.service.Settings.ApiSecret;
            result.TextChanged += (s, e) => this.service.Settings.ApiSecret = result.Text;
            return result;
        }

        private ToggleButton GetEnableDisableServiceButton(Activity activity)
        {
            var result = activity.FindViewById<ToggleButton>(Resource.Id.enable_disable_service_button);
            result.Checked = this.service.IsEnabled;
            result.Click += (s, e) => this.service.IsEnabled = result.Checked;
            return result;
        }

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
                !this.service.IsEnabled;
        }
    }
}
