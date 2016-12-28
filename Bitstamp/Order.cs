////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    /// <summary>Represents a single element in <see cref="OrderCollection"/>.</summary>
    public sealed class Order
    {
        /// <summary>Gets the price.</summary>
        public decimal Price { get; }

        /// <summary>Gets the amount.</summary>
        public decimal Amount { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Order(decimal price, decimal amount)
        {
            this.Price = price;
            this.Amount = amount;
        }
    }
}
