////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.Graphics;

    internal static class Colors
    {
        internal static Color GetStatusColor(Status status, Color unknownColor)
        {
            switch (status)
            {
                case Status.Unknown:
                    return unknownColor;
                case Status.Ok:
                    return Color.Green;
                case Status.Warning:
                    return Color.Yellow;
                default:
                    return Color.Red;
            }
        }
    }
}
