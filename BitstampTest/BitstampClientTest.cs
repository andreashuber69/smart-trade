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

            if (lastDepositIndex >= 0)
            {
                var deposit = transactions[lastDepositIndex];
                var balance = await exchange.GetBalanceAsync();
                var secondBalance = balance.SecondCurrency;
                var secondBalanceAtDeposit = secondBalance - GetBalanceDifference(transactions.Take(lastDepositIndex));
                var elapsedTime = DateTime.UtcNow - deposit.DateTime;
                var timeSpan = TimeSpan.FromDays(DateTime.DaysInMonth(deposit.DateTime.Year, deposit.DateTime.Month));
                var secondBalanceTarget = (1M - ((decimal)elapsedTime.Ticks / timeSpan.Ticks)) * secondBalanceAtDeposit;
                var orderBook = await exchange.GetOrderBookAsync();
                var ask = orderBook.Asks[0];
                var secondAmountTarget =
                    Min(secondBalance - secondBalanceTarget, Min(ask.Amount * ask.Price, secondBalance));
                var secondAmountToBuy =
                    Floor(secondAmountTarget * balance.Fee) / balance.Fee * (1 - (balance.Fee / 100));

                // Bitstamp minimum order size
                if (secondAmountToBuy >= 5)
                {
                    var price = Round(ask.Price, 2);
                    var firstAmountToBuy = Round(secondAmountToBuy / price, 8);
                    var result = await exchange.CreateBuyOrderAsync(firstAmountToBuy, price);
                }
            }
        }

        private static decimal GetBalanceDifference(IEnumerable<ITransaction> transactions) =>
            transactions.Aggregate(0M, (s, t) => s += GetAmountWithFee(t.SecondAmount, t.Fee));

        private static decimal GetAmountWithFee(decimal amount, decimal fee) =>
            amount < 0 ? amount - fee : amount + fee;
    }
}