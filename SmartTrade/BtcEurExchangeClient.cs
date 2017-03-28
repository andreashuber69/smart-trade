////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System.Diagnostics.CodeAnalysis;

    internal sealed class BtcEurExchangeClient : ExchangeClient
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is disposed in the ExchangeClient class.")]
        public BtcEurExchangeClient()
            : base(new BtcEurSettings(), c => c.BtcEur)
        {
        }
    }
}