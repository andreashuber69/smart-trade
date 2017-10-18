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
        private sealed class BtcEurExchange : CurrencyExchange
        {
            internal BtcEurExchange(BitstampClient client)
                : base(client, BtcEurSymbol)
            {
            }

            internal sealed override IBalance CreateBalance(Balance balance) => new BtcEurBalance(balance);

            internal sealed override bool IsRelevantDepositOrWithdrawal(Transaction transaction) =>
                (transaction.Btc != 0m) || (transaction.Eur != 0m);

            internal sealed override bool IsRelevantTrade(Transaction transaction) => transaction.BtcEur.HasValue;

            internal sealed override ITransaction CreateTransaction(Transaction transaction) =>
                new BtcEurTransaction(transaction);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private sealed class BtcEurBalance : IBalance
            {
                public decimal FirstCurrency => this.balance.BtcAvailable;

                public decimal SecondCurrency => this.balance.EurAvailable;

                public decimal Fee => this.balance.BtcEurFee;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                internal BtcEurBalance(Balance balance) => this.balance = balance;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                private readonly Balance balance;
            }

            private sealed class BtcEurTransaction : ITransaction
            {
                public int Id => this.transaction.Id;

                public DateTime DateTime => this.transaction.DateTime;

                public TransactionType TransactionType => this.transaction.TransactionType;

                public decimal FirstAmount => this.transaction.Btc;

                public decimal SecondAmount => this.transaction.Eur;

                public decimal? Price => this.transaction.BtcEur;

                public decimal Fee => this.transaction.Fee;

                public int? OrderId => this.transaction.OrderId;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                internal BtcEurTransaction(Transaction transaction) => this.transaction = transaction;

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

                private readonly Transaction transaction;
            }
        }
    }
}
