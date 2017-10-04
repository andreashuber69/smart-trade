////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Bitstamp;

    internal sealed class ExchangeClient : IExchangeClient
    {
        public ISettings Settings { get; }

        public ICurrencyExchange CurrencyExchange => this.getCurrencyExchange(this.Client);

        public void Dispose()
        {
            this.client?.Dispose();
            this.Settings.Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Ctor *is* being called, CA bug?")]
        internal ExchangeClient(ISettings settings, Func<BitstampClient, ICurrencyExchange> getCurrencyExchange)
        {
            this.Settings = settings;
            this.getCurrencyExchange = getCurrencyExchange;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly Func<BitstampClient, ICurrencyExchange> getCurrencyExchange;
        private BitstampClient client;

        private BitstampClient Client => this.client ?? (this.client =
            new BitstampClient(this.Settings.CustomerId, this.Settings.ApiKey, this.Settings.ApiSecret));
    }
}