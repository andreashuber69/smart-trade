////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Collections.Generic;
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
            get
            {
                return Settings.NextTradeTime > 0;
            }

            set
            {
                if ((Settings.NextTradeTime == 0) && value && Settings.PeriodEnd.HasValue)
                {
                    Settings.PeriodStart = DateTime.UtcNow;
                }

                ScheduleTrade(value ? JavaSystem.CurrentTimeMillis() : 0);
            }
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

        private static async Task<List<ITransaction>> GetTransactions(ICurrencyExchange exchange)
        {
            var lastTradeTime = Settings.LastTransactionTimestamp;
            var result = new List<ITransaction>();

            for (int lastCount = -1, lastLimit = 0, limit = 10;
                (lastCount < result.Count) && (limit <= 1000) && GetMore(lastTradeTime, result);
                lastLimit = limit, limit *= 10)
            {
                lastCount = result.Count;
                result.AddRange(await exchange.GetTransactionsAsync(lastLimit, limit - lastLimit));
            }

            if (result.Count > 0)
            {
                Settings.LastTransactionTimestamp = result[0].DateTime;
            }

            return result;
        }

        private static bool GetMore(DateTime lastTimestamp, List<ITransaction> transactions) =>
            (transactions.Count == 0) || (transactions[transactions.Count - 1].DateTime > lastTimestamp);

        private static void SetPeriod(List<ITransaction> transactions)
        {
            var lastDepositIndex = transactions.FindIndex(
                t => (t.TransactionType == TransactionType.Deposit) && (t.SecondAmount != 0));

            if (lastDepositIndex >= 0)
            {
                var lastDepositTime = transactions[lastDepositIndex].DateTime;

                if (!Settings.PeriodStart.HasValue || (lastDepositTime > Settings.PeriodStart))
                {
                    Settings.PeriodStart = lastDepositTime;
                    var duration =
                        TimeSpan.FromDays(DateTime.DaysInMonth(lastDepositTime.Year, lastDepositTime.Month));
                    Settings.PeriodEnd = lastDepositTime + duration;
                }
            }
        }

        private static DateTime GetSegmentStart(List<ITransaction> transactions)
        {
            var lastTradeIndex = transactions.FindIndex(
                t => (t.TransactionType == TransactionType.MarketTrade) || (t.SecondAmount != 0));

            if (lastTradeIndex >= 0)
            {
                var lastTradeTime = transactions[lastTradeIndex].DateTime;
                return Settings.PeriodStart > lastTradeTime ? Settings.PeriodStart.Value : lastTradeTime;
            }
            else
            {
                return Settings.PeriodStart.Value;
            }
        }

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
                    var transactions = await GetTransactions(exchange);
                    SetPeriod(transactions);

                    if (Settings.PeriodEnd.HasValue)
                    {
                        var calculator = new UnitCostAveragingCalculator(Settings.PeriodEnd.Value, 5, balance.Fee);
                        var segmentStart = GetSegmentStart(transactions);
                        var secondBalance = balance.SecondCurrency;
                        var ask = (await exchange.GetOrderBookAsync()).Asks[0];
                        var secondAmount = calculator.GetAmount(segmentStart, secondBalance, ask.Amount * ask.Price);

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
                        return calculator.GetNextTime(segmentStart, secondBalance - secondAmount) - DateTime.UtcNow;
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
