﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient
    {
        private sealed class BchEurExchange : CurrencyExchange
        {
            internal BchEurExchange(BitstampClient client)
                : base(client, BchEurSymbol)
            {
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected sealed override bool IsRelevantDepositOrWithdrawal(Transaction t) => (t.Bch != 0m) || (t.Eur != 0m);

            protected sealed override bool IsRelevantTrade(Transaction t) => t.BchEur.HasValue;

            protected sealed override ITransaction CreateTransaction(Transaction t) =>
                CreateTransaction(t.Id, t.DateTime, t.TransactionType, t.Bch, t.Eur, t.BchEur, t.Fee, t.OrderId);
        }
    }
}
