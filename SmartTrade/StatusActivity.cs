﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.OS;
    using Android.Widget;

    using static System.Globalization.CultureInfo;
    using static System.Math;

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    internal sealed class StatusActivity : ActivityBase
    {
        internal sealed class Data
        {
            internal static Data Get(Intent intent) => new Data(intent.GetStringExtra(nameof(TickerSymbol)));

            internal Data(string tickerSymbol) => this.TickerSymbol = tickerSymbol;

            internal string TickerSymbol { get; }

            internal void Put(Intent intent) => intent.PutExtra(nameof(this.TickerSymbol), this.TickerSymbol);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetContentView(Resource.Layout.Status);
            this.service = TradeService.Create(Data.Get(this.Intent).TickerSymbol);
            this.Title = string.Format(
                InvariantCulture, this.GetString(Resource.String.StatusTitle), this.service.Settings.TickerSymbol);

            this.settingsButton = this.GetSettingsButton();
            this.enableDisableServiceButton = this.GetEnableDisableServiceButton();
            this.lastTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.LastTradeTimeTextView);
            this.lastTradeBalance1CurrencyTextView =
                this.FindViewById<TextView>(Resource.Id.LastTradeBalance1CurrencyTextView);
            this.lastTradeBalance1IntegralTextView =
                this.FindViewById<TextView>(Resource.Id.LastTradeBalance1IntegralTextView);
            this.lastTradeBalance1FractionalTextView =
                this.FindViewById<TextView>(Resource.Id.LastTradeBalance1FractionalTextView);
            this.lastTradeBalance2CurrencyTextView =
                this.FindViewById<TextView>(Resource.Id.LastTradeBalance2CurrencyTextView);
            this.lastTradeBalance2IntegralTextView =
                this.FindViewById<TextView>(Resource.Id.LastTradeBalance2IntegralTextView);
            this.lastTradeBalance2FractionalTextView =
                this.FindViewById<TextView>(Resource.Id.LastTradeBalance2FractionalTextView);
            this.lastTradeResultTextView = this.FindViewById<TextView>(Resource.Id.LastTradeStatusTextView);
            this.unknownColor = new Color(this.lastTradeResultTextView.TextColors.DefaultColor);
            this.nextTradeTimeTextView = this.FindViewById<TextView>(Resource.Id.NextTradeTimeTextView);
            this.sectionStartTextView = this.FindViewById<TextView>(Resource.Id.SectionStartTextView);
            this.sectionEndTextView = this.FindViewById<TextView>(Resource.Id.SectionEndTextView);

            this.service.PropertyChanged += this.OnPropertyChanged;
            this.service.Settings.PropertyChanged += this.OnPropertyChanged;
        }

        protected sealed override void OnStart()
        {
            base.OnStart();
            this.UpdateAllExceptTimes();
            this.UpdateTimesPeriodically();
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
                this.service.Settings.TransferToMainAccount = data.TransferToMainAccount;
                this.service.Settings.NotifyEvents = data.NotifyEvents;
            }
        }

        protected sealed override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.currentTimeUpdateId = 0;
                    this.service.Settings.PropertyChanged -= this.OnPropertyChanged;
                    this.service.PropertyChanged -= this.OnPropertyChanged;
                    this.service.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static DateTime? GetNextTradeTime(long nextTradeTime)
        {
            if (nextTradeTime == 0)
            {
                return null;
            }
            else
            {
                return DateTime.UtcNow +
                    TimeSpan.FromMilliseconds(nextTradeTime - Java.Lang.JavaSystem.CurrentTimeMillis());
            }
        }

        private static long? GetUpdateDelay(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            var span = DateTime.UtcNow - dateTime.Value;
            var delayMilliseconds =
                GetUpdateDelayIfGreaterThanTwo(span.TotalDays) * 24 * 60 * 60 * 1000 ??
                GetUpdateDelayIfGreaterThanTwo(span.TotalHours) * 60 * 60 * 1000 ??
                GetUpdateDelayIfGreaterThanTwo(span.TotalMinutes) * 60 * 1000 ??
                GetUpdateDelay(span.TotalSeconds) * 1000;

            // Round up so that the time will have moved when the delay has elapsed.
            return (long)delayMilliseconds + 1;
        }

        private static double? GetUpdateDelayIfGreaterThanTwo(double amount) =>
            Abs(amount) >= 2.0 ? GetUpdateDelay(amount) : (double?)null;

        private static double GetUpdateDelay(double amount) => Ceiling(amount) - amount;

        private readonly Handler updateHandler = new Handler();
        private TradeService service;
        private Button settingsButton;
        private ToggleButton enableDisableServiceButton;
        private TextView lastTradeTimeTextView;
        private TextView lastTradeResultTextView;
        private Color unknownColor;
        private TextView lastTradeBalance1CurrencyTextView;
        private TextView lastTradeBalance1IntegralTextView;
        private TextView lastTradeBalance1FractionalTextView;
        private TextView lastTradeBalance2CurrencyTextView;
        private TextView lastTradeBalance2IntegralTextView;
        private TextView lastTradeBalance2FractionalTextView;
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
                    using (var intent = new Intent(this, typeof(SettingsActivity)))
                    {
                        var settings = this.service.Settings;
                        var data = new SettingsActivity.Data(
                            settings.FirstCurrency,
                            settings.SecondCurrency,
                            settings.CustomerId,
                            settings.ApiKey,
                            settings.ApiSecret,
                            settings.Buy,
                            settings.TradePeriod,
                            settings.TransferToMainAccount,
                            settings.NotifyEvents);
                        data.Put(intent);
                        this.StartActivityForResult(intent, 0);
                    }
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
                case nameof(ISettings.SectionStart):
                case nameof(ISettings.PeriodEnd):
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
                !string.IsNullOrEmpty(settings.ApiKey) && !string.IsNullOrEmpty(settings.ApiSecret) &&
                (settings.TradePeriod != 0.0f);
            this.lastTradeResultTextView.Text = settings.LastStatus;
            this.lastTradeResultTextView.SetTextColor(GuiHelper.GetStatusColor(settings.Status, this.unknownColor));
            GuiHelper.SetBalance(
                this.lastTradeBalance1CurrencyTextView,
                this.lastTradeBalance1IntegralTextView,
                this.lastTradeBalance1FractionalTextView,
                settings.FirstCurrency,
                this.service.IsEnabled ? settings.LastBalanceFirstCurrency : (float?)null);
            GuiHelper.SetBalance(
                this.lastTradeBalance2CurrencyTextView,
                this.lastTradeBalance2IntegralTextView,
                this.lastTradeBalance2FractionalTextView,
                settings.SecondCurrency,
                this.service.IsEnabled ? settings.LastBalanceSecondCurrency : (float?)null);
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
            var lastTradeTime = settings.LastTradeTime;
            var nextTradeTime = GetNextTradeTime(settings.NextTradeTime);
            var sectionStart = settings.SectionStart;
            var periodEnd = settings.PeriodEnd;

            this.lastTradeTimeTextView.Text = this.Format(lastTradeTime);
            this.nextTradeTimeTextView.Text = this.Format(nextTradeTime);
            this.sectionStartTextView.Text = this.Format(sectionStart);
            this.sectionEndTextView.Text = this.Format(periodEnd);

            return new[] { lastTradeTime, nextTradeTime, sectionStart, periodEnd }.Select(t => GetUpdateDelay(t)).Min();
        }

        private string Format(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                var span = DateTime.UtcNow - dateTime.Value;

                return
                    this.Format(span.TotalDays, this.GetString(Resource.String.Days)) ??
                    this.Format(span.TotalHours, this.GetString(Resource.String.Hours)) ??
                    this.Format(span.TotalMinutes, this.GetString(Resource.String.Minutes)) ??
                    this.Format(span.TotalSeconds, this.GetString(Resource.String.Seconds)) ??
                    this.GetString(Resource.String.JustNow);
            }
            else
            {
                return null;
            }
        }

        private string Format(double amount, string unitFormat)
        {
            var absoluteAmount = Abs(amount);

            if (absoluteAmount < 2.0)
            {
                return null;
            }
            else
            {
                return string.Format(
                    CurrentCulture,
                    this.GetString(amount > 0.0 ? Resource.String.PastFix : Resource.String.FutureFix),
                    string.Format(InvariantCulture, unitFormat, Floor(absoluteAmount)));
            }
        }
    }
}
