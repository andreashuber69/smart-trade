////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient
    {
        internal abstract class CurrencyExchange : ICurrencyExchange
        {
            public string TickerSymbol { get; }

            public async Task<IBalance> GetBalanceAsync() => this.CreateBalance(await this.client.GetBalanceAsync());

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

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal abstract IBalance CreateBalance(Balance balance);

            internal abstract bool IsRelevantDepositOrWithdrawal(Transaction transaction);

            internal abstract bool IsRelevantTrade(Transaction transaction);

            internal abstract ITransaction CreateTransaction(Transaction transaction);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected CurrencyExchange(BitstampClient client, string tickerSymbol)
            {
                this.client = client;
                this.TickerSymbol = tickerSymbol;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly BitstampClient client;

            private string CurrencyPair => this.TickerSymbol.Replace("/", string.Empty).ToLowerInvariant();

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
        }
    }
}
