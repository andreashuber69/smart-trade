////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Bitstamp;
    using Java.Lang;

    using static System.FormattableString;
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
            var popup = new NotificationPopup(this, this.Resources.GetString(Resource.String.service_buying));

            using (var client = new BitstampClient())
            {
                var waitTicks = (await this.BuyAsync(client.BtcEur, popup)).GetValueOrDefault().Ticks;
                var waitTime = new TimeSpan(Max(TimeSpan.FromHours(1).Ticks, waitTicks));
                ScheduleTrade(JavaSystem.CurrentTimeMillis() + (long)waitTime.TotalMilliseconds);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly decimal MinAmount = 5;

        private static void ScheduleTrade(long time)
        {
            Settings.NextTradeTime = time;
            ScheduleTrade();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The disposables are passed to API methods, TODO.")]
        private static void ScheduleTrade()
        {
            var context = Application.Context;
            var manager = AlarmManager.FromContext(context);
            var alarmIntent = PendingIntent.GetService(
                context, 0, new Intent(context, typeof(TradeService)), PendingIntentFlags.UpdateCurrent);

            if (Settings.NextTradeTime > 0)
            {
                var earliestNextTradeTime = JavaSystem.CurrentTimeMillis() + (10 * 1000);
                var nextTradeTime = Max(earliestNextTradeTime, Settings.NextTradeTime);
                manager.Set(AlarmType.RtcWakeup, nextTradeTime, alarmIntent);
            }
            else
            {
                manager.Cancel(alarmIntent);
            }
        }

        private static decimal GetBalanceDifference(IEnumerable<ITransaction> transactions) =>
            transactions.Aggregate(0M, (s, t) => s += GetAmountWithFee(t.SecondAmount, t.Fee));

        private static decimal GetAmountWithFee(decimal amount, decimal fee) =>
            amount < 0 ? amount - fee : amount + fee;

        /// <summary>Buys on the exchange.</summary>
        /// <returns>The time to wait before buying the next time. Is <c>null</c> if no deposit could be found or if
        /// the balance is insufficient.</returns>
        private async Task<TimeSpan?> BuyAsync(ICurrencyExchange exchange, NotificationPopup popup)
        {
            try
            {
                var balance = await exchange.GetBalanceAsync();

                if (balance.SecondCurrency >= UnitCostAveragingTrader.GetMinSpendableAmount(MinAmount, balance.Fee))
                {
                    var transactions = (await exchange.GetTransactionsAsync()).ToList();
                    var lastDepositIndex = transactions.FindIndex(t => t.TransactionType == TransactionType.Deposit);

                    if (lastDepositIndex >= 0)
                    {
                        var secondBalance = balance.SecondCurrency;
                        var deposit = transactions[lastDepositIndex];
                        var secondBalanceAtDeposit = secondBalance - GetBalanceDifference(transactions.Take(lastDepositIndex));
                        var duration = TimeSpan.FromDays(DateTime.DaysInMonth(deposit.DateTime.Year, deposit.DateTime.Month));
                        var trader =
                            new UnitCostAveragingTrader(deposit.DateTime, secondBalanceAtDeposit, duration, 5, balance.Fee);
                        var orderBook = await exchange.GetOrderBookAsync();
                        var ask = orderBook.Asks[0];
                        var price = Round(ask.Price, 2); // Sometimes the price is not yet rounded to two decimals
                        var secondAmountToSpend = trader.GetAmount(secondBalance, ask.Amount * price);

                        if (secondAmountToSpend > 0)
                        {
                            var firstAmountToBuy = Round(trader.SubtractFee(secondAmountToSpend) / price, 8);
                            var result = await exchange.CreateBuyOrderAsync(firstAmountToBuy);
                            popup.Update(this, Invariant($"Bought {result.Amount * result.Price}."));
                        }
                        else
                        {
                            popup.Update(this, "Amount to spend is zero.");
                        }

                        // popup.Dispose();
                        return trader.GetNextTime(secondBalance - secondAmountToSpend) - DateTime.UtcNow;
                    }
                    else
                    {
                        popup.Update(this, "No deposit found.");
                    }
                }
                else
                {
                    popup.Update(this, "Insufficient balance.");
                }
            }
            catch (System.Exception ex) when (ex is BitstampException || ex is HttpRequestException)
            {
                popup.Update(this, ex.Message);
            }

            return null;
        }
    }
}
