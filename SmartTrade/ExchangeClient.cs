﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Bitstamp;

    internal abstract class ExchangeClient : IExchangeClient
    {
        public ICurrencyExchange CurrencyExchange => this.getCurrencyExchange(this.client);

        public void Dispose() => this.client.Dispose();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Ctor *is* being called, CA bug?")]
        protected ExchangeClient(Func<BitstampClient, ICurrencyExchange> getCurrencyExchange)
        {
            var settings = new Settings();
            this.client = new BitstampClient(settings.CustomerId, settings.ApiKey, settings.ApiSecret);
            this.getCurrencyExchange = getCurrencyExchange;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly BitstampClient client;
        private readonly Func<BitstampClient, ICurrencyExchange> getCurrencyExchange;
    }
}