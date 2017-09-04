////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Json;

    internal sealed class Transaction
    {
        internal Transaction(JsonValue data)
        {
            this.Id = data["id"];
            this.DateTime = DateTimeHelper.Parse(data["datetime"]);
            this.TransactionType = (TransactionType)(int)data["type"];
            this.Usd = data["usd"];
            this.Eur = data["eur"];
            this.Btc = data["btc"];
            this.BtcUsd = GetOptionalDecimal(data, "btc_usd");
            this.BtcEur = GetOptionalDecimal(data, "btc_eur");
            this.Fee = data["fee"];
            this.OrderId = GetOptionalInt(data, "order_id");
        }

        internal int Id { get; }

        internal DateTime DateTime { get; }

        internal TransactionType TransactionType { get; }

        internal decimal Usd { get; }

        internal decimal Eur { get; }

        internal decimal Btc { get; }

        internal decimal? BtcUsd { get; }

        internal decimal? BtcEur { get; }

        internal decimal Fee { get; }

        internal int? OrderId { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static decimal? GetOptionalDecimal(JsonValue data, string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            else
            {
                return null;
            }
        }

        private static int? GetOptionalInt(JsonValue data, string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            else
            {
                return null;
            }
        }
    }
}
