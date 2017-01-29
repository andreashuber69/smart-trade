////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;

    /// <summary>Represents the settings that need to be made persistent.</summary>
    internal interface ISettings
    {
        /// <summary>Gets or sets the next trade time.</summary>
        /// <value>The unix time of the next trade if the service is enabled; or, 0 if the trade service is disabled.
        /// </value>
        long NextTradeTime { get; set; }

        /// <summary>Gets or sets the start of the current section.</summary>
        /// <value>The start of the current section; or <c>null</c> if no section has begun yet.</value>
        /// <remarks>A section is a part of a period. The current section always runs from <see cref="SectionStart"/>
        /// to <see cref="PeriodEnd"/>. A section ends and a new one begins at the point in time when either a new
        /// deposit is detected or when the user enables the service.</remarks>
        DateTime? SectionStart { get; set; }

        /// <summary>Gets or sets the end of the current period.</summary>
        /// <value>The end of the current period; or <c>null</c> if no period has begun yet.</value>
        /// <remarks>A period always spans the whole time between two deposits. It consists of one or more sections.
        /// </remarks>
        DateTime? PeriodEnd { get; set; }

        /// <summary>Gets or sets the timestamp of the last transaction.</summary>
        /// <value>The timestamp of the last known transaction; or, <see cref="DateTime.MinValue"/> if no transaction
        /// has ever been seen.</value>
        DateTime LastTransactionTimestamp { get; set; }

        /// <summary>Gets or sets the interval between retries.</summary>
        long RetryIntervalMilliseconds { get; set; }

        /// <summary>Gets or sets the Bitstamp customer id.</summary>
        int CustomerId { get; set; }

        /// <summary>Gets or sets the Bitstamp api key.</summary>
        string ApiKey { get; set; }

        /// <summary>Gets or sets the Bitstamp api secret.</summary>
        string ApiSecret { get; set; }
    }
}