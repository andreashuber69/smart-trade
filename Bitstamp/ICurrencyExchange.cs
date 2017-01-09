////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>Represents an exchange for a currency pair.</summary>
    public interface ICurrencyExchange
    {
        /// <summary>Gets the tickersymbol for the exchange.</summary>
        string TickerSymbol { get; }

        /// <summary>Gets the account balance.</summary>
        /// <exception cref="BitstampException">The Bitstamp server reported an error.</exception>
        /// <exception cref="HttpRequestException">The Bitstamp server could either not be reached or reported an
        /// unexpected error.</exception>
        /// <exception cref="InvalidOperationException">The private API cannot be accessed with this instance.
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        Task<IBalance> GetBalanceAsync();

        /// <summary>Gets transactions in descending order.</summary>
        /// <param name="offset">The number of transactions to skip.</param>
        /// <param name="limit">The maximum number of transactions to return.</param>
        /// <exception cref="BitstampException">The Bitstamp server reported an error.</exception>
        /// <exception cref="HttpRequestException">The Bitstamp server could either not be reached or reported an
        /// unexpected error.</exception>
        /// <exception cref="InvalidOperationException">The private API cannot be accessed with this instance.
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        Task<IEnumerable<ITransaction>> GetTransactionsAsync(int offset, int limit);

        /// <summary>Gets the order book.</summary>
        /// <exception cref="BitstampException">The Bitstamp server reported an error.</exception>
        /// <exception cref="HttpRequestException">The Bitstamp server could either not be reached or reported an
        /// unexpected error.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        /// <remarks><see cref="Order.Amount"/> is always denominated in the first currency. <see cref="Order.Price"/>
        /// is always denominated in the second currency.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        Task<OrderBook> GetOrderBookAsync();

        /// <summary>Creates a market buy order.</summary>
        /// <param name="amount">The amount to buy denominated in the first currency.</param>
        /// <exception cref="BitstampException">The Bitstamp server reported an error.</exception>
        /// <exception cref="HttpRequestException">The Bitstamp server could either not be reached or reported an
        /// unexpected error.</exception>
        /// <exception cref="InvalidOperationException">The private API cannot be accessed with this instance.
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        Task<PrivateOrder> CreateBuyOrderAsync(decimal amount);

        /// <summary>Creates a buy order.</summary>
        /// <param name="amount">The amount to buy denominated in the first currency.</param>
        /// <param name="price">The price denominated in the second currency.</param>
        /// <exception cref="BitstampException">The Bitstamp server reported an error.</exception>
        /// <exception cref="HttpRequestException">The Bitstamp server could either not be reached or reported an
        /// unexpected error.</exception>
        /// <exception cref="InvalidOperationException">The private API cannot be accessed with this instance.
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        Task<PrivateOrder> CreateBuyOrderAsync(decimal amount, decimal price);
    }
}
