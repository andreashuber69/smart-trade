////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.Graphics;
    using Android.Widget;

    using static System.Globalization.CultureInfo;

    internal static class GuiHelper
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

        internal static void SetBalance(
            TextView currencyTextView, TextView integral, TextView fractional, string currency, float? value)
        {
            if (currencyTextView != null)
            {
                currencyTextView.Text = currency;
            }

            if (value.HasValue)
            {
                var parts = value.Value.ToString("f5", CurrentCulture).Split('.');
                integral.Text = parts[0];
                fractional.Text = '.' + parts[1];
            }
            else
            {
                integral.Text = null;
                fractional.Text = null;
            }
        }
    }
}
