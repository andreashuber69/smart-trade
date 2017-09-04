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
        internal Balance(JsonValue data)
        {
            this.UsdBalance = data["usd_balance"];
            this.BtcBalance = data["btc_balance"];
            this.EurBalance = data["eur_balance"];
            this.UsdReserved = data["usd_reserved"];
            this.BtcReserved = data["btc_reserved"];
            this.EurReserved = data["eur_reserved"];
            this.UsdAvailable = data["usd_available"];
            this.BtcAvailable = data["btc_available"];
            this.EurAvailable = data["eur_available"];
            this.BtcUsdFee = data["btcusd_fee"];
            this.BtcEurFee = data["btceur_fee"];
            this.EurUsdFee = data["eurusd_fee"];
        }

        internal decimal UsdBalance { get; }

        internal decimal BtcBalance { get; }

        internal decimal EurBalance { get; }

        internal decimal UsdReserved { get; }

        internal decimal BtcReserved { get; }

        internal decimal EurReserved { get; }

        internal decimal UsdAvailable { get; }

        internal decimal BtcAvailable { get; }

        internal decimal EurAvailable { get; }

        internal decimal BtcUsdFee { get; }

        internal decimal BtcEurFee { get; }

        internal decimal EurUsdFee { get; }
    }
}
