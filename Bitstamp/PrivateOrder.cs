////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>Represents the result of a call to
    /// <see cref="ICurrencyExchange.CreateBuyOrderAsync(decimal, decimal)"/>.</summary>
    [DataContract]
    public sealed class PrivateOrder
    {
        /// <summary>Gets the order ID.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "id")]
        public int Id { get; private set; }

        /// <summary>Gets the date and time of the order.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public DateTime DateTime { get; private set; }

        /// <summary>Gets the type of the order.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "type")]
        public OrderType OrderType { get; private set; }

        /// <summary>Gets the price denominated in the second currency.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "price")]
        public decimal Price { get; private set; }

        /// <summary>Gets the amount to buy denominated in the first currency.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "amount")]
        public decimal Amount { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private PrivateOrder()
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        [DataMember(Name = "datetime")]
        private string DateTimeImpl
        {
            get { return DateTimeHelper.ToString(this.DateTime); }
            set { this.DateTime = DateTimeHelper.Parse(value); }
        }
    }
}
