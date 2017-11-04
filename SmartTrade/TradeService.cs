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
                if ((this.Settings.NextTradeTime == 0) && value && this.Settings.PeriodEnd.HasValue)
                {
                    this.Settings.SectionStart = DateTime.UtcNow;
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

        protected TradeService(string tickerSymbol, decimal minTradeAmount, decimal feeStep)
        {
            this.tickerSymbol = tickerSymbol;
            this.minTradeAmount = minTradeAmount;
            this.feeStep = feeStep;
            this.Settings = new Settings(this.tickerSymbol);
            this.Settings.PropertyChanged += this.OnSettingsPropertyChanged;
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

            Action<Intent> addArguments = i => new StatusActivity.Data(this.tickerSymbol).Put(i);
            var popup = new NotificationPopup(
                this,
                typeof(StatusActivity),
                addArguments,
                Resource.String.StatusTitle,
                this.tickerSymbol,
                Resource.String.CheckingPopup);
            var intervalMilliseconds = (long)(await this.TradeAsync(popup)).GetValueOrDefault().TotalMilliseconds;
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

        private const long MinRetryIntervalMilliseconds = 2 * 60 * 1000;
        private const long MaxRetryIntervalMilliseconds = 64 * 60 * 1000;

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
            var client = new BitstampClient(this.Settings.CustomerId, this.Settings.ApiKey, this.Settings.ApiSecret);

            try
            {
                var exchange = client.Exchanges[this.tickerSymbol];
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
                    this.Settings.RetryIntervalMilliseconds = MaxRetryIntervalMilliseconds;
                    popup.Update(this, Resource.String.InsufficientBalancePopup);
                    return null;
                }

                Info("Start is at {0:o}.", start);
                Info("Current time is {0:o}.", DateTime.UtcNow);

                if (secondAmount.Value > 0m)
                {
                    // When we trade on Bitstamp, the fee is calculated as implemented in
                    // UnitCostAveragingCalculator.GetFee. The fee is charged in discrete steps (e.g. 0.01 for fiat and
                    // 0.00001 for BTC) and always rounded up to the next step. We therefore always want to sell a bit
                    // less than calculated by UnitCostAveragingCalculator.GetAmount, otherwise we end up paying a fee
                    // step more than necessary.
                    // Since the market can move between the time we query the price and the time our trade is executed,
                    // we cannot just subtract a constant amount (like e.g. 0.001, as we did in tests). In general, we
                    // need to lower the amount such that the average total paid to trade a given amount is as low as
                    // possible. The average total is higher than optimal because a) additional trades need to be made
                    // due to the lowered per trade amount and b) occasionly the amount traded goes over the fee
                    // threshold due to the moving market.
                    // Examples:
                    // - If we lowered the trade amount by just one satoshi, we would expect that roughly half of the
                    // trades pay higher fees than intended. With a 0.25% fee, for a goal of buying EUR 8000 worth of
                    // BTC we'd thus end up with 500 EUR 8 trades paying 3 cents in fees and 500 EUR 8 trades paying 2
                    // cents in fees. We'd therefore pay EUR 8025 for EUR 8000 worth of BTC.
                    // - If we lowered the amount per trade by 0.1%, we end up having to put in 1001 EUR 7.992 trades.
                    // If that reduced the number of trades going over the threshold to 20%, we'd get 200 trades paying
                    // 3 cents in fees and 801 trades paying 2 cents in fees. We'd therefore pay ~EUR 8022 for EUR 8000
                    // worth of BTC.
                    // We therefore need to lower the per trade amount such that the fees paid for the additional number
                    // of trades *and* the fees paid for the trades that go over the fee threshold reaches a minimum.
                    // Tests with 0.3% resulted in roughly 1% of the trades going over the threshold, which is why we
                    // try with 0.4% for now.
                    var secondAmountToTrade = secondAmount.Value * 0.996m;
                    Info("Amount to trade is {0} {1}.", this.Settings.SecondCurrency, secondAmountToTrade);

                    if (buy)
                    {
                        // If we're going to spend the whole second balance, we need to subtract the fee first, as the
                        // exchange will do the same.
                        if (secondAmount.Value == secondBalance)
                        {
                            secondAmountToTrade -= calculator.GetFee(secondAmountToTrade);
                        }

                        var firstAmountToTrade = Math.Round(secondAmountToTrade / ticker.Ask, 8);
                        var result = await exchange.CreateBuyOrderAsync(firstAmountToTrade);
                        this.Settings.LastTradeTime = result.DateTime;
                        firstBalance += result.Amount;
                        var bought = result.Amount * result.Price;
                        popup.Update(
                            this,
                            Resource.String.BoughtPopup,
                            this.Settings.SecondCurrency,
                            bought,
                            this.Settings.FirstCurrency);

                        start = result.DateTime;
                        secondBalance -= bought + calculator.GetFee(bought);
                    }
                    else
                    {
                        var firstAmountToTrade = Math.Round(secondAmountToTrade / ticker.Bid, 8);
                        var result = await exchange.CreateSellOrderAsync(firstAmountToTrade);
                        this.Settings.LastTradeTime = result.DateTime;
                        firstBalance -= result.Amount;
                        var sold = result.Amount * result.Price;
                        popup.Update(
                            this,
                            Resource.String.SoldPopup,
                            this.Settings.SecondCurrency,
                            sold,
                            this.Settings.FirstCurrency);

                        start = result.DateTime;
                        secondBalance += sold - calculator.GetFee(sold);
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
                var nextTradeTime =
                    calculator.GetNextTime(start, buy ? secondBalance : firstBalance * ticker.Bid) - DateTime.UtcNow;

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
                client.Dispose();
            }
        }
    }
}
