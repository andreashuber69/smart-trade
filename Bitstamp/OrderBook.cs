////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>Represents the result of a call to <see cref="ICurrencyExchange.GetOrderBookAsync"/>.</summary>
    [DataContract]
    public sealed class OrderBook
    {
        /// <summary>Gets the UTC time at which the order book was current.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "timestamp")]
        public int Timestamp { get; private set; }

        /// <summary>Gets the bids in descending order.</summary>
        public IReadOnlyList<Order> Bids => this.BidsImpl;

        /// <summary>Gets the asks in ascending order.</summary>
        public IReadOnlyList<Order> Asks => this.AsksImpl;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private OrderBook()
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "bids")]
        private OrderCollection BidsImpl { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "asks")]
        private OrderCollection AsksImpl { get; set; }
    }
}
