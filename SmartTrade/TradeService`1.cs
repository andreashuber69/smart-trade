////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
    /// <typeparam name="TExchangeClient">The type of the exchange client.</typeparam>
    /// <remarks>Reschedules itself after each buy/sell attempt.</remarks>
    internal abstract partial class TradeService<TExchangeClient> : IntentService, INotifyPropertyChanged
        where TExchangeClient : IExchangeClient, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal bool IsEnabled
        {
            get
            {
                return this.Settings.NextTradeTime > 0;
            }

            set
            {
                if ((this.Settings.NextTradeTime == 0) && value && this.Settings.PeriodEnd.HasValue)
                {
                    this.Settings.SectionStart = DateTime.UtcNow;
                }

                this.ScheduleTrade(value ? GetEarliestTradeTime() : 0);
                Info("Set {0}.{1} = {2}", this.GetType().Name, nameof(this.IsEnabled), this.IsEnabled);
            }
        }

        internal ISettings Settings => this.client.Settings;

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

        protected TradeService()
        {
            this.Settings.PropertyChanged += this.OnSettingsPropertyChanged;
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Settings.PropertyChanged -= this.OnSettingsPropertyChanged;
                this.client.Dispose();
            }

            base.Dispose(disposing);
        }

        protected sealed override async void OnHandleIntent(Intent intent)
        {
            var calendar = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.GetTimeZone("UTC"));

            // Schedule a new trade first so that we retry even if the user kills the app, the runtime crashes or the
            // current system time is wrong (see below). It is expected that this scheduled trade will virtually never
            // be executed, so it's fine to apply the maximum interval. The shortest interval is not suitable because
            // this would lead to a race condition with the trade that we're executing next. This is due to the fact
            // that the default timeout for HTTP requests is 100 seconds. Since we're typically executing 3 requests, we
            // could very well still be executing a trade when the min interval ends.
            this.ScheduleTrade(calendar.TimeInMillis + MaxRetryIntervalMilliseconds);
            this.Settings.LogCurrentValues();

            if (calendar.Get(Java.Util.CalendarField.Year) < 2017)
            {
                // Sometimes (e.g. after booting a phone), the system time is not yet set to the current date. This will
                // confuse the trading algorithm, which is why we return here. The trade scheduled above will execute as
                // soon as the clock is set to the correct time.
                return;
            }

            var popup = new NotificationPopup(this, Resource.String.CheckingPopup);
            var intervalMilliseconds = (long)(await this.TradeAsync(popup)).GetValueOrDefault().TotalMilliseconds;
            this.ScheduleTrade(Java.Lang.JavaSystem.CurrentTimeMillis() +
                Math.Max(this.Settings.RetryIntervalMilliseconds, intervalMilliseconds));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const long MinRetryIntervalMilliseconds = 2 * 60 * 1000;
        private const long MaxRetryIntervalMilliseconds = 64 * 60 * 1000;
        private static readonly decimal MinFiatAmount = 5;

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

        private readonly TExchangeClient client = new TExchangeClient();

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

        private async Task<List<ITransaction>> GetTransactions(ICurrencyExchange exchange)
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

            if (result.Count > 0)
            {
                this.Settings.LastTransactionTimestamp = result[0].DateTime;
            }

            return result;
        }

        private void SetPeriod(List<ITransaction> transactions, bool buy)
        {
            var lastDepositIndex = transactions.FindIndex(t => IsRelevantDeposit(t, buy));

            if (lastDepositIndex >= 0)
            {
                var deposit = transactions[lastDepositIndex];
                var lastDepositTime = deposit.DateTime;

                if (!this.Settings.SectionStart.HasValue || (lastDepositTime > this.Settings.SectionStart))
                {
                    this.Settings.SectionStart = lastDepositTime;
                    this.Settings.PeriodEnd = lastDepositTime + TimeSpan.FromDays(this.Settings.TradePeriod);
                }
            }
        }

        private DateTime GetStart(List<ITransaction> transactions)
        {
            var lastTradeIndex = transactions.FindIndex(t => t.TransactionType == TransactionType.MarketTrade);

            if (lastTradeIndex >= 0)
            {
                var lastTradeTime = transactions[lastTradeIndex].DateTime;
                return this.Settings.SectionStart > lastTradeTime ? this.Settings.SectionStart.Value : lastTradeTime;
            }
            else
            {
                return this.Settings.SectionStart.Value;
            }
        }

        /// <summary>Trades on the exchange.</summary>
        /// <returns>The time to wait before buying the next time. Is <c>null</c> if no deposit could be found, the
        /// balance is insufficient or if there was a temporary error.</returns>
        private async Task<TimeSpan?> TradeAsync(NotificationPopup popup)
        {
            try
            {
                var exchange = this.client.CurrencyExchange;
                this.Settings.LastTradeTime = DateTime.UtcNow;
                var balance = await exchange.GetBalanceAsync();
                var firstBalance = balance.FirstCurrency;
                var secondBalance = balance.SecondCurrency;
                this.Settings.LastBalanceFirstCurrency = (float)firstBalance;
                this.Settings.LastBalanceSecondCurrency = (float)secondBalance;
                var transactions = await this.GetTransactions(exchange);
                var buy = this.Settings.Buy;
                this.SetPeriod(transactions, buy);

                if (!this.Settings.PeriodEnd.HasValue)
                {
                    this.Settings.RetryIntervalMilliseconds = MaxRetryIntervalMilliseconds;
                    popup.Update(this, Resource.String.NoDepositPopup);
                    return null;
                }

                var fee = balance.Fee;
                var firstCurrency = exchange.TickerSymbol.Substring(0, 3);
                var secondCurrency = exchange.TickerSymbol.Substring(3);

                Info(
                    "Current balance is {0} {1}.",
                    buy ? secondCurrency : firstCurrency,
                    buy ? secondBalance : firstBalance);

                var ticker = await exchange.GetTickerAsync();
                var minSpendable = UnitCostAveragingCalculator.GetMinSpendableAmount(MinFiatAmount, fee);

                if ((buy ? secondBalance : firstBalance * ticker.Bid) < minSpendable)
                {
                    this.Settings.RetryIntervalMilliseconds = MaxRetryIntervalMilliseconds;
                    popup.Update(this, Resource.String.InsufficientBalancePopup);
                    return null;
                }

                var calculator = new UnitCostAveragingCalculator(this.Settings.PeriodEnd.Value, MinFiatAmount, fee);
                var start = this.GetStart(transactions);
                Info("Start is at {0:o}.", start);
                Info("Current time is {0:o}.", DateTime.UtcNow);
                var startBalance = buy ? secondBalance : firstBalance * ticker.Bid;
                var secondAmount = calculator.GetAmount(start, startBalance, startBalance);

                if (secondAmount > 0)
                {
                    Info("Amount to trade is {0} {1}.", secondCurrency, secondAmount);

                    // When we trade fiat on Bitstamp, the fee is calculated as implemented in
                    // UnitCostAveragingCalculator.GetFee. We always want to sell a bit less than calculated
                    // by UnitCostAveragingCalculator.GetAmount, otherwise we end up paying 1 cent more than necessary.
                    // Since the market can move between the time we query the price and the time our trade is executed,
                    // we cannot just subtract a constant amount (like e.g. 0.001, as we did in tests). Instead, we need
                    // to lower the amount such that it becomes unlikely that the fiat amount will move over the
                    // threshold. For now we try with 0.1%, which amounts to less than 1 cent for a typical trade.
                    var secondAmountToTrade = secondAmount * 0.999m;

                    if (buy)
                    {
                        var firstAmountToTrade = Math.Round(secondAmountToTrade / ticker.Ask, 8);
                        var result = await exchange.CreateBuyOrderAsync(firstAmountToTrade);
                        this.Settings.LastTradeTime = result.DateTime;
                        firstBalance += result.Amount;
                        var bought = result.Amount * result.Price;
                        popup.Update(this, Resource.String.BoughtPopup, secondCurrency, bought, firstCurrency);

                        start = result.DateTime;
                        secondAmount = bought + calculator.GetFee(bought);
                        secondBalance -= secondAmount;
                    }
                    else
                    {
                        var firstAmountToTrade = Math.Round(secondAmountToTrade / ticker.Bid, 8);
                        var result = await exchange.CreateSellOrderAsync(firstAmountToTrade);
                        this.Settings.LastTradeTime = result.DateTime;
                        firstBalance -= result.Amount;
                        var sold = result.Amount * result.Price;
                        popup.Update(this, Resource.String.SoldPopup, secondCurrency, sold, firstCurrency);

                        start = result.DateTime;
                        secondAmount = sold - calculator.GetFee(sold);
                        secondBalance += secondAmount;
                    }

                    this.Settings.LastBalanceFirstCurrency = (float)firstBalance;
                    this.Settings.LastBalanceSecondCurrency = (float)secondBalance;
                }
                else
                {
                    popup.Update(this, Resource.String.NothingToTradePopup);
                    popup.Dispose();
                }

                this.Settings.RetryIntervalMilliseconds = MinRetryIntervalMilliseconds;
                var nextTradeTime = calculator.GetNextTime(start, buy ? secondBalance : firstBalance * ticker.Bid) - DateTime.UtcNow;

                if (!nextTradeTime.HasValue)
                {
                    this.Settings.RetryIntervalMilliseconds = MaxRetryIntervalMilliseconds;
                }

                return nextTradeTime;
            }
            catch (Exception ex) when (ex is BitstampException ||
                ex is HttpRequestException || ex is WebException || ex is TaskCanceledException)
            {
                this.Settings.RetryIntervalMilliseconds = this.Settings.RetryIntervalMilliseconds * 2;
                popup.Update(this, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                popup.Update(this, Resource.String.UnexpectedErrorPopup, ex.GetType().Name, ex.Message);
                this.Settings.RetryIntervalMilliseconds = MinRetryIntervalMilliseconds;
                this.IsEnabled = false;
                Warn("The service has been disabled due to an unexpected error: {0}", ex);
                throw;
            }
            finally
            {
                this.Settings.LastResult = popup.ContentText;
                this.Settings.RetryIntervalMilliseconds = Math.Max(
                    MinRetryIntervalMilliseconds,
                    Math.Min(MaxRetryIntervalMilliseconds, this.Settings.RetryIntervalMilliseconds));
            }
        }
    }
}
