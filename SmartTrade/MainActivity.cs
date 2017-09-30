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
    using static System.Math;

    [Activity(Label = "@string/AppName", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    internal sealed class MainActivity : Activity
    {
        internal void UpdateTimesPeriodically() => this.UpdateTimesPeriodicallyImpl(++this.currentTimeUpdateId);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Main);

            this.settingsButton = this.GetSettingsButton();
            this.enableDisableServiceButton = this.GetEnableDisableServiceButton();
            this.lastTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.LastTradeTimeTextView);
            this.lastTradeResultTextView = this.FindViewById<TextView>(Resource.Id.LastTradeResultTextView);
            this.lastTradeBalance1TextView = this.FindViewById<TextView>(Resource.Id.LastTradeBalance1TextView);
            this.lastTradeBalance2TextView = this.FindViewById<TextView>(Resource.Id.LastTradeBalance2TextView);
            this.nextTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.NextTradeTimeTextView);
            this.sectionStartTextView = this.FindViewById<TextView>(Resource.Id.SectionStartTextView);
            this.sectionEndTextView = this.FindViewById<TextView>(Resource.Id.SectionEndTextView);

            this.service.PropertyChanged += this.OnPropertyChanged;
            this.service.Settings.PropertyChanged += this.OnPropertyChanged;
            this.UpdateAllExceptTimes();
        }

        protected sealed override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                var data = SettingsActivity.Data.Get(intent);
                this.service.Settings.CustomerId = data.CustomerId;
                this.service.Settings.ApiKey = data.ApiKey;
                this.service.Settings.ApiSecret = data.ApiSecret;
                this.service.Settings.Buy = data.Buy;
                this.service.Settings.TradePeriod = data.TradePeriod;
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
        private TextView sectionStartTextView;
        private TextView sectionEndTextView;
        private long currentTimeUpdateId;

        private Button GetSettingsButton()
        {
            var result = this.FindViewById<Button>(Resource.Id.SettingsButton);
            result.Click +=
                (s, e) =>
                {
                    var settings = this.service.Settings;
                    var intent = new Intent(this, typeof(SettingsActivity));
                    var data = new SettingsActivity.Data(
                        settings.FirstCurrency,
                        settings.SecondCurrency,
                        settings.CustomerId,
                        settings.ApiKey,
                        settings.ApiSecret,
                        settings.Buy,
                        settings.TradePeriod);
                    data.Put(intent);
                    this.StartActivityForResult(intent, 0);
                };
            return result;
        }

        private ToggleButton GetEnableDisableServiceButton()
        {
            var result = this.FindViewById<ToggleButton>(Resource.Id.EnableDisableServiceButton);
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

            this.sectionStartTextView.Text = Format(settings.SectionStart);
            this.sectionEndTextView.Text = Format(settings.PeriodEnd);

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
