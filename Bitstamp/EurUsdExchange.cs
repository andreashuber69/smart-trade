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
        private sealed class EurUsdExchange : CurrencyExchange
        {
            internal EurUsdExchange(BitstampClient client)
                : base(client, EurUsdSymbol)
            {
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected sealed override bool IsRelevantDepositOrWithdrawal(Transaction t) => (t.Eur != 0m) || (t.Usd != 0m);

            protected sealed override bool IsRelevantTrade(Transaction t) => t.EurUsd.HasValue;

            protected sealed override ITransaction CreateTransaction(Transaction t) =>
                CreateTransaction(t.Id, t.DateTime, t.TransactionType, t.Eur, t.Usd, t.EurUsd, t.Fee, t.OrderId);
        }
    }
}
