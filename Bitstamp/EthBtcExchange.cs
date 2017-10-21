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
        private sealed class EthBtcExchange : CurrencyExchange
        {
            internal EthBtcExchange(BitstampClient client)
                : base(client, EthBtcSymbol)
            {
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            protected sealed override IBalance CreateBalance(Balance b) =>
                CreateBalance(b.EthAvailable, b.BtcAvailable, b.EthBtcFee);

            protected sealed override bool IsRelevantDepositOrWithdrawal(Transaction t) => (t.Eth != 0m) || (t.Btc != 0m);

            protected sealed override bool IsRelevantTrade(Transaction t) => t.EthBtc.HasValue;

            protected sealed override ITransaction CreateTransaction(Transaction t) =>
                CreateTransaction(t.Id, t.DateTime, t.TransactionType, t.Eth, t.Btc, t.EthBtc, t.Fee, t.OrderId);
        }
    }
}
