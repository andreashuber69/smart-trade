////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    using Android.App;
    using Android.Content;
    using Android.Preferences;

    internal static class Settings
    {
        /// <summary>Gets or sets the next trade time.</summary>
        /// <value>The unix time of the next trade if the service is enabled; or, 0 if the trade service is disabled.
        /// </value>
        internal static long NextTradeTime
        {
            get { return GetLong(); }
            set { SetLong(value); }
        }

        /// <summary>Gets or sets a value indicating whether the service is currently selling or not.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Temporary, TODO.")]
        internal static bool Sell
        {
            get { return GetBoolean(); }
            set { SetBoolean(value); }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Temporary, TODO.")]
        private static bool GetBoolean([CallerMemberName] string key = null) =>
            GetValue(p => p.GetBoolean(key, false));

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Temporary, TODO.")]
        private static void SetBoolean(bool value, [CallerMemberName] string key = null) =>
            SetValue(p => p.PutBoolean(key, value));

        private static long GetLong([CallerMemberName] string key = null) =>
            GetValue(p => p.GetLong(key, 0));

        private static void SetLong(long value, [CallerMemberName] string key = null) =>
            SetValue(p => p.PutLong(key, value));

        private static void SetValue(Action<ISharedPreferencesEditor> setValue)
        {
            GetValue(
                p =>
                {
                    using (var editor = p.Edit())
                    {
                        setValue(editor);
                        editor.Apply();
                        return false;
                    }
                });
        }

        private static T GetValue<T>(Func<ISharedPreferences, T> getValue)
        {
            using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
            {
                return getValue(preferences);
            }
        }
    }
}