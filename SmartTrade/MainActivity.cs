////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.ComponentModel;
    using Android.App;
    using Android.Content;
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

            this.settingsButton = this.GetSettingsButton();
            this.enableDisableServiceButton = this.GetEnableDisableServiceButton();
            this.lastTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.last_trade_time_text_view);
            this.lastTradeResultTextView = this.FindViewById<TextView>(Resource.Id.last_trade_result_text_view);
            this.lastTradeBalance1TextView = this.FindViewById<TextView>(Resource.Id.last_trade_balance1_text_view);
            this.lastTradeBalance2TextView = this.FindViewById<TextView>(Resource.Id.last_trade_balance2_text_view);
            this.nextTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.next_trade_time_text_view);

            this.service.PropertyChanged += this.OnPropertyChanged;
            this.service.Settings.PropertyChanged += this.OnPropertyChanged;
            this.UpdateAllExceptTimes();
            this.UpdateTimesPeriodically();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                this.service.Settings.CustomerId = data.GetIntExtra(nameof(ISettings.CustomerId), 0);
                this.service.Settings.ApiKey = data.GetStringExtra(nameof(ISettings.ApiKey));
                this.service.Settings.ApiSecret = data.GetStringExtra(nameof(ISettings.ApiSecret));
            }
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.currentTimeUpdateId = 0;
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

        private static double GetUpdateDelay(double amount) => Ceiling(amount) - amount;

        private readonly BtcEurTradeService service = new BtcEurTradeService();
        private readonly Handler updateHandler = new Handler();
        private Button settingsButton;
        private ToggleButton enableDisableServiceButton;
        private TextView lastTradeTimeTextView;
        private TextView lastTradeResultTextView;
        private TextView lastTradeBalance1TextView;
        private TextView lastTradeBalance2TextView;
        private TextView nextTradeTimeTextView;
        private long currentTimeUpdateId;

        private Button GetSettingsButton()
        {
            var result = this.FindViewById<Button>(Resource.Id.settings_button);
            result.Click +=
                (s, e) =>
                {
                    var settings = this.service.Settings;
                    var intent = new Intent(this, typeof(SettingsActivity));
                    intent.PutExtra(nameof(ISettings.FirstCurrency), settings.FirstCurrency);
                    intent.PutExtra(nameof(ISettings.SecondCurrency), settings.SecondCurrency);
                    intent.PutExtra(nameof(ISettings.CustomerId), settings.CustomerId);
                    intent.PutExtra(nameof(ISettings.ApiKey), settings.ApiKey);
                    intent.PutExtra(nameof(ISettings.ApiSecret), settings.ApiSecret);
                    this.StartActivityForResult(intent, 0);
                };
            return result;
        }

        private ToggleButton GetEnableDisableServiceButton()
        {
            var result = this.FindViewById<ToggleButton>(Resource.Id.enable_disable_service_button);
            result.Click += (s, e) => this.service.IsEnabled = result.Checked;
            return result;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISettings.NextTradeTime):
                case nameof(ISettings.LastTradeTime):
                    this.UpdateTimesPeriodically();
                    break;
                default:
                    this.UpdateAllExceptTimes();
                    break;
            }
        }

        private void UpdateAllExceptTimes()
        {
            var settings = this.service.Settings;
            this.settingsButton.Enabled = !this.service.IsEnabled;
            this.enableDisableServiceButton.Checked = this.service.IsEnabled;
            this.enableDisableServiceButton.Enabled = (settings.CustomerId != 0) &&
                !string.IsNullOrEmpty(settings.ApiKey) && !string.IsNullOrEmpty(settings.ApiSecret);
            this.lastTradeResultTextView.Text = settings.LastResult;
            this.lastTradeBalance1TextView.Text =
                Invariant($"{settings.FirstCurrency} {settings.LastBalanceFirstCurrency:F8}");
            this.lastTradeBalance2TextView.Text =
                Invariant($"{settings.SecondCurrency} {settings.LastBalanceSecondCurrency:F8}");
        }

        private void UpdateTimesPeriodically() => this.UpdateTimesPeriodicallyImpl(++this.currentTimeUpdateId);

        private void UpdateTimesPeriodicallyImpl(long timeUpdateId)
        {
            // this.currentUpdateId is used to ensure that there's only ever at most one update cycle in effect.
            // Whenever we call UpdateTimesPeriodically, this.currentUpdateId is incremented and thus any calls
            // previously scheduled with PostDelayed will fail the following test when they are executed. IOW, only the
            // update cycle initiated with the most recent call to UpdateTimesPeriodically will not fail the following
            // test and thus be able to perpetuate itself by calling Handler.PostDelayed if necessary.
            if (timeUpdateId == this.currentTimeUpdateId)
            {
                var updateDelay = this.UpdateTimes();

                if (updateDelay.HasValue)
                {
                    var newTimeUpdateId = ++this.currentTimeUpdateId;
                    this.updateHandler.PostDelayed(
                        () => this.UpdateTimesPeriodicallyImpl(newTimeUpdateId), updateDelay.Value);
                }
            }
        }

        private long? UpdateTimes()
        {
            var settings = this.service.Settings;
            this.lastTradeTimeTextView.Text = Format(settings.LastTradeTime);

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
