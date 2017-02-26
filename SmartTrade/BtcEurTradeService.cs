////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.App;

    [Service]
    internal sealed partial class BtcEurTradeService : TradeService<BtcEurExchangeClient, Settings>
    {
        public BtcEurTradeService()
            : base(typeof(BtcEurTradeService))
        {
        }
    }
}
