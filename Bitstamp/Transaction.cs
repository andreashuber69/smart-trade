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
            this.Btc = data["btc"];
            this.Eur = data["eur"];
            this.Xrp = GetOptionalDecimal(data, "xrp");
            this.Ltc = GetOptionalDecimal(data, "ltc");
            this.Eth = GetOptionalDecimal(data, "eth");
            this.BtcUsd = GetOptionalDecimal(data, "btc_usd");
            this.BtcEur = GetOptionalDecimal(data, "btc_eur");
            this.EurUsd = GetOptionalDecimal(data, "eur_usd");
            this.XrpUsd = GetOptionalDecimal(data, "xrp_usd");
            this.XrpEur = GetOptionalDecimal(data, "xrp_eur");
            this.XrpBtc = GetOptionalDecimal(data, "xrp_btc");
            this.LtcUsd = GetOptionalDecimal(data, "ltc_usd");
            this.LtcEur = GetOptionalDecimal(data, "ltc_eur");
            this.LtcBtc = GetOptionalDecimal(data, "ltc_btc");
            this.EthUsd = GetOptionalDecimal(data, "eth_usd");
            this.EthEur = GetOptionalDecimal(data, "eth_eur");
            this.EthBtc = GetOptionalDecimal(data, "eth_btc");
            this.Fee = data["fee"];
            this.OrderId = GetOptionalInt(data, "order_id");
        }

        internal int Id { get; }

        internal DateTime DateTime { get; }

        internal TransactionType TransactionType { get; }

        internal decimal Usd { get; }

        internal decimal Btc { get; }

        internal decimal Eur { get; }

        internal decimal? Xrp { get; }

        internal decimal? Ltc { get; }

        internal decimal? Eth { get; }

        internal decimal? BtcUsd { get; }

        internal decimal? BtcEur { get; }

        internal decimal? EurUsd { get; }

        internal decimal? XrpUsd { get; }

        internal decimal? XrpEur { get; }

        internal decimal? XrpBtc { get; }

        internal decimal? LtcUsd { get; }

        internal decimal? LtcEur { get; }

        internal decimal? LtcBtc { get; }

        internal decimal? EthUsd { get; }

        internal decimal? EthEur { get; }

        internal decimal? EthBtc { get; }

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
