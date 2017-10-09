////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.App;
    using Bitstamp;

    internal abstract partial class TradeService
    {
        [Service]
        private sealed class BtcEurTradeService : TradeService
        {
            public BtcEurTradeService()
                : base(BitstampClient.BtcEurSymbol, 5m, 0.01m)
            {
            }
        }
    }
}
