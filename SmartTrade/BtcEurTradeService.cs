////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.App;
    using Bitstamp;

    [Service]
    internal sealed class BtcEurTradeService : TradeService
    {
        public BtcEurTradeService()
            : base(new ExchangeClient(
                new Settings(BitstampClient.BtcEurSymbol), c => c.Exchanges[BitstampClient.BtcEurSymbol]))
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override decimal MinTradeAmount => 5m;

        protected sealed override decimal FeeStep => 0.01m;
    }
}
