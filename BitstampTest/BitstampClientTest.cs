////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace BitstampTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using Bitstamp;
    using NUnit.Framework;

    using static System.Math;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    [TestFixture]
    internal sealed class BitstampClientTest
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test method cannot be static.")]
        [Test]
        public async void MainTest()
        {
            using (var client = new BitstampClient())
            {
                await MainTestImpl(client.BtcEur);
            }
        }

        private static async Task MainTestImpl(ICurrencyExchange exchange)
        {
            var transactions = (await exchange.GetTransactionsAsync()).ToList();
            var lastDepositIndex = transactions.FindIndex(t => t.TransactionType == TransactionType.Deposit);
            var lastTradeIndex = transactions.FindIndex(t => t.TransactionType != TransactionType.Withdrawal);

            if ((lastDepositIndex >= 0) && (lastTradeIndex >= 0))
            {
                var deposit = transactions[lastDepositIndex];
                var balance = await exchange.GetBalanceAsync();
                var secondBalance = balance.SecondCurrency;
                var secondBalanceAtDeposit = secondBalance - GetBalanceDifference(transactions.Take(lastDepositIndex));
                var duration = TimeSpan.FromDays(DateTime.DaysInMonth(deposit.DateTime.Year, deposit.DateTime.Month));
                var trader = new UnitCostAveragingTrader(deposit.DateTime + duration, 5, balance.Fee);
                var orderBook = await exchange.GetOrderBookAsync();
                var ask = orderBook.Asks[0];

                var lastTradeTime = transactions[lastTradeIndex].DateTime;
                var secondAmountToBuy = trader.GetAmount(lastTradeTime, secondBalance, ask.Amount * ask.Price);
                var time = trader.GetNextTime(lastTradeTime, secondBalance);

                if (secondAmountToBuy > 0)
                {
                    var firstAmountToBuy = Round(secondAmountToBuy / ask.Price, 8);
                    var result = await exchange.CreateBuyOrderAsync(firstAmountToBuy);
                }
            }
        }

        private static decimal GetBalanceDifference(IEnumerable<ITransaction> transactions) =>
            transactions.Aggregate(0M, (s, t) => s += GetAmountWithFee(t.SecondAmount, t.Fee));

        private static decimal GetAmountWithFee(decimal amount, decimal fee) =>
            amount < 0 ? amount - fee : amount + fee;
    }
}