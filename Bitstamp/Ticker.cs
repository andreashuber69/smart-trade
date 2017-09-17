////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Json;

    /// <summary>Represents the result of a call to <see cref="ICurrencyExchange.GetTickerAsync"/>.</summary>
    public sealed class Ticker
    {
        /// <summary>Gets the last price.</summary>
        public decimal Last { get; }

        /// <summary>Gets the 24 hour high.</summary>
        public decimal High { get; }

        /// <summary>Gets the 24 hour low.</summary>
        public decimal Low { get; }

        /// <summary>Gets the 24 hour volume weighted average price.</summary>
        public decimal Vwap { get; }

        /// <summary>Gets the 24 hour volume.</summary>
        public decimal Volume { get; }

        /// <summary>Gets the price of the highest buy order.</summary>
        public decimal Bid { get; }

        /// <summary>Gets the price of the lowest sell order.</summary>
        public decimal Ask { get; }

        /// <summary>Gets the UTC time at which the ticker was current.</summary>
        public DateTime Timestamp { get; }

        /// <summary>Gets the first price of the day.</summary>
        public decimal Open { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Ticker(JsonValue data)
        {
            this.Last = data["last"];
            this.High = data["high"];
            this.Low = data["low"];
            this.Vwap = data["vwap"];
            this.Volume = data["volume"];
            this.Bid = data["bid"];
            this.Ask = data["ask"];
            this.Timestamp = DateTimeOffset.FromUnixTimeSeconds(data["timestamp"]).UtcDateTime;
            this.Open = data["open"];
        }
    }
}
