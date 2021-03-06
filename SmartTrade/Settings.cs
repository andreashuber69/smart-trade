﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Android.App;
    using Android.Content;
    using Android.Preferences;
    using Android.Security;
    using Android.Security.Keystore;
    using Java.Math;
    using Java.Security;
    using Java.Util;
    using Javax.Crypto;
    using Javax.Security.Auth.X500;

    using static Logger;

    /// <summary>Holds the settings for a given ticker symbol.</summary>
    /// <remarks>The object implementing <see cref="ISharedPreferencesOnSharedPreferenceChangeListener"/> apparently
    /// needs to derive from <see cref="Java.Lang.Object"/>.</remarks>
    internal sealed class Settings : Java.Lang.Object, ISettings, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string TickerSymbol { get; }

        public string FirstCurrency => this.TickerSymbol.Substring(0, this.TickerSymbol.IndexOf('/'));

        public string SecondCurrency => this.TickerSymbol.Substring(this.TickerSymbol.IndexOf('/') + 1);

        public int CustomerId
        {
            get => (int)this.GetLong();
            set => this.SetLong(value);
        }

        public string ApiKey
        {
            get => this.GetString();
            set => this.SetString(value);
        }

        public string ApiSecret
        {
            get => this.GetPrivateString();
            set => this.SetPrivateString(value);
        }

        public bool Buy
        {
            get => this.GetLong() != 0;
            set => this.SetLong(value ? 1 : 0);
        }

        public float TradePeriod
        {
            get => this.GetFloat();
            set => this.SetFloat(value);
        }

        public TransferToMainAccount TransferToMainAccount
        {
            get => (TransferToMainAccount)this.GetLong();
            set => this.SetLong((long)value);
        }

        public NotifyEvents NotifyEvents
        {
            get => (NotifyEvents)this.GetLong();
            set => this.SetLong((long)value);
        }

        public DateTime? LastTradeTime
        {
            get => this.GetDateTime();
            set => this.SetDateTime(value);
        }

        public string LastStatus
        {
            get => this.GetString();
            set => this.SetString(value);
        }

        public float LastBalanceFirstCurrency
        {
            get => this.GetFloat();
            set => this.SetFloat(value);
        }

        public float LastBalanceSecondCurrency
        {
            get => this.GetFloat();
            set => this.SetFloat(value);
        }

        public long NextTradeTime
        {
            get => this.GetLong();

            set
            {
                if ((this.GetLong() != 0) && (value == 0))
                {
                    this.ClearSettings();
                }

                this.SetLong(value);
            }
        }

        public bool IsSubaccount
        {
            get => this.GetLong() != 0;
            set => this.SetLong(value ? 1 : 0);
        }

        public DateTime? SectionStart
        {
            get => this.GetDateTime();
            set => this.SetDateTime(value);
        }

        public DateTime? PeriodEnd
        {
            get => this.GetDateTime();
            set => this.SetDateTime(value);
        }

        public DateTime LastTransactionTimestamp
        {
            get => this.GetDateTime() ?? DateTime.MinValue;
            set => this.SetDateTime(value);
        }

        public int TradeCountSinceLastTransfer
        {
            get => (int)this.GetLong();
            set => this.SetLong(value);
        }

        public long RetryIntervalMilliseconds
        {
            get => this.GetLong();

            set => this.SetLong(
                Math.Max(this.MinRetryIntervalMilliseconds, Math.Min(this.MaxRetryIntervalMilliseconds, value)));
        }

        public long MinRetryIntervalMilliseconds => 2 * 60 * 1000;

        public long MaxRetryIntervalMilliseconds => 64 * 60 * 1000;

        public Status Status
        {
            get
            {
                if (this.RetryIntervalMilliseconds >= this.MaxRetryIntervalMilliseconds)
                {
                    // Show an error no matter whether the service is enabled or not. This is necessary so that the user
                    // can distinguish a service that has been disabled manually vs. one that has been disabled by an
                    // unexpected error.
                    return Status.Error;
                }
                else if (this.NextTradeTime == 0)
                {
                    return Status.Unknown;
                }
                else if (this.NextTradeTime > Java.Lang.JavaSystem.CurrentTimeMillis() - 1000)
                {
                    return
                        this.RetryIntervalMilliseconds > this.MinRetryIntervalMilliseconds ? Status.Warning : Status.Ok;
                }
                else
                {
                    // This should never happen in production. It happens e.g. when a new version is deployed from
                    // Visual Studio or when the debugger is used to restart the application.
                    // Note: We're deliberately not updating this periodically, even though this branch being run
                    // depends on the current time. It is expected that this branch will only run on developer
                    // phones or when there is a bug in the application.
                    return Status.Error;
                }
            }
        }

        public void LogCurrentValues()
        {
            this.LogCurrentValue(nameof(this.TickerSymbol), this.TickerSymbol);
            this.LogCurrentValue(nameof(this.CustomerId), this.CustomerId);
            this.LogCurrentValue(nameof(this.ApiKey), this.ApiKey);
            this.LogCurrentValue(nameof(this.ApiSecret), this.ApiSecret, ":");
            this.LogCurrentValue(nameof(this.Buy), this.Buy);
            this.LogCurrentValue(nameof(this.TradePeriod), this.TradePeriod);
            this.LogCurrentValue(nameof(this.TransferToMainAccount), this.TransferToMainAccount);
            this.LogCurrentValue(nameof(this.NotifyEvents), this.NotifyEvents);
            this.LogCurrentValue(nameof(this.LastTradeTime), this.LastTradeTime, ":o");
            this.LogCurrentValue(nameof(this.LastStatus), this.LastStatus);
            this.LogCurrentValue(nameof(this.LastBalanceFirstCurrency), this.LastBalanceFirstCurrency, ":f8");
            this.LogCurrentValue(nameof(this.LastBalanceSecondCurrency), this.LastBalanceSecondCurrency, ":f8");
            this.LogCurrentValue(nameof(this.NextTradeTime), this.NextTradeTime);
            this.LogCurrentValue(nameof(this.IsSubaccount), this.IsSubaccount);
            this.LogCurrentValue(nameof(this.SectionStart), this.SectionStart, ":o");
            this.LogCurrentValue(nameof(this.PeriodEnd), this.PeriodEnd, ":o");
            this.LogCurrentValue(nameof(this.LastTransactionTimestamp), this.LastTransactionTimestamp, ":o");
            this.LogCurrentValue(nameof(this.TradeCountSinceLastTransfer), this.TradeCountSinceLastTransfer);
            this.LogCurrentValue(nameof(this.RetryIntervalMilliseconds), this.RetryIntervalMilliseconds);
            this.LogCurrentValue(nameof(this.Status), this.Status);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void ISharedPreferencesOnSharedPreferenceChangeListener.OnSharedPreferenceChanged(
            ISharedPreferences sharedPreferences, string key)
        {
            if (key.StartsWith(this.groupName, StringComparison.Ordinal))
            {
                this.RaisePropertyChanged(key.Substring(this.groupName.Length));
            }
        }

        internal static ISettings Create(string tickerSymbol) => new Settings(tickerSymbol);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void Dispose(bool disposing)
        {
            try
            {
                this.preferences.UnregisterOnSharedPreferenceChangeListener(this);

                // Apparently the call to PreferenceManager.GetDefaultSharedPreferences() in the constructor returns a
                // singleton instance. Therefore, calling this.preferences.Dispose() here works as intended if and only
                // if at most one instance of this class ever exists. When multiple instances are in existence,
                // calling this.preferences.Dispose() here will render invalid this.preferences for *all* Settings
                // objects as soon as the Settings.Dispose() is called on any object.
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string KeyStoreName = "AndroidKeyStore";
        private const string KeyName = "BitstampApiCredentials";

        private static string CamelCase(string str) =>
            char.ToUpperInvariant(str[0]) + str.Substring(1).ToLowerInvariant();

        private static void GenerateKey()
        {
            var keyGenerator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KeyStoreName);

// KeyPairGeneratorSpec seems to offer the only way to generate RSA keys with the API level we're currently targeting
// (level 18). Since this has been deprecated in API level 23 and later and we're always compiling against the latest
// platform, we're bound to get CS0618 here.
#pragma warning disable 0618
            using (var builder = new KeyPairGeneratorSpec.Builder(Application.Context))
#pragma warning restore
            using (var principal = new X500Principal("CN=" + KeyName))
            {
                var cal = Calendar.Instance;
                var start = cal.Time;
                cal.Add(CalendarField.Year, 10);
                var end = cal.Time;

                var spec = builder
                    .SetAlias(KeyName)
                    .SetStartDate(start)
                    .SetEndDate(end)
                    .SetSerialNumber(BigInteger.One)
                    .SetSubject(principal).Build();
                keyGenerator.Initialize(spec);
                keyGenerator.GenerateKeyPair();
            }
        }

        private readonly string groupName;
        private readonly ISharedPreferences preferences;
        private readonly KeyStore keyStore;

        private Settings(string tickerSymbol)
        {
            this.TickerSymbol = tickerSymbol;
            var slashPosition = tickerSymbol.IndexOf('/');
            this.groupName = CamelCase(tickerSymbol.Substring(0, slashPosition)) +
                CamelCase(tickerSymbol.Substring(slashPosition + 1));
            this.preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            this.preferences.RegisterOnSharedPreferenceChangeListener(this);
            this.keyStore = KeyStore.GetInstance(KeyStoreName);
            this.keyStore.Load(null);
        }

        private DateTime? GetDateTime([CallerMemberName] string key = null)
        {
            var ticks = this.GetLong(key);
            return ticks == 0 ? (DateTime?)null : new DateTime(ticks, DateTimeKind.Utc);
        }

        private void SetDateTime(DateTime? value, [CallerMemberName] string key = null)
        {
            if (value.HasValue && (value.Value.Kind != DateTimeKind.Utc))
            {
                throw new ArgumentException("UTC kind expected.", nameof(value));
            }

            this.SetLong(value?.Ticks ?? 0, key);
            this.LogSetValue(key, value, ":o");
        }

        private long GetLong([CallerMemberName] string key = null) => this.GetValue((p, k) => p.GetLong(k, 0), key);

        private void SetLong(long value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutLong(k, v), key, this.GetLong(key), value);

        private float GetFloat([CallerMemberName] string key = null) =>
            this.GetValue((p, k) => p.GetFloat(k, 0.0f), key);

        private void SetFloat(float value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutFloat(k, v), key, this.GetFloat(key), value, ":f8");

        private string GetString([CallerMemberName] string key = null) =>
            this.GetValue((p, k) => p.GetString(k, string.Empty), key);

        private void SetString(string value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutString(k, v), key, this.GetString(key), value);

        private void ClearSettings()
        {
            this.SetLong(0, nameof(this.LastTradeTime));
            this.SetString(string.Empty, nameof(this.LastStatus));
            this.SetFloat(0.0f, nameof(this.LastBalanceFirstCurrency));
            this.SetFloat(0.0f, nameof(this.LastBalanceSecondCurrency));
            this.SetLong(0, nameof(this.IsSubaccount));
            this.SetLong(0, nameof(this.SectionStart));

            // Note: Clearing this setting means that SectionStart will be reset to the time of the last deposit when
            // the service is enabled the next time.
            this.SetLong(0, nameof(this.PeriodEnd));
            this.SetLong(0, nameof(this.LastTransactionTimestamp));
            this.SetLong(0, nameof(this.RetryIntervalMilliseconds));
        }

        private T GetValue<T>(Func<ISharedPreferences, string, T> getValue, string key) =>
            getValue(this.preferences, this.GetGroupedKey(key));

        private void SetValue<T>(
            Action<ISharedPreferencesEditor, string, T> setValue,
            string key,
            T currentValue,
            T newValue,
            string valueFormat = null)
        {
            if (object.Equals(currentValue, newValue))
            {
                return;
            }

            using (var editor = this.preferences.Edit())
            {
                setValue(editor, this.GetGroupedKey(key), newValue);
                editor.Apply();
            }

            this.LogSetValue(key, newValue, valueFormat);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case nameof(this.RetryIntervalMilliseconds):
                case nameof(this.NextTradeTime):
                    this.RaisePropertyChanged(nameof(this.Status));
                    break;
            }
        }

        private string GetPrivateString([CallerMemberName] string key = null) =>
            this.Decrypt(this.GetValue((p, k) => p.GetString(k, string.Empty), key));

        private void SetPrivateString(string value, [CallerMemberName] string key = null)
        {
            // Because two calls to Encrypt passing the same value will always yield a different cipher text, we have to
            // check for equality before encryption.
            if (this.GetPrivateString(key) == value)
            {
                return;
            }

            this.SetValue((p, k, v) => p.PutString(k, v), key, null, this.Encrypt(value), ":");
        }

        private string Decrypt(string encryptedValue) =>
            Encoding.UTF8.GetString(this.Crypt(Convert.FromBase64String(encryptedValue), CipherMode.DecryptMode));

        private string Encrypt(string value) =>
            Convert.ToBase64String(this.Crypt(Encoding.UTF8.GetBytes(value), CipherMode.EncryptMode));

        private byte[] Crypt(byte[] input, CipherMode mode)
        {
            var transformation = KeyProperties.KeyAlgorithmRsa + '/' +
                KeyProperties.BlockModeEcb + '/' + KeyProperties.EncryptionPaddingRsaPkcs1;

            using (var cipher = Cipher.GetInstance(transformation))
            {
                var key = this.GetKey();

                if (mode == CipherMode.EncryptMode)
                {
                    cipher.Init(mode, key.Certificate);
                }
                else
                {
                    if (input.Length == 0)
                    {
                        return input;
                    }

                    cipher.Init(mode, key.PrivateKey);
                }

                return cipher.DoFinal(input);
            }
        }

        private KeyStore.PrivateKeyEntry GetKey()
        {
            if (!this.keyStore.ContainsAlias(KeyName))
            {
                GenerateKey();

                // If we generate a new key, old encrypted data is useless.
                this.SetValue((p, k, v) => p.PutString(k, v), nameof(this.ApiSecret), null, string.Empty);
            }

            return (KeyStore.PrivateKeyEntry)this.keyStore.GetEntry(KeyName, null);
        }

        private void LogSetValue<T>(string key, T value, string valueFormat = null) =>
            this.LogValue("Set", key, value, valueFormat);

        private void LogCurrentValue<T>(string key, T value, string valueFormat = null) =>
            this.LogValue("Current Value", key, value, valueFormat);

        private void LogValue<T>(string prefix, string key, T value, string valueFormat = null)
        {
            var format = valueFormat == ":" ? "{0} {1}.{2}{3} = <secret>." : "{0} {1}.{2}{3} = {4" + valueFormat + "}.";
            Info(format, prefix, nameof(Settings), this.groupName, key, value);
        }

        private string GetGroupedKey(string key) => this.groupName + key;
    }
}