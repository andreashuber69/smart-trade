////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    /// <summary>Represents the balance for a currency pair.</summary>
    public interface IBalance
    {
        /// <summary>Gets the balance of the first currency.</summary>
        decimal FirstCurrency { get; }

        /// <summary>Gets the balance of the second currency.</summary>
        decimal SecondCurrency { get; }

        /// <summary>Gets the transaction fee in percent.</summary>
        decimal Fee { get; }
    }
}
