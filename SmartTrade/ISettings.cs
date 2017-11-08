////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.ComponentModel;

    internal enum TransferToMainAccount
    {
        Never,
        TradePeriodEnd,
        EveryHundredthTrade,
        EveryTenthTrade,
        EveryTrade
    }

    internal enum NotifyEvents
    {
        TradesTransfersWarningsErrors,
        TransfersWarningsErrors,
        WarningsErrors,
        Errors
    }

    /// <summary>Represents the settings that need to be made persistent.</summary>
    internal interface ISettings : INotifyPropertyChanged, IDisposable
    {
        /// <summary>Gets the ticker symbol of the currency pair associated with the settings.</summary>
        string TickerSymbol { get; }

        /// <summary>Gets the first currency symbol.</summary>
        string FirstCurrency { get; }

        /// <summary>Gets the second currency symbol.</summary>
        string SecondCurrency { get; }

        /// <summary>Gets or sets the Bitstamp customer id.</summary>
        int CustomerId { get; set; }

        /// <summary>Gets or sets the Bitstamp api key.</summary>
        string ApiKey { get; set; }

        /// <summary>Gets or sets the Bitstamp api secret.</summary>
        string ApiSecret { get; set; }

        /// <summary>Gets or sets a value indicating whether to buy.</summary>
        /// <value><c>true</c> if the trade mode is buy; <c>false</c> if the trade mode is sell.</value>
        bool Buy { get; set; }

        /// <summary>Gets or sets the trade period.</summary>
        float TradePeriod { get; set; }

        /// <summary>Gets or sets a value indicating at what points acquired currency should be transferred back to the
        /// main account.</summary>
        /// <remarks>This setting is ignored when <see cref="ApiKey"/> is associated with the main account.</remarks>
        TransferToMainAccount TransferToMainAccount { get; set; }

        /// <summary>Gets or sets a value indicating about what events the user should be notified.</summary>
        NotifyEvents NotifyEvents { get; set; }

        /// <summary>Gets or sets the time of the last trade.</summary>
        /// <value>The time of the last trade attempt; or <c>null</c> if no trade has been made yet.</value>
        DateTime? LastTradeTime { get; set; }

        /// <summary>Gets or sets the result of the last trade attempt.</summary>
        string LastResult { get; set; }

        /// <summary>Gets or sets the balance of the first currency after the last trade attempt.</summary>
        float LastBalanceFirstCurrency { get; set; }

        /// <summary>Gets or sets the balance of the second currency after the last trade attempt.</summary>
        float LastBalanceSecondCurrency { get; set; }

        /// <summary>Gets or sets the next trade time.</summary>
        /// <value>The unix time of the next trade if the service is enabled; or, 0 if the trade service is disabled.
        /// </value>
        long NextTradeTime { get; set; }

        /// <summary>Gets or sets a value indicating whether this is a subaccount.</summary>
        /// <value><c>true</c> if this is a subaccount; otherwise, <c>false</c>.</value>
        bool IsSubaccount { get; set; }

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

        /// <summary>Gets or sets the number of trades performed since the last transfer to the main account.</summary>
        int TradeCountSinceLastTransfer { get; set; }

        /// <summary>Gets or sets the interval between retries.</summary>
        long RetryIntervalMilliseconds { get; set; }

        /// <summary>Logs all current values.</summary>
        void LogCurrentValues();
    }
}