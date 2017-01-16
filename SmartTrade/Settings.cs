////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Runtime.CompilerServices;

    using Android.App;
    using Android.Content;
    using Android.Preferences;

    using static Logger;

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

        /// <summary>Gets or sets the start of the current section.</summary>
        /// <value>The start of the current section; or <c>null</c> if no section has begun yet.</value>
        /// <remarks>A section is a part of a period. The current section always runs from <see cref="SectionStart"/>
        /// to <see cref="PeriodEnd"/>. A section ends and a new one begins at the point in time when either a new
        /// deposit is detected or when the user enables the service.</remarks>
        internal static DateTime? SectionStart
        {
            get { return GetDateTime(); }
            set { SetDateTime(value); }
        }

        /// <summary>Gets or sets the end of the current period.</summary>
        /// <value>The end of the current period; or <c>null</c> if no period has begun yet.</value>
        /// <remarks>A period always spans the whole time between two deposits. It consists of one or more sections.
        /// </remarks>
        internal static DateTime? PeriodEnd
        {
            get { return GetDateTime(); }
            set { SetDateTime(value); }
        }

        /// <summary>Gets or sets the timestamp of the last transaction.</summary>
        /// <value>The timestamp of the last known transaction; or, <see cref="DateTime.MinValue"/> if no transaction
        /// has ever been seen.</value>
        internal static DateTime LastTransactionTimestamp
        {
            get { return GetDateTime() ?? DateTime.MinValue; }
            set { SetDateTime(value); }
        }

        /// <summary>Gets or sets the interval between retries.</summary>
        internal static long RetryIntervalMilliseconds
        {
            get { return GetLong(); }
            set { SetLong(value); }
        }

        internal static void LogAll()
        {
            Log(nameof(NextTradeTime), NextTradeTime);
            Log(nameof(SectionStart), SectionStart);
            Log(nameof(PeriodEnd), PeriodEnd);
            Log(nameof(LastTransactionTimestamp), LastTransactionTimestamp);
            Log(nameof(RetryIntervalMilliseconds), RetryIntervalMilliseconds);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static DateTime? GetDateTime([CallerMemberName] string key = null)
        {
            var ticks = GetLong(key);
            return ticks == 0 ? (DateTime?)null : new DateTime(ticks, DateTimeKind.Utc);
        }

        private static void SetDateTime(DateTime? value, [CallerMemberName] string key = null)
        {
            if (value.HasValue && (value.Value.Kind != DateTimeKind.Utc))
            {
                throw new ArgumentException("UTC kind expected.", nameof(value));
            }

            SetLong(value?.Ticks ?? 0, key);
            Info("Set {0}.{1} = {2:o}.", nameof(Settings), key, value);
        }

        private static long GetLong([CallerMemberName] string key = null) => GetValue(p => p.GetLong(key, 0));

        private static void SetLong(long value, [CallerMemberName] string key = null)
        {
            SetValue(p => p.PutLong(key, value));
            Info("Set {0}.{1} = {2}.", nameof(Settings), key, value);
        }

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

        private static void Log(string propertyName, long value) =>
            Info("Current Value {0}.{1} = {2}.", nameof(Settings), propertyName, value);

        private static void Log(string propertyName, DateTime? value) =>
            Info("Current Value {0}.{1} = {2:o}.", nameof(Settings), propertyName, value);
    }
}