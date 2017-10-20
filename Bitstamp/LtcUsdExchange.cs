////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient
    {
        private sealed class LtcUsdExchange : CurrencyExchange
        {
            internal LtcUsdExchange(BitstampClient client)
                : base(client, LtcUsdSymbol)
            {
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected sealed override IBalance CreateBalance(Balance b) =>
                CreateBalance(b.LtcAvailable, b.UsdAvailable, b.LtcUsdFee);

            protected sealed override bool IsRelevantDepositOrWithdrawal(Transaction t) => (t.Ltc != 0m) || (t.Usd != 0m);

            protected sealed override bool IsRelevantTrade(Transaction t) => t.LtcUsd.HasValue;

            protected sealed override ITransaction CreateTransaction(Transaction t) =>
                CreateTransaction(t.Id, t.DateTime, t.TransactionType, t.Ltc, t.Usd, t.LtcUsd, t.Fee, t.OrderId);
        }
    }
}
