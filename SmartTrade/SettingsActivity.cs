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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal sealed class Data
        {
            internal static Data Get(Intent intent)
            {
                return new Data(
                    intent.GetStringExtra(nameof(FirstCurrency)),
                    intent.GetStringExtra(nameof(SecondCurrency)),
                    intent.GetIntExtra(nameof(CustomerId), 0),
                    intent.GetStringExtra(nameof(ApiKey)),
                    intent.GetStringExtra(nameof(ApiSecret)),
                    intent.GetBooleanExtra(nameof(Buy), false));
            }

            internal Data(
                string firstCurrency, string secondCurrency, int customerId, string apiKey, string apiSecret, bool buy)
            {
                this.FirstCurrency = firstCurrency;
                this.SecondCurrency = secondCurrency;
                this.CustomerId = customerId;
                this.ApiKey = apiKey;
                this.ApiSecret = apiSecret;
                this.Buy = buy;
            }

            internal string FirstCurrency { get; }

            internal string SecondCurrency { get; }

            internal int CustomerId { get; }

            internal string ApiKey { get; }

            internal string ApiSecret { get; }

            internal bool Buy { get; }

            internal void Put(Intent intent)
            {
                intent.PutExtra(nameof(this.FirstCurrency), this.FirstCurrency);
                intent.PutExtra(nameof(this.SecondCurrency), this.SecondCurrency);
                intent.PutExtra(nameof(this.CustomerId), this.CustomerId);
                intent.PutExtra(nameof(this.ApiKey), this.ApiKey);
                intent.PutExtra(nameof(this.ApiSecret), this.ApiSecret);
                intent.PutExtra(nameof(this.Buy), this.Buy);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Settings);
            this.data = Data.Get(this.Intent);
            this.Title = string.Format(
                InvariantCulture,
                this.GetString(Resource.String.SettingsTitle),
                this.data.FirstCurrency,
                this.data.SecondCurrency);

            this.customerIdEditText = this.GetCustomerIdEditText();
            this.apiKeyEditText = this.GetApiKeyEditText();
            this.apiSecretEditText = this.GetApiSecretEditText();
            this.modeSpinner = this.GetModeSpinner();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string Invariant(string format, params string[] args) =>
            string.Format(CultureInfo.InvariantCulture, format, args);

        private Data data;
        private EditText customerIdEditText;
        private EditText apiKeyEditText;
        private EditText apiSecretEditText;
        private Spinner modeSpinner;

        private EditText GetCustomerIdEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.UserId);
            result.Text = this.data.CustomerId == 0 ? string.Empty : this.data.CustomerId.ToString(InvariantCulture);
            return result;
        }

        private EditText GetApiKeyEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.ApiKey);
            result.Text = this.data.ApiKey;
            return result;
        }

        private EditText GetApiSecretEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.ApiSecret);
            result.Text = this.data.ApiSecret;
            return result;
        }

        private Spinner GetModeSpinner()
        {
            var items =
                new[]
                {
                    Invariant(this.GetString(Resource.String.Sell), this.data.FirstCurrency),
                    Invariant(this.GetString(Resource.String.Buy), this.data.FirstCurrency)
                };

            var adapter = new ArrayAdapter<string>(this, Resource.Layout.SimpleSpinnerDropDownItem, items);
            var result = this.FindViewById<Spinner>(Resource.Id.Mode);
            result.Adapter = adapter;
            result.SetSelection(this.data.Buy ? 1 : 0);
            return result;
        }

        private void SaveChanges()
        {
            var intent = new Intent(this, typeof(MainActivity));
            this.data = new Data(
                this.data.FirstCurrency,
                this.data.SecondCurrency,
                this.GetCustomerId(),
                this.apiKeyEditText.Text,
                this.apiSecretEditText.Text,
                this.modeSpinner.SelectedItemPosition == 1);
            this.data.Put(intent);
            this.SetResult(Result.Ok, intent);
        }

        private int GetCustomerId() => int.TryParse(
            this.customerIdEditText.Text, NumberStyles.None, InvariantCulture, out var customerId) ? customerId : 0;
    }
}