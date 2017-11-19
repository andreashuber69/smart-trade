////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient
    {
        internal abstract class CurrencyExchange : ICurrencyExchange
        {
            public string TickerSymbol { get; }

            public async Task<IBalance> GetBalanceAsync() =>
                this.CreateBalance(await this.client.GetBalanceAsync(this.FirstCurrency, this.SecondCurrency));

            public async Task<IEnumerable<ITransaction>> GetTransactionsAsync(int offset, int limit)
            {
                var transactions = await this.client.GetTransactionsAsync(offset, limit);
                return transactions.Where(t => this.IsRelevant(t)).Select(t => this.CreateTransaction(t));
            }

            public Task<Ticker> GetTickerAsync() => this.client.GetTickerAsync(this.CurrencyPair);

            public Task<OrderBook> GetOrderBookAsync() => this.client.GetOrderBookAsync(this.CurrencyPair);

            public Task<PrivateOrder> CreateBuyOrderAsync(decimal amount) =>
                this.client.CreateBuyOrderAsync(this.CurrencyPair, amount);

            public Task<PrivateOrder> CreateBuyOrderAsync(decimal amount, decimal price) =>
                this.client.CreateBuyOrderAsync(this.CurrencyPair, amount, price);

            public Task<PrivateOrder> CreateSellOrderAsync(decimal amount) =>
                this.client.CreateSellOrderAsync(this.CurrencyPair, amount);

            public Task<PrivateOrder> CreateSellOrderAsync(decimal amount, decimal price) =>
                this.client.CreateSellOrderAsync(this.CurrencyPair, amount, price);

            public Task TransferToMainAccountAsync(bool firstCurrency, decimal amount) =>
                this.client.TransferToMainAccountAsync(firstCurrency ? this.FirstCurrency : this.SecondCurrency, amount);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected static ITransaction CreateTransaction(
                int id,
                DateTime dateTime,
                TransactionType transactionType,
                decimal? firstAmount,
                decimal? secondAmount,
                decimal? price,
                decimal fee,
                int? orderId)
            {
                return new TransactionImpl(id, dateTime, transactionType, firstAmount, secondAmount, price, fee, orderId);
            }

            protected CurrencyExchange(BitstampClient client, string tickerSymbol)
            {
                this.client = client;
                this.TickerSymbol = tickerSymbol;
            }

            protected abstract bool IsRelevantDepositOrWithdrawal(Transaction t);

            protected abstract bool IsRelevantTrade(Transaction t);

            protected abstract ITransaction CreateTransaction(Transaction t);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly BitstampClient client;

            // TODO: This duplicates code in the Settings class.
            private string FirstCurrency => this.TickerSymbol.Substring(0, this.TickerSymbol.IndexOf('/'));

            // TODO: This duplicates code in the Settings class.
            private string SecondCurrency => this.TickerSymbol.Substring(this.TickerSymbol.IndexOf('/') + 1);

            private string CurrencyPair => this.TickerSymbol.Replace("/", string.Empty).ToLowerInvariant();

            private IBalance CreateBalance(Balance b) => new BalanceImpl(b.FirstAvailable, b.SecondAvailable, b.Fee);

            private bool IsRelevant(Transaction transaction)
            {
                switch (transaction.TransactionType)
                {
                    case TransactionType.Deposit:
                    case TransactionType.Withdrawal:
                    case TransactionType.SubaccountTransfer:
                        return this.IsRelevantDepositOrWithdrawal(transaction);
                    case TransactionType.MarketTrade:
                        return this.IsRelevantTrade(transaction);
                    default:
                        return false;
                }
            }

            private sealed class BalanceImpl : IBalance
            {
                public decimal FirstCurrency { get; }

                public decimal SecondCurrency { get; }

                public decimal Fee { get; }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                internal BalanceImpl(decimal firstCurrency, decimal secondCurrency, decimal fee)
                {
                    this.FirstCurrency = firstCurrency;
                    this.SecondCurrency = secondCurrency;
                    this.Fee = fee;
                }
            }

            private sealed class TransactionImpl : ITransaction
            {
                public int Id { get; }

                public DateTime DateTime { get; }

                public TransactionType TransactionType { get; }

                public decimal? FirstAmount { get; }

                public decimal? SecondAmount { get; }

                public decimal? Price { get; }

                public decimal Fee { get; }

                public int? OrderId { get; }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                internal TransactionImpl(
                    int id,
                    DateTime dateTime,
                    TransactionType transactionType,
                    decimal? firstAmount,
                    decimal? secondAmount,
                    decimal? price,
                    decimal fee,
                    int? orderId)
                {
                    this.Id = id;
                    this.DateTime = dateTime;
                    this.TransactionType = transactionType;
                    this.FirstAmount = firstAmount;
                    this.SecondAmount = secondAmount;
                    this.Price = price;
                    this.Fee = fee;
                    this.OrderId = orderId;
                }
            }
        }
    }
}
