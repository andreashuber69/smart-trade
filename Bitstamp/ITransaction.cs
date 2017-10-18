////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;

    /// <summary>Represents a transaction for a currency pair.</summary>
    public interface ITransaction
    {
        /// <summary>Gets the transaction ID.</summary>
        int Id { get; }

        /// <summary>Gets the date and time of the transaction.</summary>
        DateTime DateTime { get; }

        /// <summary>Gets the type of the transaction.</summary>
        TransactionType TransactionType { get; }

        /// <summary>Gets the amount in the first currency.</summary>
        decimal? FirstAmount { get; }

        /// <summary>Gets the amount in the second currency.</summary>
        decimal? SecondAmount { get; }

        /// <summary>Gets the price denominated in the second currency.</summary>
        /// <remarks>Equals <c>null</c> if <see cref="TransactionType"/> is not equal to
        /// <see cref="TransactionType.MarketTrade"/>.</remarks>
        decimal? Price { get; }

        /// <summary>Gets the transaction fee denominated in the second currency.</summary>
        decimal Fee { get; }

        /// <summary>Gets the ID of the order that triggered this transaction.</summary>
        int? OrderId { get; }
    }
}
