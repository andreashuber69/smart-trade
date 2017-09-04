////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System.Collections.Generic;
    using System.Json;

    internal sealed class TransactionCollection : List<Transaction>
    {
        internal TransactionCollection(JsonValue data)
        {
            foreach (var element in (JsonArray)data)
            {
                this.Add(new Transaction(element));
            }
        }
    }
}
