////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Json;

    /// <summary>Represents the result of a call to
    /// <see cref="ICurrencyExchange.CreateBuyOrderAsync(decimal, decimal)"/>.</summary>
    public sealed class PrivateOrder
    {
        /// <summary>Gets the order ID.</summary>
        public int Id { get; }

        /// <summary>Gets the date and time of the order.</summary>
        public DateTime DateTime { get; }

        /// <summary>Gets the type of the order.</summary>
        public OrderType OrderType { get; }

        /// <summary>Gets the price denominated in the second currency.</summary>
        public decimal Price { get; }

        /// <summary>Gets the amount to buy denominated in the first currency.</summary>
        public decimal Amount { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal PrivateOrder(JsonValue data)
        {
            this.Id = data["id"];
            this.DateTime = DateTimeHelper.Parse(data["datetime"]);
            this.OrderType = (OrderType)(int)data["type"];
            this.Price = data["price"];
            this.Amount = data["amount"];
        }
    }
}
