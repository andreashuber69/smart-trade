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

    internal sealed class Settings : ISettings
    {
        public long NextTradeTime
        {
            get { return GetLong(); }
            set { SetLong(value); }
        }

        public DateTime? SectionStart
        {
            get { return GetDateTime(); }
            set { SetDateTime(value); }
        }

        public DateTime? PeriodEnd
        {
            get { return GetDateTime(); }
            set { SetDateTime(value); }
        }

        public DateTime LastTransactionTimestamp
        {
            get { return GetDateTime() ?? DateTime.MinValue; }
            set { SetDateTime(value); }
        }

        public long RetryIntervalMilliseconds
        {
            get { return GetLong(); }
            set { SetLong(value); }
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
    }
}