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
        private sealed class BtcUsdExchange : CurrencyExchange
        {
            internal BtcUsdExchange(BitstampClient client)
                : base(client, BtcUsdSymbol)
            {
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected sealed override IBalance CreateBalance(Balance b) =>
                CreateBalance(b.BtcAvailable, b.UsdAvailable, b.BtcUsdFee);

            protected sealed override bool IsRelevantDepositOrWithdrawal(Transaction t) => (t.Btc != 0m) || (t.Usd != 0m);

            protected sealed override bool IsRelevantTrade(Transaction t) => t.BtcUsd.HasValue;

            protected sealed override ITransaction CreateTransaction(Transaction t) =>
                CreateTransaction(t.Id, t.DateTime, t.TransactionType, t.Btc, t.Usd, t.BtcUsd, t.Fee, t.OrderId);
        }
    }
}