////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System.Globalization;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Widget;

    using static System.Globalization.CultureInfo;

    [Activity]
    internal sealed class SettingsActivity : Activity
    {
        public sealed override void OnBackPressed()
        {
            this.SaveChanges();
            base.OnBackPressed();
        }

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Settings);
            this.Title = string.Format(
                InvariantCulture,
                this.Resources.GetString(Resource.String.settings_title_format),
                this.Intent.GetStringExtra(nameof(ISettings.FirstCurrency)),
                this.Intent.GetStringExtra(nameof(ISettings.SecondCurrency)));

            this.customerIdEditText = this.GetCustomerIdEditText();
            this.apiKeyEditText = this.GetApiKeyEditText();
            this.apiSecretEditText = this.GetApiSecretEditText();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private EditText customerIdEditText;
        private EditText apiKeyEditText;
        private EditText apiSecretEditText;

        private EditText GetCustomerIdEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.user_id);
            var customerId = this.Intent.GetIntExtra(nameof(ISettings.CustomerId), 0);
            result.Text = customerId == 0 ? string.Empty : customerId.ToString(InvariantCulture);
            return result;
        }

        private EditText GetApiKeyEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.api_key);
            result.Text = this.Intent.GetStringExtra(nameof(ISettings.ApiKey));
            return result;
        }

        private EditText GetApiSecretEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.api_secret);
            result.Text = this.Intent.GetStringExtra(nameof(ISettings.ApiSecret));
            return result;
        }

        private void SaveChanges()
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.PutExtra(nameof(ISettings.CustomerId), this.GetCustomerId());
            intent.PutExtra(nameof(ISettings.ApiKey), this.apiKeyEditText.Text);
            intent.PutExtra(nameof(ISettings.ApiSecret), this.apiSecretEditText.Text);
            this.SetResult(Result.Ok, intent);
        }

        private int GetCustomerId() => int.TryParse(
            this.customerIdEditText.Text, NumberStyles.None, InvariantCulture, out var customerId) ? customerId : 0;
    }
}