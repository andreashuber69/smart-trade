////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;

    using static System.Globalization.CultureInfo;
    using static System.Globalization.DateTimeStyles;

    internal static class DateTimeHelper
    {
        internal static DateTime Parse(string value) =>
            DateTime.Parse(value, InvariantCulture, AssumeUniversal | AdjustToUniversal);

        internal static string ToString(DateTime dateTime) => dateTime.ToString(Format, InvariantCulture);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string Format = "yyyy-MM-dd HH:mm:ss.ffffff";
    }
}
