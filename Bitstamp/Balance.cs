////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System.Json;

    internal sealed class Balance
    {
        internal Balance(string firstCurrency, string secondCurrency, JsonValue data)
        {
            this.FirstBalance = data[firstCurrency + "_balance"];
            this.FirstReserved = data[firstCurrency + "_reserved"];
            this.FirstAvailable = data[firstCurrency + "_available"];

            this.SecondBalance = data[secondCurrency + "_balance"];
            this.SecondReserved = data[secondCurrency + "_reserved"];
            this.SecondAvailable = data[secondCurrency + "_available"];

            this.Fee = data["fee"];
        }

        internal decimal FirstBalance { get; }

        internal decimal FirstReserved { get; }

        internal decimal FirstAvailable { get; }

        internal decimal SecondBalance { get; }

        internal decimal SecondReserved { get; }

        internal decimal SecondAvailable { get; }

        internal decimal Fee { get; }
    }
}
