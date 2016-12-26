namespace SmartTrade
{
    using System;
    using System.Runtime.CompilerServices;
    using Android.App;
    using Android.Content;
    using Android.Preferences;

    internal static class Settings
    {
        internal static bool IsEnabled
        {
            get { return GetBoolean(); }
            set { SetBoolean(value); }
        }

        internal static long NextTradeTime
        {
            get { return GetLong(); }
            set { SetLong(value); }
        }

        internal static bool Sell
        {
            get { return GetBoolean(); }
            set { SetBoolean(value); }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static bool GetBoolean([CallerMemberName] string key = null) =>
            GetValue(p => p.GetBoolean(key, false));

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