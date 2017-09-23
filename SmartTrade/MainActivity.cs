////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Widget;

    using static System.FormattableString;
    using static System.Globalization.CultureInfo;
    using static System.Math;

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    internal sealed class MainActivity : Activity
    {
        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);

            this.enableDisableServiceButton = this.GetEnableDisableServiceButton();
            this.customerIdEditText = this.GetCustomerIdEditText();
            this.apiKeyEditText = this.GetApiKeyEditText();
            this.apiSecretEditText = this.GetApiSecretEditText();
            this.lastTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.last_trade_time_text_view);
            this.lastTradeResultTextView = this.FindViewById<TextView>(Resource.Id.last_trade_result_text_view);
            this.lastTradeBalance1TextView = this.FindViewById<TextView>(Resource.Id.last_trade_balance1_text_view);
            this.lastTradeBalance2TextView = this.FindViewById<TextView>(Resource.Id.last_trade_balance2_text_view);
            this.nextTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.next_trade_time_text_view);

            this.service.PropertyChanged += this.OnPropertyChanged;
            this.service.Settings.PropertyChanged += this.OnPropertyChanged;
            this.UpdateViewPeriodically();
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.updateHandler.RemoveCallbacks(this.UpdateViewPeriodicallyIfServiceEnabled);
                this.service.Settings.PropertyChanged -= this.OnPropertyChanged;
                this.service.PropertyChanged -= this.OnPropertyChanged;
                this.service.Dispose();
            }

            base.Dispose(disposing);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static string Format(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                var span = (DateTime.UtcNow - dateTime).Value;

                return
                    Format(span.TotalDays, "day") ?? Format(span.TotalHours, "hour") ??
                    Format(span.TotalMinutes, "minute") ?? Format(span.TotalSeconds, "second") ??
                    "just now";
            }
            else
            {
                return "never";
            }
        }

        private static string Format(double amount, string unit)
        {
            var absoluteAmount = Abs(amount);

            if (absoluteAmount < 1.0)
            {
                return null;
            }

            var formatted = Invariant($"{Math.Floor(absoluteAmount)} {unit}");

            if (absoluteAmount >= 2.0)
            {
                formatted += "s";
            }

            return amount > 0.0 ? formatted + " ago" : "in " + formatted;
        }

        private static long? GetUpdateDelay(DateTime? dateTime)
        {
            var nullable = DateTime.UtcNow - dateTime;

            if (nullable.HasValue)
            {
                var span = nullable.Value;
                var delayMilliseconds =
                    GetUpdateDelayIfGreaterThanOne(span.TotalDays) * 24 * 60 * 60 * 1000 ??
                    GetUpdateDelayIfGreaterThanOne(span.TotalHours) * 60 * 60 * 1000 ??
                    GetUpdateDelayIfGreaterThanOne(span.TotalMinutes) * 60 * 1000 ??
                    GetUpdateDelay(span.TotalSeconds) * 1000;

                // Round up so that the time will have moved when the delay has elapsed.
                return (long)delayMilliseconds + 1;
            }
            else
            {
                return null;
            }
        }

        private static double? GetUpdateDelayIfGreaterThanOne(double amount) =>
            Abs(amount) > 1.0 ? GetUpdateDelay(amount) : (double?)null;

        private static double GetUpdateDelay(double amount) =>
            amount > 0.0 ? Ceiling(amount) - amount : amount - Floor(amount);

        private readonly BtcEurTradeService service = new BtcEurTradeService();
        private readonly Handler updateHandler = new Handler();
        private EditText customerIdEditText;
        private EditText apiKeyEditText;
        private EditText apiSecretEditText;
        private ToggleButton enableDisableServiceButton;
        private TextView lastTradeTimeTextView;
        private TextView lastTradeResultTextView;
        private TextView lastTradeBalance1TextView;
        private TextView lastTradeBalance2TextView;
        private TextView nextTradeTimeTextView;

        private EditText GetCustomerIdEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.user_id);

            result.TextChanged +=
                (s, e) =>
                {
                    int customerId;
                    this.service.Settings.CustomerId =
                        int.TryParse(result.Text, NumberStyles.None, InvariantCulture, out customerId) ? customerId : 0;
                };

            return result;
        }

        private EditText GetApiKeyEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.api_key);
            result.TextChanged += (s, e) => this.service.Settings.ApiKey = result.Text;
            return result;
        }

        private EditText GetApiSecretEditText()
        {
            var result = this.FindViewById<EditText>(Resource.Id.api_secret);
            result.TextChanged += (s, e) => this.service.Settings.ApiSecret = result.Text;
            return result;
        }

        private ToggleButton GetEnableDisableServiceButton()
        {
            var result = this.FindViewById<ToggleButton>(Resource.Id.enable_disable_service_button);
            result.Click +=
                (s, e) =>
                {
                    this.service.IsEnabled = result.Checked;
                    this.UpdateViewPeriodicallyIfServiceEnabled();
                };
            return result;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISettings.CustomerId):
                case nameof(ISettings.ApiKey):
                case nameof(ISettings.ApiSecret):
                    // These settings are only ever changed from the view itself, the view is therefore already up to
                    // date. Setting the same value again will reset the cursor and thus make the EditText unusable.
                    break;
                default:
                    this.UpdateView();
                    break;
            }
        }

        private void UpdateViewPeriodicallyIfServiceEnabled()
        {
            // Periodic updates should only be done when the trade service is enabled. When it's disabled, an update
            // can wreck settings entry (TODO: This could probably be disabled once setting are on their own screen).
            if (this.service.IsEnabled)
            {
                this.UpdateViewPeriodically();
            }
        }

        private void UpdateViewPeriodically()
        {
            var updateDelay = this.UpdateView();

            if (updateDelay.HasValue)
            {
                // It appears that PostDelayed sometimes calls a little early. Adding a few milliseconds should
                // account for that.
                this.updateHandler.PostDelayed(this.UpdateViewPeriodicallyIfServiceEnabled, updateDelay.Value + 100);
            }
        }

        private long? UpdateView()
        {
            var settings = this.service.Settings;
            this.customerIdEditText.Text =
                settings.CustomerId == 0 ? string.Empty : settings.CustomerId.ToString(InvariantCulture);
            this.apiKeyEditText.Text = settings.ApiKey;
            this.apiSecretEditText.Text = settings.ApiSecret;
            this.customerIdEditText.Enabled = this.apiKeyEditText.Enabled = this.apiSecretEditText.Enabled =
                !this.service.IsEnabled;
            this.enableDisableServiceButton.Checked = this.service.IsEnabled;

            this.lastTradeTimeTextView.Text = Format(settings.LastTradeTime);
            this.lastTradeResultTextView.Text = settings.LastResult;
            this.lastTradeBalance1TextView.Text =
                Invariant($"{settings.FirstCurrency} {settings.LastBalanceFirstCurrency:F8}");
            this.lastTradeBalance2TextView.Text =
                Invariant($"{settings.SecondCurrency} {settings.LastBalanceSecondCurrency:F8}");

            if (settings.NextTradeTime == 0)
            {
                this.nextTradeTimeTextView.Text = Format(null);
                return GetUpdateDelay(settings.LastTradeTime);
            }
            else
            {
                var nextTradeTime = DateTime.UtcNow +
                    TimeSpan.FromMilliseconds(settings.NextTradeTime - Java.Lang.JavaSystem.CurrentTimeMillis());
                this.nextTradeTimeTextView.Text = Format(nextTradeTime);

                var lastDelay = GetUpdateDelay(settings.LastTradeTime);
                var nextDelay = GetUpdateDelay(nextTradeTime);

                if (lastDelay.HasValue && nextDelay.HasValue)
                {
                    return Min(lastDelay.Value, nextDelay.Value);
                }
                else
                {
                    return lastDelay ?? nextDelay;
                }
            }
        }
    }
}
