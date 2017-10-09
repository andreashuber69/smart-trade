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
            this.UsdReserved = data["usd_reserved"];
            this.UsdAvailable = data["usd_available"];

            this.BtcBalance = data["btc_balance"];
            this.BtcReserved = data["btc_reserved"];
            this.BtcAvailable = data["btc_available"];

            this.EurBalance = data["eur_balance"];
            this.EurReserved = data["eur_reserved"];
            this.EurAvailable = data["eur_available"];

            this.XrpBalance = data["xrp_balance"];
            this.XrpReserved = data["xrp_reserved"];
            this.XrpAvailable = data["xrp_available"];

            this.LtcBalance = data["ltc_balance"];
            this.LtcReserved = data["ltc_reserved"];
            this.LtcAvailable = data["ltc_available"];

            this.EthBalance = data["eth_balance"];
            this.EthReserved = data["eth_reserved"];
            this.EthAvailable = data["eth_available"];

            this.BtcUsdFee = data["btcusd_fee"];
            this.BtcEurFee = data["btceur_fee"];
            this.EurUsdFee = data["eurusd_fee"];

            this.XrpUsdFee = data["xrpusd_fee"];
            this.XrpEurFee = data["xrpeur_fee"];
            this.XrpBtcFee = data["xrpbtc_fee"];

            this.LtcUsdFee = data["ltcusd_fee"];
            this.LtcEurFee = data["ltceur_fee"];
            this.LtcBtcFee = data["ltcbtc_fee"];

            this.EthUsdFee = data["ethusd_fee"];
            this.EthEurFee = data["etheur_fee"];
            this.EthBtcFee = data["ethbtc_fee"];
        }

        internal decimal UsdBalance { get; }

        internal decimal UsdReserved { get; }

        internal decimal UsdAvailable { get; }

        internal decimal BtcBalance { get; }

        internal decimal BtcReserved { get; }

        internal decimal BtcAvailable { get; }

        internal decimal EurBalance { get; }

        internal decimal EurReserved { get; }

        internal decimal EurAvailable { get; }

        internal decimal XrpBalance { get; }

        internal decimal XrpReserved { get; }

        internal decimal XrpAvailable { get; }

        internal decimal LtcBalance { get; }

        internal decimal LtcReserved { get; }

        internal decimal LtcAvailable { get; }

        internal decimal EthBalance { get; }

        internal decimal EthReserved { get; }

        internal decimal EthAvailable { get; }

        internal decimal BtcUsdFee { get; }

        internal decimal BtcEurFee { get; }

        internal decimal EurUsdFee { get; }

        internal decimal XrpUsdFee { get; }

        internal decimal XrpEurFee { get; }

        internal decimal XrpBtcFee { get; }

        internal decimal LtcUsdFee { get; }

        internal decimal LtcEurFee { get; }

        internal decimal LtcBtcFee { get; }

        internal decimal EthUsdFee { get; }

        internal decimal EthEurFee { get; }

        internal decimal EthBtcFee { get; }
    }
}
