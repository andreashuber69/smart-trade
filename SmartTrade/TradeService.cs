////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Bitstamp;
    using Java.Lang;
    using Java.Util;

    using static System.Math;

    /// <summary>Buys or sells according to the configured schedule.</summary>
    /// <remarks>Reschedules itself after each buy/sell attempt.</remarks>
    [Service]
    internal sealed partial class TradeService : IntentService
    {
        internal static bool IsEnabled
        {
            get { return Settings.NextTradeTime > 0; }
            set { ScheduleTrade(value ? JavaSystem.CurrentTimeMillis() : 0); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override async void OnHandleIntent(Intent intent)
        {
            var calendar = Calendar.GetInstance(Java.Util.TimeZone.GetTimeZone("UTC"));

            // Schedule a new trade first so that we retry even if the user kills the app, the runtime crashes or the
            // current system time is wrong (see below). It is expected that this scheduled trade will virtually never
            // be exected, so it's fine to apply the maximum interval. The shortest interval is not suitable because
            // this would lead to a race condition with the trade that we're executing next. This is due to the fact
            // that the default timeout for HTTP requests is 100 seconds. Since we're typically executing 3 requests, we
            // could very well still be executing a trade when the min interval ends.
            ScheduleTrade(calendar.TimeInMillis + MaxRetryIntervalMilliseconds);

            if (calendar.Get(CalendarField.Year) < 2017)
            {
                // Sometimes (e.g. after booting a phone), the system time is not yet set to the current date. This will
                // confuse the trading algorithm, which is why we return here. The trade scheduled above will execute as
                // soon as the clock is set to the correct time.
                return;
            }

            Settings.RetryIntervalMilliseconds = Max(
                MinRetryIntervalMilliseconds, Min(MaxRetryIntervalMilliseconds, Settings.RetryIntervalMilliseconds));

            var popup = new NotificationPopup(this, Resource.String.service_checking);

            using (var client = new BitstampClient())
            {
                var intervalMilliseconds =
                    (long)(await this.BuyAsync(client.BtcEur, popup)).GetValueOrDefault().TotalMilliseconds;
                ScheduleTrade(
                    JavaSystem.CurrentTimeMillis() + Max(Settings.RetryIntervalMilliseconds, intervalMilliseconds));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const long MinRetryIntervalMilliseconds = 2 * 60 * 1000;
        private const long MaxRetryIntervalMilliseconds = 64 * 60 * 1000;
        private static readonly decimal MinAmount = 5;

        private static void ScheduleTrade(long time)
        {
            Settings.NextTradeTime = time;
            ScheduleTrade();
        }

        private static void ScheduleTrade()
        {
            var context = Application.Context;
            var manager = AlarmManager.FromContext(context);

            using (var intent = new Intent(context, typeof(TradeService)))
            using (var alarmIntent = PendingIntent.GetService(context, 0, intent, PendingIntentFlags.UpdateCurrent))
            {
                if (Settings.NextTradeTime > 0)
                {
                    var earliestNextTradeTime = JavaSystem.CurrentTimeMillis() + 5000;
                    var nextTradeTime = Max(earliestNextTradeTime, Settings.NextTradeTime);
                    manager.Set(AlarmType.RtcWakeup, nextTradeTime, alarmIntent);
                }
                else
                {
                    manager.Cancel(alarmIntent);
                }
            }
        }

        private static decimal GetBalanceDifference(IEnumerable<ITransaction> transactions) =>
            transactions.Aggregate(0M, (s, t) => s += GetAmountWithFee(t.SecondAmount, t.Fee));

        private static decimal GetAmountWithFee(decimal amount, decimal fee) =>
            amount < 0 ? amount - fee : amount + fee;

        /// <summary>Buys on the exchange.</summary>
        /// <returns>The time to wait before buying the next time. Is <c>null</c> if no deposit could be found, the
        /// balance is insufficient or if there was a temporary error.</returns>
        private async Task<TimeSpan?> BuyAsync(ICurrencyExchange exchange, NotificationPopup popup)
        {
            try
            {
                var balance = await exchange.GetBalanceAsync();

                if (balance.SecondCurrency >= UnitCostAveragingCalculator.GetMinSpendableAmount(MinAmount, balance.Fee))
                {
                    var transactions = (await exchange.GetTransactionsAsync(0, 100)).ToList();
                    var lastDepositIndex = transactions.FindIndex(t => t.TransactionType == TransactionType.Deposit);
                    var lastTradeIndex = transactions.FindIndex(t => t.TransactionType != TransactionType.Withdrawal);

                    if ((lastDepositIndex >= 0) && (lastTradeIndex >= 0))
                    {
                        var secondBalance = balance.SecondCurrency;
                        var deposit = transactions[lastDepositIndex];
                        var secondBalanceAtDeposit =
                            secondBalance - GetBalanceDifference(transactions.Take(lastDepositIndex));
                        var duration =
                            TimeSpan.FromDays(DateTime.DaysInMonth(deposit.DateTime.Year, deposit.DateTime.Month));
                        var calculator = new UnitCostAveragingCalculator(deposit.DateTime + duration, 5, balance.Fee);
                        var orderBook = await exchange.GetOrderBookAsync();
                        var ask = orderBook.Asks[0];
                        var lastTradeTime = transactions[lastTradeIndex].DateTime;
                        var secondAmount = calculator.GetAmount(lastTradeTime, secondBalance, ask.Amount * ask.Price);

                        if (secondAmount > 0)
                        {
                            var firstAmountToBuy =
                                Round((secondAmount - calculator.GetFee(secondAmount)) / ask.Price, 8);
                            var result = await exchange.CreateBuyOrderAsync(firstAmountToBuy);
                            var secondSymbol = exchange.TickerSymbol.Substring(3);
                            var secondAmountBought = result.Amount * result.Price;
                            var firstSymbol = exchange.TickerSymbol.Substring(0, 3);
                            popup.Update(
                                this, Resource.String.service_bought, secondSymbol, secondAmountBought, firstSymbol);
                            secondAmount = secondAmountBought + calculator.GetFee(secondAmountBought);
                        }
                        else
                        {
                            popup.Dispose();
                        }

                        Settings.RetryIntervalMilliseconds = MinRetryIntervalMilliseconds;
                        return calculator.GetNextTime(lastTradeTime, secondBalance - secondAmount) - DateTime.UtcNow;
                    }
                    else
                    {
                        Settings.RetryIntervalMilliseconds = MaxRetryIntervalMilliseconds;
                        popup.Update(this, Resource.String.service_no_deposit);
                    }
                }
                else
                {
                    Settings.RetryIntervalMilliseconds = MaxRetryIntervalMilliseconds;
                    popup.Update(this, Resource.String.service_insufficient_balance);
                }
            }
            catch (System.Exception ex) when (ex is BitstampException ||
                ex is HttpRequestException || ex is WebException || ex is TaskCanceledException)
            {
                Settings.RetryIntervalMilliseconds = Settings.RetryIntervalMilliseconds * 2;
                popup.Update(this, ex.Message);
            }
            catch (System.Exception ex)
            {
                popup.Update(this, Resource.String.service_unexpected_error, ex.GetType().Name, ex.Message);
                Settings.RetryIntervalMilliseconds = MinRetryIntervalMilliseconds;
                IsEnabled = false;
                throw;
            }

            return null;
        }
    }
}
