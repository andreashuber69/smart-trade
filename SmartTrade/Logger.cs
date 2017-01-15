////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.Util;

    internal static class Logger
    {
        internal static void Info(string format, params object[] args) => Log.Info(LogTag, format, args);

        internal static void Warn(string format, params object[] args) => Log.Warn(LogTag, format, args);

        internal static void Error(string format, params object[] args) => Log.Error(LogTag, format, args);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string LogTag = "SmartTrade";
    }
}