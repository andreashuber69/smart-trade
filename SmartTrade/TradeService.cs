﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Bitstamp;

    using static Logger;

    /// <summary>Buys or sells according to the configured schedule.</summary>
    /// <remarks>Reschedules itself after each buy/sell attempt.</remarks>
    internal abstract partial class TradeService : IntentService, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static TradeService Create(string tickerSymbol)
        {
            switch (tickerSymbol)
            {
                case BitstampClient.BtcUsdSymbol:
                    return new BtcUsdTradeService();
                case BitstampClient.BtcEurSymbol:
                    return new BtcEurTradeService();
                case BitstampClient.EurUsdSymbol:
                    return new EurUsdTradeService();
                case BitstampClient.XrpUsdSymbol:
                    return new XrpUsdTradeService();
                case BitstampClient.XrpEurSymbol:
                    return new XrpEurTradeService();
                case BitstampClient.XrpBtcSymbol:
                    return new XrpBtcTradeService();
                case BitstampClient.LtcUsdSymbol:
                    return new LtcUsdTradeService();
                case BitstampClient.LtcEurSymbol:
                    return new LtcEurTradeService();
                case BitstampClient.LtcBtcSymbol:
                    return new LtcBtcTradeService();
                case BitstampClient.EthUsdSymbol:
                    return new EthUsdTradeService();
                case BitstampClient.EthEurSymbol:
                    return new EthEurTradeService();
                case BitstampClient.EthBtcSymbol:
                    return new EthBtcTradeService();
                case BitstampClient.BchUsdSymbol:
                    return new BchUsdTradeService();
                case BitstampClient.BchEurSymbol:
                    return new BchEurTradeService();
                case BitstampClient.BchBtcSymbol:
                    return new BchBtcTradeService();
                default:
                    throw new ArgumentException("Unsupported symbol.", nameof(tickerSymbol));
            }
        }

        internal bool IsEnabled
        {
            get
            {
                return this.Settings.NextTradeTime > 0;
            }

            set
            {
                if ((this.Settings.NextTradeTime == 0) && value)
                {
                    this.Settings.RetryIntervalMilliseconds = this.Settings.MinRetryIntervalMilliseconds;

                    if (this.Settings.PeriodEnd.HasValue)
                    {
                        this.Settings.SectionStart = DateTime.UtcNow;
                    }
                }

                this.ScheduleTrade(value ? GetEarliestTradeTime() : 0);
                Info("Set {0}.{1} = {2}", this.GetType().Name, nameof(this.IsEnabled), this.IsEnabled);
            }
        }

        internal ISettings Settings { get; }

        internal void ScheduleTrade()
        {
            var context = Application.Context;
            var manager = AlarmManager.FromContext(context);

            using (var intent = new Intent(context, this.GetType()))
            using (var alarmIntent = PendingIntent.GetService(context, 0, intent, PendingIntentFlags.UpdateCurrent))
            {
                if (this.Settings.NextTradeTime > 0)
                {
                    var currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                    Info("Current UNIX time is {0}.", currentTime);
                    var nextTradeTime = Math.Max(GetEarliestTradeTime(), this.Settings.NextTradeTime);
                    manager.Set(AlarmType.RtcWakeup, nextTradeTime, alarmIntent);
                    Info("Set alarm time to {0}.", nextTradeTime);
                }
                else
                {
                    manager.Cancel(alarmIntent);
                    Info("Cancelled alarm.");
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected TradeService(
            string tickerSymbol, int firstDecimals, int secondDecimals, decimal minTradeAmount, decimal feeStep)
        {
            this.tickerSymbol = tickerSymbol;
            this.firstDecimals = firstDecimals;
            this.secondDecimals = secondDecimals;
            this.minTradeAmount = minTradeAmount;
            this.feeStep = feeStep;
            this.Settings = SmartTrade.Settings.Create(this.tickerSymbol);
            this.Settings.PropertyChanged += this.OnSettingsPropertyChanged;
        }

        protected sealed override async void OnHandleIntent(Intent intent)
        {
            var currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            Info("Current time to trade time difference: {0}", currentTime - this.Settings.NextTradeTime);
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.GetTimeZone("UTC"));

            // Schedule a new trade first so that we retry even if the user kills the app, the runtime crashes or the
            // current system time is wrong (see below). It is expected that this scheduled trade will virtually never
            // be executed, so it's fine to apply the maximum interval. The shortest interval is not suitable because
            // this would lead to a race condition with the trade that we're executing next. This is due to the fact
            // that the default timeout for HTTP requests is 100 seconds. Since we're typically executing 3 requests, we
            // could very well still be executing a trade when the min interval ends.
            this.ScheduleTrade(calendar.TimeInMillis + this.Settings.MaxRetryIntervalMilliseconds);
            this.Settings.LogCurrentValues();

            if (calendar.Get(Java.Util.CalendarField.Year) < 2017)
            {
                // Sometimes (e.g. after booting a phone), the system time is not yet set to the current date. This will
                // confuse the trading algorithm, which is why we return here. The trade scheduled above will execute as
                // soon as the clock is set to the correct time.
                return;
            }

            Action<Intent> addArguments = i => new StatusActivity.Data(this.tickerSymbol).Put(i);
            var notification = new Notification(
                this, typeof(StatusActivity), addArguments, Resource.String.StatusTitle, this.tickerSymbol);
            var intervalMilliseconds =
                (long)(await this.TradeAsync(notification)).GetValueOrDefault().TotalMilliseconds;
            this.ScheduleTrade(Java.Lang.JavaSystem.CurrentTimeMillis() +
                Math.Max(this.Settings.RetryIntervalMilliseconds, intervalMilliseconds));
        }

        protected sealed override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.Settings.PropertyChanged -= this.OnSettingsPropertyChanged;
                    this.Settings.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static long GetEarliestTradeTime() => Java.Lang.JavaSystem.CurrentTimeMillis() + 5000;

        private static bool GetMore(DateTime lastTimestamp, List<ITransaction> transactions) =>
            (transactions.Count == 0) || (transactions[transactions.Count - 1].DateTime > lastTimestamp);

        private static bool IsRelevantDeposit(ITransaction transaction, bool buy)
        {
            switch (transaction.TransactionType)
            {
                case TransactionType.Deposit:
                case TransactionType.Withdrawal:
                case TransactionType.SubaccountTransfer:
                    return buy == (transaction.SecondAmount > 0);
                default:
                    return false;
            }
        }

        private readonly string tickerSymbol;
        private readonly int firstDecimals;
        private readonly int secondDecimals;
        private readonly decimal minTradeAmount;
        private readonly decimal feeStep;

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISettings.NextTradeTime))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEnabled)));
            }
        }

        private void ScheduleTrade(long time)
        {
            this.Settings.NextTradeTime = time;
            this.ScheduleTrade();
        }

        private async Task<List<ITransaction>> GetTransactionsAsync(ICurrencyExchange exchange)
        {
            var lastTradeTime = this.Settings.LastTransactionTimestamp;
            var result = new List<ITransaction>();

            for (int lastCount = -1, lastLimit = 0, limit = 10;
                (lastCount < result.Count) && (limit <= 1000) && GetMore(lastTradeTime, result);
                lastLimit = limit, limit *= 10)
            {
                lastCount = result.Count;
                Info("Retrieving transactions with offset={0} and limit={1}...", lastLimit, limit - lastLimit);
                result.AddRange(await exchange.GetTransactionsAsync(lastLimit, limit - lastLimit));
                Info("Retrieved {0} relevant transactions.", result.Count - lastCount);
            }

            if ((result.Count > 0) && (result[0].DateTime > this.Settings.LastTransactionTimestamp))
            {
                this.Settings.LastTransactionTimestamp = result[0].DateTime;
            }

            return result;
        }

        private bool SetPeriod(List<ITransaction> transactions, bool buy)
        {
            var lastDepositIndex = transactions.FindIndex(t => IsRelevantDeposit(t, buy));

            if (lastDepositIndex >= 0)
            {
                var deposit = transactions[lastDepositIndex];
                this.Settings.IsSubaccount = deposit.TransactionType == TransactionType.SubaccountTransfer;
                var lastDepositTime = deposit.DateTime;
                var result = this.Settings.SectionStart.HasValue;

                if (!result || (lastDepositTime > this.Settings.SectionStart))
                {
                    this.Settings.SectionStart = lastDepositTime;
                    this.Settings.PeriodEnd = lastDepositTime + TimeSpan.FromDays(this.Settings.TradePeriod);
                    return result;
                }
            }

            return false;
        }

        private DateTime GetStart(List<ITransaction> transactions)
        {
            var lastTransactionIndex = transactions.FindIndex(t => t.TransactionType == TransactionType.MarketTrade);

            if (lastTransactionIndex >= 0)
            {
                var lastTransactionTime = transactions[lastTransactionIndex].DateTime;

                if (this.Settings.SectionStart > lastTransactionTime)
                {
                    return this.Settings.SectionStart.Value;
                }
                else
                {
                    // Bitstamp has been observed to sometimes not report the latest of the transactions. This had the
                    // effect of this method returning an earlier time than the last trade. We guard against this by
                    // taking the maximum of both the latest transaction and this.Settings.LastTransactionTimestamp.
                    return this.Settings.LastTransactionTimestamp > lastTransactionTime ?
                        this.Settings.LastTransactionTimestamp : lastTransactionTime;
                }
            }
            else
            {
                return this.Settings.SectionStart.Value;
            }
        }

        /// <summary>Trades on the exchange.</summary>
        /// <returns>The time to wait before buying the next time. Is <c>null</c> if no deposit could be found, the
        /// balance is insufficient or if there was a temporary error.</returns>
        private async Task<TimeSpan?> TradeAsync(Notification notification)
        {
            var client = new BitstampClient(this.Settings.CustomerId, this.Settings.ApiKey, this.Settings.ApiSecret);

            try
            {
                this.Settings.LastStatus = this.GetString(Resource.String.TradeInProgressStatus);
                var exchange = client.Exchanges[this.tickerSymbol];
                this.Settings.LastTradeTime = DateTime.UtcNow;
                var balance = await exchange.GetBalanceAsync();
                var firstBalance = balance.FirstCurrency;
                var secondBalance = balance.SecondCurrency;
                this.Settings.LastBalanceFirstCurrency = (float)firstBalance;
                this.Settings.LastBalanceSecondCurrency = (float)secondBalance;
                var transactions = await this.GetTransactionsAsync(exchange);
                var buy = this.Settings.Buy;
                var hasTradePeriodEnded =
                    this.SetPeriod(transactions, buy) && ((buy ? firstBalance : secondBalance) > 0m);

                if (!this.Settings.PeriodEnd.HasValue)
                {
                    this.Settings.RetryIntervalMilliseconds = this.Settings.MaxRetryIntervalMilliseconds;
                    notification.Update(
                        this, Kind.Warning, this.Settings.NotifyEvents, Resource.String.NoDepositNotification);
                    return null;
                }

                Info(
                    "Current balance is {0} {1}.",
                    buy ? this.Settings.SecondCurrency : this.Settings.FirstCurrency,
                    buy ? secondBalance : firstBalance);

                var ticker = await exchange.GetTickerAsync();
                var calculator = new UnitCostAveragingCalculator(
                    this.Settings.PeriodEnd.Value, this.minTradeAmount, balance.Fee, this.feeStep);

                var startBalance = buy ? secondBalance : firstBalance * ticker.Bid;
                var start = this.GetStart(transactions);
                var secondAmount = calculator.GetTradeAmount(start, startBalance, startBalance);

                if (!secondAmount.HasValue)
                {
                    this.Settings.RetryIntervalMilliseconds = this.Settings.MaxRetryIntervalMilliseconds;
                    notification.Update(
                        this,
                        Kind.Warning,
                        this.Settings.NotifyEvents,
                        Resource.String.InsufficientBalanceNotification);
                    return null;
                }

                Info("Start is at {0:o}.", start);
                Info("Current time is {0:o}.", DateTime.UtcNow);
                Info("Amount to trade is {0} {1}.", this.Settings.SecondCurrency, secondAmount.Value);

                if (buy)
                {
                    // If this is the last trade, we need to subtract the fee first, as the exchange will do the
                    // same.
                    if (calculator.IsLastTrade(secondBalance, secondAmount.Value))
                    {
                        secondAmount -= calculator.GetFee(secondAmount.Value);
                    }

                    var firstAmountToTrade = Math.Round(secondAmount.Value / ticker.Ask, this.firstDecimals);
                    var result = await exchange.CreateBuyOrderAsync(firstAmountToTrade);
                    this.Settings.LastTradeTime = result.DateTime;
                    firstBalance += result.Amount;
                    var bought = result.Amount * result.Price;
                    notification.Update(
                        this,
                        Kind.Trade,
                        this.Settings.NotifyEvents,
                        Resource.String.BoughtNotification,
                        this.Settings.SecondCurrency,
                        bought,
                        this.Settings.FirstCurrency);

                    start = result.DateTime;
                    secondBalance -= bought + calculator.GetFee(bought);
                }
                else
                {
                    var firstAmountToTrade = Math.Round(secondAmount.Value / ticker.Bid, this.firstDecimals);
                    var result = await exchange.CreateSellOrderAsync(firstAmountToTrade);
                    this.Settings.LastTradeTime = result.DateTime;
                    firstBalance -= result.Amount;
                    var sold = result.Amount * result.Price;
                    notification.Update(
                        this,
                        Kind.Trade,
                        this.Settings.NotifyEvents,
                        Resource.String.SoldNotification,
                        this.Settings.SecondCurrency,
                        sold,
                        this.Settings.FirstCurrency);

                    start = result.DateTime;
                    secondBalance += sold - calculator.GetFee(sold);
                }

                ++this.Settings.TradeCountSinceLastTransfer;
                this.Settings.LastBalanceFirstCurrency = (float)firstBalance;
                this.Settings.LastBalanceSecondCurrency = (float)secondBalance;

                this.Settings.RetryIntervalMilliseconds = this.Settings.MinRetryIntervalMilliseconds;
                var nextTradeTime =
                    calculator.GetNextTime(start, buy ? secondBalance : firstBalance * ticker.Bid) - DateTime.UtcNow;

                if (!nextTradeTime.HasValue)
                {
                    this.Settings.RetryIntervalMilliseconds = this.Settings.MaxRetryIntervalMilliseconds;
                    hasTradePeriodEnded = true;
                }

                if (((secondAmount.Value > 0m) || hasTradePeriodEnded) &&
                    this.Settings.IsSubaccount && this.MakeTransfer(hasTradePeriodEnded))
                {
                    // Apparently, after getting confirmation for a successful trade, the traded currency is not yet
                    // credited to the balance. Waiting for a few seconds takes care of that...
                    await Task.Delay(5000);
                    var currency = buy ? this.Settings.FirstCurrency : this.Settings.SecondCurrency;
                    var factor = (decimal)Math.Pow(10, buy ? this.firstDecimals : this.secondDecimals);
                    var amount = Math.Floor((buy ? firstBalance : secondBalance) * factor) / factor;
                    Info("Amount to transfer is {0} {1}.", currency, amount);
                    await exchange.TransferToMainAccountAsync(this.Settings.Buy, amount);
                    notification.Append(
                        this,
                        Kind.Transfer,
                        this.Settings.NotifyEvents,
                        Resource.String.TransferredNotification,
                        currency,
                        amount);

                    this.Settings.TradeCountSinceLastTransfer = 0;

                    if (buy)
                    {
                        this.Settings.LastBalanceFirstCurrency = 0.0f;
                    }
                    else
                    {
                        this.Settings.LastBalanceSecondCurrency = 0.0f;
                    }
                }

                return nextTradeTime;
            }
            catch (Exception ex) when (ex is BitstampException ||
                ex is HttpRequestException || ex is WebException || ex is TaskCanceledException)
            {
                this.Settings.RetryIntervalMilliseconds *= 2;
                notification.Append(this, Kind.Warning, this.Settings.NotifyEvents, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                notification.Append(
                    this,
                    Kind.Error,
                    this.Settings.NotifyEvents,
                    Resource.String.UnexpectedErrorNotification,
                    ex.GetType().Name,
                    ex.Message);
                this.Settings.RetryIntervalMilliseconds = this.Settings.MaxRetryIntervalMilliseconds;
                this.IsEnabled = false;
                Warn("The service has been disabled due to an unexpected error: {0}", ex);
                throw;
            }
            finally
            {
                this.Settings.LastStatus = notification.ContentText;
                client.Dispose();
            }
        }

        private bool MakeTransfer(bool hasTradePeriodEnded)
        {
            switch (this.Settings.TransferToMainAccount)
            {
                case TransferToMainAccount.TradePeriodEnd:
                    return hasTradePeriodEnded;
                case TransferToMainAccount.EveryHundredthTrade:
                    return hasTradePeriodEnded || (this.Settings.TradeCountSinceLastTransfer >= 100);
                case TransferToMainAccount.EveryTenthTrade:
                    return hasTradePeriodEnded || (this.Settings.TradeCountSinceLastTransfer >= 10);
                case TransferToMainAccount.EveryTrade:
                    return this.Settings.TradeCountSinceLastTransfer >= 1;
                default:
                    return false;
            }
        }
    }
}
