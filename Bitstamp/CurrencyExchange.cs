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
            public string TickerSymbol => this.tickerSymbol.ToUpperInvariant();

            public async Task<IBalance> GetBalanceAsync() => this.CreateBalance(await this.client.GetBalanceAsync());

            public async Task<IEnumerable<ITransaction>> GetTransactionsAsync(int offset, int limit)
            {
                var transactions = await this.client.GetTransactionsAsync(offset, limit);
                return transactions.Select(t => this.CreateTransaction(t)).Where(t => t != null);
            }

            public Task<Ticker> GetTickerAsync() => this.client.GetTickerAsync(this.tickerSymbol);

            public Task<OrderBook> GetOrderBookAsync() => this.client.GetOrderBookAsync(this.tickerSymbol);

            public Task<PrivateOrder> CreateBuyOrderAsync(decimal amount) =>
                this.client.CreateBuyOrderAsync(this.tickerSymbol, amount);

            public Task<PrivateOrder> CreateBuyOrderAsync(decimal amount, decimal price) =>
                this.client.CreateBuyOrderAsync(this.tickerSymbol, amount, price);

            public Task<PrivateOrder> CreateSellOrderAsync(decimal amount) =>
                this.client.CreateSellOrderAsync(this.tickerSymbol, amount);

            public Task<PrivateOrder> CreateSellOrderAsync(decimal amount, decimal price) =>
                this.client.CreateSellOrderAsync(this.tickerSymbol, amount, price);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal CurrencyExchange(BitstampClient client, string tickerSymbol)
            {
                this.client = client;
                this.tickerSymbol = tickerSymbol;
            }

            internal abstract IBalance CreateBalance(Balance balance);

            internal abstract ITransaction CreateTransaction(Transaction transaction);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly BitstampClient client;
            private readonly string tickerSymbol;
        }
    }
}
