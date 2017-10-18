////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;

    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient
    {
        private sealed class LtcBtcExchange : CurrencyExchange
        {
            internal LtcBtcExchange(BitstampClient client)
                : base(client, LtcBtcSymbol)
            {
            }

            internal sealed override IBalance CreateBalance(Balance balance) => new LtcBtcBalance(balance);

            internal sealed override bool IsRelevantDepositOrWithdrawal(Transaction transaction) =>
                (transaction.Ltc != 0m) || (transaction.Btc != 0m);

            internal sealed override bool IsRelevantTrade(Transaction transaction) => transaction.LtcBtc.HasValue;

            internal sealed override ITransaction CreateTransaction(Transaction transaction) =>
                new LtcBtcTransaction(transaction);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private sealed class LtcBtcBalance : IBalance
            {
                public decimal FirstCurrency => this.balance.LtcAvailable;

                public decimal SecondCurrency => this.balance.BtcAvailable;

                public decimal Fee => this.balance.LtcBtcFee;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                internal LtcBtcBalance(Balance balance) => this.balance = balance;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                private readonly Balance balance;
            }

            private sealed class LtcBtcTransaction : ITransaction
            {
                public int Id => this.transaction.Id;

                public DateTime DateTime => this.transaction.DateTime;

                public TransactionType TransactionType => this.transaction.TransactionType;

                public decimal FirstAmount => this.transaction.Ltc.Value;

                public decimal SecondAmount => this.transaction.Btc;

                public decimal? Price => this.transaction.LtcBtc;

                public decimal Fee => this.transaction.Fee;

                public int? OrderId => this.transaction.OrderId;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                internal LtcBtcTransaction(Transaction transaction) => this.transaction = transaction;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                private readonly Transaction transaction;
            }
        }
    }
}
