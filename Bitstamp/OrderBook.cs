////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System.Collections.Generic;
    using System.Json;

    /// <summary>Represents the result of a call to <see cref="ICurrencyExchange.GetOrderBookAsync"/>.</summary>
    public sealed class OrderBook
    {
        /// <summary>Gets the UTC time at which the order book was current.</summary>
        public int Timestamp { get; }

        /// <summary>Gets the bids in descending order.</summary>
        public IReadOnlyList<Order> Bids { get; }

        /// <summary>Gets the asks in ascending order.</summary>
        public IReadOnlyList<Order> Asks { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal OrderBook(JsonValue data)
        {
            this.Timestamp = data["timestamp"];
            this.Bids = new OrderCollection(data["bids"]);
            this.Asks = new OrderCollection(data["asks"]);
        }
    }
}
