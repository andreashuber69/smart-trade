////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    /// <summary>The type of the <see cref="Transaction.TransactionType"/> property.</summary>
    public enum TransactionType
    {
        /// <summary>Deposit transaction.</summary>
        Deposit = 0,

        /// <summary>Withdrawal transaction.</summary>
        Withdrawal = 1,

        /// <summary>Market trade transaction.</summary>
        MarketTrade = 2,

        /// <summary>Subaccount transfer transaction.</summary>
        SubaccountTransfer = 14
    }
}
